using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;

namespace fakebook.Services.v1;

public static class ServiceHelper
{
    public static int? GetCurrentUserClaim(string authorization, string claimKey)
    {
        if (AuthenticationHeaderValue.TryParse(authorization, out var token))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadToken(token.Parameter) as JwtSecurityToken;
            var claimsValue = jwt.Claims
                .Where(c => c.Type == claimKey)
                .Select(c => c.Value).First();
            if (int.TryParse(claimsValue, out int UserId)) return UserId;
        }
        return null;
    }
}
