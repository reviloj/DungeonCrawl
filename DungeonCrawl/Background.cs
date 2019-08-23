
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using System;

public class Background
{
    public static float layerSpeedDiff = 0.8f;
    private int layers;
    public string Name { get; }
    public Texture2D Sprite { get; }
    public Vector2 IndividualSpriteSize { get; }
    private Vector2 numSprites;

    public Background(string name, Texture2D sprite, Vector2 gridSize, int layers)
    {
        this.layers = layers;
        Name = name;
        Sprite = sprite;
        IndividualSpriteSize = gridSize;
        numSprites = new Vector2(sprite.Width, sprite.Height) / IndividualSpriteSize;
        if (numSprites.X != (int)numSprites.X || numSprites.Y != (int)numSprites.Y)
            throw new Exception();
    }


    public void draw(SpriteBatch sb, Vector2 origin)
    {
        for (int i = 0; i < layers; ++i)
        {
            double layerSpeed = 0.2 + i * (layerSpeedDiff / (layers - 1));
            Vector2 layerPos = new Vector2((float)(-origin.X * layerSpeed % IndividualSpriteSize.X), IndividualSpriteSize.Y - 720);
            int width = (IndividualSpriteSize.X >= layerPos.X + 1280) ? 1280 : (int)(IndividualSpriteSize.X - layerPos.X);
            sb.Draw(Sprite, new Vector2(0, 0) * DC.DungeonCrawl.ResolustionScale,
                new Rectangle((int)(IndividualSpriteSize.X * (i % numSprites.X) + layerPos.X), (int)(IndividualSpriteSize.Y * (int)(i / numSprites.X) + layerPos.Y), width, 720),
                Color.White, 0.0f, new Vector2(0, 0), DC.DungeonCrawl.ResolustionScale, SpriteEffects.None, 0.0f);
            sb.Draw(Sprite, new Vector2(width, 0) * DC.DungeonCrawl.ResolustionScale,
                new Rectangle((int)(IndividualSpriteSize.X * (i % numSprites.X)), (int)(IndividualSpriteSize.Y * (int)(i / numSprites.X) + layerPos.Y), 1280 - width, 720),
                Color.White, 0.0f, new Vector2(0, 0), DC.DungeonCrawl.ResolustionScale, SpriteEffects.None, 0.0f);
        }
    }
}
