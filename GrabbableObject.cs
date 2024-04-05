using Godot;
using System;

public partial class GrabbableObject : RigidBody3D
{
    [Export] public bool lock_position = false;
    public override void _Ready()
    {
        if (lock_position)
        {
            this.AxisLockLinearX = lock_position;
            this.AxisLockLinearY = lock_position;
            this.AxisLockLinearZ = lock_position;
        }
    }
}
