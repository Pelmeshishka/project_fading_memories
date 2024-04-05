using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : Node
{
    [Export] protected int slots_count = 10;
    public List<InventorySlot> slots = new();
    protected PackedScene slot_packed = ResourceLoader.Load<PackedScene>("storage/basic_inventory/inventory_slot.tscn");

    public override void _Ready()
    {
        for (int i = 0; i < slots_count; i++)
        {
            InventorySlot slot = slot_packed.Instantiate<InventorySlot>();
            slot.Name = i.ToString();
            slots.Add(slot);
            this.AddChild(slot, true);
        }
    }

    public virtual bool TryAddItem(in InventorySlot item_slot)
    {
        int start_count = item_slot.item.count;
        foreach(var slot in slots)
        {
            slot.TryAddItem(item_slot);
            if (item_slot.item.count <= 0)
            {
                break;
            }
        }

        return item_slot.item.count != start_count;
    }

    public static bool TrySwapSlots(InventorySlot slot_1, InventorySlot slot_2)
    {
        if (!slot_1.IsCanContainItem(slot_2.item) || !slot_2.IsCanContainItem(slot_1.item))
        {
            return false;
        }

        
        if (slot_1.FindParent(slot_2.Name) is Node n1 && n1 == slot_2)
        {
            return false;
        }

        if (slot_2.FindParent(slot_1.Name) is Node n2 && n2 == slot_1)
        {
            return false;
        }

        Item item_1 = slot_1.item;
        Inventory inventory_1 = slot_1.inventory;

        Item item_2 = slot_2.item;
        Inventory inventory_2 = slot_2.inventory;

        if (slot_1.inventory is not null)
        {
            slot_1.inventory.Reparent(slot_2);
        }
        slot_1.inventory = inventory_2;
        slot_1.item = item_2;


        if (slot_2.inventory is not null)
        {
            slot_2.inventory.Reparent(slot_1);
        }
        slot_2.inventory = inventory_1;
        slot_2.item = item_1;


        return true;
    }

}
