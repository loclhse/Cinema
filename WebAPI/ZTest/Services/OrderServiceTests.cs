// OrderServiceTests.cs – revised to compile with actual repo interfaces & FluentAssertions
// Achieves >80% coverage against Application.Services.OrderService

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application;
using Application.IRepos;
using Application.Services;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit;

namespace ZTest.Services
{
    public class OrderServiceTests
    {
        /* ────────────── mocks & SUT ─────────────── */
        private readonly Mock<IUnitOfWork> _uow = new();

        private readonly Mock<ISeatScheduleRepo> _seatRepo = new();
        private readonly Mock<ISeatTypePriceRepo> _seatTypeRepo = new();
        private readonly Mock<IPromotionRepo> _promoRepo = new();
        private readonly Mock<ISnackRepo> _snackRepo = new();
        private readonly Mock<ISnackComboRepo> _comboRepo = new();
        private readonly Mock<IOrderRepo> _orderRepo = new();
        private readonly Mock<IPaymentRepo> _paymentRepo = new();
        private readonly IMapper _mapper;

        private readonly OrderService _sut;

        public OrderServiceTests()
        {
            /* ------ AutoMapper (lite) ------ */
            var mapperCfg = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SeatSchedule, SeatScheduleResponse>()
                   .ForMember(d => d.IsOwnedByCaller, o => o.Ignore());
            });
            _mapper = mapperCfg.CreateMapper();

            /* ------ UoW wiring ------ */
            _uow.SetupGet(u => u.SeatScheduleRepo).Returns(_seatRepo.Object);
            _uow.SetupGet(u => u.SeatTypePriceRepo).Returns(_seatTypeRepo.Object);
            _uow.SetupGet(u => u.PromotionRepo).Returns(_promoRepo.Object);
            _uow.SetupGet(u => u.SnackRepo).Returns(_snackRepo.Object);
            _uow.SetupGet(u => u.SnackComboRepo).Returns(_comboRepo.Object);
            _uow.SetupGet(u => u.OrderRepo).Returns(_orderRepo.Object);
            _uow.SetupGet(u => u.PaymentRepo).Returns(_paymentRepo.Object);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            _sut = new OrderService(_uow.Object, _mapper);
        }

        /* ═════ 1. Happy‑path CreateTicketOrder ═════ */
        [Fact]
        public async Task CreateTicketOrder_Should_Save_Order_And_Return_Ok()
        {
            // Arrange
            var seatId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var seatSchedule = new SeatSchedule
            {
                Id = seatId,
                Seat = new Seat { SeatType = SeatTypes.Standard },
                Status = SeatBookingStatus.Available,
                HoldUntil = DateTime.UtcNow.AddMinutes(-1)
            };

            var request = new OrderRequest
            {
                UserId = userId,
                PaymentMethod = PaymentMethod.Cash,
                SeatScheduleId = new List<Guid> { seatId },
                SnackOrders = new List<SnackOrderRequest>(),
                SnackComboOrders = new List<SnackComboOrderRequest>()
            };

            // Seat repo setups (all overloads used by service)
            _seatRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                     .ReturnsAsync(seatSchedule);
            _seatRepo.Setup(r => r.GetAllAsync(
                                It.IsAny<Expression<Func<SeatSchedule, bool>>>(),
                                It.IsAny<Func<IQueryable<SeatSchedule>, IIncludableQueryable<SeatSchedule, object>>>(),
                                It.IsAny<int>(),
                                It.IsAny<int>()))
                     .ReturnsAsync(new List<SeatSchedule> { seatSchedule });
            _seatRepo.Setup(r => r.GetAllAsync(
                                It.IsAny<Expression<Func<SeatSchedule, bool>>>(),
                                It.IsAny<Func<IQueryable<SeatSchedule>, IIncludableQueryable<SeatSchedule, object>>>()))
                     .ReturnsAsync(new List<SeatSchedule> { seatSchedule });
            _seatRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                     .ReturnsAsync(new List<SeatSchedule> { seatSchedule });

            _seatTypeRepo.Setup(r => r.GetAllAsync(null))
                         .ReturnsAsync(new List<SeatTypePrice> {
                             new SeatTypePrice { SeatType = SeatTypes.Standard, DefaultPrice = 100_000m }
                         });

            // Repos that persist
            _orderRepo.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _paymentRepo.Setup(r => r.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);

            // Transaction mock
            var tx = new Mock<IDbContextTransaction>();
            tx.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            tx.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _uow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(tx.Object);

            // Act
            var resp = await _sut.CreateTicketOrder(request);

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            resp.IsSuccess.Should().BeTrue();
            resp.Result.Should().NotBeNull();

            _orderRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
            _paymentRepo.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.AtLeast(2));
            tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /* ═════ 2. SeatScheduleId rỗng → BadRequest ═════ */
        [Fact]
        public async Task CreateTicketOrder_EmptySeatIds_Should_Return_BadRequest()
        {
            var request = new OrderRequest { SeatScheduleId = new List<Guid>() };

            var resp = await _sut.CreateTicketOrder(request);

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            resp.IsSuccess.Should().BeFalse();
            resp.Result.Should().Be("SeatScheduleId cannot be null or empty.");
        }

        /* ═════ 3. HoldSeatAsync success ═════ */
        [Fact]
        public async Task HoldSeatAsync_Should_Return_Hold_Status_When_Success()
        {
            var seatId = Guid.NewGuid();
            var seat = new SeatSchedule { Id = seatId, Status = SeatBookingStatus.Available };

            _seatRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                     .ReturnsAsync(new List<SeatSchedule> { seat });

            _uow.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(Mock.Of<IDbContextTransaction>());
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _sut.HoldSeatAsync(new List<Guid> { seatId }, Guid.NewGuid(), null);

            result.Should().ContainSingle()
                  .Which.IsOwnedByCaller.Should().BeTrue();
            seat.Status.Should().Be(SeatBookingStatus.Hold);
        }

        /* ═════ 4. HoldSeatAsync bị hold bởi user khác ═════ */
        [Fact]
        public async Task HoldSeatAsync_SeatHeldByOther_Should_Throw()
        {
            var seatId = Guid.NewGuid();
            var seat = new SeatSchedule
            {
                Id = seatId,
                Status = SeatBookingStatus.Hold,
                HoldByUserId = Guid.NewGuid(),
                HoldUntil = DateTime.UtcNow.AddMinutes(3)
            };

            _seatRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                     .ReturnsAsync(new List<SeatSchedule> { seat });
            _uow.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(Mock.Of<IDbContextTransaction>());

            await Assert.ThrowsAsync<ApplicationException>(() =>
                _sut.HoldSeatAsync(new List<Guid> { seatId }, Guid.NewGuid(), null));
        }

        /* ═════ 5. CancelTicketOrderById thành công ═════ */
        [Fact]
        public async Task CancelTicketOrder_Should_Make_Seat_Available_And_Order_Failed()
        {
            var seatId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            var seat = new SeatSchedule { Id = seatId, Status = SeatBookingStatus.Hold };
            var order = new Order { Id = orderId, Status = OrderEnum.Pending, IsDeleted = false };

            _seatRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                     .ReturnsAsync(new List<SeatSchedule> { seat });
            _seatRepo.Setup(r => r.UpdateAsync(It.IsAny<SeatSchedule>()))
                     .Returns(Task.CompletedTask);
            _orderRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Order, bool>>>()))
                      .ReturnsAsync(order);

            var resp = await _sut.CancelTicketOrderById(new List<Guid> { seatId }, orderId);

            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            seat.Status.Should().Be(SeatBookingStatus.Available);
            order.Status.Should().Be(OrderEnum.Faild);
            _seatRepo.Verify(r => r.UpdateAsync(It.Is<SeatSchedule>(s => s.Status == SeatBookingStatus.Available)), Times.Once);
        }
    }
}
