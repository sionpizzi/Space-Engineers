
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;


namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

        public Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set Runtime.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)


        {
            //attach this to your custom data to make the script work
            //Cobalt:30k
            //CobaltOre: 167k
            //Gold: 450
            //GoldOre: 35k
            //Iron: 280k
            //IronOre: 400k
            //MagnesiumOre: 135k
            //Magnesium: 10k
            //Nickel: 23k
            //NickelOre: 300k
            // Platinum: 100
            // PlatinumOre: 24k
            // SiliconOre: 135k
            // Silicon: 27k
            // Silver: 800
            //  SilverOre: 50k
            // Uranium: 70
            //UraniumOre: 5k
            //format the customdata as follows
            // Iron:10k
            //
            //This functino uses the custom data to get the amounts requested, only other requirement is placing [sale] in the inventory name in which you want
            string default_amount = GridTerminalSystem.GetBlockWithName("Prepare_Cargo_Script").CustomData;
            bool use_default = true;
            string name_of_target_inventory = "[sale]";
            string ingot_prefix = "MyObjectBuilder_Ingot";
            string ore_prefix = "MyObjectBuilder_Ore";
            string ore_suffix = "Ore";
            float multiplier = 1;
            if (argument == "2")
            {
                multiplier = 2;
            }
            else if (argument == "3")
            {
                multiplier = 3;
            }
            else if (argument == "4")
            {
                multiplier = 4;
            }
            List<IMyCargoContainer> inventories = new List<IMyCargoContainer>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(inventories);

            // step 1 get the target inventory
            List<IMyTerminalBlock> target_inventories = new List<IMyTerminalBlock>();

            GridTerminalSystem.SearchBlocksOfName(name_of_target_inventory, target_inventories);
            // validate the inventory, make sure there is no more than one
            if (target_inventories.Count > 1)
            {
                Echo("only one inventory with name [sale] allowed");
                return;
            }
            else if (target_inventories.Count == 0)
            {
                Echo("error: no inventories found with name [sale]");
                return;
            }
            else
            {

            }
            // parse custom data
            string target_inventory_custom_data = target_inventories[0].CustomData;
            if (use_default) target_inventory_custom_data = default_amount;
            string[] cd_lines = target_inventory_custom_data.Split('\n');
            //iterate over new lines, search ingots
            foreach (string line in cd_lines)
            {
                // get the name and the amount
                if (line.Length < 2) break;
                string ingot_name = line.Split(':')[0];
                string ingot_amount = line.Split(':')[1];
                bool isOre = false;
                if (ingot_name.Substring(ingot_name.Length -3, 3 ) == ore_suffix)
                {
                    isOre = true;
                }
                MyFixedPoint amount;
                if (ingot_amount[ingot_amount.Length - 1] == 'k')
                {

                    amount = MyFixedPoint.DeserializeString(ingot_amount.Remove(ingot_amount.Length - 1, 1));
                    amount = amount * 1000;

                }
                else
                {
                    amount = MyFixedPoint.DeserializeString(ingot_amount);
                }
                amount = amount * multiplier;

                //now we can try to move the item by iterating through the inventories
                foreach (IMyCargoContainer inv in inventories)

                {

                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    inv.GetInventory().GetItems(items);
                    foreach (MyInventoryItem item in items)
                    {
                        //check if the item type and subtype match the given item;
                        if (!isOre)
                        {
                            if (item.Type.TypeId.ToString() == ingot_prefix && item.Type.SubtypeId == ingot_name)
                            {
                                List<MyInventoryItem> items_in_target_cargo = new List<MyInventoryItem>();
                                target_inventories[0].GetInventory().GetItems(items_in_target_cargo);
                                MyFixedPoint amount_to_transfer = Get_Missing_Amount(ingot_prefix, ingot_name, items_in_target_cargo, amount);
                                Echo("attempted transfer for following item:");
                                Echo(item.Type.SubtypeId);
                                Echo(amount_to_transfer.ToString());
                                //transfer item to target inventory
                                inv.GetInventory().TransferItemTo(target_inventories[0].GetInventory(), item, amount_to_transfer);

                            }
                        }
                        else
                        {
                            if (item.Type.TypeId.ToString() == ore_prefix && item.Type.SubtypeId == ingot_name.Substring(0, ingot_name.Length-3))
                            {
                                List<MyInventoryItem> items_in_target_cargo = new List<MyInventoryItem>();
                                target_inventories[0].GetInventory().GetItems(items_in_target_cargo);
                                MyFixedPoint amount_to_transfer = Get_Missing_Amount(ore_prefix, ingot_name.Substring(0,ingot_name.Length - 3), items_in_target_cargo, amount);
                                Echo("attempted transfer for following item:");
                                Echo(item.Type.SubtypeId);
                                Echo(amount_to_transfer.ToString());
                                //transfer item to target inventory
                                inv.GetInventory().TransferItemTo(target_inventories[0].GetInventory(), item, amount_to_transfer);

                            }
                        }

                    }
                }
            }
        }


        private MyFixedPoint Get_Missing_Amount(string TypeId, string SubtypeId, List<MyInventoryItem> items_in_cargo, MyFixedPoint amount)
        {
            foreach (MyInventoryItem i in items_in_cargo)
            {
                if (i.Type.TypeId.ToString() == TypeId && i.Type.SubtypeId == SubtypeId && i.Amount >= amount)

                {
                    Echo("Item already in target inventory");
                    return MyFixedPoint.Zero;
                }
                else if (i.Type.TypeId.ToString() == TypeId && i.Type.SubtypeId == SubtypeId && i.Amount < amount)
                {

                    return amount - i.Amount;
                }
            }
            Echo("no item found in target inventory");
            return amount;
        }
       private void WriteMissingOres_ingots(string targetLCD, List<IMyInventoryItem> items_in_cargo, string name)
        {
            List<IMyTerminalBlock> l = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName(targetLCD, l);
            IMyTextPanel lcd = l[0] as IMyTextPanel;

        }
    }
}
