using Godot;
using System;
using System.Collections.Generic;

public partial class InventorySlot : Node
{
    private Item _item;

    [Export] public Item item { get { return _item; } 
        set { 
            if (_item is not null)
            {
                _item.OnItemUsed -= OnItemUsed;
            }
            _item = value;
            if (_item is not null)
            {
                _item.OnItemUsed += OnItemUsed;
            }
            EmitSignal(SignalName.OnNewItemSet); 
        } }
    [Export] public Inventory inventory;
     
    [Signal] public delegate void OnNewItemSetEventHandler();
    [Signal] public delegate void OnDropItemEventHandler(Item item, Inventory inventory);
    [Signal] public delegate void OnItemUpdatedEventHandler();

    public virtual bool IsCanContainItem(Item item)
    {
        return true;
    }

    public bool TryAddItem(in InventorySlot slot)
    {
        if (item is not null)
        {
            if (!IsCanContainItem(slot.item))
            {
                return false;
            }

            if (item.identifier != slot.item.identifier)
            {
                return false;
            }

            if (item.count > item.max_count)
            {
                return false;
            }

            if (inventory is not null || slot.inventory is not null)
            {
                return false;
            }

            if (item.count + slot.item.count <= item.max_count)
            {
                item.count += slot.item.count;
                slot.item.count = 0;
            } 
            else
            {
                int remains = (item.count + slot.item.count) - item.max_count;
                item.count += slot.item.count - remains;
                slot.item.count = remains;
            }
            EmitSignal(SignalName.OnItemUpdated);
            return true;
        }
        else
        {
            if (!IsCanContainItem(slot.item))
            {
                return false;
            }

            inventory = slot.inventory;
            item = (Item)slot.item.Duplicate();
            
            if (slot.inventory is not null)
            {
                slot.inventory.Reparent(this);
            }

            slot.inventory = null;
            slot.item.count = 0;
            
            return true;
        }
    }

    public virtual void TryUseItem()
    {
        if (item is not null)
        {
            item.Use();
        }
    }

    protected virtual void OnItemUsed()
    {
        if (item.count <= 0)
        {
            if (inventory is not null)
            {
                this.RemoveChild(inventory);
                inventory.QueueFree();
            }
            inventory = null;
            item = null;
        }
        else
        {
            EmitSignal(SignalName.OnItemUpdated);
        }
    }


    public virtual void DropItem(Vector3 position, Subworld subworld, Vector3 rotation)
    {
        if (item is null)
        {
            return;
        }

        if (!ResourceLoader.Exists($"items/{item.identifier}/{item.identifier}.tscn"))
        {
            GD.PrintErr($"Item body for item {item.identifier} doesnt exists");
            return;
        }

        ItemBody item_body = ResourceLoader.Load<PackedScene>($"items/{item.identifier}/{item.identifier}.tscn").Instantiate<ItemBody>();

        if (item_body.slot.inventory is not null)
        {
            item_body.RemoveChild(item_body.slot.inventory);
        }
        item_body.slot.inventory = inventory;
        if (inventory is not null)
        {
            inventory.Reparent(item_body);
        }
        inventory = null;

        item_body.slot.item = item;
        item = null;

        item_body.RotationDegrees = rotation;
        item_body.Position = position;
        subworld.SpawnItemBody(item_body);
        EmitSignal(SignalName.OnDropItem, item_body.slot.item, item_body.slot.inventory);
    }

}
