
using Microsoft.Xna.Framework;

using System.Collections.Generic;
using System;

public class Effect
{
    protected Buff buff;
    protected List<StatTypes> triggerStats;
    protected EffectTargets effectTarget;

    public Effect(Buff buff, List<StatTypes> trigStats, EffectTargets target)
    {
        this.buff = buff;
        triggerStats = trigStats;
        effectTarget = target;
    }

    public Effect(Effect e) : this(new Buff(e.buff), new List<StatTypes>(e.triggerStats), e.effectTarget) { }

    public float triggerEffect(StatTypes statType, BuffableObject obj, BuffableObject target = null)
    {
        if (triggerStats.Exists(x => x == statType))
        {
            if (effectTarget == EffectTargets.Target)
            {
                if (buff != null)
                    target.addBuff(new Buff(buff));
                objectEffect(obj, target);
            }
            else
            {
                if (buff != null)
                    obj.addBuff(new Buff(buff));
                objectEffect(target, obj);
            }
        }
        return 0;
    }

    virtual protected void objectEffect(BuffableObject obj, BuffableObject target)
    {

    }
}

public class KnockbackEffect : Effect
{
    public KnockbackEffect(Buff buff, List<StatTypes> trigStats, EffectTargets target) : base(buff, trigStats, target)
    {

    }

    public KnockbackEffect(KnockbackEffect e) : base(e)
    {

    }

    override
    protected void objectEffect(BuffableObject obj, BuffableObject target)
    {
        target.states[(int)TypesOfStates.MovementLock] = ObjectStates.ControlLocked;
        target.states[(int)TypesOfStates.Status] = ObjectStates.Knockback;
        target.states[(int)TypesOfStates.Attack] = ObjectStates.Idle;
        Vector2 vec = target.Position - obj.Position;
        float scale = (float)Math.Sqrt(Math.Pow(BuffableObject.KNOCKBACK_SPEED , 2) / (Math.Pow(Math.Abs(vec.X), 2) + Math.Pow(Math.Abs(vec.Y), 2)));
        target.setVelocity(scale * vec);
    }
}

public class StunEffect : Effect
{
    public StunEffect(Buff buff, List<StatTypes> trigStats, EffectTargets target) : base(buff, trigStats, target)
    {

    }

    public StunEffect(KnockbackEffect e) : base(e)
    {

    }

    override
    protected void objectEffect(BuffableObject obj, BuffableObject target)
    {
        target.states[(int)TypesOfStates.MovementLock] = ObjectStates.ControlLocked;
        target.states[(int)TypesOfStates.Status] = ObjectStates.Stun;
        target.states[(int)TypesOfStates.Attack] = ObjectStates.Idle;
        target.setVelocity(new Vector2(0, target.Velocity.Y));
    }
}

