namespace RepairTracker.Client.Services;

// Central place to derive online/offline state from real request outcomes, instead of every
// caller doing its own try/catch + report. Wraps the HttpClient used by ApiItemService/
// ApiSettingsService (see Program.cs) so every real API call updates ConnectivityService for free.
//
// A response with a status code - even an error one like a 404 or 500 - means the server was
// reached, so that's reported as online: HttpRequestException.StatusCode is only null when no
// response was received at all (DNS failure, connection refused, the browser's fetch() rejecting
// due to no network), which is the only case that actually means "offline".
public class ConnectivityReportingHandler(ConnectivityService connectivity) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            connectivity.ReportOnline();
            return response;
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode is null)
            {
                connectivity.ReportOffline();
            }
            else
            {
                connectivity.ReportOnline();
            }

            throw;
        }
    }
}
