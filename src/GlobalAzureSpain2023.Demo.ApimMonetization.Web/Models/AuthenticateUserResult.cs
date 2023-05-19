namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Models;

public class AuthenticateUserResult
{
    public bool Authenticated { get; init; }

    public string UserId { get; init; }

    public string Token { get; init; }
}