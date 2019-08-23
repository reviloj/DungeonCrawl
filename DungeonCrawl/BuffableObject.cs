
using Microsoft.Xna.Framework;

using System.Collections.Generic;
using System;

abstract public class BuffableObject : GameObject
{
    public const int KNOCKBACK_SPEED = 11;

    protected string name;
    public string Name { get { return name; } }

    protected string description;
    public string Description { get { return description; } }

    protected List<Stat> stats;
    protected List<Effect> effects;
    protected List<Buff> buffs = new List<Buff>();

    public BuffableObject(Vector2 pos, Vector2 size, Sprite sprite, CollisionTypes colType, string name, string desc, List<Stat> stats, List<Effect> effects) : base(pos, size, sprite, colType)
    {
        this.name = name;
        description = desc;
        this.stats = stats;
        this.effects = effects;
    }

    public void addBuff(Buff buff)
    {
        buffs.Add(buff);
    }

    public void ageBuffs(float timeDisplacement)
    {
        for(int i = buffs.Count - 1; i >= 0; --i)
            if (buffs[i].ageBuff(timeDisplacement))
                buffs.RemoveAt(i);
    }

    override
    public void update(float timeDisplacement)
    {
        base.update(timeDisplacement);
        bool knockedback = hasStat(StatTypes.Knockback);
        bool stunned = hasStat(StatTypes.Stun);
        ageBuffs(timeDisplacement);
        if (!hasStat(StatTypes.Knockback) && hasStat(StatTypes.Knockback) != knockedback)
            states[(int)TypesOfStates.MovementLock] = ObjectStates.Idle;
        if (!hasStat(StatTypes.Stun) && hasStat(StatTypes.Stun) != stunned)
            states[(int)TypesOfStates.MovementLock] = ObjectStates.Idle;
    }

    public bool hasStat(StatTypes statType)
    {
        if (stats.Exists(x => x.statType == statType))
            return true;
        foreach (Buff buff in buffs)
            if (buff.hasStat(statType))
                return true;
        return false;
    }

    public float? getBaseStat(StatTypes statType)
    {
        if (stats.Exists(x => x.statType == statType))
            return stats.Find(x => x.statType == statType).Value;
        else
            return null;
    }

    virtual public Stat calculateBaseStat(StatTypes statType)
    {
        if (!hasStat(statType))
            return null;
        float value = 0;
        int? minMax = null;
        int? maxMin = null;
        Stat objStat = stats.Find(x => x.statType == statType);
        if (objStat != null)
        {
            value = objStat.Value;
            minMax = objStat.MaxValue;
            maxMin = objStat.MinValue;
        }
        foreach (Buff buff in buffs)
        {
            StatBuff statBuff = buff.getStat(statType);
            if (statBuff != null && statBuff.BuffType == BuffTypes.Additive)
            {
                value += statBuff.Value;
                if (minMax == null || statBuff.MaxValue != null && statBuff.MaxValue < minMax)
                    minMax = statBuff.MaxValue;
                if (maxMin == null || statBuff.MinValue != null && statBuff.MinValue > maxMin)
                    maxMin = statBuff.MinValue;
            }
        }
        foreach (Buff buff in buffs)
        {
            StatBuff statBuff = buff.getStat(statType);
            if (statBuff != null && statBuff.BuffType == BuffTypes.Multiplicative)
            {
                value *= statBuff.Value;
                if (minMax == null || statBuff.MaxValue != null && statBuff.MaxValue < minMax)
                    minMax = statBuff.MaxValue;
                if (maxMin == null || statBuff.MinValue != null && statBuff.MinValue > maxMin)
                    maxMin = statBuff.MinValue;
            }
        }
        return new Stat(value, statType, minMax, maxMin);
    }

    virtual public float? calculateStat(StatTypes statType)
    {
        if (!hasStat(statType))
            return null;
        float value = 0;
        int? maxMax = null;
        int? minMin = null;
        foreach (Buff buff in buffs)
        {
            StatBuff statBuff = buff.getStat(statType);
            if (statBuff != null && statBuff.BuffType == BuffTypes.TotalAdditive)
            {
                if (statBuff.baseStat != null)
                    value += calculateBaseStat((StatTypes)statBuff.baseStat).Value * statBuff.Value;
                else
                    value += statBuff.Value;
                if (maxMax == null || statBuff.MaxValue != null && statBuff.MaxValue < maxMax)
                    maxMax = statBuff.MaxValue;
                if (minMin == null || statBuff.MinValue != null && statBuff.MinValue > minMin)
                    minMin = statBuff.MinValue;
            }
        }
        value = (maxMax != null && value > maxMax) ? (int)maxMax : (minMin != null && value < minMin) ? (int)minMin : value;
        return value + calculateBaseStat(statType).Value;
    }

    public float? useStat(StatTypes statType, BuffableObject target = null)
    {
        foreach (Effect effect in effects)
            effect.triggerEffect(statType, this, target);
        return calculateStat(statType);
    }
}
