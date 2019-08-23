
using Microsoft.Xna.Framework;

using System.Collections.Generic;
using System;

abstract public class Character : BuffableObject {

    public Item[] Items { get; } = new Item[Enum.GetNames(typeof(ItemSlots)).Length];
    protected Item[] DefaultItems { get; } = new Item[Enum.GetNames(typeof(ItemSlots)).Length];
    private List<TimeSpan> attackedID = new List<TimeSpan>();
    protected ItemSlots? skillAnimationOverride;

    public Character(Vector2 pos, Vector2 size, Sprite sprite, string name, string desc, List<Stat> stats, List<Effect> effects, List<Item> defItems) : base(pos, size, sprite, CollisionTypes.Enemy, name, desc, stats, effects)
    {
        foreach (Item dI in defItems)
            foreach (ItemSlots iS in dI.AcceptedItemSlot)
                if (DefaultItems[(int)iS] == null)
                {
                    DefaultItems[(int)iS] = dI;
                    break;
                }
        foreach (Item defItem in DefaultItems)
            if (defItem != null)
                equiptItem(defItem.getCopy());
        stats.Add(new Stat(100, StatTypes.CritDamage, null, 50));
        stats.Add(new Stat(0, StatTypes.Kills, null, 0));
    }

    public Character(Character orig)
        : base(new Vector2(orig.Position.X, orig.Position.Y), new Vector2(orig.size.X, orig.size.Y), orig.sprite, orig.collisionType, orig.name, orig.description, new List<Stat>(orig.stats.ConvertAll(s => new Stat(s))), new List<Effect>(orig.effects.ConvertAll(e => new Effect(e))))
    {
        for (int i = 0; i < Enum.GetNames(typeof(ItemSlots)).Length; ++i)
        {
            if (orig.DefaultItems[i] != null)
                DefaultItems[i] = orig.DefaultItems[i].getCopy();
            if (orig.Items[i] != null)
                Items[i] = orig.Items[i].getCopy();
        }
        foreach (Vector2 p in orig.collisionArea)
            collisionArea.Add(new Vector2(p.X, p.Y));
        for (int i = 0; i < states.Length; ++i)
            states[i] = orig.states[i];
        setVelocity(orig.Velocity);
    }

    abstract public Character getCopy();

    virtual public void moveLeft()
    {
        if (setVelocity(new Vector2(-(float)useStat(StatTypes.Movement), Velocity.Y)))
        {
            direction = Direction.Left;
            syncItemDir();
            if (states[(int)TypesOfStates.Movement] != ObjectStates.Airborne)
                states[(int)TypesOfStates.Movement] = ObjectStates.Running;
        }
    }

    virtual public void moveRight()
    {
        if (setVelocity(new Vector2((float)useStat(StatTypes.Movement), Velocity.Y)))
        {
            direction = Direction.Right;
            syncItemDir();
            if (states[(int)TypesOfStates.Movement] != ObjectStates.Airborne)
                states[(int)TypesOfStates.Movement] = ObjectStates.Running;
        }
    }

    virtual public bool jump()
    {
        if (canJump() && setVelocity(new Vector2(Velocity.X, -(float)useStat(StatTypes.Jump))))
        {
            states[(int)TypesOfStates.Movement] = ObjectStates.Airborne;
            return true;
        }
        return false;
    }

    virtual public bool canJump()
    {
        return states[(int)TypesOfStates.Movement] == ObjectStates.Running || states[(int)TypesOfStates.Movement] == ObjectStates.Idle;
    }

    private void syncItemPos()
    {
        foreach (Item item in Items)
            if (item != null)
                item.setPosition(Position);
    }
    private void syncItemVel()
    {
        foreach (Item item in Items)
            if (item != null)
                item.setVelocity(Velocity);
    }
    protected void syncItemDir()
    {
        foreach (Item item in Items)
            if (item != null)
                item.direction = direction;
    }

    override
    public bool setPosition(Vector2 pos)
    {
        if (base.setPosition(pos))
        {
            syncItemPos();
            return true;
        }
        return false;
    }
    override
    public bool addPosition(Vector2 pos)
    {
        if (base.addPosition(pos))
        {
            syncItemPos();
            return true;
        }
        return false;
    }

    override
    public bool setVelocity(Vector2 vel)
    {
        if (base.setVelocity(vel))
        {
            syncItemPos();
            return true;
        }
        return false;
    }
    override
    public bool addVelocity(Vector2 vel)
    {
        if (base.addVelocity(vel))
        {
            syncItemPos();
            return true;
        }
        return false;
    }

    public void swapItem(Item item, ItemSlots slot, Level level)
    {
        level.removeItem(item);
        level.addItem(Items[(int)slot]);
        Items[(int)slot].unEquipt();
        equiptItem(item);
    }

    public void equiptItem(Item item)
    {
        item.equipt(this);
        foreach (ItemSlots slot in item.AcceptedItemSlot)
            if (Items[(int)slot] == null)
            {
                Items[(int)slot] = item;
                return;
            }
        Items[(int)item.AcceptedItemSlot[0]] = item;
    }

    public void unEquiptItem(Item item, Level level)
    {
        item.unEquipt();
        level.addItem(item);
        for(int i = 0; i < DefaultItems.Length; ++i)
            if (DefaultItems[i] != null && item.AcceptedItemSlot.Exists(x => (int)x == i))
                equiptItem(DefaultItems[i].getCopy());
    }

    public void clearSkillAnimationOVerride()
    {
        skillAnimationOverride = null;
    }

    public float useAttackDamage(BuffableObject target)
    {
        float? killChance = useStat(StatTypes.InstantKill, target);
        if (killChance != null && new Random().Next(100) + 1 < killChance)
            return 999999999;
        else
        {
            float? attack = useStat(StatTypes.Attack, target);
            if (attack != null)
            {
                float? critChance = useStat(StatTypes.CritChance, target);
                if (critChance != null && new Random().Next(100) + 1 < critChance)
                    return (float)attack * (1 + (float)useStat(StatTypes.CritDamage, target) / 100);
                else
                    return (float)attack;
            }
        }
        return 0;
    }
    
    public void addAttackID(TimeSpan ID)
    {
        attackedID.Add(ID);
    }

    public bool getAttackedByID(TimeSpan ID)
    {
        for (int i = attackedID.Count - 1; i >= 0; --i)
            if ((DC.DungeonCrawl.gameTime.TotalGameTime - attackedID[i]).TotalSeconds > 5)
                attackedID.Remove(attackedID[i]);
        return attackedID.Exists(x => x == ID);
    }

    public void addKillCount()
    {
        stats.Find(x => x.statType == StatTypes.Kills).Value++;

        float? vampirism = calculateStat(StatTypes.Vampirism);
        if (vampirism != null)
            stats.Find(x => x.statType == StatTypes.Health).Value += (float)vampirism;
    }

    public void handleItemCollisions(GameObject obj, float timeDisplacement)
    {
        if(Items[(int)ItemSlots.Weapon] != null)
        Items[(int)ItemSlots.Weapon].handleCollision(obj, timeDisplacement);
        if (Items[(int)ItemSlots.Skill1] != null)
            Items[(int)ItemSlots.Skill1].handleCollision(obj, timeDisplacement);
        if (Items[(int)ItemSlots.Skill2] != null)
            Items[(int)ItemSlots.Skill2].handleCollision(obj, timeDisplacement);
        if (Items[(int)ItemSlots.Skill3] != null)
            Items[(int)ItemSlots.Skill3].handleCollision(obj, timeDisplacement);
    }

    public void attack()
    {
        if (states[(int)TypesOfStates.Attack] != ObjectStates.Attacking)
        {
            states[(int)TypesOfStates.Attack] = ObjectStates.Attacking;
            animationStates[(int)TypesOfStates.Attack] = ObjectStates.Attacking;
            Items[(int)ItemSlots.Weapon].activate();
        }
    }

    virtual public void takeDamage(float damage)
    {
        if (skillAnimationOverride != null)
            Items[(int)skillAnimationOverride].deactivate();
        float? block = useStat(StatTypes.Block);
        if (block != null)
            damage -= (float)block;
        float? armour = useStat(StatTypes.Armour);
        if (armour != null)
            damage *= 1 - (float)armour;
        if (damage > 0)
        {
            useStat(StatTypes.Health);
            stats.Find(x => x.statType == StatTypes.Health).Value -= damage;
            states[(int)TypesOfStates.Attack] = ObjectStates.Idle;
        }
    }

    override
    public void update(float timeDisplacement)
    {
        base.update(timeDisplacement);
        foreach (Item item in Items)
            if (item != null)
                item.update(timeDisplacement);
        syncItemPos();
        syncItemVel();
        if (states[(int)TypesOfStates.Movement] != ObjectStates.Airborne && Velocity.X == 0)
            states[(int)TypesOfStates.Movement] = ObjectStates.Idle;
        if (Math.Abs(Velocity.Y) > 0.1)
            states[(int)TypesOfStates.Movement] = ObjectStates.Airborne;
    }

    override
    public float? calculateStat(StatTypes statType)
    {
        float? sum = null;
        sum = base.calculateStat(statType);
        foreach (Item item in Items)
            if (item != null)
            {
                float? f = item.calculateStat(statType);
                if (sum != null)
                    sum += (float)((f != null) ? f : 0);
                else
                    sum = (float)((f != null) ? f : 0);
            }
        return sum;
    }
}

