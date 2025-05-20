using Evently.Common.Application.Authentication;
using Microsoft.AspNetCore.Http;

namespace Evently.Common.Infrastructure.Authentication;

internal sealed class CurrentActor(IHttpContextAccessor httpContextAccessor) : ICurrentActor
{
    public Guid Id => httpContextAccessor.HttpContext?.User.GetUserId() ?? Guid.Empty;
    public string IdentityId => httpContextAccessor.HttpContext?.User.GetIdentityId() ?? string.Empty;
}
