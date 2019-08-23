
using Microsoft.Xna.Framework;

using System.Collections.Generic;

abstract public class Skill : Item
{
    protected int duration;
    protected int cooldown;
    protected float cooldownValue = 0;

    public Skill(Skill skill) : base(skill)
    {
        duration = skill.duration;
        cooldown = skill.cooldown;
    }

    public Skill(Vector2 pos, Vector2 size, Sprite sprite, Sprite thumbnail, string name, string desc, List<Stat> stats, List<Effect> effects, int duration, int cooldown) 
        : base(pos, size, sprite, thumbnail, name, desc, stats, effects, new List<ItemSlots> { ItemSlots.Skill1, ItemSlots.Skill2, ItemSlots.Skill3 })
    {
        this.duration = duration;
        this.cooldown = cooldown;
    }

    override
    public void update(float timeDisplacement)
    {
        base.update(timeDisplacement);
        cooldownValue -= timeDisplacement * (hasStat(StatTypes.CoolDownMultiplyer) ? (float)useStat(StatTypes.CoolDownMultiplyer) : 1);
    }

    abstract public int getAnimationIndex(double animationTmer);
}

public class DashAttackSkill : Skill
{
    private int velocity = 30;

    public DashAttackSkill(DashAttackSkill dashAttackSkill) : base(dashAttackSkill) { }

    public DashAttackSkill(Vector2 pos, Sprite sprite, Sprite thumbnail, List<Stat> stats, List<Effect> effects) 
        : base(pos, new Vector2(20, 20), sprite, thumbnail, "Dash Attack", "Lunge forwards and attack", stats, effects, 15, 300)
    {

    }

    override
    public Item getCopy()
    {
        return new DashAttackSkill(this);
    }

    override
    public bool activate()
    {
        if (cooldownValue <= 0)
        {
            cooldownValue = cooldown;
            character.setVelocity(new Vector2(velocity * (int)character.direction, 0));
            character.states[(int)TypesOfStates.MovementLock] = ObjectStates.VelocityLocked;
            collisionArea.Add(new Vector2(0, -46));
            collisionArea.Add(new Vector2(60, -46));
            collisionArea.Add(new Vector2(60, 74));
            collisionArea.Add(new Vector2(0, 74));
            return true;
        }
        return false;
    }

    override
    public void deactivate()
    {
        collisionArea.Clear();
        character.clearSkillAnimationOVerride();
        character.states[(int)TypesOfStates.MovementLock] = ObjectStates.Idle;
        character.setVelocity(new Vector2(0, 0));
    }

    override
    public void collide(CollidableObj obj, List<Vector2>[] axis)
    {
        foreach (Vector2 vec in axis[0])
            if (vec.X != 0)
            {
                deactivate();
                character.attack();
            }
        if(axis[0].Count == 0)
        {
            deactivate();
            character.attack();
        }
    }

    override
    public int getAnimationIndex(double animationTimer)
    {
        if (animationTimer > duration)
            deactivate();
        return (int)(animationTimer / 2.5) % 6 + 14;
    }
}
