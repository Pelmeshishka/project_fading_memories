using Godot;
using System;

public partial class Flashlight : Item
{
    [Export] public bool isActive { get; private set; } = false;

    public override void Use()
    {
        isActive = !isActive;
        base.Use();
    }
}
