using Godot;
using System.Collections.Generic;

public partial class Subworlds : Node
{
    private static Dictionary<string, Subworld> subworlds = new();

    public override void _Ready()
    {
        int i = 0;
        while(i < GetChildCount())
        {
            Node node = GetChild(i);
            if (node is not Subworld)
            {
                this.RemoveChild(node);
                continue;
            }

            subworlds.Add(node.Name, (Subworld)node);
            i++;
        }
    }

    public static bool TryGetSubworld(string key, out Subworld result)
    {
        return subworlds.TryGetValue(key, out result);
    }
}
