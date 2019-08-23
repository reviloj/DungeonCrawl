
using System.Collections.Generic;

public class Stat
{
    protected float value;
    public float Value
    {
        get { return value; }
        set { this.value = (MaxValue != null && value > MaxValue) ? (int)MaxValue : (MinValue != null && value < MinValue) ? (int)MinValue : value; }
    }
    public StatTypes statType;
    public int? MaxValue { get; }
    public int? MinValue { get; }

    public Stat(float value, StatTypes statType, int? maxValue = null, int? minValue = null)
    {
        this.value = (MaxValue != null && value > MaxValue) ? (int)MaxValue : (MinValue != null && value < MinValue) ? (int)MinValue : value;
        this.statType = statType;
        MaxValue = maxValue;
        MinValue = minValue;
    }

    public Stat(Stat s) : this(s.value, s.statType, s.MaxValue, s.MinValue)
    {

    }
}

public class StatBuff : Stat
{
    public BuffTypes BuffType { get; }
    public StatTypes? baseStat { get; }

    public StatBuff(float value, StatTypes statType, int? maxValue = null, int? minValue = null, BuffTypes buffType = BuffTypes.Additive, StatTypes? baseStat = null) : base(value, statType, maxValue, minValue)
    {
        BuffType = buffType;
        this.baseStat = baseStat;
    }
    public StatBuff(StatBuff stat) : base(stat.value, stat.statType, stat.MaxValue, stat.MinValue)
    {
        BuffType = stat.BuffType;
        baseStat = stat.baseStat;
    }
}

public class Buff
{
    private List<StatBuff> stats;
    private float duration;

    public Buff(List<StatBuff> stats, int duration)
    {
        this.stats = stats;
        this.duration = duration;
    }
    public Buff(Buff buff)
    {
        stats = new List<StatBuff>();
        foreach (StatBuff stat in buff.stats)
            stats.Add(new StatBuff(stat));
        duration = buff.duration;
    }

    public bool hasStat(StatTypes stat)
    {
        return stats.Exists(x => x.statType == stat);
    }

    public StatBuff getStat(StatTypes stat)
    {
        return stats.Find(x => x.statType == stat);
    }

    public bool ageBuff(float timeDisplacement)
    {
        duration -= timeDisplacement;
        return duration <= 0;
    }
}
