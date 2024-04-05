using Godot;
using Godot.Collections;
using System;

public partial class TrainingStatsArea : Area3D
{
    [Export] private EStats estat = EStats.Satiety;

    [Export] private float value = 0.5f;
    [Export] private float timeStep = 0.25f;
    Timer timer;
    public override void _Ready()
    {
        timer = new Timer();
        timer.WaitTime = timeStep;
        timer.Autostart = true;
        timer.OneShot = false;
        this.AddChild(timer);
    }

    private Dictionary<Entity, Callable> callables = new();

    private void Func(Entity entity)
    {
        if (entity.stats.TryGetStat(estat, out Stat stat))
        {
            stat.SetValue(stat.Value + value);
            GD.Print(entity.Name, " => ", estat.ToString(), " : ", stat.Value);
        }
    }

    public void OnBodyEnter(Node3D body)
    {
        if (body is not Entity entiry)
        {
            return;
        }
        Callable c = Callable.From(() => Func(entiry));
        callables.Add(entiry, c);
        timer.Connect(Timer.SignalName.Timeout, c);
    }

    public void OnBodyExit(Node3D body)
    {
        if (body is not Entity entiry)
        {
            return;
        }
        timer.Disconnect(Timer.SignalName.Timeout, callables[entiry]);
        callables.Remove(entiry);
    }
}
