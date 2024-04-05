using Godot;
using System;

public partial class UIStatLabel2 : Label
{
	[Export] string preText;

    public void OnStatChanged(Stat stat, float prev_val)
	{
		float ratio = stat.GetRatio();
        this.Text = $"{preText}:{Mathf.Round(stat.GetRatio() * 100f)}% -> {stat.Value}/{stat.MaxValue * stat.MaxValueRatio}({stat.MaxValue})";

        if (ratio < 0)
        {
            ratio *= -1;
        }

        this.AddThemeColorOverride("font_color", Color.Color8((byte)220, (byte)Mathf.Clamp(220 * ratio, 5, 255), (byte)Mathf.Clamp(220 * ratio, 5, 255)));

    }
}
