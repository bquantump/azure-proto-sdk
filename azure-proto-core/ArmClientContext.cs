﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Azure.Core;

namespace azure_proto_core
{
    /// <summary>
    ///     Base class tracking context information for clients - when we have changed client constructors, this should not be
    ///     necessary
    /// </summary>
    public class ArmClientContext
    {
        public ArmClientContext(Uri baseUri, TokenCredential credential)
        {
            BaseUri = baseUri;
            Credential = credential;
        }

        public ArmClientContext(ArmClientContext other)
            : this(other.BaseUri, other.Credential)
        {
        }

        internal TokenCredential Credential { get; }

        internal Uri BaseUri { get; }

        /// <summary>
        ///     Note that this is currently adapting to underlying management clients - once generator changes are in, this would
        ///     likely be unnecessary
        /// </summary>
        /// <typeparam name="T">Operations class</typeparam>
        /// <param name="creator">Method to construct the operations class</param>
        /// <returns>Constructed operations class</returns>
        internal T GetClient<T>(Func<Uri, TokenCredential, T> creator)
        {
            return creator(BaseUri, Credential);
        }
    }
}
