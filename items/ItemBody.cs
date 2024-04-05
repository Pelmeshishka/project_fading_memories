using Godot;
using System;

public partial class ItemBody : GrabbableObject
{
    [Export] public InventorySlot slot;

    object lockobj = new object();

    public override void _Ready()
    {
        base._Ready();
        slot.OnNewItemSet += OnNewItemSet;
        slot.OnItemUpdated += OnItemUpdated;
    }

    protected virtual void OnNewItemSet()
    {
        if (slot.item is null)
        {
            Remove();
        }
    }

    protected virtual void OnItemUpdated()
    {
        
    }

    public bool TryPickUp(Inventory inventory)
    {
        lock (lockobj)
        {
            bool result = inventory.TryAddItem(in slot);
            if (slot.item.count <= 0)
            {
                Remove();
            }
            return result;
        }
    }

    public void Remove()
    {
        slot.OnNewItemSet -= OnNewItemSet;
        slot.OnItemUpdated -= OnItemUpdated;
        this.QueueFree();
    }

}
