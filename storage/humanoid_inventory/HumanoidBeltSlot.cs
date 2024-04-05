using Godot;
using System;

public partial class HumanoidBeltSlot : InventorySlot
{
    public override bool IsCanContainItem(Item item)
    {
        return item is null || item is Beltpack;
    }
}
