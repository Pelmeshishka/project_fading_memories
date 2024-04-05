using Godot;
using System;

public partial class StatHealth : StatSpeedDecrease
{
    [Export] private Stat Pain;
    [Export] private Stat Blood;
    
    public override void _Ready()
    {
        this.StatType = EStats.Health;

        if (this.Pain is not null)
        {
            this.Pain.OnValueChanged += OnBloodPainChanged;
        }

        if (this.Blood is not null)
        {
            this.Blood.OnValueChanged += OnBloodPainChanged;
        }
        base._Ready();
        OnBloodPainChanged(null, 0);
    }

    public void OnBloodPainChanged(Stat stat, float prev_val)
    {
        if (selfHealEnabled && stat is not null && ((stat.defaultIsMax && stat.Value < prev_val) || (!stat.defaultIsMax && stat.Value > prev_val)))
        {
            this.canBeHealed = false;
            waitTimer.Start();
        }

        this.SetMaxValueRatio(Mathf.Min(this.Pain is null ? 1 : 1 - this.Pain.GetRatio(), this.Blood is null ? 1 : this.Blood.GetRatio()));
    }
  
}
