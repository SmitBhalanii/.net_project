using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RentKart.Core.Entities;
using RentKart.Infrastructure.Data;
using RentKart.Web.Controllers;
using System.Threading.Tasks;
using Xunit;

namespace RentKart.Tests.Controllers;

public class AccountControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<ILogger<AccountController>> _mockLogger;

    public AccountControllerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(_mockUserManager.Object, contextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);
        
        _mockLogger = new Mock<ILogger<AccountController>>();
    }

    [Fact]
    public void Login_ReturnsViewResult()
    {
        // Arrange
        var controller = new AccountController(_mockUserManager.Object, _mockSignInManager.Object, null!, _mockLogger.Object);

        // Act
        var result = controller.Login();

        // Assert
        Assert.IsType<ViewResult>(result);
    }
}
