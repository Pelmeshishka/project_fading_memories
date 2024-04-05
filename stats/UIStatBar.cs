using Godot;
using System;

public partial class UIStatBar : TextureProgressBar
{
    public void OnStatChanged(Stat stat, float prev_val)
    {
        this.MaxValue = 1;

        float ratio = stat.GetRatio();
        this.Value = Mathf.Clamp(ratio, 0, 1);

        if (ratio < 0)
        {
            ratio *= -1;
        }

        this.TintProgress = Color.Color8((byte)220, (byte)Mathf.Clamp(220 * ratio, 5, 255), (byte)Mathf.Clamp(220 * ratio, 5, 255));
    }
}
