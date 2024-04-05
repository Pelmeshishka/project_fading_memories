using Godot;
using System;

public partial class UIStatLabel : Label
{
	[Export] string preText;

    public void OnStatChanged(Stat stat, float prev_val)
	{
		float ratio = stat.GetRatio();
        this.Text = $"{preText}: {Mathf.Round(ratio * 100)}%";

        if (ratio < 0)
        {
            ratio *= -1;
        }

		if (stat.criticalIsMax)
		{
            ratio = 1 - ratio;
        }
        this.AddThemeColorOverride("font_color", Color.Color8((byte)220, (byte)Mathf.Clamp(220 * ratio, 5, 255), (byte)Mathf.Clamp(220 * ratio, 5, 255)));

    }
}
