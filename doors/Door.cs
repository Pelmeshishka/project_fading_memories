using Godot;
using System;

public partial class Door : GrabbableObject
{
    public bool closed = false;
    [Export] public bool locked = false;
    [Export] protected string key_identifier = "training_key";

    public override void _Ready()
    {
        base._Ready();
        if (locked)
        {
            closed = true;
            this.Freeze = true;
        }
        else
        {
            this.Freeze = false;
        }
    }

    public virtual bool TryUseItem(Item item)
    {
        if (item is IKey && item.identifier == key_identifier)
        {
            if (locked)
            {
                Unlock();
            } 
            else if (closed)
            {
                Lock();
            }

            return true;
        }

        return false;
    }

    public virtual void Lock()
    {
        Close();
        Freeze = true;
        LockRotation = true;
        locked = true;
    }

    public virtual void Unlock()
    {
        locked = false;
        LockRotation = false;
        Freeze = false;
        Close();
    }

    protected virtual void Close()
    {
        closed = true;
    }

    protected virtual void Open()
    {
        closed = false;
    }
}
