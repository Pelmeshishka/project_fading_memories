using Godot;
using System;
using System.Net.Http;
using static Godot.HttpRequest;

public partial class Entity : CharacterBody3D
{
    [ExportGroup("Stats")]
    [Export] public EntityStats stats { get; protected set; }

    [ExportGroup("Head")]
    [Export] protected Node3D headY;
    [Export] protected Node3D headX;
    [Export] protected Camera3D head;

    [ExportGroup("Inventory")]
    [Export] protected Inventory inventory;

    [ExportGroup("Movement")]
    [Export] protected float walk_speed = 5.0f;
    [Export] protected float spring_ratio = 1.5f;
    protected float speed;
    protected bool is_sprinting = false;

	public float defaultGravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public Vector3 look_direction => -head.GlobalBasis.Z;
	public Node look_at_node;
    public Vector3 look_at_point;
    public Vector3 look_at_normal;

    protected Subworld subworld;

    public override void _Ready()
    {
		subworld = this.GetParent().GetParent().GetParent().GetParent<Subworld>();
    }

    public virtual float GetSpeed()
    {
        return is_sprinting ? walk_speed * stats.speedRatio * spring_ratio : walk_speed * stats.speedRatio;
    }

    public override void _PhysicsProcess(double delta)
    {
        speed = GetSpeed();
        Vector3 look_point = head.GlobalPosition + look_direction * 4f;
        Godot.Collections.Dictionary look_at = GetWorld3D().DirectSpaceState.IntersectRay(PhysicsRayQueryParameters3D.Create(head.GlobalPosition, look_point));
		if (look_at.Count > 0)
		{
			look_at_node = (Node)look_at["collider"];
			look_at_point = (Vector3)look_at["position"];
            look_at_normal = (Vector3)look_at["normal"];
        }
		else
		{
			look_at_node = null;
			look_at_point = look_point;
            look_at_normal = Vector3.Zero;
        }
    }

}
