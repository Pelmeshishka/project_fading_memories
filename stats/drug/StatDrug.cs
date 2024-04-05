using Godot;
using System;

public partial class StatDrug : StatSpeedIncrease
{
    public override void _Ready()
    {
        this.StatType = EStats.Drug;
        this.damageType = EDamage.Toxins;
        base._Ready();
    }
}
