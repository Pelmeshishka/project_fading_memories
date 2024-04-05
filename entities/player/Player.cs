using Godot;
using System;

public partial class Player : Entity
{
    [Export] protected CollisionShape3D coll;
    private CapsuleShape3D shape => (CapsuleShape3D)coll.Shape;

    [ExportGroup("Stand")]
    [Export] protected float stand_height;
    [Export] protected Vector3 headY_stand_pos;

    [ExportGroup("Crouch")]
    protected bool is_crouching = false;
    [Export] protected float crouch_height;
    [Export] protected Vector3 headY_crouch_pos;

    [ExportGroup("Movement")]
    [Export] protected float jumpVelocity = 5;
    [Export] float pushForce = 12f;
    [Export] float mouseSensitivity = 20f;
    private Vector2 rotDir;
    [Export] private float crouch_ratio = 0.5f;

    [ExportGroup("Grab Objects")]
    [Export] private Label label_rot_X;
    protected bool ungrab = false;
    protected GrabbableObject grabbed_object;
    protected Vector3 grab_position;
    private bool rot_grabbed_object = false;
    private bool rot_X_inverted = false;
    

    [ExportGroup("Climb Cast")]
    [Export] private RayCast3D climb_cast;
    [Export] private RayCast3D climb_cast_side;
    [Export] private RayCast3D wall_cast;
    [Export] private ShapeCast3D stand_shape_cast;

    [ExportGroup("Inventory")]
    [Export] private UIHumanoidInventory ui_hum_inventory;

    private HumanoidInventory hum_inventory;

    public override void _Ready()
    {
        base._Ready();

        if (inventory is HumanoidInventory hum_inventory)
        {
            this.hum_inventory = hum_inventory;
            ui_hum_inventory.Initial();
            ui_hum_inventory.ConnectInventory(hum_inventory);
        }

        climb_cast.CallDeferred("reparent", subworld.other_container);
        climb_cast_side.CallDeferred("reparent", subworld.other_container);
        wall_cast.CallDeferred("reparent", subworld.other_container);
        stand_shape_cast.CallDeferred("reparent", subworld.other_container);

        label_rot_X.Text = rot_X_inverted ? "Z" : "Y";
        label_rot_X.Visible = false;

        UIPlayerGUI.AddUIInventory(ui_hum_inventory);
    }

    public override float GetSpeed()
    {
        return is_crouching ? base.GetSpeed() * crouch_ratio : base.GetSpeed();
    }
    protected virtual void SetStand()
    {
        CapsuleShape3D cs = (CapsuleShape3D)stand_shape_cast.Shape;
        cs.Height = stand_height;
        stand_shape_cast.GlobalRotation = this.GlobalRotation;
        stand_shape_cast.GlobalPosition = this.GlobalPosition + this.GlobalBasis.Y * (stand_height / 2 + 0.01f);
        stand_shape_cast.ForceShapecastUpdate();
        if (stand_shape_cast.IsColliding())
        {
            return;
        }

        is_crouching = false;
        shape.Height = stand_height;
        coll.Position = Vector3.Up * shape.Height / 2;
        headY.Position = headY_stand_pos;
        climb_cast.TargetPosition = -Vector3.Up * stand_height;

    }

    protected virtual void SetCrouch()
    {
        is_crouching = true;
        shape.Height = crouch_height;
        coll.Position = Vector3.Up * shape.Height / 2;
        headY.Position = headY_crouch_pos;
        climb_cast.TargetPosition = -Vector3.Up * crouch_height;
    }


    public override void _Process(double delta)
    {
        float fdelta = (float)delta;

        if (is_climbing_animation)
        {
            return;
        }
        else if (ladder is not null)
        {
            LadderProcess(fdelta);
        }
        else
        {
            BasicProcess(fdelta);
        }

        is_sprinting = Input.IsActionPressed("sprint");

        if (Input.IsActionJustPressed("invert_rotate"))
        {
            rot_X_inverted = !rot_X_inverted;
            label_rot_X.Text = rot_X_inverted ? "Z" : "Y";
        }

        if (Input.IsActionJustPressed("jump") && !UIPlayerGUI.isMainInventoryOpened)
        {
            is_jumping = true;
        }

        if (Input.IsActionJustPressed("inventory"))
        {
            UIPlayerGUI.SwitchMainInventory();
        }

        if (Input.IsActionJustPressed("interact"))
        {
            if (UIPlayerGUI.isMainInventoryOpened)
            {
                if (UIPlayerGUI.DaDSlot.focused_slot is not null)
                {
                    UIPlayerGUI.DaDSlot.focused_slot.TryUseItem();
                }
            }
            else if (look_at_node is ItemBody ib)
            {
                Callable func = Callable.From(() => { ui_hum_inventory.ShowFastPlane(); });
                hum_inventory.left_hand_slot.Connect(InventorySlot.SignalName.OnNewItemSet, func);
                hum_inventory.right_hand_slot.Connect(InventorySlot.SignalName.OnNewItemSet, func);
                if(hum_inventory.belt_slot.inventory is not null)
                {
                    foreach(var slot in hum_inventory.belt_slot.inventory.slots)
                    {
                        slot.Connect(InventorySlot.SignalName.OnNewItemSet, func);
                    }
                }

                ib.TryPickUp(inventory);

                hum_inventory.left_hand_slot.Disconnect(InventorySlot.SignalName.OnNewItemSet, func);
                hum_inventory.right_hand_slot.Disconnect(InventorySlot.SignalName.OnNewItemSet, func);
                if (hum_inventory.belt_slot.inventory is not null)
                {
                    foreach (var slot in hum_inventory.belt_slot.inventory.slots)
                    {
                        slot.Disconnect(InventorySlot.SignalName.OnNewItemSet, func);
                    }
                }
            }
            else if (look_at_node is Door dr && !dr.locked)
            {
                if (dr.closed)
                {
                    dr.LinearVelocity = Vector3.Zero;
                    dr.AngularVelocity = Vector3.Zero;
                    dr.ApplyImpulse(-dr.GlobalBasis.Z * pushForce* pushForce * fdelta);
                }
                else
                {
                    dr.LinearVelocity = Vector3.Zero;
                    dr.AngularVelocity = Vector3.Zero;
                    dr.ApplyImpulse(dr.GlobalBasis.Z * pushForce * pushForce * fdelta);
                }
            }
        
        }

        if (Input.IsActionJustPressed("drop_item"))
        {
            Vector3 drop_pos = head.GlobalPosition - head.GlobalBasis.Z;
            if (ladder is not null)
            {
                drop_pos = head.GlobalPosition + ladder.GlobalBasis.Z;
            }

            if (UIPlayerGUI.isMainInventoryOpened)
            {
                if (UIPlayerGUI.DaDSlot.focused_slot is not null)
                {
                    UIPlayerGUI.DaDSlot.focused_slot.slot.DropItem(drop_pos, subworld, head.GlobalRotationDegrees);
                }
            }
            if (Input.IsPhysicalKeyPressed(Godot.Key.Alt))
            {
                if (hum_inventory.left_hand_slot.item is not null)
                {
                    hum_inventory.left_hand_slot.DropItem(drop_pos, subworld, head.GlobalRotationDegrees);
                }
            }
            else
            {
                if (hum_inventory.right_hand_slot.item is not null)
                {
                    hum_inventory.right_hand_slot.DropItem(drop_pos, subworld, head.GlobalRotationDegrees);
                }
            }
        }

        if (Input.IsActionJustPressed("use_item") && !UIPlayerGUI.isMainInventoryOpened)
        {
            if (grabbed_object is not null)
            {
                if (!rot_grabbed_object)
                {
                    label_rot_X.Text = rot_X_inverted ? "Z" : "Y";
                    rot_grabbed_object = true;
                }
            }
            else if (look_at_node is Door door)
            {
                door.TryUseItem(Input.IsPhysicalKeyPressed(Godot.Key.Alt) ? hum_inventory.left_hand_slot.item : hum_inventory.right_hand_slot.item);
            }
            else if (Input.IsPhysicalKeyPressed(Godot.Key.Alt))
            {
                hum_inventory.left_hand_slot.TryUseItem();
            }
            else
            {
                hum_inventory.right_hand_slot.TryUseItem();
            }
        }
        else if (Input.IsActionJustReleased("use_item"))
        {
            rot_grabbed_object = false;
        }

        if (Input.IsActionJustPressed("swap_hands"))
        {
            Inventory.TrySwapSlots(hum_inventory.right_hand_slot, hum_inventory.left_hand_slot);
        }
        else if (Input.IsActionJustPressed("key1")) { this.TrySwapBeltSlot(0); }
        else if (Input.IsActionJustPressed("key2")) { this.TrySwapBeltSlot(1); }
        else if (Input.IsActionJustPressed("key3")) { this.TrySwapBeltSlot(2); }
        else if (Input.IsActionJustPressed("key4")) { this.TrySwapBeltSlot(3); }
        else if (Input.IsActionJustPressed("key5")) { this.TrySwapBeltSlot(4); }
        else if (Input.IsActionJustPressed("key6")) { this.TrySwapBeltSlot(5); }
        else if (Input.IsActionJustPressed("key7")) { this.TrySwapBeltSlot(6); }
        else if (Input.IsActionJustPressed("key8")) { this.TrySwapBeltSlot(7); }
        else if (Input.IsActionJustPressed("key9")) { this.TrySwapBeltSlot(8); }
        else if (Input.IsActionJustPressed("key0")) { this.TrySwapBeltSlot(9); }
    }

   

    private void LadderProcess(float fdelta)
    {
        if (rotDir.Length() > 0)
        {
            float rotSpeed = mouseSensitivity * fdelta;
            float changeY = -rotDir.X * rotSpeed;
            if (changeY + headY.RotationDegrees.Y < 120 && changeY + headY.RotationDegrees.Y > -120)
            {
                headY.RotateY(Mathf.DegToRad(changeY));
            }
            
            float changeX = -rotDir.Y * rotSpeed;
            if (changeX + headX.RotationDegrees.X < 90 && changeX + headX.RotationDegrees.X > -90)
            {
                headX.RotateX(Mathf.DegToRad(changeX));
            }
            rotDir = Vector2.Zero;
        }
    }
    private void BasicProcess(float fdelta)
    {
        if (rotDir.Length() > 0)
        {
            if (!rot_grabbed_object)
            {
                float rotSpeed = mouseSensitivity * fdelta;
                headY.RotateY(Mathf.DegToRad(-rotDir.X * rotSpeed));
                float changeX = -rotDir.Y * rotSpeed;
                if (changeX + headX.RotationDegrees.X < 90 && changeX + headX.RotationDegrees.X > -90)
                {
                    headX.RotateX(Mathf.DegToRad(changeX));
                }
            }
            rotDir = Vector2.Zero;
        }

        if (Input.IsActionJustPressed("grab_object"))
        {
            if (grabbed_object is null && look_at_node is GrabbableObject gobj)
            {
                ungrab = false;
                label_rot_X.Visible = true;
                grabbed_object = gobj;
                grabbed_object.CanSleep = false;
                grab_position = look_at_point;
            }
        }
        else if (Input.IsActionJustReleased("grab_object"))
        {
            if (grabbed_object is not null)
            {
                ungrab = true;
                label_rot_X.Visible = false;
            }

        }

        if (Input.IsActionJustPressed("crouch"))
        {
            if (is_crouching)
            {
                SetStand();
            }
            else
            {
                SetCrouch();
            }
        }
    }
   

    private bool TrySwapBeltSlot(int slot_index)
    {
        if (hum_inventory.belt_slot.inventory is Inventory belt_inventory && belt_inventory.slots.Count >= slot_index+1 && Inventory.TrySwapSlots(Input.IsPhysicalKeyPressed(Godot.Key.Alt) ? hum_inventory.left_hand_slot : hum_inventory.right_hand_slot, belt_inventory.slots[slot_index]))
        {
            ui_hum_inventory.ShowFastPlane();
            return true;
        }
        return false;
    }

    Tween climb_tween;
    bool is_climbing_animation = false;
    Ladder ladder;
    bool is_jumping = false;

    public override void _PhysicsProcess(double delta)
	{
        base._PhysicsProcess(delta);

        if (is_climbing_animation)
        {
            is_jumping = false;
            return;
        }
        else if (ladder is not null)
        {
            LadderMovement((float)delta);
        }
        else
        {
            BasicMovement((float)delta);
        }

        is_jumping = false;

        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D c = GetSlideCollision(i);


            if (c.GetCollider() is RigidBody3D rb)
            {
                if (rb.LinearVelocity.Length() > speed)
                {
                    GD.Print("set rigid");
                }
                else
                {
                    rb.ApplyImpulse(-c.GetNormal() * pushForce * (float)delta, c.GetPosition() - rb.GlobalPosition);
                }
            }
        }

        if (grabbed_object is not null)
        {
            if (!IsInstanceValid(grabbed_object))
            {
                grabbed_object = null;
            }
            else if (ungrab)
            {
                ungrab = false;
                grabbed_object.Sleeping = false;
                grabbed_object = null;
            }
            else
            {
                Vector3 look_point = head.GlobalPosition + look_direction * 2f;
                if (grabbed_object is PushableDoor pd)
                {
                    if (!pd.locked)
                    {
                        grabbed_object.ApplyForce((look_point - pd.GlobalPosition) * pushForce, pd.grab_point.Position);
                    }
                }
                else
                {
                    Godot.Collections.Array < Rid >  ignore = new Godot.Collections.Array<Rid> { grabbed_object.GetRid() };

                    Godot.Collections.Dictionary look_at = GetWorld3D().DirectSpaceState.IntersectRay(PhysicsRayQueryParameters3D.Create(head.GlobalPosition, look_point, exclude: ignore));
                    if (look_at.Count > 0)
                    {
                        look_point = (Vector3)look_at["position"];
                    }
                    
                    Godot.Collections.Dictionary check_obstacle = GetWorld3D().DirectSpaceState.IntersectRay(PhysicsRayQueryParameters3D.Create(head.GlobalPosition, grabbed_object.GlobalPosition, exclude: ignore));
                    if (check_obstacle.Count > 0)
                    {
                        grabbed_object.LinearVelocity = Vector3.Zero;
                        grabbed_object = null;
                    }
                    else
                    {
                        grabbed_object.LinearVelocity = (look_point - grabbed_object.GlobalPosition) * pushForce;
                        grabbed_object.AngularVelocity = Vector3.Zero;

                        if (rot_grabbed_object)
                        {
                            grabbed_object.AngularVelocity = ((head.GlobalBasis.X * rotDir.Y - (rot_X_inverted ? head.GlobalBasis.Z : head.GlobalBasis.Y) * rotDir.X).Normalized()) * 4f;
                        }
                    }
                }
            }
        }
	}

    private void LadderMovement(float fdelta)
    {
        if (is_jumping)
        {
            is_jumping = false;
            if (this.ladder is not null && look_at_node is Ladder ld && headY.GlobalPosition.DistanceTo(look_at_point) < shape.Radius * 4f && ld != this.ladder)
            {
                ClimbOnLadder(ld);
                return;
            }
            else if (Input.IsActionPressed("backward") || (look_at_point - this.GlobalPosition).Dot(-this.ladder.GlobalBasis.Y) > 0)
            {
                is_climbing_animation = true;
                this.Velocity = Vector3.Zero;

                this.Reparent(subworld.entity_container);
                Vector3 headYrot = headY.GlobalRotationDegrees;
                this.RotationDegrees = Vector3.Zero;
                headY.GlobalRotationDegrees = headYrot;

                if (climb_tween != null)
                {
                    climb_tween.Kill();
                }
                climb_tween = GetTree().CreateTween().SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In);

                float valY = Mathf.RadToDeg(headY.GlobalBasis.Y.AngleTo(this.GlobalBasis.Y));
                climb_tween.TweenProperty(headY, "rotation_degrees", new Vector3(0, headY.RotationDegrees.Y, 0), valY / 30);
                climb_tween.Finished += () =>
                {
                    ladder = null;
                    is_climbing_animation = false;
                };
                return;
            }
            else if (TryEdgeClimbFromLadder(out bool crouching, out Vector3 vertical_point, out Vector3 target_point, out Vector3 wall_normal) || TryEdgeClimbFromLadder2(out crouching, out vertical_point, out target_point, out wall_normal))
            {
                is_climbing_animation = true;
                this.Velocity = Vector3.Zero;

                if (crouching)
                {
                    SetCrouch();
                }

                this.Reparent(subworld.entity_container);
                Vector3 headYrot = headY.GlobalRotationDegrees;
                this.RotationDegrees = Vector3.Zero;
                headY.GlobalRotationDegrees = headYrot;

                if (climb_tween != null)
                {
                    climb_tween.Kill();
                }
                climb_tween = GetTree().CreateTween().SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In).SetParallel();

                float valY = Mathf.RadToDeg(headY.GlobalBasis.Y.AngleTo(this.GlobalBasis.Y));
                climb_tween.TweenProperty(headY, "rotation_degrees", new Vector3(0, headY.RotationDegrees.Y, 0), valY / 30);
                climb_tween.TweenProperty(this, "global_position", vertical_point, 0.4f);
                climb_tween.Chain().TweenProperty(this, "global_position", target_point, 0.2f);
                climb_tween.Finished += () =>
                {
                    GD.Print(vertical_point, " : ", target_point);

                    ladder = null;
                    is_climbing_animation = false;
                };
                return;
            }
        }

        Vector3 velocity = Velocity;

        Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
        Vector3 direction = (ladder.GlobalBasis.Y * -inputDir.Y).Normalized();
        if (direction != Vector3.Zero)
        {
            velocity = direction * speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
            velocity.Y = Mathf.MoveToward(Velocity.Y, 0, speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
        }

        Velocity = velocity;
        MoveAndSlide();
        this.Position = new Vector3(0, Math.Clamp(this.Position.Y, ladder.point1.Position.Y, ladder.point2.Position.Y - stand_height/2), shape.Radius + 0.25f);

    }
    private void BasicMovement(float fdelta)
    {
        Vector3 velocity = Velocity;

        if (!IsOnFloor())
            velocity.Y -= defaultGravity * fdelta;

        if (is_jumping)
        {
            is_jumping = false;
            if (this.ladder is null && look_at_node is Ladder ld && headY.GlobalPosition.DistanceTo(look_at_point) < shape.Radius * 4f)
            {
                ClimbOnLadder(ld);
                return;
            }
            else if (TryEdgeClimb(out bool crouching, out Vector3 vertical_point, out Vector3 target_point, out Vector3 wall_normal))
            {
                is_climbing_animation = true;
                ungrab = true;
                this.Velocity = Vector3.Zero;

                if (crouching && !is_crouching)
                {
                    SetCrouch();
                    this.GlobalPosition += headY.GlobalBasis.Y * (headY_stand_pos - headY_crouch_pos);
                }

                if (climb_tween != null)
                {
                    climb_tween.Kill();
                }
                climb_tween = GetTree().CreateTween().SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In).SetParallel();
                //climb_tween.TweenProperty(headX, "global_rotation_degrees", this.Transform.LookingAt(target_point), 0.3f);
                float valY = Mathf.RadToDeg(headY.GlobalBasis.Z.SignedAngleTo(wall_normal, headY.GlobalBasis.Y));
                climb_tween.TweenProperty(headY, "rotation_degrees:y", headY.RotationDegrees.Y + valY, valY/180 * (valY < 0 ? -1 : 1));
                climb_tween.Chain().TweenProperty(this, "global_position", vertical_point, 0.4f);
                climb_tween.Chain().TweenProperty(this, "global_position", target_point, 0.2f);
                climb_tween.Finished += () =>
                {
                    is_climbing_animation = false;
                };

                return;
            }

            if (IsOnFloor())
            {
                velocity.Y = jumpVelocity;
            }
            
        }

        Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
        Vector3 direction = (headY.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * speed;
            velocity.Z = direction.Z * speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    private void ClimbOnLadder(Ladder ld)
    {
        if ((this.GlobalPosition - ld.GlobalPosition).Dot(ld.GlobalBasis.Z) <= 0)
        {
            return;
        }

        is_climbing_animation = true;
        this.Velocity = Vector3.Zero;

        ungrab = true;
        this.ladder = ld;
        this.Reparent(ladder);
        Vector3 headYrot = headY.GlobalRotationDegrees;
        this.Rotation = Vector3.Zero;
        headY.GlobalRotationDegrees = headYrot;

        if (climb_tween != null)
        {
            climb_tween.Kill();
        }
        climb_tween = GetTree().CreateTween().SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In).SetParallel();
        climb_tween.TweenProperty(headY, "rotation_degrees", new Vector3(0, Mathf.Clamp(headY.RotationDegrees.Y, -120, 120), 0), 0.3f);
        Vector3 target_point = new Vector3(0, Math.Clamp(this.Position.Y, ladder.point1.Position.Y, ladder.point2.Position.Y - 1), shape.Radius + 0.25f);
        float dist = this.Position.DistanceTo(target_point);
        climb_tween.TweenProperty(this, "position", target_point, dist/5);
        climb_tween.Finished += () =>
        {
            if (is_crouching)
            {
                SetStand();
            }
            is_climbing_animation = false;
        };
    }

    private bool TryEdgeClimbFromLadder(out bool crouching, out Vector3 vertical_point, out Vector3 target_point, out Vector3 wall_normal)
    {
        crouching = false;
        vertical_point = Vector3.Zero;
        target_point = Vector3.Zero;
        wall_normal = Vector3.Zero;

        climb_cast.GlobalRotation = headY.GlobalRotation;

        CapsuleShape3D cs = (CapsuleShape3D)stand_shape_cast.Shape;

        target_point = this.GlobalPosition + (look_at_point - this.GlobalPosition).Project(this.GlobalBasis.Y) + this.GlobalBasis.Y * 0.6f - (this.GlobalPosition - look_at_point).Project(headY.GlobalBasis.Z);
        climb_cast.GlobalPosition = target_point;
        climb_cast.ForceRaycastUpdate();

        if (headY.GlobalPosition.DistanceTo(look_at_point) > shape.Radius * 4f || !climb_cast.IsColliding())
        {
            for (float i = 0.6f; i < 1.5f; i += 0.05f)
            {
                climb_cast.GlobalPosition = this.GlobalPosition + this.GlobalBasis.Y * (stand_height + stand_height / 4) - headY.GlobalBasis.Z * i;
                climb_cast.ForceRaycastUpdate();
                if (climb_cast.IsColliding())
                {
                    break;
                }
            }
        }

        if (!climb_cast.IsColliding() || (climb_cast.GetCollider() is RigidBody3D rb && !rb.Sleeping))
        {
            return false;
        }
        target_point = climb_cast.GetCollisionPoint();
        
        wall_cast.GlobalPosition = target_point + headY.GlobalBasis.Z * 0.5f - climb_cast.GetCollisionNormal() * 0.05f;
        wall_cast.GlobalRotation = headY.GlobalRotation;
        wall_cast.ForceRaycastUpdate();

        if (!wall_cast.IsColliding())
        {
            return false;
        }
        wall_normal = wall_cast.GetCollisionNormal();
        Vector3 point_left = -climb_cast.GetCollisionNormal().Cross(wall_normal).Normalized() * 0.6f;
        climb_cast_side.GlobalPosition = climb_cast.GetCollisionPoint() + climb_cast.GlobalBasis.Y * 0.1f;
        climb_cast_side.TargetPosition = point_left;
        climb_cast_side.ForceRaycastUpdate();
        if (climb_cast_side.IsColliding())
        {
            point_left = climb_cast_side.GetCollisionPoint();
        }
        else
        {
            point_left += climb_cast_side.GlobalPosition;
        }

        Vector3 point_right = climb_cast.GetCollisionNormal().Cross(wall_normal).Normalized() * 0.6f;
        climb_cast_side.TargetPosition = point_right;
        climb_cast_side.ForceRaycastUpdate();
        if (climb_cast_side.IsColliding())
        {
            point_right = climb_cast_side.GetCollisionPoint();
        }
        else
        {
            point_right += climb_cast_side.GlobalPosition;
        }

        climb_cast.GlobalPosition = (point_left + point_right) / 2;
        climb_cast.ForceRaycastUpdate();
        if (!climb_cast.IsColliding())
        {
            return false;
        }
        target_point = climb_cast.GetCollisionPoint() + climb_cast.GetCollisionNormal() * 0.01f;
        
        cs.Height = stand_height;

        vertical_point = this.GlobalPosition + (target_point - this.GlobalPosition).Project(climb_cast.GetCollisionNormal());
        if (vertical_point.DistanceTo(target_point) > cs.Radius * 4f)
        {
            return false;
        }

        stand_shape_cast.GlobalRotation = this.GlobalRotation;
        stand_shape_cast.GlobalPosition = vertical_point;
        stand_shape_cast.ForceShapecastUpdate();
        if (stand_shape_cast.IsColliding())
        {
            return false;
        }

        stand_shape_cast.GlobalRotation = Vector3.Zero;
        stand_shape_cast.GlobalPosition = vertical_point + headY.GlobalBasis.Y * (cs.Height / 2 + 0.01f);
        stand_shape_cast.ForceShapecastUpdate();
        if (!stand_shape_cast.IsColliding())
        {
            stand_shape_cast.GlobalPosition = target_point + headY.GlobalBasis.Y * (cs.Height / 2 + 0.01f);
            stand_shape_cast.ForceShapecastUpdate();
            if (!stand_shape_cast.IsColliding())
            {
                return true;
            }
        }
        cs.Height = crouch_height;
        stand_shape_cast.GlobalPosition = vertical_point + this.GlobalBasis.Y * (cs.Height / 2 + 0.01f);
        stand_shape_cast.ForceShapecastUpdate();
        if (!stand_shape_cast.IsColliding())
        {
            stand_shape_cast.GlobalPosition = target_point + this.GlobalBasis.Y * (cs.Height / 2 + 0.01f);
            stand_shape_cast.ForceShapecastUpdate();
            if (!stand_shape_cast.IsColliding())
            {
                crouching = true;
                return true;
            }
        }

        return false;
    }
    private bool TryEdgeClimbFromLadder2(out bool crouching, out Vector3 vertical_point, out Vector3 target_point, out Vector3 wall_normal)
    {
        crouching = false;
        vertical_point = Vector3.Zero;
        target_point = Vector3.Zero;
        wall_normal = ladder.GlobalBasis.Z;

        if (this.Position.Y < ladder.point2.Position.Y - stand_height / 2 - 0.1f)
        {
            return false;
        }

        target_point = ladder.point2.GlobalPosition + Vector3.Up * 0.1f;
        vertical_point = this.GlobalPosition + (target_point - this.GlobalPosition).Project(this.GlobalBasis.Y);

        CapsuleShape3D cs = (CapsuleShape3D)stand_shape_cast.Shape;
        cs.Height = stand_height;
        stand_shape_cast.GlobalRotation = this.GlobalRotation;
        stand_shape_cast.GlobalPosition = vertical_point;
        stand_shape_cast.ForceShapecastUpdate();
        if (stand_shape_cast.IsColliding())
        {
            return false;
        }

        stand_shape_cast.GlobalRotation = Vector3.Zero;
        stand_shape_cast.GlobalPosition = target_point + Vector3.Up * cs.Height/2;
        stand_shape_cast.ForceShapecastUpdate();
        if (!stand_shape_cast.IsColliding())
        {
            return true;
        }

        cs.Height = crouch_height;
        stand_shape_cast.GlobalPosition = target_point + Vector3.Up * cs.Height / 2;
        stand_shape_cast.ForceShapecastUpdate();
        if (!stand_shape_cast.IsColliding())
        {
            crouching = true;
            return true;
        }
        return false;
    }
    private bool TryEdgeClimb(out bool crouching, out Vector3 vertical_point, out Vector3 target_point, out Vector3 wall_normal)
    {
        crouching = false;
        vertical_point = Vector3.Zero;
        target_point = Vector3.Zero;
        wall_normal = Vector3.Zero;

        climb_cast.GlobalRotation = headY.GlobalRotation;
        for (float i = 0.6f; i < 1f; i+=0.05f)
        {
            climb_cast.GlobalPosition = this.GlobalPosition + this.GlobalBasis.Y * (shape.Height + shape.Height/4) - headY.GlobalBasis.Z * i;
            climb_cast.ForceRaycastUpdate();
            if (climb_cast.IsColliding())
            {
                break;
            }
        }

        
        if (!climb_cast.IsColliding() || (climb_cast.GetCollider() is RigidBody3D rb && !rb.Sleeping))
        {
            return false;
        }

        wall_cast.GlobalPosition = climb_cast.GetCollisionPoint() + climb_cast.GlobalBasis.Z * 0.5f - climb_cast.GlobalBasis.Y * 0.05f;
        wall_cast.GlobalRotation = climb_cast.GlobalRotation;
        wall_cast.ForceRaycastUpdate();

        if (!wall_cast.IsColliding())
        {
            return false;
        }

        wall_normal = wall_cast.GetCollisionNormal();

        Vector3 point_left = -climb_cast.GetCollisionNormal().Cross(wall_normal).Normalized() * 0.6f;
        climb_cast_side.GlobalPosition = climb_cast.GetCollisionPoint() + climb_cast.GetCollisionNormal() * 0.1f;
        climb_cast_side.TargetPosition = point_left;
        climb_cast_side.ForceRaycastUpdate();
        if (climb_cast_side.IsColliding())
        {
            point_left = climb_cast_side.GetCollisionPoint();
        }
        else
        {
            point_left += climb_cast_side.GlobalPosition;
        }

        Vector3 point_right = climb_cast.GetCollisionNormal().Cross(wall_normal).Normalized() * 0.6f;
        climb_cast_side.TargetPosition = point_right;
        climb_cast_side.ForceRaycastUpdate();
        if (climb_cast_side.IsColliding())
        {
            point_right = climb_cast_side.GetCollisionPoint();
        }
        else
        {
            point_right += climb_cast_side.GlobalPosition;
        }

        climb_cast.GlobalPosition = (point_left + point_right) / 2;
        climb_cast.ForceRaycastUpdate();
        if (!climb_cast.IsColliding())
        {
            return false;
        }
        target_point = climb_cast.GetCollisionPoint() + climb_cast.GetCollisionNormal() * 0.01f;

        CapsuleShape3D cs = (CapsuleShape3D)stand_shape_cast.Shape;
        cs.Height = stand_height;

        vertical_point = target_point + wall_normal * (cs.Radius + 0.05f) + (wall_cast.GetCollisionPoint() - target_point).Project(wall_normal);
        stand_shape_cast.GlobalPosition = vertical_point;
        stand_shape_cast.ForceShapecastUpdate();
        if (stand_shape_cast.IsColliding())
        {
            return false;
        }
         if (!is_crouching)
        {
            stand_shape_cast.GlobalPosition = target_point + climb_cast.GetCollisionNormal() * (cs.Height / 2 + 0.01f);
            stand_shape_cast.ForceShapecastUpdate();
            if (!stand_shape_cast.IsColliding())
            {
                return true;
            }
        }

        cs.Height = crouch_height;
        stand_shape_cast.GlobalPosition = target_point + climb_cast.GetCollisionNormal() * (cs.Height / 2 + 0.01f);
        stand_shape_cast.ForceShapecastUpdate();
        if (!stand_shape_cast.IsColliding())
        {
            crouching = true;
            return true;
        }
        return false;
    }

    public override void _Input(InputEvent @event)
    {
        if (!UIPlayerGUI.isMainInventoryOpened && @event is InputEventMouseMotion motion)
        {
            rotDir = motion.Relative;
        }
    }
}
