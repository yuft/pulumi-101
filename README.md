# Web Server Using Azure Virtual Machine with health check

This example provisions a Linux web server in an Azure Virtual Machine and gives it a public IP address.
It also creates an application insights instance with a web test against the public IP address.

## Prerequisites

- [.Net6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [Download and install the Pulumi CLI](https://www.pulumi.com/docs/get-started/install/)
- [Connect Pulumi with your Azure account](https://www.pulumi.com/docs/intro/cloud-providers/azure/setup/) (if your `az` CLI is configured, no further changes are required)

## Running the App

1.  Create a new stack:

    ```
    $ pulumi stack init dev
    ```

1.  Configure the app deployment. 

    ```
    $ pulumi config set azure-native:location australiaeast    # any valid Azure region will do
    ```

1.  Login to Azure CLI (you will be prompted to do this during deployment if you forget this step):

    ```
    $ az login
    ```

1.  Build .Net dependencies:

    ```
    $ dotnet build
    ```

1.  Run `pulumi up` to preview and deploy changes:

    ``` 
    $ pulumi up
    Previewing changes:
    ...

    Performing changes:
    ...
    info: 7 changes performed:
        + 7 resources created
    Update duration: 2m38s
    ```

1.  Clean up

    ```
    $ pulumi destory
    ```