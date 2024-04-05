using Godot;
using System;

public partial class TestThrowRigidbody : Node3D
{
	[Export] RigidBody3D body;


    public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("drop_item"))
		{
			body.Position = Vector3.Zero;
            body.ApplyForce(this.GlobalBasis.Z * 5000f);
        }
	}
}
