﻿using azure_proto_compute;
using azure_proto_core;
using azure_proto_core.Resources;
using System;

namespace client
{
    class ShutdownVmsByTag : Scenario
    {
        public override void Execute()
        {
            var createMultipleVms = new CreateMultipleVms(Context);
            createMultipleVms.Execute();

            var rg = new ArmClient().ResourceGroup(Context.SubscriptionId, Context.RgName);

            //set tags on random vms
            Random rand = new Random(Environment.TickCount);
            foreach (var vm in rg.VirtualMachines().List(Environment.UserName))
            {
                if (rand.NextDouble() > 0.5)
                {
                    Console.WriteLine("adding tag to {0}", vm.Id.Name);
                    vm.AddTag("tagkey", "tagvalue");
                }
            }

            foreach (var vm in rg.VirtualMachines().ListByTag(new ArmTagFilter("tagkey", "tagvalue")))
            {
                Console.WriteLine("--------Stopping VM {0}--------", vm.Id.Name);
                vm.PowerOff();
                Console.WriteLine("--------Starting VM {0}--------", vm.Id.Name);
                vm.PowerOn();
            }
        }
    }
}
