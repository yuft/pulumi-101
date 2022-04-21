using Pulumi;

public abstract class HelloResourceBase
{
    protected const string StackName = "web-server";

    protected readonly InputMap<string> GlobalTags = new() { { "product", "pulumi-101" }, { "component", StackName } };

    protected HelloResourceBase()
    {

    }
}