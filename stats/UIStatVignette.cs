using Godot;
using System;

public partial class UIStatVignette : Panel
{

    public void OnStatChanged(Stat stat, float prev_val)
    {
        float value = 1-stat.GetRatio();

        (this.Material as ShaderMaterial).SetShaderParameter("vignette_intensity", value * 2f);
    }
}
