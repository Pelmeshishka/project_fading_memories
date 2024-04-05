using Godot;
using System;

public partial class HoldingSlot : InventorySlot
{
    [Export] private Node3D holding_container;

    public override void _Ready()
    {
        base._Ready();
        this.OnNewItemSet += NewItemSet;
    }

    private void NewItemSet()
    {
        foreach (var node in holding_container.GetChildren())
        {
            holding_container.RemoveChild(node);
            if (node is ItemBody ib)
            {
                ib.Remove();
            }
            else
            {
                node.QueueFree();
            }
        }

        if (item is null)
        {
            return;
        }

        if (!ResourceLoader.Exists($"items/{item.identifier}/{item.identifier}.tscn"))
        {
            GD.PrintErr($"Try spawn item in holding slot. Item body for item {item.identifier} doesnt exists");
            return;
        }

        ItemBody item_body = ResourceLoader.Load<PackedScene>($"items/{item.identifier}/{item.identifier}.tscn").Instantiate<ItemBody>();
        item_body.CollisionLayer = 0;
        item_body.CollisionMask = 0;
        item_body.Freeze = true;
        item_body.LockRotation = true;
        if (item_body.slot.inventory is not null)
        {
            item_body.RemoveChild(item_body.slot.inventory);
            item_body.slot.inventory.QueueFree();
        }
        item_body.RemoveChild(item_body.slot);
        item_body.slot.QueueFree();
        item_body.slot = this;
        holding_container.AddChild(item_body);
        item_body.Position = Vector3.Zero;
        item_body.RotationDegrees = Vector3.Zero;
    }

    public override void DropItem(Vector3 position, Subworld subworld, Vector3 rotation)
    {
        rotation = holding_container.GlobalRotationDegrees;
        base.DropItem(position, subworld, rotation);
    }
}
