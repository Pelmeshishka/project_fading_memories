using Godot;
using System;

public partial class UIInventorySlot : Control
{
	[Export] protected bool canOpenInventory = true;
	[Export] public TextureRect icon;
	[Export] public Label count;

	public InventorySlot slot { get; protected set; }
	
    [Signal] public delegate void OnNewItemSetEventHandler();


    public void Update()
	{
		if (slot is null || slot.item is null)
		{
			icon.Visible = false;
			count.Visible = false;
		}
		else
		{
			icon.Visible = true;
			icon.Texture = slot.item.icon;
			if (slot.item.max_count > 1)
			{
				count.Text = slot.item.count.ToString();
				count.Visible = true;
			} 
			else
			{
                count.Visible = false;
            }
        }
	}

	public void ConnectSlot(InventorySlot slot)
	{
        if (this.slot is not null)
        {
            this.slot.OnNewItemSet -= NewItemSet;
            this.slot.OnDropItem -= OnDropItem;
            this.slot.OnItemUpdated -= Update;
        }

        this.slot = slot;
        this.slot.OnNewItemSet += NewItemSet;
        this.slot.OnDropItem += OnDropItem;
        this.slot.OnItemUpdated += Update;
        Update();
	}

    public void DisconnectSlot()
    {
		if (this.slot is not null)
		{
            this.slot.OnNewItemSet -= NewItemSet;
            this.slot.OnDropItem -= OnDropItem;
            this.slot.OnItemUpdated -= Update;
            this.slot = null;
            Update();
        }
    }

	private void NewItemSet()
	{
		EmitSignal(SignalName.OnNewItemSet);
		Update();
	}

    public virtual void TryUseItem()
    {
		if (canOpenInventory && slot.inventory is not null)
		{
			if (!UIPlayerGUI.TryRemoveInventory(slot.inventory))
			{
                UIPlayerGUI.AddInventory(slot.inventory);
            }
		}
		else
		{
			slot.TryUseItem();
		}
    }


    public void OnGUIInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton b)
		{
			if (b.ButtonIndex == MouseButton.Left)
			{
				if (b.Pressed)
				{
                    UIPlayerGUI.DaDSlot.TryDragSlot(slot);
                }
                else if (b.IsReleased())
				{
                    UIPlayerGUI.DaDSlot.TryDropSlot();
                }
			}
        }
    }

	public void OnDropItem(Item item, Inventory inventory)
	{
		if (inventory is not null)
		{
            UIPlayerGUI.TryRemoveInventory(inventory);
        }
    }

	

	public void OnMouseEnter()
	{
        UIPlayerGUI.DaDSlot.focused_slot = this;
	}

	public void OnMouseExit()
	{
        if (slot == UIPlayerGUI.DaDSlot.focused_slot.slot)
		{
            UIPlayerGUI.DaDSlot.focused_slot = null;
        }
	}

}
