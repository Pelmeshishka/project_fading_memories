using Godot;
using System;
using System.Data;
using System.Threading.Tasks;

public partial class UIHumanoidInventory : UIInventory
{
    private Vector2 fast_plane_target;
	[Export] public Control fast_plane;
	[Export] public UIInventorySlot left_hand_slot;
    [Export] public UIInventorySlot right_hand_slot;
    [Export] public UIInventory belt_inventory;

    private Vector2 equipment_plane_target;
    [Export] public Control equipment_plane;
    [Export] public UIInventorySlot helmet_slot;
    [Export] public UIInventorySlot chest_slot;
    [Export] public UIInventorySlot pants_slot;
    [Export] public UIInventorySlot gloves_slot;
    [Export] public UIInventorySlot boots_slot;
    [Export] public UIInventorySlot back_slot;
    [Export] public UIInventorySlot belt_slot;

    public override void Initial()
    {
        fast_plane_target = fast_plane.GlobalPosition;
        equipment_plane_target = equipment_plane.GlobalPosition;
        belt_slot.OnNewItemSet += BeltInventoryToggle;
        base.Initial();
    }

    public override void ConnectInventory(Inventory inventory)
    {
        base.ConnectInventory(inventory);
        if (inventory is not HumanoidInventory humanoidInventory)
        {
            return;
        }

        left_hand_slot.ConnectSlot(humanoidInventory.left_hand_slot);
        right_hand_slot.ConnectSlot(humanoidInventory.right_hand_slot);
        helmet_slot.ConnectSlot(humanoidInventory.helmet_slot);
        chest_slot.ConnectSlot(humanoidInventory.chest_slot);
        pants_slot.ConnectSlot(humanoidInventory.pants_slot);
        gloves_slot.ConnectSlot(humanoidInventory.gloves_slot);
        boots_slot.ConnectSlot(humanoidInventory.boots_slot);
        back_slot.ConnectSlot(humanoidInventory.back_slot);
        belt_slot.ConnectSlot(humanoidInventory.belt_slot); BeltInventoryToggle();
    }

    public void BeltInventoryToggle()
    {
        if (belt_slot.slot.inventory is null)
        {
            belt_inventory.Visible = false;
            belt_inventory.ConnectInventory(null);
            
        }
        else
        {
            belt_inventory.ConnectInventory(belt_slot.slot.inventory);
            belt_inventory.Visible = true;
            if (UIPlayerGUI.TryGetUIInventory(belt_slot.slot.inventory, out UIInventory ui_inventory))
            {
                ui_inventory.Visible = false;
            }
            
        }
    }


    Tween ui_tween;

    public void ShowFastPlane()
    {
        /*if (ui_tween is not null)
        {
            ui_tween.Kill();
        }
        ui_tween = GetTree().CreateTween().SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In);
        ui_tween.TweenProperty(fast_plane, "global_position", fast_plane_target, 0.4f);
        ui_tween.TweenProperty(fast_plane, "global_position", fast_plane_target - Vector2.Up * fast_plane.Size.Y, 0.4f).SetDelay(2);*/
    }

    public override void HideInventory()
    {
        /*fast_plane_timer.Stop();
        if (ui_tween is not null)
        {
            ui_tween.Kill();
        }
        ui_tween = GetTree().CreateTween().SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In).SetParallel(true);
        ui_tween.TweenProperty(fast_plane, "global_position", fast_plane_target - Vector2.Up * fast_plane.Size.Y, 0.4f);
        ui_tween.TweenProperty(equipment_plane, "global_position", equipment_plane_target - Vector2.Right * equipment_plane.Size.X, 0.4f);
        ui_tween.Finished += () =>
        {
            this.Visible = false;
        };*/
        this.Visible = false;
    }

    public override void ShowInventory()
    {
        this.Visible = true;
        /*if (ui_tween is not null)
        {
            ui_tween.Kill();
        }
        ui_tween = GetTree().CreateTween().SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In).SetParallel(true);
        ui_tween.TweenProperty(fast_plane, "global_position", fast_plane_target, 0.4f);
        ui_tween.TweenProperty(equipment_plane, "global_position", equipment_plane_target, 0.4f);*/
    }
}
