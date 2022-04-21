using Pulumi;

class HelloStack : Stack
{
    public HelloStack()
    {
        var webServer = new WebServerResource();

        var appInsights = new AppInsightsResource(new(webServer.ResourceGroup.Name)).AppInsights;

        _ = new UpTimeCheckResource(new(webServer.ResourceGroup.Name, appInsights.Id, webServer.WebServerIp));
    }
}
