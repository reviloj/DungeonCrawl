
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

using System;
using System.Collections.Generic;

public class Map : GameState
{
    private Action<SpriteBatch> backGroundStateDraw;
    private Texture2D drawingPixel;
    private Level level;
    private Hero hero;
    private Vector2 borderSize = new Vector2(1080, 520);
    private Vector2 origMapSize = new Vector2(980, 420);
    private int mapZoom = 3;
    private Vector2 mapOrigin;
    Vector2 mapSize;

    public Map(Action<SpriteBatch> backGroundStateDraw, Level level, Hero hero)
    {
        this.backGroundStateDraw = backGroundStateDraw;
        this.level = level;
        this.hero = hero;
        mapSize = new Vector2((level.Size.X / mapZoom < origMapSize.X) ? level.Size.X / mapZoom : origMapSize.X, (level.Size.Y / mapZoom < origMapSize.Y) ? level.Size.Y / mapZoom : origMapSize.Y);
        mapOrigin = hero.Position;
        if (mapSize.X != origMapSize.X)
            mapOrigin.X = level.Size.X / 2;
        else
            mapOrigin.X = (mapOrigin.X < origMapSize.X/ 2 * mapZoom - 48) ? origMapSize.X / 2 * mapZoom - 48 :
                (mapOrigin.X > level.Size.X + 48 - origMapSize.X / 2 * mapZoom) ? level.Size.X + 48 - origMapSize.X / 2 * mapZoom : mapOrigin.X;
        if (mapSize.Y != origMapSize.Y)
            mapOrigin.Y = -level.Size.Y / 2;
        else
            mapOrigin.Y = (mapOrigin.Y > -origMapSize.Y / 2 * mapZoom + 48) ? -origMapSize.Y / 2 * mapZoom + 48 :
                (mapOrigin.Y < -(level.Size.Y + 48 - origMapSize.Y / 2 * mapZoom)) ? -(level.Size.Y + 48 - origMapSize.Y / 2 * mapZoom) : mapOrigin.Y;
    }

    override
    public void loadContent(Dictionary<string, Texture2D> textures)
    {
        drawingPixel = textures["DrawingPixel"];
    }

    override
    public GameState update(float timeDisplacement)
    {
        TouchCollection touchCollection = TouchPanel.GetState();
        if (touchCollection.Count > 0)
            foreach (TouchLocation tl in touchCollection) {
                TouchLocation prevTL;
                if (tl.State == TouchLocationState.Released
                    && tl.Position.X > 1070 * DC.DungeonCrawl.ResolustionScale && tl.Position.X < 1166 * DC.DungeonCrawl.ResolustionScale
                    && tl.Position.Y > 50 * DC.DungeonCrawl.ResolustionScale && tl.Position.Y < 146 * DC.DungeonCrawl.ResolustionScale)
                    return this;
                else if (tl.State == TouchLocationState.Moved
                    && tl.Position.X > (640 - mapSize.X / 2) * DC.DungeonCrawl.ResolustionScale && tl.Position.X < (640 + mapSize.X / 2) * DC.DungeonCrawl.ResolustionScale
                    && tl.Position.Y > (360 - mapSize.Y / 2) * DC.DungeonCrawl.ResolustionScale && tl.Position.Y < (360 + mapSize.Y / 2) * DC.DungeonCrawl.ResolustionScale)
                {
                    tl.TryGetPreviousLocation(out prevTL);
                    moveMap((prevTL.Position - tl.Position) * mapZoom / DC.DungeonCrawl.ResolustionScale);
                }
                    }
        return null;
    }

    private void moveMap(Vector2 move)
    {
        if (mapSize.X == origMapSize.X)
            mapOrigin.X = (mapOrigin.X + move.X < origMapSize.X / 2 * mapZoom - 48) ? origMapSize.X / 2 * mapZoom - 48 : 
                (mapOrigin.X + move.X > level.Size.X + 48 - origMapSize.X / 2 * mapZoom) ? level.Size.X + 48 - origMapSize.X / 2 * mapZoom : mapOrigin.X + move.X;
        if (mapSize.Y == origMapSize.Y)
            mapOrigin.Y = (mapOrigin.Y + move.Y > -origMapSize.Y / 2 * mapZoom + 48) ? -origMapSize.Y / 2 * mapZoom + 48 :
                (mapOrigin.Y + move.Y < -(level.Size.Y + 48 - origMapSize.Y / 2 * mapZoom)) ? -(level.Size.Y + 48 - origMapSize.Y / 2 * mapZoom) : mapOrigin.Y + move.Y;
    }

    private Vector2[] calculatePosSpriteOffsets(Vector2 position, Vector2 size)
    {
        Vector2 posOffset = new Vector2(0, 0);
        Vector2 sizeOffset = new Vector2(0, 0);
        if (position.X - size.X / 2 < mapOrigin.X - mapSize.X / 2 * mapZoom)
        {
            posOffset.X = (mapOrigin.X - mapSize.X / 2 * mapZoom) - (position.X - size.X / 2);
            sizeOffset.X += posOffset.X;
        }
        if (position.X + size.X / 2 > mapOrigin.X + mapSize.X / 2 * mapZoom)
            sizeOffset.X += (position.X + size.X / 2) - (mapOrigin.X + mapSize.X / 2 * mapZoom);
        if (position.Y - size.Y / 2 < mapOrigin.Y - mapSize.Y / 2 * mapZoom)
        {
            posOffset.Y = (mapOrigin.Y - mapSize.Y / 2 * mapZoom) - (position.Y - size.Y / 2);
            sizeOffset.Y += posOffset.Y;
        }
        if (position.Y + size.Y / 2 > mapOrigin.Y + mapSize.Y / 2 * mapZoom)
            sizeOffset.Y += (position.Y + size.Y / 2) - (mapOrigin.Y + mapSize.Y / 2 * mapZoom);
        posOffset /= mapZoom;
        sizeOffset /= mapZoom;
        return new Vector2[] { posOffset, sizeOffset };
    }

    override
    public void draw(SpriteBatch sb)
    {
        backGroundStateDraw(sb);
        sb.Draw(drawingPixel, new Vector2(100, 100) * DC.DungeonCrawl.ResolustionScale, null, new Color(165, 165, 165, 150), 0f, Vector2.Zero, borderSize * DC.DungeonCrawl.ResolustionScale, SpriteEffects.None, 0f);
        Vector2 mapDrawingCorner = new Vector2(150 + ((mapSize.X < origMapSize.X) ? (origMapSize.X - mapSize.X) / 2 : 0), 150 + ((mapSize.Y < origMapSize.Y) ? (origMapSize.Y - mapSize.Y) / 2 : 0));
        sb.Draw(drawingPixel, mapDrawingCorner * DC.DungeonCrawl.ResolustionScale, null, new Color(100, 100, 100, 200), 0f, Vector2.Zero, mapSize * DC.DungeonCrawl.ResolustionScale, SpriteEffects.None, 0f);

        Vector2 heroMapPos = hero.Position + new Vector2(0, hero.Sprite.IndividualSpriteSize.Y / 2 - hero.size.Y / 2);
        if (heroMapPos.X + hero.size.X / 2 > mapOrigin.X - mapSize.X / 2 * mapZoom && heroMapPos.X - hero.size.X / 2 < mapOrigin.X + mapSize.X / 2 * mapZoom
            && heroMapPos.Y + hero.size.Y / 2 > mapOrigin.Y - mapSize.Y / 2 * mapZoom && heroMapPos.Y - hero.size.Y / 2 < mapOrigin.Y + mapSize.Y / 2 * mapZoom)
        {
            Vector2[] posSizeOffset = calculatePosSpriteOffsets(heroMapPos, hero.size);
            sb.Draw(drawingPixel,
                (new Vector2(640, 360) + (heroMapPos - hero.size / 2  - mapOrigin) / mapZoom + posSizeOffset[0]) * DC.DungeonCrawl.ResolustionScale,
                null, new Color(144, 248, 144, 200), 0f, Vector2.Zero, (hero.size / mapZoom - posSizeOffset[1]) * DC.DungeonCrawl.ResolustionScale, SpriteEffects.None, 0f);
        }
        foreach (SortedList<int, Platform> list in level.Platforms.Values)
            foreach (Platform p in list.Values)
                if (p.Position.X + p.size.X / 2 > mapOrigin.X - mapSize.X / 2 * mapZoom && p.Position.X - p.size.X / 2 < mapOrigin.X + mapSize.X / 2 * mapZoom
                && p.Position.Y + p.size.Y / 2 > mapOrigin.Y - mapSize.Y / 2 * mapZoom && p.Position.Y - p.size.Y / 2 < mapOrigin.Y + mapSize.Y / 2 * mapZoom)
                {
                    Vector2[] posSizeOffset = calculatePosSpriteOffsets(p.Position, p.size);
                    sb.Draw(drawingPixel, (new Vector2(640, 360) + (p.Position - p.size / 2 - mapOrigin) / mapZoom + posSizeOffset[0]) * DC.DungeonCrawl.ResolustionScale,
                        null, new Color(0, 0, 0, 200), 0f, Vector2.Zero, (p.size / mapZoom - posSizeOffset[1]) * DC.DungeonCrawl.ResolustionScale, SpriteEffects.None, 0f);
                }

    }
}