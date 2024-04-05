using Godot;
using System;

public partial class Ladder : StaticBody3D
{
    [Export] public Node3D point1 { get; private set; }
    [Export] public Node3D point2 { get; private set; }
}
