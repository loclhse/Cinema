//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Net;
//using System.Threading.Tasks;
//using Application;
//using Application.IRepos;
//using Application.Services;
//using Application.ViewModel;
//using Application.ViewModel.Request;
//using Application.ViewModel.Response;
//using AutoMapper;
//using Domain.Entities;
//using Domain.Enums;
//using FluentAssertions;
//using Microsoft.EntityFrameworkCore.Query;
//using Microsoft.EntityFrameworkCore.Storage;
//using Moq;
//using Xunit;

//namespace ZTest.Services
//{
//    public class OrderServiceTests
//    {
//        /* ────────────── mocks & SUT ─────────────── */
//        private readonly Mock<IUnitOfWork> _uow = new();
//        private readonly Mock<IMapper> _mapper = new();

//        private readonly Mock<ISeatScheduleRepo> _seatRepo = new();
//        private readonly Mock<ISeatTypePriceRepo> _seatTypeRepo = new();
//        private readonly Mock<IPromotionRepo> _promoRepo = new();
//        private readonly Mock<ISnackRepo> _snackRepo = new();
//        private readonly Mock<ISnackComboRepo> _comboRepo = new();
//        private readonly Mock<IOrderRepo> _orderRepo = new();
//        private readonly Mock<IPaymentRepo> _paymentRepo = new();

//        private readonly OrderService _sut;

//        public OrderServiceTests()
//        {
//            _uow.SetupGet(u => u.SeatScheduleRepo).Returns(_seatRepo.Object);
//            _uow.SetupGet(u => u.SeatTypePriceRepo).Returns(_seatTypeRepo.Object);
//            _uow.SetupGet(u => u.PromotionRepo).Returns(_promoRepo.Object);
//            _uow.SetupGet(u => u.SnackRepo).Returns(_snackRepo.Object);
//            _uow.SetupGet(u => u.SnackComboRepo).Returns(_comboRepo.Object);
//            _uow.SetupGet(u => u.OrderRepo).Returns(_orderRepo.Object);
//            _uow.SetupGet(u => u.PaymentRepo).Returns(_paymentRepo.Object);

//            _sut = new OrderService(_uow.Object, _mapper.Object);
//        }

//        /* ═════ 1. Happy-path CreateTicketOrder ═════ */
//        [Fact]
//        public async Task CreateTicketOrder_Should_SaveOrder_AndReturnOk()
//        {
//            /* ---------- Arrange ---------- */
//            var seatId = Guid.NewGuid();
//            var userId = Guid.NewGuid();

//            var seatSch = new SeatSchedule
//            {
//                Id = seatId,
//                Seat = new Seat { SeatType = SeatTypes.Standard },
//                Status = SeatBookingStatus.Available,
//                HoldUntil = DateTime.UtcNow.AddMinutes(-1)
//            };

//            var req = new OrderRequest
//            {
//                UserId = userId,
//                PaymentMethod = PaymentMethod.Cash,
//                SeatScheduleId = new List<Guid> { seatId },
//                Snack = new List<SnackOrderRequest>(),
//                SnackCombo = new List<SnackComboOrderRequest>()
//            };

//            /* ---- SeatScheduleRepo ---- */
//            _seatRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
//                     .ReturnsAsync(seatSch);

//            // overload 4 tham số (khi service truyền page)
//            _seatRepo.Setup(r => r.GetAllAsync(
//                                It.IsAny<Expression<Func<SeatSchedule, bool>>>(),
//                                It.IsAny<Func<IQueryable<SeatSchedule>, IIncludableQueryable<SeatSchedule, object>>>(),
//                                It.IsAny<int>(),
//                                It.IsAny<int>()))
//                     .ReturnsAsync(new List<SeatSchedule> { seatSch });

//            // **overload 2 tham số (fix lỗi)**  
//            _seatRepo.Setup(r => r.GetAllAsync(
//                                It.IsAny<Expression<Func<SeatSchedule, bool>>>(),
//                                It.IsAny<Func<IQueryable<SeatSchedule>, IIncludableQueryable<SeatSchedule, object>>>()))
//                     .ReturnsAsync(new List<SeatSchedule> { seatSch });

//            // overload 1 tham số (HoldSeatAsync)
//            _seatRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
//                     .ReturnsAsync(new List<SeatSchedule> { seatSch });

//            _seatRepo.Setup(r => r.UpdateAsync(It.IsAny<SeatSchedule>()))
//                     .Returns(Task.CompletedTask);

//            /* ---- SeatTypePriceRepo ---- */
//            _seatTypeRepo.Setup(r => r.GetAllAsync(null))
//                         .ReturnsAsync(new List<SeatTypePrice>{
//                         new SeatTypePrice { SeatType = SeatTypes.Standard, DefaultPrice = 100_000m } });

//            /* ---- Mapper ---- */
//            _mapper.Setup(m => m.Map<List<SeatScheduleResponse>>(It.IsAny<List<SeatSchedule>>()))
//                   .Returns(new List<SeatScheduleResponse>{
//                   new SeatScheduleResponse { Id = seatId, IsOwnedByCaller = true } });

//            _mapper.Setup(m => m.Map<OrderResponse>(It.IsAny<Order>()))
//                   .Returns(new OrderResponse { Id = Guid.NewGuid() });

//            /* ---- Repos thêm Order / Payment ---- */
//            _orderRepo.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
//            _paymentRepo.Setup(r => r.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);

//            /* ---- Transaction ---- */
//            var txMock = new Mock<IDbContextTransaction>();
//            txMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
//            txMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
//            _uow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(txMock.Object);

//            /* ---- SaveChangesAsync nhiều lần ---- */
//            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

//            /* ---------- Act ---------- */
//            var resp = await _sut.CreateTicketOrder(req);

//            /* ---------- Assert ---------- */
//            resp.StatusCode.Should().Be(HttpStatusCode.OK);
//            resp.IsSuccess.Should().BeTrue();
//            resp.Result.Should().NotBeNull();

//            _orderRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
//            _paymentRepo.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
//            _uow.Verify(u => u.SaveChangesAsync(), Times.AtLeast(2));
//            txMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
//        }

//        /* ═════ 2. SeatScheduleId rỗng → BadRequest ═════ */
//        [Fact]
//        public async Task CreateTicketOrder_EmptySeatIds_ShouldReturnBadRequest()
//        {
//            var req = new OrderRequest { SeatScheduleId = new List<Guid>() };

//            var resp = await _sut.CreateTicketOrder(req);

//            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
//            resp.IsSuccess.Should().BeFalse();
//            resp.Result.Should().Be("SeatScheduleId cannot be null or empty.");

//        }

//        /* ═════ 3. HoldSeatAsync thành công ═════ */
//        [Fact]
//        public async Task HoldSeatAsync_Should_ReturnSeatResponse_WhenSuccess()
//        {
//            var seatId = Guid.NewGuid();
//            var seat = new SeatSchedule { Id = seatId, Status = SeatBookingStatus.Available };

//            _seatRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
//                     .ReturnsAsync(new List<SeatSchedule> { seat });

//            _uow.Setup(u => u.BeginTransactionAsync())
//                .ReturnsAsync(Mock.Of<IDbContextTransaction>());

//            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

//            _mapper.Setup(m => m.Map<List<SeatScheduleResponse>>(It.IsAny<List<SeatSchedule>>()))
//                   .Returns(new List<SeatScheduleResponse>{
//                       new SeatScheduleResponse{ SeatId = seatId, Status = SeatBookingStatus.Hold }});

//            var result = await _sut.HoldSeatAsync(new List<Guid> { seatId }, Guid.NewGuid(), null);

//            result.Should().ContainSingle()
//                   .Which.Status.Should().Be(SeatBookingStatus.Hold);
//        }

//        /* ═════ 4. HoldSeatAsync – bị hold bởi user khác ═════ */
//        [Fact]
//        public async Task HoldSeatAsync_SeatHeldByOther_ShouldThrow()
//        {
//            var seatId = Guid.NewGuid();
//            var seat = new SeatSchedule
//            {
//                Id = seatId,
//                Status = SeatBookingStatus.Hold,
//                HoldByUserId = Guid.NewGuid(),
//                HoldUntil = DateTime.UtcNow.AddMinutes(3)
//            };

//            _seatRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
//                     .ReturnsAsync(new List<SeatSchedule> { seat });

//            _uow.Setup(u => u.BeginTransactionAsync())
//                .ReturnsAsync(Mock.Of<IDbContextTransaction>());

//            await Assert.ThrowsAsync<ApplicationException>(() =>
//                _sut.HoldSeatAsync(new List<Guid> { seatId }, Guid.NewGuid(), null));
//        }

//        /* ═════ 5. CancelTicketOrderById thành công ═════ */
//        [Fact]
//        public async Task CancelTicketOrder_Should_UpdateStatus_ToAvailable()
//        {
//            // Arrange
//            var seatId = Guid.NewGuid();
//            var orderId = Guid.NewGuid();

//            var seat = new SeatSchedule
//            {
//                Id = seatId,
//                Status = SeatBookingStatus.Hold
//            };

//            var order = new Order
//            {
//                Id = orderId,
//                Status = OrderEnum.Pending,  // giả định ban đầu
//                IsDeleted = false
//            };

//            _seatRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
//                     .ReturnsAsync(new List<SeatSchedule> { seat });

//            _seatRepo.Setup(r => r.UpdateAsync(It.IsAny<SeatSchedule>()))
//                     .Returns(Task.CompletedTask);

//            _orderRepo.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Order, bool>>>()))
//                      .ReturnsAsync(order);

//            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

//            // Act
//            var resp = await _sut.CancelTicketOrderById(new List<Guid> { seatId }, orderId);

//            // Assert
//            resp.StatusCode.Should().Be(HttpStatusCode.OK);
//            resp.IsSuccess.Should().BeTrue();

//            _seatRepo.Verify(r => r.UpdateAsync(It.Is<SeatSchedule>(s => s.Status == SeatBookingStatus.Available)),
//                             Times.Once);

//            _orderRepo.Verify(r => r.GetAsync(It.IsAny<Expression<Func<Order, bool>>>()), Times.Once);

//            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
//        }

//    }
//}
