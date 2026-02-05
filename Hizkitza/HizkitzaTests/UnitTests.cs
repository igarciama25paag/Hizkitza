using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace HizkitzaTests
{
    // Interfaz de autenticación para desacoplar la lógica de Login en los tests
    public interface IAuthService
    {
        Task<bool> ValidateUserAsync(string username, string password);
        Task<bool> IsAdminAsync(string username);
    }

    // ViewModel sencillo (sin code-behind) que encapsula la lógica de login
    public class LoginViewModel
    {
        private readonly IAuthService _authService;

        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool IsAuthenticated { get; private set; }
        public bool IsAdmin { get; private set; }
        public string? ErrorMessage { get; private set; }

        public LoginViewModel(IAuthService authService) => _authService = authService;

        public async Task<bool> LoginAsync()
        {
            // Entrada vacía
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Erabiltzaile edo pasahitz hutsik";
                IsAuthenticated = false;
                return false;
            }

            try
            {
                var valid = await _authService.ValidateUserAsync(Username!, Password!);
                if (!valid)
                {
                    ErrorMessage = "Erabiltzaile edo pasahitz ezegokia";
                    IsAuthenticated = false;
                    return false;
                }

                IsAdmin = await _authService.IsAdminAsync(Username!);
                IsAuthenticated = true;
                ErrorMessage = null;
                return true;
            }
            catch (InvalidOperationException)
            {
                ErrorMessage = "Konexio errorea";
                IsAuthenticated = false;
                return false;
            }
        }
    }

    public class LoginUnitTests
    {
        [Fact]
        public async Task EmptyUsernameOrPassword_ReturnsFalseAndErrorMessage()
        {
            var mock = new Mock<IAuthService>(MockBehavior.Strict);
            var vm = new LoginViewModel(mock.Object)
            {
                Username = "",
                Password = "whatever"
            };

            var result = await vm.LoginAsync();

            Assert.False(result);
            Assert.False(vm.IsAuthenticated);
            Assert.Equal("Erabiltzaile edo pasahitz hutsik", vm.ErrorMessage);
            mock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task WrongCredentials_ValidateReturnsFalse_ReturnsFalseAndErrorMessage()
        {
            var mock = new Mock<IAuthService>();
            mock.Setup(s => s.ValidateUserAsync("user", "bad")).ReturnsAsync(false);

            var vm = new LoginViewModel(mock.Object)
            {
                Username = "user",
                Password = "bad"
            };

            var result = await vm.LoginAsync();

            Assert.False(result);
            Assert.False(vm.IsAuthenticated);
            Assert.Equal("Erabiltzaile edo pasahitz ezegokia", vm.ErrorMessage);
            mock.Verify(s => s.ValidateUserAsync("user", "bad"), Times.Once);
            mock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task NormalUserLogin_ValidateTrue_IsAdminFalse_ReturnsTrueAndIsAdminFalse()
        {
            var mock = new Mock<IAuthService>();
            mock.Setup(s => s.ValidateUserAsync("user", "1234")).ReturnsAsync(true);
            mock.Setup(s => s.IsAdminAsync("user")).ReturnsAsync(false);

            var vm = new LoginViewModel(mock.Object)
            {
                Username = "user",
                Password = "1234"
            };

            var result = await vm.LoginAsync();

            Assert.True(result);
            Assert.True(vm.IsAuthenticated);
            Assert.False(vm.IsAdmin);
            Assert.Null(vm.ErrorMessage);
            mock.Verify(s => s.ValidateUserAsync("user", "1234"), Times.Once);
            mock.Verify(s => s.IsAdminAsync("user"), Times.Once);
        }

        [Fact]
        public async Task AdminLogin_ValidateTrue_IsAdminTrue_ReturnsTrueAndIsAdminTrue()
        {
            var mock = new Mock<IAuthService>();
            mock.Setup(s => s.ValidateUserAsync("admin", "admin1234")).ReturnsAsync(true);
            mock.Setup(s => s.IsAdminAsync("admin")).ReturnsAsync(true);

            var vm = new LoginViewModel(mock.Object)
            {
                Username = "admin",
                Password = "admin1234"
            };

            var result = await vm.LoginAsync();

            Assert.True(result);
            Assert.True(vm.IsAuthenticated);
            Assert.True(vm.IsAdmin);
            Assert.Null(vm.ErrorMessage);
            mock.Verify(s => s.ValidateUserAsync("admin", "admin1234"), Times.Once);
            mock.Verify(s => s.IsAdminAsync("admin"), Times.Once);
        }

        [Fact]
        public async Task ValidateThrowsInvalidOperationException_ReturnsFalseAndConnectionErrorMessage()
        {
            var mock = new Mock<IAuthService>();
            mock.Setup(s => s.ValidateUserAsync("any", "any")).ThrowsAsync(new InvalidOperationException("network"));

            var vm = new LoginViewModel(mock.Object)
            {
                Username = "any",
                Password = "any"
            };

            var result = await vm.LoginAsync();

            Assert.False(result);
            Assert.False(vm.IsAuthenticated);
            Assert.Equal("Konexio errorea", vm.ErrorMessage);
            mock.Verify(s => s.ValidateUserAsync("any", "any"), Times.Once);
        }

        // Optional: simple integration test template (runs against real DB). Marked so it can be filtered.
        [Fact, Trait("Type", "Integration")]
        public async Task Integration_ValidateAgainstDatabase_Template()
        {
            // This test is a template. To enable it:
            // - Prepare a test database with seed users (admin/admin1234 (admin), user/1234 (user))
            // - Ensure connection string is reachable from the test environment
            // - Replace the body below with a concrete IAuthService implementation that queries the test DB
            // - Use transactions or clean-up (rollback/truncate) so tests don't pollute DB

            // Example: assert template only
            await Task.CompletedTask;
            Assert.True(true);
        }
    }
}