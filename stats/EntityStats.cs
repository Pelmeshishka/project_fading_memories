using Godot;
using System;
using System.Collections.Generic;
using static Godot.WebSocketPeer;

public partial class EntityStats : Node
{
    private Dictionary<EStats, Stat> stats = new();
    public float speedRatio { get; private set; }

    private StatHealth health;

    public override void _Ready()
    {
        foreach(var node in this.GetChildren())
        {
            if (node is not Stat stat)
            {
                this.RemoveChild(node);
                node.QueueFree();
                continue;
            }
            else
            {
                stats.Add(stat.StatType, stat);
                stat.OnValueChanged += (v1, v2) => { SetSpeedRatio(); };

                if (stat is StatHealth health) { this.health = health; }
            }
        }
        SetSpeedRatio();
    }

    public bool TryGetStat(EStats type, out Stat stat)
    {
        return stats.TryGetValue(type, out stat);
    }

    private void SetSpeedRatio()
    {
        float result = 1f;
        float addToResult = 0;
        foreach (Stat stat in stats.Values)
        {
            if (stat is not StatSpeedAffect ssa)
            {
                continue;
            }

            float ratio = ssa.GetSpeedRatio();

            if (stat is StatSpeedIncrease)
            {
                if (addToResult < ratio)
                {
                    addToResult = ratio;
                }
            }
            else if (result > ratio)
            {
                result = ratio;
            }
            //GD.Print(stat.StatType.ToString(), " : ", ratio);
        }

        speedRatio = result + addToResult;
        //GD.Print("Speed ratio : ", speedRatio);
    }

    public void Damage(Node from, EDamage damageType, float value)
    {

        switch (damageType)
        {
            case EDamage.Hunger:
            case EDamage.Thirsty:
            case EDamage.Toxins:
                {

                    if (health is not null)
                    {
                        health.SetValue(health.Value - value);
                    }
                    break;
                }
        }
    }
}

public enum EStats
{
    Satiety, Water, Energy, Blood, Pain, Adrenaline, Drug, Toxins, Sanity, SelfControl, Health
}


public enum EDamage
{
    None, Hunger, Thirsty, Toxins, Wound, Hit, 
}

