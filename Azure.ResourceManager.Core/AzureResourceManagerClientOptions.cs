﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;

namespace Azure.ResourceManager.Core
{
    /// <summary>
    /// A class representing Azure resource manager client options.
    /// </summary>
    public class AzureResourceManagerClientOptions : ClientOptions
    {
        private static readonly object _overridesLock = new object();

        private Dictionary<Type, object> _overrides = new Dictionary<Type, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClientOptions"/> class.
        /// </summary>
        public AzureResourceManagerClientOptions()
            : this(new Uri(AzureResourceManagerClient.DefaultUri), new DefaultAzureCredential())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClientOptions"/> class.
        /// </summary>
        /// <param name="baseUri"> The base URI of the service. </param>
        /// <param name="credential"> A credential used to authenticate to an Azure Service. </param>
        /// <param name="other"> The client parameters to use in these operations. </param>
        public AzureResourceManagerClientOptions(Uri baseUri, TokenCredential credential, AzureResourceManagerClientOptions other = null)
        {
            BaseUri = baseUri;
            Credential = credential;

            // Will go away when moved into core since we will have directy acces the policies and transport, so just need to set those
            if (!ReferenceEquals(other, null))
                Copy(other);
        }

        /// <summary>
        /// Gets each http call policies.
        /// </summary>
        /// <returns> A collection of http pipeline policy that may take multiple service requests to iterate over. </returns>
        internal IList<HttpPipelinePolicy> PerCallPolicies { get; } = new List<HttpPipelinePolicy>();

        /// <summary>
        /// Gets each http retry call policies.
        /// </summary>
        /// <returns> A collection of http pipeline policy that may take multiple service requests to iterate over. </returns>
        internal IList<HttpPipelinePolicy> PerRetryPolicies { get; } = new List<HttpPipelinePolicy>();

        /// <summary>
        /// Gets the Azure credential.
        /// </summary>
        internal TokenCredential Credential { get; }

        /// <summary>
        /// Gets the base URI of the service.
        /// </summary>
        internal Uri BaseUri { get; }

        /// <summary>
        /// Converts client options.
        /// </summary>
        /// <typeparam name="T"> The type of the underlying model this class wraps. </typeparam>
        public T Convert<T>()
            where T : ClientOptions, new()
        {
            var newOptions = new T();
            newOptions.Transport = Transport;
            foreach (var pol in PerCallPolicies)
            {
                newOptions.AddPolicy(pol, HttpPipelinePosition.PerCall);
            }

            foreach (var pol in PerRetryPolicies)
            {
                newOptions.AddPolicy(pol, HttpPipelinePosition.PerRetry);
            }

            return newOptions;
        }

        /// <summary>
        /// Adds a policy for Azure resource manager client http call.
        /// </summary>
        /// <param name="policy"> The http call policy in the pipeline. </param>
        /// <param name="position"> The position of the http call policy in the pipeline. </param>
        public new void AddPolicy(HttpPipelinePolicy policy, HttpPipelinePosition position)
        {
            // TODO policy lists are internal hence we don't have acces to them by inheriting ClientOptions in this Asembly, this is a wrapper for now to convert to the concrete
            // policy options.
            switch (position)
            {
                case HttpPipelinePosition.PerCall:
                    PerCallPolicies.Add(policy);
                    break;
                case HttpPipelinePosition.PerRetry:
                    PerRetryPolicies.Add(policy);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }

            base.AddPolicy(policy, position);
        }

        /// <summary>
        /// Gets override object.
        /// </summary>
        /// <typeparam name="T"> The type of the underlying model this class wraps. </typeparam>
        /// <param name="ctor"> A function which returns an object. </param>
        /// <returns> The override object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object GetOverrideObject<T>(Func<object> ctor)
        {
            object overrideObject;
            Type type = typeof(T);
            if (!_overrides.TryGetValue(type, out overrideObject))
            {
                lock (_overridesLock)
                {
                    if (!_overrides.TryGetValue(type, out overrideObject))
                    {
                        overrideObject = ctor();
                        _overrides[type] = overrideObject;
                    }
                }
            }

            return overrideObject;
        }

        /// <summary>
        /// Gets the HTTP client options that will be used for all clients created from this Azure Resource Manger Client.
        /// Note that this is currently adapting to underlying management clients - once generator changes are in, this would
        /// likely be unnecessary.
        /// </summary>
        /// <typeparam name="T"> The type of the underlying model this class wraps. </typeparam>
        /// <param name="creator"> A method to construct the operations class. </param>
        /// <returns> The constructed operations class. </returns>
        internal T GetClient<T>(Func<Uri, TokenCredential, T> creator)
        {
            return creator(BaseUri, Credential);
        }

        // Will be removed like AddPolicy when we move to azure core
        private void Copy(AzureResourceManagerClientOptions other)
        {
            Transport = other.Transport;
            foreach (var pol in other.PerCallPolicies)
            {
                AddPolicy(pol, HttpPipelinePosition.PerCall);
            }

            foreach (var pol in other.PerRetryPolicies)
            {
                AddPolicy(pol, HttpPipelinePosition.PerRetry);
            }
        }
    }
}
