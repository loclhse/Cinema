﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit;
using Application.Common;
using Application.IServices;
using Application.Services;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using Application.IRepos;
using static Dapper.SqlMapper;

namespace Application.Tests.Services
{
    public class CinemaRoomServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ICinemaRoomRepo> _roomRepo;
        private readonly Mock<IRoomLayoutRepo> _layoutRepo;
        private readonly Mock<ISeatRepo> _seatRepo;
        private readonly Mock<IDbContextTransaction> _tx;
        private readonly CinemaRoomService _sut;

        public CinemaRoomServiceTests()
        {
            _uow = new Mock<IUnitOfWork>();
            _mapper = new Mock<IMapper>();
            _roomRepo = new Mock<ICinemaRoomRepo>();
            _layoutRepo = new Mock<IRoomLayoutRepo>();
            _seatRepo = new Mock<ISeatRepo>();
            _tx = new Mock<IDbContextTransaction>();

            /* ---------- repo wiring ---------- */
            _uow.SetupGet(x => x.CinemaRoomRepo).Returns(_roomRepo.Object);
            _uow.SetupGet(x => x.RoomLayoutRepo).Returns(_layoutRepo.Object);
            _uow.SetupGet(x => x.SeatRepo).Returns(_seatRepo.Object);

            /* ---------- transaction ---------- */
            _uow.Setup(x => x.BeginTransactionAsync())
                .ReturnsAsync(_tx.Object);
            _tx.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);
            _tx.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            _sut = new CinemaRoomService(_uow.Object, _mapper.Object);
        }

        private static bool MatchExpr(Expression<Func<CinemaRoom, bool>> expr,
                              Guid id,
                              bool isDeleted) =>
    expr.Compile()(new CinemaRoom { Id = id, IsDeleted = isDeleted });


        /* =====================================================================
         * 1. CREATE ROOM
         * ===================================================================*/

        [Fact]
        public async Task CreateRoomAsync_NewName_ShouldAddAndReturnResponse()
        {
            // Arrange
            var dto = new CinemaRoomCreateRequest { Name = "RoomA", TotalRows = 5, TotalCols = 6 };
            var entity = new CinemaRoom { Name = "RoomA", TotalRows = 5, TotalCols = 6 };
            var response = new CinemaRoomResponse { Id = entity.Id, Name = "RoomA" };

            _mapper.Setup(m => m.Map<CinemaRoom>(dto))
                   .Returns(entity);

            _roomRepo.Setup(r =>
                    r.GetAsync(It.IsAny<Expression<Func<CinemaRoom, bool>>>()))
                     .ReturnsAsync((CinemaRoom?)null);

            _mapper.Setup(m => m.Map<CinemaRoomResponse>(entity))
                   .Returns(response);

            // Act
            var result = await _sut.CreateRoomAsync(dto);

            // Assert
            _roomRepo.Verify(r => r.AddAsync(entity), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task CreateRoomAsync_DuplicateName_ShouldThrow()
        {
            // Arrange
            var dto = new CinemaRoomCreateRequest { Name = "RoomA" };

            _mapper.Setup(m => m.Map<CinemaRoom>(dto))
                   .Returns(new CinemaRoom { Name = "RoomA" });

            _roomRepo.Setup(r =>
                    r.GetAsync(It.IsAny<Expression<Func<CinemaRoom, bool>>>()))
                     .ReturnsAsync(new CinemaRoom { Name = "RoomA" });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.CreateRoomAsync(dto));
            _roomRepo.Verify(r => r.AddAsync(It.IsAny<CinemaRoom>()), Times.Never);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        /* =====================================================================
         * 2. UPDATE ROOM
         * ===================================================================*/

        [Fact]
        public async Task UpdateRoomAsync_Exists_ShouldSaveAndReturnResponse()
        {
            var id = Guid.NewGuid();
            var dto = new CinemaRoomUpdateRequest { Name = "NewName" };
            var ent = new CinemaRoom { Id = id, Name = "Old", TotalRows = 5, TotalCols = 5, IsDeleted = false };
            var resp = new CinemaRoomResponse { Id = id, Name = "NewName", TotalRows = 5, TotalCols = 5 };

            _roomRepo.Setup(r => r.GetAsync(
        It.Is<Expression<Func<CinemaRoom,bool>>>(e => MatchExpr(e, id, It.IsAny<bool>()))))
         .ReturnsAsync(ent);


            _mapper.Setup(m => m.Map(dto, ent)).Verifiable();
            _mapper.Setup(m => m.Map<CinemaRoomResponse>(ent)).Returns(resp);

            var result = await _sut.UpdateRoomAsync(id, dto);

            _mapper.Verify();
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.Equal(resp.Id, result.Id);
            Assert.Equal(resp.Name, result.Name);
        }



        [Fact]
        public async Task UpdateRoomAsync_NotFound_ShouldThrowKeyNotFound()
        {
            // Arrange
            _roomRepo.Setup(r =>
                    r.GetAsync(It.IsAny<Expression<Func<CinemaRoom, bool>>>(),
                               It.IsAny<Func<IQueryable<CinemaRoom>,
                                             IIncludableQueryable<CinemaRoom, object>>>()))
                     .ReturnsAsync((CinemaRoom?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _sut.UpdateRoomAsync(Guid.NewGuid(), new CinemaRoomUpdateRequest()));

            _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        /* =====================================================================
         * 3. DELETE / RESTORE ROOM
         * ===================================================================*/

        [Fact]
        public async Task DeleteRoomAsync_Exists_ShouldSoftDelete()
        {
            var id = Guid.NewGuid();
            var ent = new CinemaRoom { Id = id, Name = "Room", TotalRows = 5, TotalCols = 5, IsDeleted = false };

            _roomRepo.Setup(r => r.GetAsync(
        It.Is<Expression<Func<CinemaRoom, bool>>>(e => MatchExpr(e, id, false))))
         .ReturnsAsync(ent);

            var result = await _sut.DeleteRoomAsync(id);

            Assert.True(result.Succeeded);
            Assert.True(ent.IsDeleted);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }


        [Fact]
        public async Task DeleteRoomAsync_NotFound_ShouldReturnFailed()
        {
            // Arrange
            _roomRepo.Setup(r =>
                    r.GetAsync(It.IsAny<Expression<Func<CinemaRoom, bool>>>(),
                               It.IsAny<Func<IQueryable<CinemaRoom>,
                                             IIncludableQueryable<CinemaRoom, object>>>()))
                     .ReturnsAsync((CinemaRoom?)null);

            // Act
            var result = await _sut.DeleteRoomAsync(Guid.NewGuid());

            // Assert
            Assert.False(result.Succeeded);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task RestoreRoomAsync_Exists_ShouldClearIsDeleted()
        {
            var id = Guid.NewGuid();
            var ent = new CinemaRoom { Id = id, Name = "Room", IsDeleted = true, TotalRows = 5, TotalCols = 5 };

            _roomRepo.Setup(r => r.GetAsync(
        It.Is<Expression<Func<CinemaRoom, bool>>>(e => MatchExpr(e, id, true))))
         .ReturnsAsync(ent);

            var result = await _sut.RestoreRoomAsync(id);

            Assert.True(result.Succeeded);
            Assert.False(ent.IsDeleted);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }


        /* =====================================================================
         * 4. READ OPERATIONS
         * ===================================================================*/

        [Fact]
        public async Task GetRoomByIdAsync_Found_ShouldReturnMapped()
        {
            var id = Guid.Parse("8627f0f9-b407-4335-b594-338bbd8c3f61");
            var ent = new CinemaRoom { Id = id, Name = "R1", TotalRows = 5, TotalCols = 5, IsDeleted = false };
            var resp = new CinemaRoomResponse { Id = id, Name = "R1", TotalRows = 5, TotalCols = 5 };

            _roomRepo.Setup(r => r.GetAsync(
        It.Is<Expression<Func<CinemaRoom, bool>>>(e => MatchExpr(e, id, false))))
         .ReturnsAsync(ent);

            _mapper.Setup(m => m.Map<CinemaRoomResponse>(ent)).Returns(resp);

            var result = await _sut.GetRoomByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(resp.Id, result.Id);
            Assert.Equal(resp.Name, result.Name);
            Assert.Equal(resp.TotalRows, result.TotalRows);
            Assert.Equal(resp.TotalCols, result.TotalCols);
        }

        [Fact]
        public async Task GetAllRoomsAsync_ShouldReturnMappedList()
        {
            // Arrange
            var list = new List<CinemaRoom>
            {
                new CinemaRoom { Id = Guid.NewGuid(), Name = "A" },
                new CinemaRoom { Id = Guid.NewGuid(), Name = "B" }
            };

            var mapped = new List<CinemaRoomResponse>
            {
                new CinemaRoomResponse { Id = list[0].Id, Name = "A" },
                new CinemaRoomResponse { Id = list[1].Id, Name = "B" }
            };

            _roomRepo.Setup(r =>
                    r.GetAllAsync(It.IsAny<Expression<Func<CinemaRoom, bool>>>(),
                                  It.IsAny<Func<IQueryable<CinemaRoom>,
                                                IIncludableQueryable<CinemaRoom, object>>>(),
                                  1, 10))
                     .ReturnsAsync(list);

            _mapper.Setup(m => m.Map<List<CinemaRoomResponse>>(list)).Returns(mapped);

            // Act
            var result = await _sut.GetAllRoomsAsync(1, 10);

            // Assert
            Assert.Equal(mapped, result);
        }

        /* =====================================================================
         * 5. WITH SEATS & MATRIX
         * ===================================================================*/

        [Fact]
        public async Task GetRoomWithSeatsAsync_Found_ShouldReturnComposite()
        {
            // Arrange
            var id = Guid.NewGuid();
            var seats = new List<Seat> { new Seat { Id = Guid.NewGuid(), RowIndex = 1, ColIndex = 1 } };
            var room = new CinemaRoom { Id = id, Name = "R", Seats = seats };
            var seatResp = new List<SeatResponse> { new SeatResponse() };

            _roomRepo.Setup(r =>
                    r.GetAsync(It.IsAny<Expression<Func<CinemaRoom, bool>>>(),
                               It.IsAny<Func<IQueryable<CinemaRoom>,
                                             IIncludableQueryable<CinemaRoom, object>>>()))
                     .ReturnsAsync(room);

            _mapper.Setup(m => m.Map<List<SeatResponse>>(seats)).Returns(seatResp);

            // Act
            var result = await _sut.GetRoomWithSeatsAsync(id);

            // Assert
            Assert.Equal(id, result.Id);
            Assert.Equal("R", result.Name);
            Assert.Equal(seatResp, result.Seats);
        }

        [Fact]
        public async Task GetSeatMatrixAsync_Found_ShouldBuildMatrix()
        {
            // Arrange
            var id = Guid.NewGuid();
            var seats = new List<Seat>
            {
                new Seat { RowIndex = 1, ColIndex = 2, SeatType = SeatTypes.Standard },
                new Seat { RowIndex = 1, ColIndex = 1, SeatType = SeatTypes.None }
            };
            var room = new CinemaRoom { Id = id, TotalRows = 1, TotalCols = 2, Seats = seats };
            var m0 = new SeatResponse();
            var m1 = new SeatResponse();

            _roomRepo.Setup(r =>
                    r.GetAsync(It.IsAny<Expression<Func<CinemaRoom, bool>>>(),
                               It.IsAny<Func<IQueryable<CinemaRoom>,
                                             IIncludableQueryable<CinemaRoom, object>>>()))
                     .ReturnsAsync(room);

            _mapper.Setup(m => m.Map<SeatResponse>(seats[1])).Returns(m1);
            _mapper.Setup(m => m.Map<SeatResponse>(seats[0])).Returns(m0);

            // Act
            var res = await _sut.GetSeatMatrixAsync(id);

            // Assert
            Assert.Equal(1, res.TotalRows);
            Assert.Equal(2, res.TotalCols);
            Assert.Collection(res.Seats2D.Single(),
                s => Assert.Same(m1, s),
                s => Assert.Same(m0, s));
        }

        /* =====================================================================
         * 6. LAYOUT & GENERATE SEATS
         * ===================================================================*/

        [Fact]
        public async Task UpdateLayoutJsonAsync_Exists_ShouldAddHistory()
        {
            // Arrange
            var id = Guid.NewGuid();
            var room = new CinemaRoom { Id = id };
            var json = JsonDocument.Parse("{\"foo\":123}");

            _roomRepo.Setup(r => r.GetAsync(
        It.Is<Expression<Func<CinemaRoom, bool>>>(e => MatchExpr(e, id, It.IsAny<bool>()))))
         .ReturnsAsync(room);


            // Act
            var result = await _sut.UpdateLayoutJsonAsync(id, json);

            // Assert
            Assert.True(result.Succeeded);
            _layoutRepo.Verify(l => l.AddAsync(It.Is<RoomLayout>(rl =>
                rl.CinemaRoomId == id &&
                rl.LayoutJson.RootElement.GetProperty("foo").GetInt32() == 123)), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GenerateSeatsFromLayoutAsync_InvalidJson_ShouldRollback()
        {
            // Arrange
            var id = Guid.NewGuid();
            var room = new CinemaRoom { Id = id, TotalRows = 2, TotalCols = 2 };

            _roomRepo.Setup(r =>
                    r.GetAsync(It.IsAny<Expression<Func<CinemaRoom, bool>>>(),
                               It.IsAny<Func<IQueryable<CinemaRoom>,
                                             IIncludableQueryable<CinemaRoom, object>>>()))
                     .ReturnsAsync(room);

            _seatRepo.Setup(s => s.GetAllAsync(It.IsAny<Expression<Func<Seat, bool>>>(),
                                               null, 1, 25))
                     .ReturnsAsync(new List<Seat>());

            var badJson = JsonDocument.Parse("{}"); // thiếu "layout"

            // Act
            var result = await _sut.GenerateSeatsFromLayoutAsync(id, badJson);

            // Assert
            Assert.False(result.Succeeded);
            _tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
    }
}
