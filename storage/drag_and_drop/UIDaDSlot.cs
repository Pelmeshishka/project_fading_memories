using Godot;
using System;

public partial class UIDaDSlot : UIInventorySlot
{
    public UIInventorySlot focused_slot;
    public override void _Ready()
    {
        this.Visible = false;
        base._Ready();
    }

    public void TryDragSlot(InventorySlot dragged_slot)
    {
        if (dragged_slot is null || dragged_slot.item is null)
        {
            return;
        }

        this.slot = dragged_slot;
        this.Update();
        this.Visible = true;
    }

    public void HideSlot()
    {
        this.slot = null;
        this.Update();
        this.Visible = false;
    }

    public void TryDropSlot()
    {
        if (this.slot is not null && focused_slot is not null && focused_slot.slot != this.slot)
        {
            Inventory.TrySwapSlots(this.slot, focused_slot.slot);
        }
        HideSlot();
    }

    /*public override void TryUseItem()
    {
        if (this.focused_slot is not null && this.focused_slot.slot.item is not null)
        {
            if (this.focused_slot.slot.inventory is not null && UIPlayerGUI.TryGetUIInventory(this.focused_slot.slot.inventory, out UIInventory ui_inventory))
            {
                ui_inventory.Visible = !ui_inventory.Visible;
            }
        }
    }*/

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            this.Position = motion.Position;
        }
    }


}
