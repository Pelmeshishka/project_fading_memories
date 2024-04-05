using Godot;
using System;

public partial class StatSpeedAffect : Stat
{
    public virtual float GetSpeedRatio()
    {
        throw new Exception("Choose increase or decrese speed with using StatSpeedDecrease or StatSpeedIncrease class");
    }
}
