using Godot;
using System;

public partial class HumanoidBackSlot : InventorySlot
{
    public override bool IsCanContainItem(Item item)
    {
        return item is null || item is Backpack;
    }
}
