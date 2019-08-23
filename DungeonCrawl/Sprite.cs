
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using System;

public class Sprite
{
    public string Name { get; }
    public Texture2D sprite { get; private set; }
    public Vector2 IndividualSpriteSize { get; }
    private Vector2 numSprites;

    public Sprite(string name, Texture2D sprite)
    {
        Name = name;
        this.sprite = sprite;
        IndividualSpriteSize = new Vector2(sprite.Width, sprite.Height);
    }

    public Sprite(string name, Texture2D sprite, Vector2 gridSize)
    {
        Name = name;
        this.sprite = sprite;
        IndividualSpriteSize = gridSize;
        numSprites = new Vector2(sprite.Width, sprite.Height) / IndividualSpriteSize;
        if (numSprites.X != (int)numSprites.X || numSprites.Y != (int)numSprites.Y)
            throw new Exception();
    }

    public void drawSprite(SpriteBatch sb, Vector2 position, Direction direction)
    {
        sb.Draw(sprite, (position - new Vector2(sprite.Width, sprite.Height) / 2) * DC.DungeonCrawl.ResolustionScale,
                new Rectangle(0, 0, sprite.Width, sprite.Height), Color.White, 0.0f, new Vector2(0, 0), DC.DungeonCrawl.ResolustionScale,
                (direction > 0) ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.0f);
    }
    public void drawSprite(SpriteBatch sb, Vector2 position, Vector2 origin, Direction direction)
    {
        sb.Draw(sprite, (position - new Vector2(sprite.Width, sprite.Height) / 2 + origin) * DC.DungeonCrawl.ResolustionScale,
                new Rectangle(0, 0, sprite.Width, sprite.Height), Color.White, 0.0f, new Vector2(0, 0), DC.DungeonCrawl.ResolustionScale,
                (direction > 0) ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.0f);
    }
    public void drawSpriteSheet(SpriteBatch sb, Vector2 position, Direction direction, int startSpriteIndex, int spriteIndexOffset)
    {
        sb.Draw(sprite, (position - IndividualSpriteSize / 2) * DC.DungeonCrawl.ResolustionScale,
                new Rectangle((int)(IndividualSpriteSize.X * ((startSpriteIndex + spriteIndexOffset) % numSprites.X)), (int)(IndividualSpriteSize.Y * ((startSpriteIndex + spriteIndexOffset) / numSprites.Y)), (int)IndividualSpriteSize.X, (int)IndividualSpriteSize.Y), 
                Color.White, 0.0f, new Vector2(0, 0), DC.DungeonCrawl.ResolustionScale,
                (direction > 0) ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.0f);
    }
    public void drawSpriteSheet(SpriteBatch sb, Vector2 position, Vector2 origin, Direction direction, int startSpriteIndex, int spriteIndexOffset)
    {
        sb.Draw(sprite, (position - IndividualSpriteSize / 2 + origin) * DC.DungeonCrawl.ResolustionScale,
                new Rectangle((int)(IndividualSpriteSize.X * ((startSpriteIndex + spriteIndexOffset) % numSprites.X)), (int)(IndividualSpriteSize.Y * (int)((startSpriteIndex + spriteIndexOffset) / numSprites.X)), (int)IndividualSpriteSize.X, (int)IndividualSpriteSize.Y),
                Color.White, 0.0f, new Vector2(0, 0), DC.DungeonCrawl.ResolustionScale,
                (direction == Direction.Right) ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.0f);
    }
}
