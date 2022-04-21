using Pulumi;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.Insights.Inputs;
using Pulumi.Random;

public record UpTimeCheckResourceArgs(Input<string> ResourceGroupName, Input<string> AppInsightsId, Output<string> HostIp);

public class UpTimeCheckResource : HelloResourceBase
{
    public UpTimeCheckResource(UpTimeCheckResourceArgs args)
    {
        var webTestId = new RandomUuid($"{StackName}-up-time-check").Result;
        var healthCheckUrl = Output.Format($"http://{args.HostIp}");

        _ = args.AppInsightsId.Apply(id => new WebTest($"{StackName}-up-time-check",
            new WebTestArgs
            {
                Configuration = new WebTestPropertiesConfigurationArgs
                {
                    WebTest = Output.Format(@$"<WebTest Name=""{StackName}-up-time-check"" Id=""{webTestId}"" Enabled=""True"" CssProjectStructure="""" CssIteration="""" Timeout=""0"" WorkItemIds="""" xmlns=""http://microsoft.com/schemas/VisualStudio/TeamTest/2010"" Description="""" CredentialUserName="""" CredentialPassword="""" PreAuthenticate=""True"" Proxy=""default"" StopOnError=""False"" RecordedResultFile="""" ResultsLocale="""">
  <Items>
    <Request Method=""GET"" Guid=""{webTestId}"" Version=""1.1"" Url=""{healthCheckUrl}"" ThinkTime=""0"" Timeout=""300"" ParseDependentRequests=""True"" FollowRedirects=""True"" RecordResult=""True"" Cache=""False"" ResponseTimeGoal=""0"" Encoding=""utf-8"" ExpectedHttpStatusCode=""200"" ExpectedResponseUrl="""" ReportingName="""" IgnoreHttpStatusCode=""False"" />
  </Items>
</WebTest>"),
                },
                Description = Output.Format($"Ping web test alert for {healthCheckUrl}"),
                Enabled = true,
                Frequency = 60,
                Kind = WebTestKind.Ping,
                Locations = { new WebTestGeolocationArgs { Location = "us-fl-mia-edge" } },
                ResourceGroupName = args.ResourceGroupName,
                RetryEnabled = true,
                SyntheticMonitorId = $"{StackName}-up-time-check",
                Timeout = 120,
                WebTestKind = WebTestKind.Ping,
                WebTestName = $"{StackName}-up-time-check",
                Tags = { { $"hidden-link:{id}", "Resource" } }
            }));
    }
}