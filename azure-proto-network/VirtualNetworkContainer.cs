﻿using Azure;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using Azure.ResourceManager.Core;
using Azure.ResourceManager.Core.Adapters;
using Azure.ResourceManager.Core.Resources;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace azure_proto_network
{
    /// <summary>
    /// A class representing collection of virtual nerwork and their operations over a resource group.
    /// </summary>
    public class VirtualNetworkContainer : ResourceContainerBase<VirtualNetwork, VirtualNetworkData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNetworkContainer"/> class.
        /// </summary>
        /// <param name="genericOperations"> Operations to create this operations class from. </param>
        internal VirtualNetworkContainer(ArmResourceOperations genericOperations)
            : base(genericOperations.ClientOptions, genericOperations.Id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNetworkContainer"/> class.
        /// </summary>
        /// <param name="options"> The client parameters to use in these operations. </param>
        /// <param name="resourceGroup"> The data of Resource Group. </param>
        internal VirtualNetworkContainer(AzureResourceManagerClientOptions options, ResourceGroupData resourceGroup)
            : base(options, resourceGroup)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNetworkContainer"/> class.
        /// </summary>
        /// <param name="options"> The client parameters to use in these operations. </param>
        /// <param name="parentId"> The resource Id of the parent resource. </param>
        internal VirtualNetworkContainer(AzureResourceManagerClientOptions options, ResourceIdentifier parentId)
            : base(options, parentId)
        {
        }

        /// <inheritdoc/>
        protected override ResourceType ValidResourceType => ResourceGroupOperations.ResourceType;

        private VirtualNetworksOperations Operations => GetClient<NetworkManagementClient>((uri, cred) => new NetworkManagementClient(Id.Subscription, uri, cred,
            ClientOptions.Convert<NetworkManagementClientOptions>())).VirtualNetworks;

        /// <inheritdoc/>
        public override ArmResponse<VirtualNetwork> Create(string name, VirtualNetworkData resourceDetails)
        {
            var operation = Operations.StartCreateOrUpdate(Id.ResourceGroup, name, resourceDetails);
            return new PhArmResponse<VirtualNetwork, Azure.ResourceManager.Network.Models.VirtualNetwork>(
                operation.WaitForCompletionAsync().ConfigureAwait(false).GetAwaiter().GetResult(),
                n => new VirtualNetwork(ClientOptions, new VirtualNetworkData(n)));
        }

        /// <inheritdoc/>
        public async override Task<ArmResponse<VirtualNetwork>> CreateAsync(string name, VirtualNetworkData resourceDetails, CancellationToken cancellationToken = default)
        {
            var operation = await Operations.StartCreateOrUpdateAsync(Id.ResourceGroup, name, resourceDetails, cancellationToken).ConfigureAwait(false);
            return new PhArmResponse<VirtualNetwork, Azure.ResourceManager.Network.Models.VirtualNetwork>(
                await operation.WaitForCompletionAsync(cancellationToken).ConfigureAwait(false),
                n => new VirtualNetwork(ClientOptions, new VirtualNetworkData(n)));
        }

        /// <inheritdoc/>
        public override ArmOperation<VirtualNetwork> StartCreate(string name, VirtualNetworkData resourceDetails, CancellationToken cancellationToken = default)
        {
            return new PhArmOperation<VirtualNetwork, Azure.ResourceManager.Network.Models.VirtualNetwork>(
                Operations.StartCreateOrUpdate(Id.ResourceGroup, name, resourceDetails, cancellationToken),
                n => new VirtualNetwork(ClientOptions, new VirtualNetworkData(n)));
        }

        /// <inheritdoc/>
        public async override Task<ArmOperation<VirtualNetwork>> StartCreateAsync(string name, VirtualNetworkData resourceDetails, CancellationToken cancellationToken = default)
        {
            return new PhArmOperation<VirtualNetwork, Azure.ResourceManager.Network.Models.VirtualNetwork>(
                await Operations.StartCreateOrUpdateAsync(Id.ResourceGroup, name, resourceDetails, cancellationToken).ConfigureAwait(false),
                n => new VirtualNetwork(ClientOptions, new VirtualNetworkData(n)));
        }

        /// <summary>
        /// Constructs an object used to create a virtual nerwork.
        /// </summary>
        /// <param name="vnetCidr"> The CIDR of the resource. </param>
        /// <param name="location"> The location of the resource. </param>
        /// <returns> A builder with <see cref="VirtualNetwork"> and <see cref="VirtualNetworkData"/>. </returns>
        public ArmBuilder<VirtualNetwork, VirtualNetworkData> Construct(string vnetCidr, Location location = null)
        {
            var vnet = new Azure.ResourceManager.Network.Models.VirtualNetwork()
            {
                Location = location ?? DefaultLocation,
                AddressSpace = new AddressSpace() { AddressPrefixes = new List<string>() { vnetCidr } },
            };

            return new ArmBuilder<VirtualNetwork, VirtualNetworkData>(this, new VirtualNetworkData(vnet));
        }

        /// <summary>
        /// Lists the virtual network for this resource group.
        /// </summary>
        /// <param name="cancellationToken"> A token to allow the caller to cancel the call to the service. The default value is <see cref="P:System.Threading.CancellationToken.None" />. </param>
        /// <returns> A collection of resource operations that may take multiple service requests to iterate over. </returns>
        public Pageable<VirtualNetwork> List(CancellationToken cancellationToken = default)
        {
            return new PhWrappingPageable<Azure.ResourceManager.Network.Models.VirtualNetwork, VirtualNetwork>(
                Operations.List(Id.Name, cancellationToken),
                Convertor());
        }

        /// <summary>
        /// Lists the virtual network for this resource group.
        /// </summary>
        /// <param name="cancellationToken"> A token to allow the caller to cancel the call to the service. The default value is <see cref="P:System.Threading.CancellationToken.None" />. </param>
        /// <returns> An async collection of resource operations that may take multiple service requests to iterate over. </returns>
        public AsyncPageable<VirtualNetwork> ListAsync(CancellationToken cancellationToken = default)
        {
            return new PhWrappingAsyncPageable<Azure.ResourceManager.Network.Models.VirtualNetwork, VirtualNetwork>(
                Operations.ListAsync(Id.Name, cancellationToken),
                Convertor());
        }

        /// <summary>
        /// Filters the list of virtual nerwork for this resource group represented as generic resources.
        /// </summary>
        /// <param name="filter"> The filter used in this operation. </param>
        /// <param name="top"> The number of results to return. </param>
        /// <param name="cancellationToken"> A token to allow the caller to cancel the call to the service. The default value is <see cref="P:System.Threading.CancellationToken.None" />. </param>
        /// <returns> A collection of resource that may take multiple service requests to iterate over. </returns>
        public Pageable<ArmResource> ListByName(ArmSubstringFilter filter, int? top = null, CancellationToken cancellationToken = default)
        {
            ArmFilterCollection filters = new ArmFilterCollection(VirtualNetworkData.ResourceType);
            filters.SubstringFilter = filter;
            return ResourceListOperations.ListAtContext(ClientOptions, Id, filters, top, cancellationToken);
        }

        /// <summary>
        /// Filters the list of virtual nerwork for this resource group represented as generic resources.
        /// </summary>
        /// <param name="filter"> The filter used in this operation. </param>
        /// <param name="top"> The number of results to return. </param>
        /// <param name="cancellationToken"> A token to allow the caller to cancel the call to the service. The default value is <see cref="P:System.Threading.CancellationToken.None" />. </param>
        /// <returns> An async collection of resource that may take multiple service requests to iterate over. </returns>
        public AsyncPageable<ArmResource> ListByNameAsync(ArmSubstringFilter filter, int? top = null, CancellationToken cancellationToken = default)
        {
            ArmFilterCollection filters = new ArmFilterCollection(VirtualNetworkData.ResourceType);
            filters.SubstringFilter = filter;
            return ResourceListOperations.ListAtContextAsync(ClientOptions, Id, filters, top, cancellationToken);
        }

        /// <summary>
        /// Filters the list of virtual nerwork for this resource group.
        /// Makes an additional network call to retrieve the full data model for each virtual nerwork.
        /// </summary>
        /// <param name="filter"> The filter used in this operation. </param>
        /// <param name="top"> The number of results to return. </param>
        /// <param name="cancellationToken"> A token to allow the caller to cancel the call to the service. The default value is <see cref="P:System.Threading.CancellationToken.None" />. </param>
        /// <returns> A collection of resource that may take multiple service requests to iterate over. </returns>
        public Pageable<VirtualNetwork> ListByNameExpanded(ArmSubstringFilter filter, int? top = null, CancellationToken cancellationToken = default)
        {
            var results = ListByName(filter, top, cancellationToken);
            return new PhWrappingPageable<ArmResource, VirtualNetwork>(results, s => new VirtualNetworkOperations(s).Get().Value);
        }

        /// <summary>
        /// Filters the list of virtual nerwork for this resource group.
        /// Makes an additional network call to retrieve the full data model for each virtual nerwork.
        /// </summary>
        /// <param name="filter"> The filter used in this operation. </param>
        /// <param name="top"> The number of results to return. </param>
        /// <param name="cancellationToken"> A token to allow the caller to cancel the call to the service. The default value is <see cref="P:System.Threading.CancellationToken.None" />. </param>
        /// <returns> An asyc collection of availability set that may take multiple service requests to iterate over. </returns>
        public AsyncPageable<VirtualNetwork> ListByNameExpandedAsync(ArmSubstringFilter filter, int? top = null, CancellationToken cancellationToken = default)
        {
            var results = ListByNameAsync(filter, top, cancellationToken);
            return new PhWrappingAsyncPageable<ArmResource, VirtualNetwork>(results, s => new VirtualNetworkOperations(s).Get().Value);
        }

        private Func<Azure.ResourceManager.Network.Models.VirtualNetwork, VirtualNetwork> Convertor()
        {
            return s => new VirtualNetwork(ClientOptions, new VirtualNetworkData(s));
        }
    }
}
