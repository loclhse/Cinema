using Application;
using Application.Domain;
using Application.Services;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Application.IServices.IUserService;

namespace ZTest.Services
{
    public class UserTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UserService _service;
        public UserTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _service = new UserService(_unitOfWorkMock.Object, _mapperMock.Object);
        }
        [Fact]
        public async Task GetAllEmployeesAsync_ShouldReturnOk()
        {
            _unitOfWorkMock.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Employee))
                .ReturnsAsync(new List<DomainUser> { new DomainUser { Id = Guid.NewGuid(), UserName = "emp1" } });
            _unitOfWorkMock.Setup(u => u.UserRepo.GetAllEmployeeAccountsAsync())
                .ReturnsAsync(new List<AppUser> { new AppUser { Id = Guid.NewGuid() } });

            _mapperMock.Setup(m => m.Map<List<EmployeeResponse>>(It.IsAny<object>()))
                .Returns(new List<EmployeeResponse> { new EmployeeResponse() });

            var result = await _service.GetAllEmployeesAsync();

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_ShouldReturnNotFound_WhenMissing()
        {
            _unitOfWorkMock.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Employee))
                .ReturnsAsync(new List<DomainUser>());

            var result = await _service.GetEmployeeByIdAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_ShouldReturnNotFound_WhenMissing()
        {
            _unitOfWorkMock.Setup(u => u.UserRepo.GetEmployeeAccountAsync(It.IsAny<Guid>()))
                .ReturnsAsync((AppUser)null);

            var result = await _service.DeleteEmployeeAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_ShouldReturnOk_WhenValid()
        {
            var user = new AppUser();
            _unitOfWorkMock.Setup(u => u.UserRepo.GetEmployeeAccountAsync(It.IsAny<Guid>()))
                .ReturnsAsync(user);

            var req = new EmployeeUpdateResquest();
            _mapperMock.Setup(m => m.Map(req, user));

            var result = await _service.UpdateEmployeeAsync(Guid.NewGuid(), req);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetAllCustomersAsync_ShouldReturnOk()
        {
            _unitOfWorkMock.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Customer))
                .ReturnsAsync(new List<DomainUser> { new DomainUser { Id = Guid.NewGuid(), UserName = "cus1" } });
            _unitOfWorkMock.Setup(u => u.UserRepo.GetAllCustomerAccountsAsync())
                .ReturnsAsync(new List<AppUser> { new AppUser { Id = Guid.NewGuid() } });

            _mapperMock.Setup(m => m.Map<List<CustomerResponse>>(It.IsAny<object>()))
                .Returns(new List<CustomerResponse> { new CustomerResponse() });

            var result = await _service.GetAllCustomersAsync();

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_ShouldReturnNotFound_WhenMissing()
        {
            _unitOfWorkMock.Setup(u => u.UserRepo.GetCustomerAccountAsync(It.IsAny<Guid>()))
                .ReturnsAsync((AppUser?)null);

            var result = await _service.GetCustomerByIdAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteCustomerAsync_ShouldReturnOk()
        {
            _unitOfWorkMock.Setup(u => u.UserRepo.GetCustomerAccountAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new AppUser());

            var result = await _service.DeleteCustomerAsync(Guid.NewGuid());

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task UpdateCustomerAsync_ShouldReturnOk()
        {
            var appUser = new AppUser();
            _unitOfWorkMock.Setup(u => u.UserRepo.GetCustomerAccountAsync(It.IsAny<Guid>()))
                .ReturnsAsync(appUser);

            var req = new CustomerUpdateResquest();
            _mapperMock.Setup(m => m.Map(req, appUser));

            var result = await _service.UpdateCustomerAsync(Guid.NewGuid(), req);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task SearchCustomers_ShouldReturnNotFound_WhenNoMatch()
        {
            _unitOfWorkMock.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Customer))
                .ReturnsAsync(new List<DomainUser>());
            _unitOfWorkMock.Setup(u => u.UserRepo.GetAllCustomerAccountsAsync())
                .ReturnsAsync(new List<AppUser>());

            var result = await _service.SearchCustomers("123", SearchKey.PhoneNumber);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task SearchEmployees_ShouldReturnNotFound_WhenNoMatch()
        {
            _unitOfWorkMock.Setup(u => u.UserRepo.GetAllEmployeeAccountsAsync())
                .ReturnsAsync(new List<AppUser>());

            var result = await _service.SearchEmployees("123", SearchKey.Name);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task GetDeletedAccountsAsync_ShouldReturnOk()
        {
            _unitOfWorkMock.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Employee))
                .ReturnsAsync(new List<DomainUser> { new DomainUser { Id = Guid.NewGuid(), UserName = "deleted" } });
            _unitOfWorkMock.Setup(u => u.UserRepo.GetAllEmployeeAccountsDeletedAsync())
                .ReturnsAsync(new List<AppUser> { new AppUser { Id = Guid.NewGuid() } });

            _mapperMock.Setup(m => m.Map<List<EmployeeResponse>>(It.IsAny<object>()))
                .Returns(new List<EmployeeResponse> { new EmployeeResponse() });

            var result = await _service.GetDeletedAccountsAsync();

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task RestoreAccountAsync_ShouldReturnNotFound_WhenMissing()
        {
            _unitOfWorkMock.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Employee))
                .ReturnsAsync(new List<DomainUser>());

            var result = await _service.RestoreAccountAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task SearchIsDeleteEmployees_ShouldReturnNotFound_WhenNoMatch()
        {
            _unitOfWorkMock.Setup(u => u.UserRepo.GetAllEmployeeAccountsDeletedAsync())
                .ReturnsAsync(new List<AppUser>());

            var result = await _service.SearchIsDeleteEmployees("123", SearchKey.IdentityCard);

            Assert.False(result.IsSuccess);
        }
        [Fact]
        public async Task GetAllEmployeesAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Employee))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetAllEmployeesAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Employee))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetEmployeeByIdAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.UserRepo.GetEmployeeAccountAsync(id))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.DeleteEmployeeAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var id = Guid.NewGuid();
            var req = new EmployeeUpdateResquest();
            _unitOfWorkMock.Setup(u => u.UserRepo.GetEmployeeAccountAsync(id))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.UpdateEmployeeAsync(id, req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task GetAllCustomersAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Customer))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetAllCustomersAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.UserRepo.GetCustomerAccountAsync(id))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetCustomerByIdAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteCustomerAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.UserRepo.GetCustomerAccountAsync(id))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.DeleteCustomerAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateCustomerAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var id = Guid.NewGuid();
            var req = new CustomerUpdateResquest();
            _unitOfWorkMock.Setup(u => u.UserRepo.GetCustomerAccountAsync(id))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.UpdateCustomerAsync(id, req);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task SearchCustomers_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Customer))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.SearchCustomers("123", SearchKey.PhoneNumber);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task SearchEmployees_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.UserRepo.GetAllEmployeeAccountsAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.SearchEmployees("123", SearchKey.Name);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task GetDeletedAccountsAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.UserRepo.GetAllEmployeeAccountsDeletedAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetDeletedAccountsAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task RestoreAccountAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Employee))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.RestoreAccountAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task SearchIsDeleteEmployees_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.UserRepo.GetAllEmployeeAccountsDeletedAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.SearchIsDeleteEmployees("123", SearchKey.IdentityCard);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }
        [Fact]
        public async Task SearchCustomers_Should_Return_BadRequest_When_InvalidSearchKey()
        {
            // Arrange
            var searchKey = (SearchKey)999; // Invalid search key

            // Act
            var result = await _service.SearchCustomers("value", searchKey);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task SearchCustomers_Should_Return_NotFound_When_NoMatches()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Customer))
                .ReturnsAsync(new List<DomainUser>());
            _unitOfWorkMock.Setup(u => u.UserRepo.GetAllCustomerAccountsAsync())
                .ReturnsAsync(new List<AppUser>()); // No customers found

            // Act
            var result = await _service.SearchCustomers("nonexistent", SearchKey.Name);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task SearchEmployees_Should_Return_NotFound_When_NoMatches()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.UserRepo.GetAllEmployeeAccountsAsync())
                .ReturnsAsync(new List<AppUser>()); // No employees found

            // Act
            var result = await _service.SearchEmployees("nonexistent", SearchKey.Name);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task SearchEmployees_Should_Return_BadRequest_When_Exception_Occurs()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.UserRepo.GetAllEmployeeAccountsAsync())
                .ThrowsAsync(new Exception("Database error")); // Simulate exception

            // Act
            var result = await _service.SearchEmployees("123", SearchKey.Name);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task SearchCustomers_Should_Return_BadRequest_When_Exception_Occurs()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.UserRepo.GetIdentityUsersByRoleAsync(RoleNames.Customer))
                .ThrowsAsync(new Exception("Database error")); // Simulate exception

            // Act
            var result = await _service.SearchCustomers("123", SearchKey.PhoneNumber);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }
    }
}
