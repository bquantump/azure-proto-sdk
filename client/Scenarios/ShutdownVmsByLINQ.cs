﻿using azure_proto_compute;
using Azure.ResourceManager.Core;
using System;
using System.Linq;

namespace client
{
    class ShutdownVmsByLINQ : Scenario
    {
        public override void Execute()
        {
            var createMultipleVms = new CreateMultipleVms(Context);
            createMultipleVms.Execute();

            var client = new AzureResourceManagerClient();
            foreach (var sub in client.Subscriptions().List())
            {
                var vmList = sub.ListVirtualMachines();
                foreach (var vm in vmList.Where(armResource => armResource.Data.Name.Contains("-o")))
                {
                    Console.WriteLine($"In subscription list: Stopping {vm.Id}");
                    vm.PowerOff();
                    Console.WriteLine($"In subscription list: Starting {vm.Id}");
                    vm.PowerOn();
                }
            }

            var resourceGroup = new AzureResourceManagerClient().ResourceGroup(Context.SubscriptionId, Context.RgName);

            resourceGroup.VirtualMachines().List().Select(vm =>
            {
                var parts = vm.Id.Name.Split('-');
                var n = Convert.ToInt32(parts[0].Last());
                return (vm, n);
            })
                .Where(tuple => tuple.n % 2 == 0)
                .ToList()
                .ForEach(tuple =>
                {
                    Console.WriteLine($"In resource group list: Stopping {tuple.vm.Id.Name}");
                    tuple.vm.PowerOff();
                    Console.WriteLine($"In resource group list: Starting {tuple.vm.Id.Name}");
                    tuple.vm.PowerOn();
                });
        }
    }
}
