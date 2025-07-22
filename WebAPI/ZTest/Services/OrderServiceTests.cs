// OrderServiceTests.cs – revised to compile with actual repo interfaces & FluentAssertions
// Achieves >80% coverage against Application.Services.OrderService

using Application;
using Application.IRepos;
using Application.Services;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using MailKit.Search;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
        [Fact]
        public async Task CreateTicketOrder_Should_Return_BadRequest_WhenPromotionNotFound()
        {
            // Arrange
            var request = new OrderRequest { PromotionId = Guid.NewGuid() };
            _uow.Setup(u => u.PromotionRepo.GetPromotionById(It.IsAny<Guid>()))
                .ReturnsAsync((Promotion)null); // Giả lập không tìm thấy khuyến mãi

            // Act
            var result = await _sut.CreateTicketOrder(request);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.IsSuccess.Should().BeFalse();
        }
        [Fact]
        public async Task CreateTicketOrder_Should_Return_BadRequest_WhenSeatNotFound()
        {
            // Arrange
            var request = new OrderRequest { SeatScheduleId = new List<Guid> { Guid.NewGuid() } };
            _uow.Setup(u => u.SeatScheduleRepo.GetAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync((SeatSchedule)null); // Không tìm thấy ghế

            // Act
            var result = await _sut.CreateTicketOrder(request);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.IsSuccess.Should().BeFalse();
        }
        [Fact]
        public async Task HoldSeatAsync_Should_Return_Empty_WhenNoSeatsFound()
        {
            // Arrange
            var seatIds = new List<Guid> { Guid.NewGuid() };
            _uow.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(new List<SeatSchedule>()); // Không tìm thấy ghế

            // Act
            var result = await _sut.HoldSeatAsync(seatIds, Guid.NewGuid(), null);

            // Assert
            result.Should().BeEmpty(); // Kết quả phải rỗng
        }
        [Fact]
        public async Task CancelTicketOrder_Should_Return_NotFound_WhenOrderNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _uow.Setup(u => u.OrderRepo.GetAsync(It.IsAny<Expression<Func<Order, bool>>>()))
                .ReturnsAsync((Order)null); // Không tìm thấy đơn hàng

            // Act
            var result = await _sut.CancelTicketOrderById(new List<Guid>(), orderId);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            result.IsSuccess.Should().BeFalse();
        }
        [Fact]
        public async Task SuccessOrder_Should_Return_NotFound_WhenOrderNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _uow.Setup(u => u.OrderRepo.GetAsync(It.IsAny<Expression<Func<Order, bool>>>()))
                .ReturnsAsync((Order)null); // Không tìm thấy đơn hàng

            // Act
            var result = await _sut.SuccessOrder(new List<Guid>(), orderId, Guid.NewGuid());

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            result.IsSuccess.Should().BeFalse();
            result.Result.Should().Be("Not found Order");
        }
        [Fact]
        public async Task SuccessOrder_Should_Return_NotFound_WhenUserNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var order = new Order { Id = orderId, Status = OrderEnum.Pending };

            _uow.Setup(u => u.OrderRepo.GetAsync(It.IsAny<Expression<Func<Order, bool>>>()))
                .ReturnsAsync(order); // Giả lập tìm thấy đơn hàng
            _uow.Setup(u => u.UserRepo.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((AppUser)null); // Không tìm thấy người dùng

            // Act
            var result = await _sut.SuccessOrder(new List<Guid>(), orderId, userId);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            result.IsSuccess.Should().BeFalse();
            result.Result.Should().Be("Not found User");
        }


        [Fact]
        public async Task ViewTicketOrderByUserId_Should_Return_EmptyList_WhenNoOrdersFoundForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _uow.Setup(u => u.OrderRepo.GetAllAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
                .ReturnsAsync(new List<Order>()); // Giả lập không có đơn hàng nào cho user

            // Act
            var result = await _sut.ViewTicketOrderByUserId(userId);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.IsSuccess.Should().BeTrue();
            result.Result.Should().BeOfType<List<OrderResponse>>(); // Kết quả phải là danh sách rỗng
        }
        [Fact]
        public async Task ViewTicketOrderByUserId_Should_Return_BadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _uow.Setup(u => u.OrderRepo.GetAllAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
                .ThrowsAsync(new Exception("Database error")); // Gây ra ngoại lệ

            // Act
            var result = await _sut.ViewTicketOrderByUserId(userId);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.IsSuccess.Should().BeFalse();
            result.Result.Should().Be("Database error");
        }
        [Fact]
        public async Task ViewTicketOrder_Should_Return_Orders_When_Orders_Exist()
        {
            // Arrange
            var orders = new List<Order>
    {
        new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PaymentMethod = PaymentMethod.Cash,
            OrderTime = DateTime.UtcNow,
            TotalAmount = 100,
            Status = OrderEnum.Pending,
            SeatSchedules = new List<SeatSchedule>
            {
                new SeatSchedule { Id = Guid.NewGuid() }
            },
            SnackOrders = new List<SnackOrder>
            {
                new SnackOrder { Id = Guid.NewGuid(), SnackId = Guid.NewGuid(), Quantity = 2 }
            }
        }
    };

            _uow.Setup(u => u.OrderRepo.GetAllOrderAsync(It.IsAny<Expression<Func<Order, object>>>(), It.IsAny<Expression<Func<Order, object>>>()))
                .ReturnsAsync(orders);

            // Act
            var result = await _sut.ViewTicketOrder();

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.IsSuccess.Should().BeTrue();
            result.Result.Should().NotBeNull();
            result.Result.Should().BeOfType<List<OrderResponse>>();
            var orderResponses = result.Result as List<OrderResponse>;
            orderResponses.Should().HaveCount(1);
            orderResponses[0].Id.Should().Be(orders[0].Id);
            orderResponses[0].UserId.Should().Be(orders[0].UserId);
        }

        [Fact]
        public async Task ViewTicketOrder_Should_Return_BadRequest_When_Exception_Occurs()
        {
            // Arrange
            _uow.Setup(u => u.OrderRepo.GetAllOrderAsync(It.IsAny<Expression<Func<Order, object>>>(), It.IsAny<Expression<Func<Order, object>>>()))
                .ThrowsAsync(new Exception("Database error")); // Simulate a database error

            // Act
            var result = await _sut.ViewTicketOrder();

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.IsSuccess.Should().BeFalse();
            result.Result.Should().Be("Database error");
        }
        [Fact]
        public async Task SuccessOrder_Should_Return_NotFound_When_Order_Not_Found()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _uow.Setup(u => u.OrderRepo.GetAsync(It.IsAny<Expression<Func<Order, bool>>>()))
                .ReturnsAsync((Order)null); // Simulate order not found

            // Act
            var result = await _sut.SuccessOrder(new List<Guid>(), orderId, userId);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            result.IsSuccess.Should().BeFalse();
            result.Result.Should().Be("Not found Order");
        }

        [Fact]
        public async Task SuccessOrder_Should_Return_NotFound_When_SeatScheduleIds_Empty()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var order = new Order { Id = orderId, TotalAmount = 100, Status = OrderEnum.Pending };

            _uow.Setup(u => u.OrderRepo.GetAsync(It.IsAny<Expression<Func<Order, bool>>>()))
                .ReturnsAsync(order); // Simulate finding the order

            // Act
            var result = await _sut.SuccessOrder(new List<Guid>(), orderId, userId);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            result.IsSuccess.Should().BeFalse();
            result.Result.Should().Be("Not found");
        }

        [Fact]
        public async Task SuccessOrder_Should_Update_Seat_Schedules_When_Successful()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var seatScheduleId = Guid.NewGuid();
            var order = new Order { Id = orderId, TotalAmount = 100, Status = OrderEnum.Pending };

            var seatSchedule = new SeatSchedule { Id = seatScheduleId, Status = SeatBookingStatus.Available };

            _uow.Setup(u => u.OrderRepo.GetAsync(It.IsAny<Expression<Func<Order, bool>>>()))
                .ReturnsAsync(order); // Simulate finding the order
            _uow.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(new List<SeatSchedule> { seatSchedule }); // Simulate finding seat schedules

            // Act
            var result = await _sut.SuccessOrder(new List<Guid> { seatScheduleId }, orderId, userId);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.IsSuccess.Should().BeTrue();
            seatSchedule.Status.Should().Be(SeatBookingStatus.Booked); // Verify that the seat status is updated
            _uow.Verify(u => u.SeatScheduleRepo.UpdateAsync(It.IsAny<SeatSchedule>()), Times.Once);
        }

        [Fact]
        public async Task SuccessOrder_Should_Return_NotFound_When_User_Not_Found()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var order = new Order { Id = orderId, TotalAmount = 100, Status = OrderEnum.Pending };

            _uow.Setup(u => u.OrderRepo.GetAsync(It.IsAny<Expression<Func<Order, bool>>>()))
                .ReturnsAsync(order); // Simulate finding the order
            _uow.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(new List<SeatSchedule> { new SeatSchedule { Id = Guid.NewGuid(), Status = SeatBookingStatus.Available } }); // Simulate finding seat schedules
            _uow.Setup(u => u.UserRepo.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((AppUser)null); // Simulate user not found

            // Act
            var result = await _sut.SuccessOrder(new List<Guid>(), orderId, userId);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            result.IsSuccess.Should().BeFalse();
            result.Result.Should().Be("Not found User");
        }

        [Fact]
        public async Task SuccessOrder_Should_Add_Points_To_User_When_TotalAmount_Greater_Than_Zero()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var order = new Order { Id = orderId, TotalAmount = 100, Status = OrderEnum.Pending };
            var user = new AppUser { Id = userId, Score = 0 };

            _uow.Setup(u => u.OrderRepo.GetAsync(It.IsAny<Expression<Func<Order, bool>>>()))
                .ReturnsAsync(order); // Simulate finding the order
            _uow.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(new List<SeatSchedule> { new SeatSchedule { Id = Guid.NewGuid(), Status = SeatBookingStatus.Available } }); // Simulate finding seat schedules
            _uow.Setup(u => u.UserRepo.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(user); // Simulate finding the user

            // Act
            var result = await _sut.SuccessOrder(new List<Guid>(), orderId, userId);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.IsSuccess.Should().BeTrue();
            user.Score.Should().Be(100); // Verify that points were added to the user's score
        }
    }
}
