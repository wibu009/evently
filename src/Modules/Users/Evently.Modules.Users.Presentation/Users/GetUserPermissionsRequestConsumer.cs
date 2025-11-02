using Evently.Common.Application.Authorization;
using Evently.Common.Domain;
using Evently.Modules.Users.IntegrationEvents.Users;
using MassTransit;

namespace Evently.Modules.Users.Presentation.Users;

public sealed class GetUserPermissionsRequestConsumer(IPermissionService permissionService)
    : IConsumer<GetUserPermissionsRequest>
{
    public async Task Consume(ConsumeContext<GetUserPermissionsRequest> context)
    {
        Result<PermissionsResponse> result =
            await permissionService.GetUserPermissionsAsync(context.Message.IdentityId);

        if (result.IsSuccess)
        {
            await context.RespondAsync(result.Value);
        }
        else
        {
            await context.RespondAsync(result.Error);
        }
    }
}
