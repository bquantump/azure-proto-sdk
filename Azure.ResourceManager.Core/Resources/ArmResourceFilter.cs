﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Azure.ResourceManager.Core.Resources
{
    /// <summary>
    /// Syntactic sugar for creating ARM filters
    /// </summary>
    public abstract class ArmResourceFilter : IEquatable<string>, IEquatable<ArmResourceFilter>
    {
        /// <inheritdoc/>
        public abstract bool Equals(ArmResourceFilter other);

        /// <inheritdoc/>
        public abstract bool Equals(string other);

        /// <summary>
        /// Gets the filter as a string.
        /// </summary>
        /// <returns> The string representation of the filter. </returns>
        public abstract string GetFilterString();

        /// <inheritdoc/>
        public override string ToString()
        {
            return GetFilterString();
        }
    }
}
