using Godot;
using System;

public partial class StatSpeedDecrease : StatSpeedAffect
{
    public override float GetSpeedRatio()
    {
        if (criticalIsMax)
        {
            return (2 - Mathf.Clamp(GetRatio(), 0, 1)) / 2;
        }
        else
        {
            return (Mathf.Clamp(GetRatio(), 0, 1) + 1) / 2;
        }

    }
}
