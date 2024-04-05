using Godot;
using System;

public partial class FlashlightBody : ItemBody
{
    [Export] private SpotLight3D spot_light;
    Flashlight flashlight_item;

    public override void _Ready()
    {
        if (slot.item is Flashlight fl)
        {
            flashlight_item = fl;
            spot_light.Visible = flashlight_item.isActive;
        }
        base._Ready();
    }

    protected override void OnItemUpdated()
    {
        spot_light.Visible = flashlight_item.isActive;
        base.OnItemUpdated();
    }
}
