
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;

public class Chest : GameObject
{
    public bool Open { get; private set; } = false;
    private List<Item> items;

    public Chest(Vector2 pos, Sprite sprite, List<Item> items) : base(pos, sprite.IndividualSpriteSize, sprite, CollisionTypes.Obstacle)
    {
        collisionArea.Add(new Vector2(-size.X / 2, -size.Y / 2));
        collisionArea.Add(new Vector2(size.X / 2, -size.Y / 2));
        collisionArea.Add(new Vector2(size.X / 2, size.Y / 2));
        collisionArea.Add(new Vector2(-size.X / 2, size.Y / 2));
        this.items = items;
        foreach (Item item in items)
            item.setPosition(Position);
    }

    public List<Item> releaseItems()
    {
        for (int i = 0; i < items.Count; ++i)
            items[i].setVelocity(new Vector2((float)(-0.75 + (1.5 / (items.Count + 1) * (i + 1))), -2));
        List<Item> r = new List<Item>(items);
        items.Clear();
        return r;
    }

    public void open()
    {
        Open = true;
        animationTimer = 0;
    }

    override
    public void draw(SpriteBatch sb, Vector2 origin)
    {
        if(Open)
            sprite.drawSpriteSheet(sb, Position, origin, direction, 1, ((int)animationTimer / 200 > 1) ? 1 : (int)animationTimer / 200);
        else
            sprite.drawSpriteSheet(sb, Position, origin, direction, 0, 0);
    }

    override
    public void collide(CollidableObj obj, List<Vector2>[] axis) { }

}

