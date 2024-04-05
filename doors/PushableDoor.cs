using Godot;
using System;

public partial class PushableDoor : Door
{
    [Export] protected Label3D label1;
    [Export] protected Label3D label2;

    [Export] public Node3D grab_point; 

    private bool full_open = false;

    [Export] protected Vector3 closed_pos;
    [Export] protected Vector3 hinge_pos;

    public override void _Ready()
    {
        base._Ready();
        /*if (locked)
        {
            this.LinearVelocity = Vector3.Zero;
            this.AngularVelocity = Vector3.Zero;
            this.RotationDegrees = Vector3.Zero;
            label1.Text = "locked";
            label2.Text = "locked";
        }
        else
        {
            this.LinearVelocity = Vector3.Zero;
            this.AngularVelocity = Vector3.Zero;
            this.RotationDegrees = Vector3.Zero;
            closed = true;
            label1.Text = "closed";
            label2.Text = "closed";
        } */
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!locked)
        {
            
            if (Mathf.Abs(this.RotationDegrees.Y) > 1.5f && closed)
            {
                Open();
            }
            else if (Mathf.Abs(this.RotationDegrees.Y) <= 1.5f && !closed)
            {
                Close();
            }

            if (Mathf.Abs(this.RotationDegrees.Y) >= 88.5f)
            {
                if (!full_open)
                {
                    full_open = true;
                    this.Freeze = true;
                    this.LinearVelocity = Vector3.Zero;
                    this.AngularVelocity = Vector3.Zero;
                    this.RotationDegrees = this.Basis.Y * (this.RotationDegrees.Y > 0 ? 90 : -90);
                    this.Position = (-hinge_pos + closed_pos).Rotated(this.Basis.Y, Mathf.DegToRad(this.RotationDegrees.Y > 0 ? 90 : -90)) + hinge_pos;
                    this.Freeze = false;
                }
            }
            else {
                full_open = false;
            }
        }
        
    }

    public override bool TryUseItem(Item item)
    {
        if (base.TryUseItem(item))
        {
            if (locked)
            {
                label1.Text = "locked";
                label2.Text = "locked";
            }
            else if (Mathf.Abs(RotationDegrees.Y) > 1f )
            {
                label1.Text = "open";
                label2.Text = "open";
            }
            else
            {
                label1.Text = "closed";
                label2.Text = "closed";
            }

            return true;
        }

        return false;
    }

    public override void Lock()
    {
        base.Lock();
    }

    public override void Unlock()
    {
        base.Unlock();
    }

    protected override void Close()
    {
        base.Close();
        this.Freeze = true;
        this.LinearVelocity = Vector3.Zero;
        this.AngularVelocity = Vector3.Zero;
        this.RotationDegrees = Vector3.Zero;
        this.Position = closed_pos;
        this.Freeze = false;
        label1.Text = "closed";
        label2.Text = "closed";
    }

    protected override void Open()
    {
        base.Open();
        label1.Text = "open";
        label2.Text = "open";
    }
}
