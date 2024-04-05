using Godot;
using System;

public partial class StatSpeedIncrease : StatSpeedAffect
{
    public override float GetSpeedRatio()
    {
        return Mathf.Clamp(GetRatio(), 0, 1) / 2;
    }
}
