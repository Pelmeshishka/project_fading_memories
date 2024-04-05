using Godot;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection.PortableExecutable;
using static Godot.WebSocketPeer;

public partial class Stat : Node
{
    [Export] public EStats StatType { get; protected set; }

    [ExportGroup("Values")]

    [Export] private float _extra_val = 0f;
    [Export] public float MaxValue { get; private set; } = 100f;
    [Export] public float MaxValueRatio { get; private set; } = 1f;
    [Export] public float Value { get; private set; } = 100f;

    [ExportGroup("Heal Self")]
    [Export] protected bool selfHealEnabled = false;
    [Export] public bool defaultIsMax { get; protected set; } = false;
    [Export] private float healCount = 2f;
    protected bool canBeHealed = true;
    protected Timer waitTimer;


    [ExportGroup("Damage Other")]
    [Export] protected EDamage damageType;
    [Export] public bool damageOnlyCritical { get; protected set; } = true;
    [Export] public bool criticalIsMax { get; protected set; } = false;
    [Export] private float damageCount = 1f;
    

    private EntityStats statsOwner;
    [Signal] public delegate void OnValueChangedEventHandler(Stat stat, float prev_val);

    public override void _Ready()
    {
        Value = Mathf.Clamp(Value, 0, (MaxValue + _extra_val) * MaxValueRatio);
        EmitSignal(SignalName.OnValueChanged, this, Value);

        if (selfHealEnabled) 
        {
            waitTimer = new Timer();
            waitTimer.OneShot = true;
            waitTimer.WaitTime = 3;
            this.AddChild(waitTimer);

            waitTimer.Timeout += () => canBeHealed = true;
            this.OnValueChanged += OnSelfChanged;
        }

        statsOwner = this.GetParent<EntityStats>();
    }

    public void SetMaxValueRatio(float maxValueRatio)
    {
        float prevMaxValRatio = this.MaxValueRatio;
        this.MaxValueRatio = maxValueRatio;
        float prevVal = Value;
        SetValue(Value);

        if (prevVal == Value && prevMaxValRatio != MaxValueRatio)
        {
            EmitSignal(SignalName.OnValueChanged, this, Value);
        }

    }

    public virtual void SetValue(float newVal) 
    {
        float prev = Value;
        Value = Mathf.Clamp(newVal, 0, (MaxValue + _extra_val) * MaxValueRatio);
        if (prev != Value)
        {
            EmitSignal(SignalName.OnValueChanged, this, prev);
        }
    }

    public float GetRatio()
    {
        return Value / MaxValue;
    }

    public void OnSelfChanged(Stat stat, float prev_val)
    {
        if (defaultIsMax)
        {
            if (this.Value < prev_val)
            {
                canBeHealed = false;
                waitTimer.Start();
            }
        }
        else
        {
            if (this.Value > prev_val)
            {
                canBeHealed = false;
                waitTimer.Start();
            }
        }
    }

    public override void _Process(double delta)
    {
        if (canBeHealed)
        {
            if ( (!defaultIsMax && this.Value <= 0) || (defaultIsMax && this.Value >= (this.MaxValue * this.MaxValueRatio)))
            {
                canBeHealed = false;
            }
            else
            {
                this.SetValue(this.Value + this.healCount * (float)delta * (defaultIsMax ? 1 : -1));
            }
        }

        if (damageType != EDamage.None)
        {
            float ratio = GetRatio();
            if (damageOnlyCritical)
            {
                if ((criticalIsMax && ratio >= 1) || (!criticalIsMax && ratio <= 0))
                {
                    statsOwner.Damage(this, damageType, damageCount * Mathf.Max(1, ratio) * (float)delta);
                }
            }
            else
            {
                statsOwner.Damage(this, damageType, damageCount * (criticalIsMax ? ratio : 1-ratio) * (float)delta);
            }
        }
    }
}
