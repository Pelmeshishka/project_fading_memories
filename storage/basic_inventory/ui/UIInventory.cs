using Godot;
using System;
using System.Collections.Generic;

public partial class UIInventory : Control
{
    [Export] public bool is_draggable { get; private set; }  = true;
    [Export] protected Control panel;
    [Export] protected GridContainer container;
    

    public List<UIInventorySlot> ui_slots = new();

    protected PackedScene ui_slot_packed = ResourceLoader.Load<PackedScene>("storage/basic_inventory/ui/ui_inventory_slot.tscn");

    public Inventory inventory { get; private set; }
    private bool initialized = false;

    public virtual void Initial()
    {
        if (container is not null)
        {
            foreach (var node in container.GetChildren())
            {
                container.RemoveChild(node);
                node.QueueFree();
            }
        }
        initialized = true;
    }

    public override void _Ready()
    {
        base._Ready();
        if (!initialized)
        {
            this.Initial();
        }
    }

    public virtual void ConnectInventory(Inventory inventory)
    {
        this.inventory = inventory;
        while(ui_slots.Count > 0)
        {
            UIInventorySlot ui_slot = ui_slots[0];

            ui_slot.DisconnectSlot();
            ui_slots.Remove(ui_slot);
            container.RemoveChild(ui_slot);
            ui_slot.QueueFree();
        } 
        /*for (int i = 0; i < ui_slots.Count; i++)
        {
            UIInventorySlot ui_slot = ui_slots[0];

            ui_slot.DisconnectSlot();
            ui_slot.SlotOnFocusEnter -= SlotOnFocusEnter; ui_slot.SlotOnFocusExit -= SlotOnFocusExit;
            ui_slots.Remove(ui_slot);
            container.RemoveChild(ui_slot);
            ui_slot.QueueFree();
        }*/


        if (inventory is not null && container is not null)
        { 
            foreach (var slot in inventory.slots)
            {
                UIInventorySlot ui_slot = ui_slot_packed.Instantiate<UIInventorySlot>();
                ui_slot.Name = slot.Name;
                ui_slot.ConnectSlot(slot);
                ui_slots.Add(ui_slot);
                container.AddChild(ui_slot, true);
            }
        }
    }

    private bool isDragged = false;
    private Vector2 drag_offset;
    public void OnGUIInput(InputEvent @event)
    {
        if (is_draggable && @event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                isDragged = true;
                drag_offset = this.GlobalPosition-mb.GlobalPosition;
                Vector2 newPos = mb.GlobalPosition + drag_offset;
                newPos.Y = Mathf.Clamp(newPos.Y, 0, DisplayServer.WindowGetSize().Y - this.Size.Y);
                newPos.X = Mathf.Clamp(newPos.X, 0, DisplayServer.WindowGetSize().X - this.Size.X);
                this.GlobalPosition = newPos;
                this.MoveToFront();
            }
            else if (mb.IsReleased())
            {
                isDragged = false;
            }
        }

    }

    public override void _Input(InputEvent @event)
    {
        if (isDragged && @event is InputEventMouseMotion motion)
        {
            Vector2 newPos = motion.GlobalPosition + drag_offset;
            newPos.Y = Mathf.Clamp(newPos.Y, 0, DisplayServer.WindowGetSize().Y - this.Size.Y);
            newPos.X = Mathf.Clamp(newPos.X, 0, DisplayServer.WindowGetSize().X - this.Size.X);
            this.GlobalPosition = newPos;
        }
    }

    public virtual void HideInventory()
    {
        this.Visible = false;
    }

    public virtual void ShowInventory()
    {
        this.Visible = true;
    }
}
