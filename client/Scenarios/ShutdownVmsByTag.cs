﻿using azure_proto_compute;
using azure_proto_core;
using System;
using System.Linq;

namespace client
{
    class ShutdownVmsByTag : Scenario
    {
        public override void Execute()
        {
            var createMultipleVms = new CreateMultipleVms(Context);
            createMultipleVms.Execute();

            var rg = new ArmClient().ResourceGroup(Context.SubscriptionId, Context.RgName).Get().Value;

            //set tags on random vms
            Random rand = new Random(Environment.TickCount);
            foreach (var generic in rg.VirtualMachines().ListByName(Environment.UserName))
            {
                var vm = VirtualMachineOperations.FromGeneric(generic);
                if (rand.NextDouble() > 0.5)
                {
                    Console.WriteLine("adding tag to {0}", vm.Id.Name);
                    vm.AddTag("tagkey", "tagvalue");
                }
            }

            var filteredList = rg.VirtualMachines().List().Where(vm =>
            {
                string value;
                return (vm.Data.Tags.TryGetValue("tagkey", out value) && value == "tagvalue");
            });

            foreach (var vm in filteredList)
            {
                Console.WriteLine("--------Stopping VM {0}--------", vm.Id.Name);
                vm.PowerOff();
                Console.WriteLine("--------Starting VM {0}--------", vm.Id.Name);
                vm.PowerOn();
            }
        }
    }
}
