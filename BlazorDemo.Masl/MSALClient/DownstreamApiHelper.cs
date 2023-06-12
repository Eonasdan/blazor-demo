namespace BlazorDemo.Masl.MSALClient
{
    public class DownstreamApiHelper
    {
        private string[]? _downstreamApiScopes;
        public readonly DownStreamApiConfig DownstreamApiConfig;
        private MSALClientHelper _msalClient;

        public DownstreamApiHelper(DownStreamApiConfig downstreamApiConfig, MSALClientHelper msalClientHelper)
        {
            DownstreamApiConfig = downstreamApiConfig;
            _msalClient = msalClientHelper ?? throw new ArgumentNullException(nameof(msalClientHelper));
            _downstreamApiScopes = DownstreamApiConfig.ScopesArray;
        }
    }
}
