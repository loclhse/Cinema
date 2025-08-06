using Application;
using Application.Common;
using Application.Domain;
using Application.IRepos;
using Application.IServices;
using Application.Services;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using static Application.IServices.IUserService;

namespace ZTest.Services
{
    public class MemberServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IAuthRepo> _authRepoMock;
        private readonly MemberService _sut;

        public MemberServiceTests()
        {
            _uow = new Mock<IUnitOfWork>();
            _mapper = new Mock<IMapper>();
            _authRepoMock = new Mock<IAuthRepo>();
            _sut = new MemberService(_uow.Object, _mapper.Object, _authRepoMock.Object);
        }

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
        public async Task GetAllMember_ShouldReturnNotFound_WhenNoMembersExist()
        {
            // Arrange
            var userIds = new List<DomainUser>();
            var profiles = new List<AppUser>();

            _uow.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Member)).ReturnsAsync(userIds);
            _uow.Setup(u => u.UserRepo.GetAllMemberAccountsAsync()).ReturnsAsync(profiles);
            _mapper.Setup(m => m.Map<List<CustomerResponse>>(It.IsAny<IEnumerable<IdentityWithProfile>>())).Returns(new List<CustomerResponse>());

            // Act
            var result = await _sut.GetAllMember();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Not found any Member", result.ErrorMessage);
        }

        [Fact]
        public async Task GetAllMember_ShouldReturnBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _uow.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Member))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.GetAllMember();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Database error", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteMemberAsync_Should_ReturnNotFound_If_Member_Does_Not_Exist()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            _uow.Setup(u => u.UserRepo.GetMemberAccountAsync(memberId))
                .ReturnsAsync((AppUser)null);

            // Act
            var result = await _sut.DeleteMemberAsync(memberId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Mebers not found", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteMemberAsync_Should_SoftDelete_If_Exists()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var member = new AppUser { Id = memberId, IsDeleted = false };
            
            _uow.Setup(u => u.UserRepo.GetMemberAccountAsync(memberId))
                .ReturnsAsync(member);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _sut.DeleteMemberAsync(memberId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Contains("Member deleted", result.Result.ToString());
            Assert.True(member.IsDeleted);
        }

        [Fact]
        public async Task DeleteMemberAsync_Should_ReturnBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            _uow.Setup(u => u.UserRepo.GetMemberAccountAsync(memberId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.DeleteMemberAsync(memberId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Database error", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateMemberAsync_Should_ReturnNotFound_If_NotFound()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var updateRequest = new CustomerUpdateResquest { FullName = "Updated Name" };

            _uow.Setup(u => u.UserRepo.GetMemberAccountAsync(memberId))
                .ReturnsAsync((AppUser)null);

            // Act
            var result = await _sut.UpdateMemberAsync(memberId, updateRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Member not found", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateMemberAsync_Should_Map_And_Save()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var member = new AppUser { Id = memberId, FullName = "Original Name" };
            var updateRequest = new CustomerUpdateResquest { FullName = "Updated Name" };

            _uow.Setup(u => u.UserRepo.GetMemberAccountAsync(memberId))
                .ReturnsAsync(member);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _sut.UpdateMemberAsync(memberId, updateRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Contains("Member updated", result.Result.ToString());
        }

        [Fact]
        public async Task UpdateMemberAsync_Should_ReturnBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var updateRequest = new CustomerUpdateResquest { FullName = "Updated Name" };

            _uow.Setup(u => u.UserRepo.GetMemberAccountAsync(memberId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.UpdateMemberAsync(memberId, updateRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Database error", result.ErrorMessage);
        }

        [Fact]
        public async Task SearchMembers_Should_ReturnNotFound_If_NoMatch()
        {
            // Arrange
            var searchValue = "nonexistent";
            var searchKey = SearchKey.Name;
            var userIds = new List<DomainUser> { new DomainUser { Id = Guid.NewGuid() } };
            var members = new List<AppUser> { new AppUser { Id = Guid.NewGuid(), FullName = "John Doe" } };

            _uow.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Member)).ReturnsAsync(userIds);
            _uow.Setup(u => u.UserRepo.GetAllMemberAccountsAsync()).ReturnsAsync(members);

            // Act
            var result = await _sut.SearchMembers(searchValue, searchKey);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("No members found", result.ErrorMessage);
        }

        [Fact]
        public async Task SearchMembers_Should_ReturnResults_When_Match()
        {
            // Arrange
            var searchValue = "john";
            var searchKey = SearchKey.Name;
            var userId = Guid.NewGuid();
            var userIds = new List<DomainUser> { new DomainUser { Id = userId } };
            var members = new List<AppUser> { new AppUser { Id = userId, FullName = "John Doe" } };
            var expectedResponse = new List<CustomerResponse> { new CustomerResponse { FullName = "John Doe" } };

            _uow.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Member)).ReturnsAsync(userIds);
            _uow.Setup(u => u.UserRepo.GetAllMemberAccountsAsync()).ReturnsAsync(members);
            _mapper.Setup(m => m.Map<List<CustomerResponse>>(It.IsAny<IEnumerable<IdentityWithProfile>>())).Returns(expectedResponse);

            // Act
            var result = await _sut.SearchMembers(searchValue, searchKey);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedResponse, result.Result);
        }

        [Fact]
        public async Task SearchMembers_Should_ReturnResults_When_Match_Phone()
        {
            // Arrange
            var searchValue = "123";
            var searchKey = SearchKey.PhoneNumber;
            var userId = Guid.NewGuid();
            var userIds = new List<DomainUser> { new DomainUser { Id = userId } };
            var members = new List<AppUser> { new AppUser { Id = userId, Phone = "123456789" } };
            var expectedResponse = new List<CustomerResponse> { new CustomerResponse { Phone = "123456789" } };

            _uow.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Member)).ReturnsAsync(userIds);
            _uow.Setup(u => u.UserRepo.GetAllMemberAccountsAsync()).ReturnsAsync(members);
            _mapper.Setup(m => m.Map<List<CustomerResponse>>(It.IsAny<IEnumerable<IdentityWithProfile>>())).Returns(expectedResponse);

            // Act
            var result = await _sut.SearchMembers(searchValue, searchKey);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedResponse, result.Result);
        }

        [Fact]
        public async Task SearchMembers_Should_ReturnResults_When_Match_Name()
        {
            // Arrange
            var searchValue = "doe";
            var searchKey = SearchKey.Name;
            var userId = Guid.NewGuid();
            var userIds = new List<DomainUser> { new DomainUser { Id = userId } };
            var members = new List<AppUser> { new AppUser { Id = userId, FullName = "John Doe" } };
            var expectedResponse = new List<CustomerResponse> { new CustomerResponse { FullName = "John Doe" } };

            _uow.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Member)).ReturnsAsync(userIds);
            _uow.Setup(u => u.UserRepo.GetAllMemberAccountsAsync()).ReturnsAsync(members);
            _mapper.Setup(m => m.Map<List<CustomerResponse>>(It.IsAny<IEnumerable<IdentityWithProfile>>())).Returns(expectedResponse);

            // Act
            var result = await _sut.SearchMembers(searchValue, searchKey);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedResponse, result.Result);
        }

        [Fact]
        public async Task SearchMembers_Should_ReturnResults_When_Match_IdentityCard()
        {
            // Arrange
            var searchValue = "123456";
            var searchKey = SearchKey.IdentityCard;
            var userId = Guid.NewGuid();
            var userIds = new List<DomainUser> { new DomainUser { Id = userId } };
            var members = new List<AppUser> { new AppUser { Id = userId, IdentityCard = "123456789" } };
            var expectedResponse = new List<CustomerResponse> { new CustomerResponse { IdentityCard = "123456789" } };

            _uow.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Member)).ReturnsAsync(userIds);
            _uow.Setup(u => u.UserRepo.GetAllMemberAccountsAsync()).ReturnsAsync(members);
            _mapper.Setup(m => m.Map<List<CustomerResponse>>(It.IsAny<IEnumerable<IdentityWithProfile>>())).Returns(expectedResponse);

            // Act
            var result = await _sut.SearchMembers(searchValue, searchKey);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedResponse, result.Result);
        }

        [Fact]
        public async Task SearchMembers_Should_Return_BadRequest_When_InvalidSearchKey()
        {
            // Arrange
            var searchValue = "test";
            var searchKey = (SearchKey)999; // Invalid search key
            var userIds = new List<DomainUser> { new DomainUser { Id = Guid.NewGuid() } };
            var members = new List<AppUser> { new AppUser { Id = Guid.NewGuid() } };

            _uow.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Member)).ReturnsAsync(userIds);
            _uow.Setup(u => u.UserRepo.GetAllMemberAccountsAsync()).ReturnsAsync(members);

            // Act
            var result = await _sut.SearchMembers(searchValue, searchKey);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Invalid search key", result.ErrorMessage);
        }

        [Fact]
        public async Task SearchMembers_Should_Return_BadRequest_When_Exception_Occurs()
        {
            // Arrange
            var searchValue = "test";
            var searchKey = SearchKey.Name;

            _uow.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Member))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.SearchMembers(searchValue, searchKey);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Database error", result.ErrorMessage);
        }

        [Fact]
        public async Task CustomerToMember_Should_ReturnNotFound_When_Customer_Not_Found()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _uow.Setup(u => u.UserRepo.GetCustomerAccountAsync(customerId))
                .ReturnsAsync((AppUser)null);

            // Act
            var result = await _sut.CustomerToMember(customerId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Customer not found", result.ErrorMessage);
        }

        [Fact]
        public async Task CustomerToMember_Should_Convert_Customer_To_Member()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new AppUser { Id = customerId, FullName = "Test Customer" };

            _uow.Setup(u => u.UserRepo.GetCustomerAccountAsync(customerId))
                .ReturnsAsync(customer);
            _authRepoMock.Setup(a => a.RemoveUserFromRoleAsync(customerId, "Customer"))
                .ReturnsAsync(OperationResult.Success(new[] { "Role removed successfully" }));
            _authRepoMock.Setup(a => a.AddUserToRoleAsync(customerId, "Member"))
                .ReturnsAsync(OperationResult.Success(new[] { "Role added successfully" }));
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _sut.CustomerToMember(customerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Contains("Customer converted to Member successfully", result.Result.ToString());
        }

        [Fact]
        public async Task CustomerToMember_Should_ReturnBadRequest_When_Exception_Occurs()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _uow.Setup(u => u.UserRepo.GetCustomerAccountAsync(customerId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.CustomerToMember(customerId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Database error", result.ErrorMessage);
        }

        [Fact]
        public async Task CustomerToMember_Should_ReturnBadRequest_When_Role_Assignment_Fails()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new AppUser { Id = customerId, FullName = "Test Customer" };

            _uow.Setup(u => u.UserRepo.GetCustomerAccountAsync(customerId))
                .ReturnsAsync(customer);
            _authRepoMock.Setup(a => a.RemoveUserFromRoleAsync(customerId, "Customer"))
                .ReturnsAsync(OperationResult.Success(new[] { "Role removed successfully" }));
            _authRepoMock.Setup(a => a.AddUserToRoleAsync(customerId, "Member"))
                .ReturnsAsync(OperationResult.Failed(new[] { "Failed to assign role" }));

            // Act
            var result = await _sut.CustomerToMember(customerId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Failed to assign role", result.ErrorMessage);
        }
    }
}
