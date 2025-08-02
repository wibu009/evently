using Evently.Common.Application.Authorization;
using Evently.Common.Domain;
using Evently.Modules.Users.Application.Users.GetUserPermissions;
using Evently.Modules.Users.Application.Users.RegisterUser;
using Evently.Modules.Users.Domain.Users;
using Evently.Modules.Users.IntegrationTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Users.IntegrationTests.Users;

public class GetUserPermissionTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnError_WhenUserDoesNotExist()
    {
        // Arrange
        string identityId = Guid.CreateVersion7().ToString();
        
        // Act
        Result<PermissionsResponse> userResult = await Sender.Send(new GetUserPermissionsQuery(identityId));
        
        // Assert
        userResult.Error.Should().Be(UserErrors.NotFound(identityId));
    }
    
    [Fact]
    public async Task Should_ReturnPermissions_WhenUserExists()
    {
        // Arrange
        Result<Guid> userResult = await Sender.Send(new RegisterUserCommand(
            Faker.Internet.Email(),
            Faker.Internet.Password(),
            Faker.Name.FirstName(),
            Faker.Name.LastName()));
        string identityId = DbContext.Users.Single(u => u.Id == userResult.Value).IdentityId;
        
        // Act
        Result<PermissionsResponse> permissionResult = await Sender.Send(new GetUserPermissionsQuery(identityId));
        
        // Assert
        permissionResult.IsSuccess.Should().BeTrue();
        permissionResult.Value.Permissions.Should().NotBeNull();
    }
}
