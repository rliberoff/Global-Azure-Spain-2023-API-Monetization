namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Extensions;

internal static class HttpRequestExtensions
{
    public static string BaseUrl(this HttpRequest httpRequest)
    {
        if (httpRequest == null)
        {
            return null;
        }

        var uriBuilder = new UriBuilder(httpRequest.Scheme, httpRequest.Host.Host, httpRequest.Host.Port ?? -1);

        if (uriBuilder.Uri.IsDefaultPort)
        {
            uriBuilder.Port = -1;
        }

        return uriBuilder.Uri.AbsoluteUri;
    }
}
