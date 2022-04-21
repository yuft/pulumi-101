using Pulumi;
using Pulumi.AzureNative.Insights;

public record AppInsightsArgs(Input<string> ResourceGroupName);

public class AppInsightsResource : HelloResourceBase
{
    public AppInsightsResource(AppInsightsArgs args)
    {
        AppInsights = new Component($"{StackName}-app-insights",
            new ComponentArgs
            {
                ApplicationType = ApplicationType.Web,
                FlowType = FlowType.Bluefield,
                IngestionMode = IngestionMode.ApplicationInsights,
                Kind = "web",
                RequestSource = RequestSource.Rest,
                ResourceGroupName = args.ResourceGroupName,
                ResourceName = $"{StackName}-app-insights",
                Tags = GlobalTags
            });
    }

    public Component AppInsights { get; }
}