
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;

abstract public class Item : BuffableObject
{
    private List<Vector2> thumbNailCollisionArea = new List<Vector2>();
    public List<ItemSlots> AcceptedItemSlot { get; }
    public bool equipted { get; }
    protected Character character;
    protected Sprite thumbNail;

    public Item(Item item) : this(new Vector2(item.Position.X, item.Position.X), new Vector2(item.size.X, item.size.X), item.sprite, item.thumbNail, item.name, item.description, new List<Stat>(item.stats), new List<Effect>(item.effects), new List<ItemSlots>(item.AcceptedItemSlot))
    {

    }

    public Item(Vector2 pos, Vector2 size, Sprite sprite, Sprite thumbNail, string name, string desc, List<Stat> stats, List<Effect> effects, List<ItemSlots> itemSlot) 
        : base(pos, size, sprite, CollisionTypes.Item, name, desc, stats, effects)
    {
        AcceptedItemSlot = itemSlot;
        this.thumbNail = thumbNail;
        thumbNailCollisionArea.Add(new Vector2(-size.X / 2, -size.Y / 2));
        thumbNailCollisionArea.Add(new Vector2(size.X / 2, -size.Y / 2));
        thumbNailCollisionArea.Add(new Vector2(size.X / 2, size.Y / 2));
        thumbNailCollisionArea.Add(new Vector2(-size.X / 2, size.Y / 2));
        collisionArea = new List<Vector2>(thumbNailCollisionArea);
    }

    abstract public Item getCopy();

    public void equipt(Character character)
    {
        collisionArea.Clear();
        this.character = character;
        collisionType = CollisionTypes.ActiveItem;
    }

    public void unEquipt()
    {
        collisionArea = new List<Vector2>(thumbNailCollisionArea);
        character = null;
        collisionType = CollisionTypes.Item;
        setVelocity(new Vector2(0, -1));
    }

    abstract public bool activate();

    abstract public void deactivate();

    override
    public void draw(SpriteBatch sb, Vector2 origin)
    {
        if(character == null)
        {
            thumbNail.drawSprite(sb, Position, origin, direction);
        }
    }
}
