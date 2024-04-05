using Godot;
using System;
using System.Data.Common;

public partial class Item : Resource
{
    [Export] public Texture2D icon;
    [Export] public string identifier;
    [Export] public int max_count = 1;
    [Export] public int count = 1;

    [Signal] public delegate void OnItemUsedEventHandler();

    public virtual void Use()
    {
        EmitSignal(SignalName.OnItemUsed);
    }

}
