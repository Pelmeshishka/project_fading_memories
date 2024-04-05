using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

public partial class HumanoidInventory : Inventory
{
    
    [Export] public HoldingSlot left_hand_slot { get; protected set; }
    [Export] public HoldingSlot right_hand_slot { get; protected set; }

    [Export] public InventorySlot helmet_slot { get; protected set; }
    [Export] public InventorySlot chest_slot { get; protected set; }
    [Export] public InventorySlot pants_slot { get; protected set; }
    [Export] public InventorySlot gloves_slot { get; protected set; }
    [Export] public InventorySlot boots_slot { get; protected set; }

    [Export] public HumanoidBackSlot back_slot { get; protected set; }
    [Export] public HumanoidBeltSlot belt_slot { get; protected set; }

    public override void _Ready()
    {
        base._Ready();
    }

    public override bool TryAddItem(in InventorySlot slot)
    {
        int start_count = slot.item.count;
        right_hand_slot.TryAddItem(slot);

        if (slot.item.count > 0)
        {
            left_hand_slot.TryAddItem(slot);
        }

        if (slot.item.count > 0 && belt_slot.inventory is not null)
        {
            belt_slot.inventory.TryAddItem(slot);
        }

        if (slot.item.count > 0 && back_slot.inventory is not null)
        {
            back_slot.inventory.TryAddItem(slot);
        }

        return slot.item.count != start_count;
    }

}
