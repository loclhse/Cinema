using Application.Services;
using Application.ViewModel.Response;
using Application.IServices;
using AutoMapper;
using Moq;
using Xunit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Common;
using Domain.Entities;
using Application.Domain;
using Application;

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
    }
}
