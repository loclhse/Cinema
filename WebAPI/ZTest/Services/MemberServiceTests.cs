using Application;
using Application.Common;
using Application.Domain;
using Application.IServices;
using Application.Services;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static Application.IServices.IUserService;

namespace ZTest.Services
{
    public class MemberServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IMapper> _mapper;
        private readonly MemberService _sut;

        public MemberServiceTests()
        {
            _uow = new Mock<IUnitOfWork>();
            _mapper = new Mock<IMapper>();
            _sut = new MemberService(_uow.Object, _mapper.Object);
        }

        /* Test GetAllMember */
        [Fact]
        public async Task GetAllMember_ShouldReturnMembers()
        {
            // Arrange
            var userIds = new List<DomainUser> { new DomainUser { Id = Guid.NewGuid() } };
            var profiles = new List<AppUser> { new AppUser { Id = Guid.NewGuid(), FullName = "John Doe" } };
            var expectedResponse = new List<CustomerResponse> { new CustomerResponse { FullName = "John Doe" } };

            _uow.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Member)).ReturnsAsync(userIds);
            _uow.Setup(u => u.UserRepo.GetAllMemberAccountsAsync()).ReturnsAsync(profiles);
            _mapper.Setup(m => m.Map<List<CustomerResponse>>(It.IsAny<IEnumerable<IdentityWithProfile>>())).Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllMember();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedResponse, result.Result);
        }
        [Fact]
        public async Task DeleteMemberAsync_Should_ReturnNotFound_If_Member_Does_Not_Exist()
        {
            _uow.Setup(u => u.UserRepo.GetMemberAccountAsync(It.IsAny<Guid>()))
                           .ReturnsAsync((AppUser)null);

            var result = await _sut.DeleteMemberAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteMemberAsync_Should_SoftDelete_If_Exists()
        {
            var member = new AppUser { IsDeleted = false };
            _uow.Setup(u => u.UserRepo.GetMemberAccountAsync(It.IsAny<Guid>()))
                           .ReturnsAsync(member);

            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _sut.DeleteMemberAsync(Guid.NewGuid());

            Assert.True(result.IsSuccess);
            Assert.Equal("Member deleted.", result.Result);
            Assert.True(member.IsDeleted);
        }

        [Fact]
        public async Task UpdateMemberAsync_Should_ReturnNotFound_If_NotFound()
        {
            _uow.Setup(u => u.UserRepo.GetMemberAccountAsync(It.IsAny<Guid>()))
                           .ReturnsAsync((AppUser)null);

            var result = await _sut.UpdateMemberAsync(Guid.NewGuid(), new CustomerUpdateResquest());

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateMemberAsync_Should_Map_And_Save()
        {
            var member = new AppUser();
            var updateReq = new CustomerUpdateResquest { FullName = "Test" };

            _uow.Setup(u => u.UserRepo.GetMemberAccountAsync(It.IsAny<Guid>()))
                .ReturnsAsync(member);

            _mapper.Setup(m => m.Map(updateReq, member)); 

            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _sut.UpdateMemberAsync(Guid.NewGuid(), updateReq);

            Assert.True(result.IsSuccess);
            Assert.Equal("Member updated.", result.Result);
        } 

        [Fact]
        public async Task SearchMembers_Should_ReturnNotFound_If_NoMatch()
        {
            _uow.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(It.IsAny<string>()))
                           .ReturnsAsync(new List<DomainUser>());

            _uow.Setup(u => u.UserRepo.GetAllMemberAccountsAsync())
                           .ReturnsAsync(new List<AppUser>());

            var result = await _sut.SearchMembers("123", SearchKey.IdentityCard);

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task SearchMembers_Should_ReturnResults_When_Match()
        {
            var members = new List<AppUser>
        {
            new AppUser { IdentityCard = "123", FullName = "John Doe" }
        };
            var ids = new List<DomainUser>
        {
            new DomainUser { Id = Guid.NewGuid(), Email = "john@example.com" }
        };
            var mapped = new List<CustomerResponse> { new CustomerResponse { FullName = "John Doe" } };

            _uow.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(It.IsAny<string>()))
                           .ReturnsAsync(ids);
            _uow.Setup(u => u.UserRepo.GetAllMemberAccountsAsync())
                           .ReturnsAsync(members);
            _mapper.Setup(m => m.Map<List<CustomerResponse>>(It.IsAny<IEnumerable<object>>()))
                       .Returns(mapped);

            var result = await _sut.SearchMembers("123", SearchKey.IdentityCard);

            Assert.True(result.IsSuccess);
            var responseList = Assert.IsAssignableFrom<List<CustomerResponse>>(result.Result);
            Assert.Single(responseList);
            Assert.Equal("John Doe", responseList.First().FullName);
        }
    }
}
