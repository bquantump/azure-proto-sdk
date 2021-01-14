﻿using Azure.ResourceManager.Core;

namespace azure_proto_network
{
    public class Subnet : SubnetOperations
    {
        public Subnet(ResourceOperationsBase options, SubnetData resource)
            : base(options, resource.Id)
        {
            Data = resource;
        }

        public SubnetData Data { get; private set; }
    }
}
