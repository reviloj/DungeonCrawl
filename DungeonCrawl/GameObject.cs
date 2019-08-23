
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

using System;

abstract public class GameObject : CollidableObj
{
    public Sprite Sprite { get { return sprite; } }
    protected Sprite sprite;
    public Vector2 size;
    public ObjectStates[] states = new ObjectStates[Enum.GetNames(typeof(TypesOfStates)).Length];
    protected ObjectStates[] animationStates = new ObjectStates[Enum.GetNames(typeof(TypesOfStates)).Length];
    //public Stopwatch animationTimer = new Stopwatch();
    // In milliseconds
    public double animationTimer = 0;

    public GameObject(Vector2 pos, Vector2 size, Sprite sprite, CollisionTypes colType) : base(pos, colType)
    {
        this.sprite = sprite;
        this.size = size;
        states[(int)TypesOfStates.Movement] = ObjectStates.Airborne;
        animationStates[(int)TypesOfStates.Movement] = ObjectStates.Airborne;
    }

    override
    public bool setVelocity(Vector2 vel)
    {
        if (states[(int)TypesOfStates.MovementLock] != ObjectStates.VelocityLocked)
            return base.setVelocity(vel);
        return false;
    }
    override
    public bool addVelocity(Vector2 vel)
    {
        if (states[(int)TypesOfStates.MovementLock] != ObjectStates.VelocityLocked)
            return base.addVelocity(vel);
        return false;
    }

    protected void move(float timeDisplacement)
    {
        addPosition(Velocity * timeDisplacement);
    }

    virtual public void update(float timeDisplacement)
    {
        move(timeDisplacement);
        animationTimer += timeDisplacement;
    }

    virtual public void draw(SpriteBatch sb)
    {
        sprite.drawSprite(sb, Position, size, direction);
    }

    virtual public void draw(SpriteBatch sb, Vector2 origin)
    {
        sprite.drawSprite(sb, Position, origin, direction);
    }
}
