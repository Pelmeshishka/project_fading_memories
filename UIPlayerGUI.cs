using Godot;
using Godot.Collections;
using System;
using System.Runtime.CompilerServices;

public partial class UIPlayerGUI : Control
{
	[Export] private Control inventory_container;
	[Export] private UIDaDSlot ui_dad_slot;
	public static UIDaDSlot DaDSlot => instance.ui_dad_slot;

    private static UIPlayerGUI instance;

	private static Dictionary<Inventory, UIInventory> ui_inventories = new();

    public static bool isMainInventoryOpened = false;

    public override void _EnterTree()
    {
		instance = this;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    

	public static bool TryGetUIInventory(Inventory inventory, out UIInventory ui_inventory)
	{
        return ui_inventories.TryGetValue(inventory, out ui_inventory);
    }

	

	public static void SwitchMainInventory()
	{
        if (isMainInventoryOpened)
        {
            foreach(var node in instance.inventory_container.GetChildren())
            {
                if (node is UIInventory ui_inventory)
                {
                    ui_inventory.HideInventory();
                }
            }
            isMainInventoryOpened = false;
            DaDSlot.HideSlot();
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        else
        {
            foreach (var node in instance.inventory_container.GetChildren())
            {
                if (node is UIInventory ui_inventory)
                {
                    ui_inventory.ShowInventory();
                }
            }
            isMainInventoryOpened = true;
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }

    public static void AddUIInventory(UIInventory ui_inventory)
    {
        if (ui_inventory.GetParent() is not null)
        {
            ui_inventory.Reparent(instance.inventory_container);
        }
        else
        {
            instance.inventory_container.AddChild(ui_inventory, true);
        }

        if (ui_inventory.is_draggable)
        {
            ui_inventory.GlobalPosition = DisplayServer.WindowGetSize() / 2 - ui_inventory.Size / 2;
        }
        ui_inventories.Add(ui_inventory.inventory, ui_inventory);
        if (isMainInventoryOpened)
        {
            ui_inventory.ShowInventory();
        }
        else
        {
            ui_inventory.HideInventory();
        }
    }


    public static void AddInventory(Inventory inventory)
	{
		string ui_inventory_path = "";
        if (inventory is HumanoidInventory)
        {
            ui_inventory_path = "storage/humanoid_inventory/ui/ui_humanoid_inventory.tscn";
        }
		else
		{
            ui_inventory_path = "storage/basic_inventory/ui/ui_inventory.tscn";
        }

		if (string.IsNullOrEmpty(ui_inventory_path))
		{
            GD.PrintErr("Cannot add inventory: ", inventory);
        }
		else
		{
            UIInventory ui_inventory = ResourceLoader.Load<PackedScene>(ui_inventory_path).Instantiate<UIInventory>();
            ui_inventory.Initial();
            ui_inventory.ConnectInventory(inventory);
            
            instance.inventory_container.AddChild(ui_inventory, true);
            if (ui_inventory.is_draggable)
            {
                ui_inventory.GlobalPosition = DisplayServer.WindowGetSize() / 2 - ui_inventory.Size / 2;
            }
            ui_inventories.Add(inventory, ui_inventory);

            if (isMainInventoryOpened)
            {
                ui_inventory.ShowInventory();
            }
            else
            {
                ui_inventory.HideInventory();
            }
        }
    }

	public static bool TryRemoveInventory(Inventory inventory)
	{
        if (ui_inventories.TryGetValue(inventory, out UIInventory ui_inventory))
        {
            instance.inventory_container.RemoveChild(ui_inventory);
            ui_inventory.ConnectInventory(null);
            ui_inventories.Remove(inventory);
            CloseAllInventories(inventory);
            return true;
        }

        return false;
	}

    private static void CloseAllInventories(Inventory inventory)
    {
        foreach (var child_slot in inventory.slots)
        {
            if (child_slot.inventory is not null)
            {
                TryRemoveInventory(child_slot.inventory);
            }
        }
    }

}
