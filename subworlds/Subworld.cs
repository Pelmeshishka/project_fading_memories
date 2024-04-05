using Godot;
using System;

public partial class Subworld : SubViewportContainer
{
    [Export] private Node items_container;
    [Export] public Node entity_container { get; private set; }
    [Export] public Node other_container { get; private set; }
    public override void _EnterTree()
    {
        ((SubViewport)this.GetChild(0)).OwnWorld3D = true;
    }

    public void SpawnItemBody(ItemBody item_body)
    {
        items_container.AddChild(item_body);
    }
}
