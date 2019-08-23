
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;
using System;

public abstract class Enemy : Character
{
    public AI ai { get; protected set; }
    public int maxAgroRange { get; private set; }
    public int maxAgroTime { get; private set; }

    public Enemy(Vector2 pos, Vector2 size, Sprite sprite, string name, string desc, List<Stat> stats, List<Effect> effects, List<Item> items, int maxAgroRange, int maxAgroTime, Rectangle boundingBox) 
        : base(pos, size, sprite, name, desc, stats, effects, items)
    {
        ai = new AI(this, boundingBox);
        this.maxAgroRange = maxAgroRange;
        this.maxAgroTime = maxAgroTime;
    }

    public Enemy(Enemy h) : base(h)
    {
        ai = new AI(h.ai, this);
    }

    override
    public void collide(CollidableObj obj, List<Vector2>[] axis)
    {
        /*if (!(((BuffableObject)obj).useStat(StatTypes.Invincibility) > 0))
        {
            ((Character)obj).takeDamage((float)useStat(StatTypes.Attack));
            obj.setVelocity(new Vector2(0, obj.Velocity.Y));
            obj.addVelocity(new Vector2((float)((Character)obj).calculateStat(StatTypes.Movement) * Math.Sign(obj.Position.X - Position.X), 0));
            obj.direction = (Direction)(-Math.Sign(obj.Position.X - Position.X));
        }*/
    }
}

public class Chort : Enemy
{

    public Chort(Vector2 pos, Sprite sprite, List<Stat> stats, List<Effect> effects) 
        : base(pos, new Vector2(70, 100), sprite, "Chort", "What .. is this........", stats, effects, new List<Item>(), 8000, 6000, new Rectangle(-10, 0, 1200, 800))
    {
        collisionArea.Add(new Vector2(-size.X / 2, -40));
        collisionArea.Add(new Vector2(size.X / 2, -40));
        collisionArea.Add(new Vector2(size.X / 2, 60));
        collisionArea.Add(new Vector2(-size.X / 2, 60));

        stats.Add(new Stat(150, StatTypes.Health, 500, 0));
        stats.Add(new Stat(4, StatTypes.Movement, null, 3));
        stats.Add(new Stat(10, StatTypes.Attack));
        stats.Add(new Stat(10, StatTypes.Jump));

        states[(int)TypesOfStates.Movement] = ObjectStates.Idle;
    }

    public Chort(Chort h) : base(h)
    {

    }

    override
    public Character getCopy()
    {
        return new Chort(this);
    }

    override
    public void draw(SpriteBatch sb, Vector2 origin)
    {
        bool changed = false;
        for (int i = 0; i < Enum.GetNames(typeof(TypesOfStates)).Length; ++i)
            if (animationStates[i] != states[i])
                changed = true;
        if (changed)
        {
            states.CopyTo(animationStates, 0);
            animationTimer = new Random().Next(60);
        }
        
        int startSpriteIndex = 0;
        int spriteIndexOffset = 0;

        if(animationStates[(int)TypesOfStates.Status] == ObjectStates.Stun || animationStates[(int)TypesOfStates.Status] == ObjectStates.Knockback)
        {
            startSpriteIndex = 1;
        }
        else if (animationStates[(int)TypesOfStates.Movement] == ObjectStates.Running)
        {
            spriteIndexOffset = (int)animationTimer / 12 % 4;
            startSpriteIndex = 4;
        }
        else if (animationStates[(int)TypesOfStates.Movement] == ObjectStates.Idle)
        {
            spriteIndexOffset = (int)animationTimer / 12 % 4;
            startSpriteIndex = 0;
        }

        Sprite.drawSpriteSheet(sb, Position, origin, direction, startSpriteIndex, spriteIndexOffset);
    }
}