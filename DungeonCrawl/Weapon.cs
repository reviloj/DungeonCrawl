
using Microsoft.Xna.Framework;

using System.Collections.Generic;
using System;

abstract public class Weapon : Item
{
    private TimeSpan? attackID;

    public Weapon(Weapon weapon) : base(weapon) { }

    public Weapon(Vector2 pos, Vector2 size, Sprite sprite, Sprite thumbnail, string name, string desc, List<Stat> stats, List<Effect> effects) 
        : base(pos, size, sprite, thumbnail, name, desc, stats, effects, new List<ItemSlots> { ItemSlots.Weapon })
    {

    }

    override
    sealed public bool activate()
    {
        //character.states[(int)TypesOfStates.MovementLock] = ObjectStates.ControlLocked;
        attackID = DateTime.Now.TimeOfDay;
        collisionArea.Add(new Vector2(30, -46));
        collisionArea.Add(new Vector2(100, -46));
        collisionArea.Add(new Vector2(100, 74));
        collisionArea.Add(new Vector2(30, 74));
        return true;
    }

    override
    public void deactivate()
    {
        collisionArea.Clear();
    }

    override
    sealed public void collide(CollidableObj obj, List<Vector2>[] axis)
    {
        if (character != null && obj.collisionType == CollisionTypes.Enemy && attackID != null && !((Character)obj).getAttackedByID((TimeSpan)attackID))
        {
            Character target = (Character)obj;
            target.addAttackID((TimeSpan)attackID);
            target.takeDamage(character.useAttackDamage(target));
            if (target.calculateStat(StatTypes.Health) == 0)
            {
                character.addKillCount();
                character.useStat(StatTypes.Kills);
            }
            attackID = null;
        }
    }
}

public class BasicSword : Weapon
{
    public BasicSword(BasicSword basicSword) : base(basicSword) { }

    public BasicSword(Vector2 pos, Sprite sprite, Sprite thumbnail, List<Stat> stats, List<Effect> effects) : base(pos, new Vector2(80, 80), sprite, thumbnail, "Basic Sword", "The starting weapon for scrubs", stats, effects)
    {
        stats.Add(new Stat(25, StatTypes.AttackSpeed, 90, 20));
        stats.Add(new Stat(5, StatTypes.Attack));
    }

    override
    public Item getCopy()
    {
        return new BasicSword(this);
    }
}

public class GoldSword : Weapon
{
    public GoldSword(GoldSword goldSword) : base(goldSword) { }

    public GoldSword(Vector2 pos, Sprite sprite, Sprite thumbnail, List<Stat> stats, List<Effect> effects) : base(pos, new Vector2(80, 80), sprite, thumbnail, "Gold Sword", "A sword with a majestic feel to it", stats, effects)
    {
        stats.Add(new Stat(40, StatTypes.AttackSpeed, 90, 20));
        stats.Add(new Stat(10, StatTypes.Attack));
    }

    override
    public Item getCopy()
    {
        return new GoldSword(this);
    }
}