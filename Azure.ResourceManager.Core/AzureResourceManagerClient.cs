﻿using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.Core.Adapters;
using Azure.ResourceManager.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.ResourceManager.Core
{
    /// <summary>
    /// The entry point for all ARM clients.
    /// </summary>
    public class AzureResourceManagerClient
    {
        /// <summary>
        /// The base URI of the service.
        /// </summary>
        internal static readonly string DefaultUri = "https://management.azure.com";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClient"/> class.
        /// </summary>
        public AzureResourceManagerClient()
            : this(new Uri(DefaultUri), new DefaultAzureCredential(), null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClient"/> class.
        /// </summary>
        /// <param name="options"> The client parameters to use in these operations. </param>
        public AzureResourceManagerClient(AzureResourceManagerClientOptions options)
            : this(options.BaseUri, options.Credential, null, options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClient"/> class.
        /// </summary>
        /// <param name="defaultSubscriptionId"> The id of the default Azure subscription. </param>
        public AzureResourceManagerClient(string defaultSubscriptionId)
            : this(new Uri(DefaultUri), new DefaultAzureCredential(), defaultSubscriptionId, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClient"/> class.
        /// </summary>
        /// <param name="defaultSubscriptionId"> The id of the default Azure subscription. </param>
        /// <param name="options"> The client parameters to use in these operations. </param>
        public AzureResourceManagerClient(string defaultSubscriptionId, AzureResourceManagerClientOptions options)
            : this(options.BaseUri, options.Credential, defaultSubscriptionId, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClient"/> class.
        /// </summary>
        /// <param name="credential"> A credential used to authenticate to an Azure Service. </param>
        /// <param name="defaultSubscriptionId"> The id of the default Azure subscription. </param>
        public AzureResourceManagerClient(TokenCredential credential, string defaultSubscriptionId)
            : this(new Uri(DefaultUri), credential, defaultSubscriptionId, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClient"/> class.
        /// </summary>
        /// <param name="credential"> A credential used to authenticate to an Azure Service. </param>
        /// <param name="defaultSubscriptionId"> The id of the default Azure subscription. </param>
        /// <param name="options"> The client parameters to use in these operations. </param>
        public AzureResourceManagerClient(TokenCredential credential, string defaultSubscriptionId, AzureResourceManagerClientOptions options)
            : this(options.BaseUri, credential, defaultSubscriptionId, options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClient"/> class.
        /// </summary>
        /// <param name="baseUri"> The base URI of the service. </param>
        /// <param name="credential"> A credential used to authenticate to an Azure Service. </param>
        /// <param name="options"> The client parameters to use in these operations. </param>
        public AzureResourceManagerClient(Uri baseUri, TokenCredential credential, AzureResourceManagerClientOptions options = null)
            : this(baseUri, credential, null, options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClient"/> class.
        /// </summary>
        /// <param name="baseUri"> The base URI of the service. </param>
        /// <param name="credential"> A credential used to authenticate to an Azure Service. </param>
        /// <param name="defaultSubscriptionId"> The id of the default Azure subscription. </param>
        /// <param name="options"> The client parameters to use in these operations. </param>
        public AzureResourceManagerClient(Uri baseUri, TokenCredential credential, string defaultSubscriptionId, AzureResourceManagerClientOptions options)
        {
            ClientOptions = new AzureResourceManagerClientOptions(baseUri, credential, options);
            defaultSubscriptionId ??= GetDefaultSubscription().ConfigureAwait(false).GetAwaiter().GetResult();
            DefaultSubscription = new SubscriptionOperations(ClientOptions, new ResourceIdentifier($"/subscriptions/{defaultSubscriptionId}"));
            ApiVersionOverrides = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the Api version overrides.
        /// </summary>
        public Dictionary<string, string> ApiVersionOverrides { get; private set; }

        /// <summary>
        /// Gets the default Azure subscription.
        /// </summary>
        public SubscriptionOperations DefaultSubscription { get; private set; }

        /// <summary>
        /// Gets the Azure resource manager client options.
        /// </summary>
        internal virtual AzureResourceManagerClientOptions ClientOptions { get; }

        /// <summary>
        /// Gets the subscription client.
        /// </summary>
        internal SubscriptionsOperations SubscriptionsClient => GetResourcesClient(Guid.NewGuid().ToString()).Subscriptions;

        /// <summary>
        /// Gets the Azure subscription operations.
        /// </summary>
        /// <param name="subscription">  The data model of the subscription. </param>
        /// <returns> Subscription operations. </returns>
        public SubscriptionOperations Subscription(SubscriptionData subscription) => new SubscriptionOperations(ClientOptions, subscription);

        /// <summary>
        /// Gets the Azure subscription operations.
        /// </summary>
        /// <param name="subscription"> The resource identifier of the subscription. </param>
        /// <returns> Subscription operations. </returns>
        public SubscriptionOperations Subscription(ResourceIdentifier subscription) => new SubscriptionOperations(ClientOptions, subscription);

        /// <summary>
        /// Gets the Azure subscription operations.
        /// </summary>
        /// <param name="subscription"> The id of the subscription. </param>
        /// <returns> Subscription operations. </returns>
        public SubscriptionOperations Subscription(string subscription) => new SubscriptionOperations(ClientOptions, subscription);

        /// <summary>
        /// Gets the Azure subscriptions.
        /// </summary>
        /// <returns> Subscription container. </returns>
        public SubscriptionContainer Subscriptions()
        {
            return new SubscriptionContainer(ClientOptions);
        }

        /// <summary>
        /// Lists all geo-locations.
        /// </summary>
        /// <param name="subscriptionId"> The Id of the target subscription. </param>
        /// <param name="token"> A token to allow the caller to cancel the call to the service. The default value is <see cref="P:System.Threading.CancellationToken.None" />. </param>
        /// <returns> An async collection of location data that may take multiple service requests to iterate over. </returns>
        /// <exception cref="InvalidOperationException"> <paramref name="subscriptionId"/> is null. </exception>
        public AsyncPageable<LocationData> ListLocationsAsync(string subscriptionId = null, CancellationToken token = default(CancellationToken))
        {
            async Task<AsyncPageable<LocationData>> PageableFunc()
            {
                if (string.IsNullOrWhiteSpace(subscriptionId))
                {
                    subscriptionId = await GetDefaultSubscription(token);
                    if (subscriptionId == null)
                    {
                        throw new InvalidOperationException("Please select a default subscription");
                    }
                }

                return new PhWrappingAsyncPageable<Azure.ResourceManager.Resources.Models.Location, LocationData>(SubscriptionsClient.ListLocationsAsync(subscriptionId, token), s => new LocationData(s));
            }

            return new PhTaskDeferringAsyncPageable<LocationData>(PageableFunc);
        }

        /// <summary>
        /// Lists all geo-locations.
        /// </summary>
        /// <param name="subscriptionId"> The Id of the target subscription. </param>
        /// <param name="cancellationToken"> A token to allow the caller to cancel the call to the service. The default value is <see cref="P:System.Threading.CancellationToken.None" />. </param>
        /// <returns> A collection of location data that may take multiple service requests to iterate over. </returns>
        /// <exception cref="InvalidOperationException"> <paramref name="subscriptionId"/> is null. </exception>
        public Pageable<LocationData> ListLocations(string subscriptionId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                subscriptionId = GetDefaultSubscription(cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
                if (subscriptionId == null)
                {
                    throw new InvalidOperationException("Please select a default subscription");
                }
            }

            return new PhWrappingPageable<Azure.ResourceManager.Resources.Models.Location, LocationData>(SubscriptionsClient.ListLocations(subscriptionId, cancellationToken), s => new LocationData(s));
        }

        /// <summary>
        /// Lists all available geo-locations.
        /// </summary>
        /// <param name="resourceType"> The resource type for subscription. </param>
        /// <param name="cancellationToken"> A token to allow the caller to cancel the call to the service. The default value is <see cref="P:System.Threading.CancellationToken.None" />. </param>
        /// <returns> An async collection of location that may take multiple service requests to iterate over. </returns>
        /// <exception cref="InvalidOperationException"> The default subscription id is null. </exception>
        public async IAsyncEnumerable<Location> ListAvailableLocationsAsync(ResourceType resourceType, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var subscriptionId = await GetDefaultSubscription(cancellationToken);
            if (subscriptionId == null)
            {
                throw new InvalidOperationException("Please select a default subscription");
            }

            await foreach (var location in ListAvailableLocationsAsync(subscriptionId, resourceType, cancellationToken))
            {
                yield return location;
            }
        }

        /// <summary>
        /// Lists all available geo-locations.
        /// </summary>
        /// <param name="subscription"> The Azure subscription. </param>
        /// <param name="resourceType"> The resource type for subscription. </param>
        /// <param name="cancellationToken"> A token to allow the caller to cancel the call to the service. The default value is <see cref="P:System.Threading.CancellationToken.None" />. </param>
        /// <returns> An async collection of location that may take multiple service requests to iterate over. </returns>
        public async IAsyncEnumerable<Location> ListAvailableLocationsAsync(string subscription, ResourceType resourceType, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var provider in GetResourcesClient(subscription).Providers.ListAsync(expand: "metadata", cancellationToken: cancellationToken).WithCancellation(cancellationToken))
            {
                if (string.Equals(provider.Namespace, resourceType?.Namespace, StringComparison.InvariantCultureIgnoreCase))
                {
                    var foundResource = provider.ResourceTypes.FirstOrDefault(p => resourceType.Equals(p.ResourceType));
                    foreach (var location in foundResource.Locations)
                    {
                        yield return location;
                    }
                }
            }
        }

        /// <summary>
        /// Lists all available geo-locations.
        /// </summary>
        /// <param name="resourceType"> The resource type for subscription. </param>
        /// <param name="cancellationToken"> A token to allow the caller to cancel the call to the service. The default value is <see cref="P:System.Threading.CancellationToken.None" />. </param>
        /// <returns> A collection of location that may take multiple service requests to iterate over. </returns>
        public IEnumerable<Location> ListAvailableLocations(ResourceType resourceType, CancellationToken cancellationToken = default(CancellationToken))
        {
            var subscription = GetDefaultSubscription().ConfigureAwait(false).GetAwaiter().GetResult();
            return ListAvailableLocations(subscription, resourceType, cancellationToken);
        }

        /// <summary>
        /// Lists all available geo-locations.
        /// </summary>
        /// <param name="subscription"> The Azure subscription. </param>
        /// <param name="resourceType"> The resource type for subscription. </param>
        /// <param name="cancellationToken"> A token to allow the caller to cancel the call to the service. The default value is <see cref="P:System.Threading.CancellationToken.None" />. </param>
        /// <returns> A collection of location that may take multiple service requests to iterate over. </returns>
        public IEnumerable<Location> ListAvailableLocations(string subscription, ResourceType resourceType, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetResourcesClient(subscription).Providers.List(expand: "metadata", cancellationToken: cancellationToken)
                .FirstOrDefault(p => string.Equals(p.Namespace, resourceType?.Namespace, StringComparison.InvariantCultureIgnoreCase))
                .ResourceTypes.FirstOrDefault(r => resourceType.Equals(r.ResourceType))
                .Locations.Cast<Location>();
        }

        /// <summary>
        /// Gets resource group operations.
        /// </summary>
        /// <param name="subscription"> The id of the Azure subscription. </param>
        /// <param name="resourceGroup"> The resource group name. </param>
        /// <returns> Resource group operations. </returns>
        public ResourceGroupOperations ResourceGroup(string subscription, string resourceGroup)
        {
            return new ResourceGroupOperations(ClientOptions, $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}");
        }

        /// <summary>
        /// Gets resource group operations.
        /// </summary>
        /// <param name="resourceGroup"> The resource identifier of the resource group. </param>
        /// <returns> Resource group operations. </returns>
        public ResourceGroupOperations ResourceGroup(ResourceIdentifier resourceGroup)
        {
            return new ResourceGroupOperations(ClientOptions, resourceGroup);
        }

        /// <summary>
        /// Gets resource group operations.
        /// </summary>
        /// <param name="resourceGroup"> The data model of the resource group. </param>
        /// <returns> Resource group operations. </returns>
        public ResourceGroupOperations ResourceGroup(ResourceGroupData resourceGroup)
        {
            return new ResourceGroupOperations(ClientOptions, resourceGroup.Id);
        }

        /// <summary>
        /// Gets resource operations base.
        /// </summary>
        /// <typeparam name="T"> The type of the underlying model this class wraps. </typeparam>
        /// <param name="resource"> The tracked resource. </param>
        /// <returns> Resource operations of the resource. </returns>
        public T GetResourceOperationsBase<T>(TrackedResource resource)
            where T : TrackedResource
        {
            return Activator.CreateInstance(typeof(T), ClientOptions, resource) as T;
        }

        /// <summary>
        /// Gets resource operations base.
        /// </summary>
        /// <typeparam name="T"> The type of the underlying model this class wraps. </typeparam>
        /// <param name="resource"> The resource identifier of the resource. </param>
        /// <returns> Resource operations of the resource. </returns>
        public T GetResourceOperationsBase<T>(ResourceIdentifier resource)
            where T : OperationsBase
        {
            return Activator.CreateInstance(typeof(T), ClientOptions, resource) as T;
        }

        /// <summary>
        /// Gets resource operations base.
        /// </summary>
        /// <typeparam name="T"> The type of the underlying model this class wraps. </typeparam>
        /// <param name="subscription"> The id of the Azure subscription. </param>
        /// <param name="resourceGroup"> The resource group name. </param>
        /// <param name="name"> The resource type name. </param>
        /// <returns> Resource operations of the resource. </returns>
        public T GetResourceOperationsBase<T>(string subscription, string resourceGroup, string name)
            where T : OperationsBase
        {
            string resourceType = typeof(T).GetField("ResourceType", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null).ToString();
            ResourceIdentifier id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/{resourceType}/{name}";
            return Activator.CreateInstance(
                typeof(T),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new object[] { ClientOptions, id },
                CultureInfo.InvariantCulture) as T;
        }

        /// <summary>
        /// Gets resource operations base.
        /// </summary>
        /// <typeparam name="TContainer"> The type of the container class for a specific resource. </typeparam>
        /// <typeparam name="TOperations"> The type of the operations class for a specific resource. </typeparam>
        /// <typeparam name="TResource"> The type of the class containing properties for the underlying resource. </typeparam>
        /// <param name="subscription"> The id of the Azure subscription. </param>
        /// <param name="resourceGroup"> The resource group name. </param>
        /// <param name="name"> The resource type name. </param>
        /// <param name="model"> The resource data model. </param>
        /// <param name="location"> The resource geo-location. </param>
        /// <returns> Resource operations of the resource. </returns>
        public ArmResponse<TOperations> CreateResource<TContainer, TOperations, TResource>(string subscription, string resourceGroup, string name, TResource model, Location location = default)
            where TResource : TrackedResource
            where TOperations : ResourceOperationsBase<TOperations>
            where TContainer : ResourceContainerBase<TOperations, TResource>
        {
            if (location == null)
            {
                location = Location.Default;
            }

            TContainer container = Activator.CreateInstance(typeof(TContainer), ClientOptions, new ArmResourceData($"/subscriptions/{subscription}/resourceGroups/{resourceGroup}", location)) as TContainer;

            return container.Create(name, model);
        }

        /// <summary>
        /// Gets default subscription.
        /// </summary>
        /// <param name="cancellationToken"> A token to allow the caller to cancel the call to the service. The default value is <see cref="P:System.Threading.CancellationToken.None" />. </param>
        /// <returns> A <see cref="Task"/> that on completion returns the subscription id. </returns>
        internal async Task<string> GetDefaultSubscription(CancellationToken cancellationToken = default)
        {
            string sub = DefaultSubscription?.Id?.Subscription;
            if (null == sub)
            {
                sub = await Subscriptions().GetDefaultSubscriptionAsync(cancellationToken);
            }

            return sub;
        }

        /// <summary>
        /// Gets the resource client.
        /// </summary>
        internal ResourcesManagementClient GetResourcesClient(string subscription) => ClientOptions.GetClient((uri, credential) => new ResourcesManagementClient(uri, subscription, credential));
    }
}
