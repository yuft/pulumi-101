using System;
using System.Linq;
using System.Text;
using Pulumi;
using Pulumi.AzureNative.Compute;
using Pulumi.AzureNative.Compute.Inputs;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Network;
using Pulumi.AzureNative.Network.Inputs;
using Pulumi.Random;
using IPVersion = Pulumi.AzureNative.Network.IPVersion;
using PublicIPAddressSkuName = Pulumi.AzureNative.Network.PublicIPAddressSkuName;
using PublicIPAddressSkuTier = Pulumi.AzureNative.Network.PublicIPAddressSkuTier;

public class WebServerResource : HelloResourceBase
{
    public WebServerResource()
    {
        ResourceGroup = new ResourceGroup($"{StackName}-rg", new ResourceGroupArgs
        {
            ResourceGroupName = $"{StackName}-rg",
            Tags = GlobalTags
        });

        var virtualNetwork = new VirtualNetwork($"{StackName}-vnet", new VirtualNetworkArgs
        {
            ResourceGroupName = ResourceGroup.Name,
            AddressSpace = new AddressSpaceArgs
            {
                AddressPrefixes =
                {
                    "10.0.0.0/16"
                }
            },
            VirtualNetworkName = $"{StackName}-vnet",
            Subnets =
            {
                new Pulumi.AzureNative.Network.Inputs.SubnetArgs
                {
                    Name = "default",
                    AddressPrefix = "10.0.1.0/24"
                }
            },
            Tags = GlobalTags
        });

        var publicIp = new PublicIPAddress($"{StackName}-pip", new Pulumi.AzureNative.Network.PublicIPAddressArgs
        {
            IdleTimeoutInMinutes = 10,
            PublicIPAddressVersion = IPVersion.IPv4,
            PublicIPAllocationMethod = IPAllocationMethod.Static,
            PublicIpAddressName = $"{StackName}-pip",
            ResourceGroupName = ResourceGroup.Name,
            Sku = new Pulumi.AzureNative.Network.Inputs.PublicIPAddressSkuArgs { Name = PublicIPAddressSkuName.Basic, Tier = PublicIPAddressSkuTier.Regional },
            Tags = GlobalTags
        });

        var networkSecurityGroup = new NetworkSecurityGroup($"{StackName}-nsg", new Pulumi.AzureNative.Network.NetworkSecurityGroupArgs
        {
            Id = $"{StackName}-nsg",
            ResourceGroupName = ResourceGroup.Name,
            NetworkSecurityGroupName = $"{StackName}-nsg",
            SecurityRules =
            {
                new Pulumi.AzureNative.Network.Inputs.SecurityRuleArgs
                {
                    Access = SecurityRuleAccess.Allow,
                    Description = "Allow web 80",
                    DestinationAddressPrefix = "*",
                    DestinationPortRange = "80",
                    Direction = SecurityRuleDirection.Inbound,
                    Name = "allow_web_80",
                    Priority = 100,
                    Protocol = SecurityRuleProtocol.Tcp,
                    SourceAddressPrefix = "*",
                    SourcePortRange = "*"
                },
                new Pulumi.AzureNative.Network.Inputs.SecurityRuleArgs
                {
                    Access = SecurityRuleAccess.Allow,
                    Description = "Allow is SSH",
                    DestinationAddressPrefix = "*",
                    DestinationPortRange = "22",
                    Direction = SecurityRuleDirection.Inbound,
                    Name = "allow_ssh_in",
                    Priority = 100,
                    Protocol = SecurityRuleProtocol.Tcp,
                    SourceAddressPrefix = "10.0.0.0/16",
                    SourcePortRange = "*"
                },
            },
            Tags = GlobalTags
        });

        var networkInterface = new NetworkInterface($"{StackName}-nic", new NetworkInterfaceArgs
        {
            ResourceGroupName = ResourceGroup.Name,
            NetworkInterfaceName = $"{StackName}-nic",
            IpConfigurations =
            {
                new NetworkInterfaceIPConfigurationArgs
                {
                    Name = $"{StackName}-ip-config",
                    Subnet = new Pulumi.AzureNative.Network.Inputs.SubnetArgs
                    {
                        Id = virtualNetwork.Subnets.Apply(s => s.Single(x => x.Name == "default").Id)!
                    },
                    PrivateIPAllocationMethod = IPAllocationMethod.Dynamic,
                    PublicIPAddress = new Pulumi.AzureNative.Network.Inputs.PublicIPAddressArgs
                    {
                        Id = publicIp.Id
                    }
                }
            },
            NetworkSecurityGroup = new Pulumi.AzureNative.Network.Inputs.NetworkSecurityGroupArgs
            {
                Id = networkSecurityGroup.Id
            },
            Tags = GlobalTags
        });

        const string vmName = $"{StackName}-vm";
        var vmAdminPassword = new RandomPassword($"{StackName}-vm-pwd", new RandomPasswordArgs { Length = 16, Special = true });

        const string initScript = @"#!/bin/bash
                                    echo 'Hello, Pulumi!' > index.html
                                    nohup python -m SimpleHTTPServer 80 &";

        VirtualMachine = new VirtualMachine($"{StackName}-vm",
                new VirtualMachineArgs
                {
                    HardwareProfile = new HardwareProfileArgs
                    {
                        VmSize = VirtualMachineSizeTypes.Basic_A0
                    },
                    NetworkProfile = new Pulumi.AzureNative.Compute.Inputs.NetworkProfileArgs
                    {
                        NetworkInterfaces =
                        {
                            new NetworkInterfaceReferenceArgs
                            {
                                Id =  networkInterface.Id,
                            }
                        }
                    },
                    OsProfile = new OSProfileArgs
                    {
                        AdminPassword = vmAdminPassword.Result,
                        AdminUsername = "web-server-admin",
                        ComputerName = vmName,
                        CustomData = Convert.ToBase64String(Encoding.UTF8.GetBytes(initScript)),
                        LinuxConfiguration = new LinuxConfigurationArgs
                        {
                            DisablePasswordAuthentication = false
                        }
                    },
                    ResourceGroupName = ResourceGroup.Name,
                    StorageProfile = new StorageProfileArgs
                    {
                        ImageReference = new ImageReferenceArgs
                        {
                            Offer = "UbuntuServer",
                            Publisher = "Canonical",
                            Sku = "18.04-LTS",
                            Version = "latest",
                        },
                        OsDisk = new OSDiskArgs
                        {
                            Caching = CachingTypes.ReadWrite,
                            CreateOption = DiskCreateOptionTypes.FromImage
                        },
                    },
                    VmName = vmName,
                    Tags = GlobalTags
                });

        WebServerIp = VirtualMachine.Id.Apply(_ => GetPublicIPAddress.Invoke(new GetPublicIPAddressInvokeArgs
        {
            ResourceGroupName = ResourceGroup.Name,
            PublicIpAddressName = publicIp.Name
        })).Apply(x => x.IpAddress!);
    }

    public ResourceGroup ResourceGroup { get; }

    public VirtualMachine VirtualMachine { get; }

    public Output<string> WebServerIp { get; }
}