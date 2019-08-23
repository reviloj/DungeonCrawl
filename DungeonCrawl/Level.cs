
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Threading;

public class Level
{
    public const int MAX_PLATFORM_BOUND = 360;
    public Vector2 Size { get; private set; }
    public SortedVector2List<Platform> Platforms { get; } = new SortedVector2List<Platform>();
    public List<Item> Items { get; } = new List<Item>();
    public List<Chest> Chests { get; } = new List<Chest>();
    private Background background;
    Dictionary<string, Sprite> ss;

    public Level(Vector2 size, int difficulty, Dictionary<string, Sprite> sprites, Dictionary<string, Background> backgrounds, float gravity)
    {
        Size = size;
        Platforms.Add(new Vector2(550, -100), new Platform(new Vector2(550, -100), sprites[Platform.HORIZONTAL_PLAT].IndividualSpriteSize, sprites[Platform.HORIZONTAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, sprites[Platform.HORIZONTAL_DECOR]));
        Platforms.Add(new Vector2(250, -200), new Platform(new Vector2(250, -200), sprites[Platform.HORIZONTAL_PLAT].IndividualSpriteSize, sprites[Platform.HORIZONTAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, null));
        Platforms.Add(new Vector2(1000, -180), new Platform(new Vector2(1000, -180), sprites[Platform.HORIZONTAL_PLAT].IndividualSpriteSize, sprites[Platform.HORIZONTAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, null));
        Platforms.Add(new Vector2(1300, -100), new Platform(new Vector2(1300, -100), sprites[Platform.HORIZONTAL_PLAT].IndividualSpriteSize, sprites[Platform.HORIZONTAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, null));
        Platforms.Add(new Vector2(1348, -200), new Platform(new Vector2(1348, -200), sprites[Platform.VERTICAL_PLAT].IndividualSpriteSize, sprites[Platform.VERTICAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, null));
        Platforms.Add(new Vector2(650, -380), new PlatformCollection(new Vector2(650, -380), 3, 1, sprites[Platform.HORIZONTAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, sprites[Platform.HORIZONTAL_DECOR]));

        Platform ground = new PlatformCollection(new Vector2(0, 0), new Vector2(size.X, 100), sprites[Platform.SQUARE_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Soil, sprites[Platform.HORIZONTAL_DECOR]);
        ground.addPosition(ground.size / 2);
        foreach (Platform p in ground.splitPlatform())
            Platforms.Add(p.Position, p);

        Platform wall = new PlatformCollection(new Vector2(-sprites[Platform.VERTICAL_PLAT].IndividualSpriteSize.X / 2, -size.Y / 2),
            new Vector2(sprites[Platform.VERTICAL_PLAT].IndividualSpriteSize.X, size.Y), sprites[Platform.VERTICAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, null);
        foreach (Platform p in wall.splitPlatform())
            Platforms.Add(p.Position, p);
        wall = new PlatformCollection(new Vector2(sprites[Platform.VERTICAL_PLAT].IndividualSpriteSize.X / 2 + size.X, -size.Y / 2),
            new Vector2(sprites[Platform.VERTICAL_PLAT].IndividualSpriteSize.X, size.Y), sprites[Platform.VERTICAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, null);
        foreach (Platform p in wall.splitPlatform())
            Platforms.Add(p.Position, p);


        ss = sprites;
        /*List<Platform> plats = new LevelGenerator(0, 0, 0).getPlatforms(new Vector2(5500, -4000), sprites);
        foreach (Platform p in plats)
            foreach (Platform pl in p.splitPlatform())
                Platforms.Add(pl.Position, pl);*/

        background = backgrounds["DarkForestBG"];
        Items.Add(new GoldSword(new Vector2(350, -100), sprites["GoldSwordHero"], sprites["GoldSwordTN"], new List<Stat>(), new List<Effect>()));
        Chests.Add(new Chest(new Vector2(1100, -sprites["EmptyChest"].IndividualSpriteSize.Y / 2), sprites["EmptyChest"], new List<Item> { new GoldSword(new Vector2(), sprites["GoldSwordHero"], sprites["GoldSwordTN"], new List<Stat>(), new List<Effect>()) }));
    }

    public void drawLevelBack(SpriteBatch sb, Vector2 origin, Rectangle drawingArea)
    {
        background.draw(sb, origin);
        foreach (Chest chest in Chests)
            if (!Rectangle.Intersect(new Rectangle(chest.Position.ToPoint() - chest.size.ToPoint(), (chest.size * 2).ToPoint()), drawingArea).IsEmpty)
                chest.draw(sb, origin);
    }

    public void drawLevelFront(SpriteBatch sb, Vector2 origin, Rectangle drawingArea)
    {
        foreach (Item item in Items)
            if (!Rectangle.Intersect(new Rectangle(item.Position.ToPoint() - item.size.ToPoint(), (item.size * 2).ToPoint()), drawingArea).IsEmpty)
                item.draw(sb, origin);
        foreach (SortedList<int, Platform> list in Platforms.Values)
            foreach (Platform platform in list.Values)
                if (!Rectangle.Intersect(new Rectangle(platform.Position.ToPoint() - platform.size.ToPoint(), (platform.size * 2).ToPoint()), drawingArea).IsEmpty)
                    platform.draw(sb, origin);
    }

    public void update(float timeDisplacement, Character hero, AIManager aiManager, float gravity)
    {
        if (DC.DungeonCrawl.debugVar)
        {
            DC.DungeonCrawl.debugVar = false;

            Platforms.RemoveAll();


            Platforms.Add(new Vector2(550, -100), new Platform(new Vector2(550, -100), ss[Platform.HORIZONTAL_PLAT].IndividualSpriteSize, ss[Platform.HORIZONTAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, ss[Platform.HORIZONTAL_DECOR]));
            Platforms.Add(new Vector2(250, -200), new Platform(new Vector2(250, -200), ss[Platform.HORIZONTAL_PLAT].IndividualSpriteSize, ss[Platform.HORIZONTAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, null));
            Platforms.Add(new Vector2(1000, -180), new Platform(new Vector2(1000, -180), ss[Platform.HORIZONTAL_PLAT].IndividualSpriteSize, ss[Platform.HORIZONTAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, null));
            Platforms.Add(new Vector2(1300, -100), new Platform(new Vector2(1300, -100), ss[Platform.HORIZONTAL_PLAT].IndividualSpriteSize, ss[Platform.HORIZONTAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, null));
            Platforms.Add(new Vector2(1348, -200), new Platform(new Vector2(1348, -200), ss[Platform.VERTICAL_PLAT].IndividualSpriteSize, ss[Platform.VERTICAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, null));
            Platforms.Add(new Vector2(650, -380), new PlatformCollection(new Vector2(650, -380), 3, 1, ss[Platform.HORIZONTAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, ss[Platform.HORIZONTAL_DECOR]));
            Platform ground = new PlatformCollection(new Vector2(0, 0), new Vector2(Size.X, 100), ss[Platform.SQUARE_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Soil, ss[Platform.HORIZONTAL_DECOR]);
            ground.addPosition(ground.size / 2);
            Platforms.Add(ground.Position, ground);
            Platforms.Add(new Vector2(-ss[Platform.VERTICAL_PLAT].IndividualSpriteSize.X / 2, -Size.Y / 2),
                new Platform(new Vector2(-ss[Platform.VERTICAL_PLAT].IndividualSpriteSize.X / 2, -Size.Y / 2),
                new Vector2(ss[Platform.VERTICAL_PLAT].IndividualSpriteSize.X, Size.Y), ss[Platform.VERTICAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, null));
            Platforms.Add(new Vector2(ss[Platform.VERTICAL_PLAT].IndividualSpriteSize.X / 2 + Size.X, -Size.Y / 2),
                new Platform(new Vector2(ss[Platform.VERTICAL_PLAT].IndividualSpriteSize.X / 2 + Size.X, -Size.Y / 2),
                new Vector2(ss[Platform.VERTICAL_PLAT].IndividualSpriteSize.X, Size.Y), ss[Platform.VERTICAL_PLAT], LevelThemes.Cosmos, PlatformSubTheme.Stone, null));



            List<Platform> plats = new LevelGenerator(0, 0, 0).getPlatforms(new Vector2(5500, -4000), ss);
            int max = -3000;

            for (int i = 0; i < plats.Count; ++i)
                if (plats[i].Position.Y > max)
                    max = (int)plats[i].Position.Y;
            max = -150 - max;
            for (int i = 0; i < plats.Count; ++i)
                plats[i].setPosition(new Vector2(plats[i].Position.X, plats[i].Position.Y + max));


            foreach (Platform p in plats)
                foreach (Platform pl in p.splitPlatform())
                    Platforms.Add(pl.Position, pl);
        }
        foreach (Item item in Items)
            item.update(timeDisplacement);
        foreach (Chest chest in Chests)
        {
            chest.update(timeDisplacement);
            if (chest.Open)
                Items.AddRange(chest.releaseItems());
        }
    }

    public void removeItem(Item item)
    {
        Items.Remove(item);
    }

    public void addItem(Item item)
    {
        Items.Add(item);
    }

    private class LevelGenerator
    {

        private class PlatformFrame
        {
            public string type;
            public Vector2 relativePos;
            public Vector2 size;
            public List<Vector2> connections = new List<Vector2>();
            public Vector2 platformTypeBounds;
            public const int continuousPlatformXBound = 60;

            public PlatformFrame(string type, Vector2 relativePos, Vector2 size, List<Vector2> connections)
            {
                this.type = type;
                this.relativePos = relativePos;
                this.size = size;
                this.connections = connections;
                if (type == Platform.HORIZONTAL_PLAT)
                    platformTypeBounds = new Vector2(72, 24);
                else if (type == Platform.SQUARE_PLAT)
                    platformTypeBounds = new Vector2(continuousPlatformXBound, 72);
            }

            public PlatformFrame(PlatformFrame pF)
            {
                type = pF.type;
                relativePos = pF.relativePos;
                size = pF.size;
                connections.AddRange(pF.connections);
                platformTypeBounds = pF.platformTypeBounds;
            }
        }

        private class SetPiece
        {
            public Vector2 position;
            public List<Vector2> connectionPoints
            {
                get
                {
                    List<Vector2> ret = new List<Vector2>();
                    foreach (PlatformFrame pF in platforms)
                        ret.AddRange(pF.connections);
                    return ret;
                }
            }
            public List<PlatformFrame> platforms;
            public Vector2 bounds = new Vector2();
            public Vector2 flipable;

            public SetPiece(List<PlatformFrame> plats, Vector2 flipable, Vector2 position = new Vector2())
            {
                this.position = position;
                platforms = plats;
                this.flipable = flipable;
                foreach (PlatformFrame p in platforms)
                {
                    if (Math.Abs(p.relativePos.X) + p.platformTypeBounds.X + (p.size.X - 1) * PlatformFrame.continuousPlatformXBound > bounds.X)
                        bounds.X = Math.Abs(p.relativePos.X) + p.platformTypeBounds.X + (p.size.X - 1) * PlatformFrame.continuousPlatformXBound;
                    if (Math.Abs(p.relativePos.Y) + p.platformTypeBounds.Y * p.size.Y > bounds.Y)
                        bounds.Y = Math.Abs(p.relativePos.Y) + p.platformTypeBounds.Y * p.size.Y;
                }
            }

            public SetPiece(SetPiece sp)
            {
                position = sp.position;
                platforms = new List<PlatformFrame>(sp.platforms.ConvertAll(pF => new PlatformFrame(pF)));
                bounds = sp.bounds;
                flipable = sp.flipable;
            }

            virtual public void stretch(int platform, int amount, Direction direction)
            {
                int pixelAmount = amount * 132;
                if (platform + 1 != platforms.Count)
                    if ((platforms[platform + 1].relativePos.X - platforms[platform].relativePos.X) * (int)direction > 0)
                        shiftPlatforms(pixelAmount, direction, platform, 1);
                if (platform != 0)
                    if ((platforms[platform - 1].relativePos.X - platforms[platform].relativePos.X) * (int)direction > 0)
                        shiftPlatforms(pixelAmount, direction, platform, -1);

                for (int i = 0; i < platforms[platform].connections.Count; ++i)
                    platforms[platform].connections[i] = platforms[platform].connections[i] + new Vector2(pixelAmount / 2 * Math.Sign(platforms[platform].connections[i].X), 0);
                platforms[platform].size.X += amount;
                platforms[platform].relativePos.X += pixelAmount / 2 * (int)direction;

                position.X += pixelAmount / 2 * (int)direction;
                bounds.X += pixelAmount / 2;
                foreach (PlatformFrame pF in platforms)
                    pF.relativePos.X += pixelAmount / 2 * -(int)direction;
            }

            private void shiftPlatforms(int pixelAmount, Direction direction, int platform, int pathDirection)
            {
                if (platform + pathDirection != platforms.Count && platform + pathDirection != -1)
                {
                    platforms[platform + pathDirection].relativePos.X += pixelAmount * (int)direction;
                    shiftPlatforms(pixelAmount, direction, platform + pathDirection, pathDirection);
                }
            }

            virtual public void flip(Vector2 axis)
            {
                for (int i = 0; i < platforms.Count; ++i)
                {
                    platforms[i].relativePos *= axis;
                    for (int j = 0; j < platforms[i].connections.Count; ++j)
                        platforms[i].connections[j] *= axis;
                }
            }

            public List<Vector2> getBoundPoints()
            {
                return new List<Vector2> { position, position + bounds, position - bounds, new Vector2(position.X + bounds.X, position.Y - bounds.Y), new Vector2(position.X - bounds.X, position.Y + bounds.Y) };
            }

            virtual public List<SetPiece> getSetPieces()
            {
                return new List<SetPiece> { this };
            }

            virtual public SetPiece copy()
            {
                return new SetPiece(this);
            }
        }

        private class CaveLoopSetPiece : SetPiece
        {
            private List<SetPiece> pieces;
            private List<Rectangle> loopSubAreas;

            public CaveLoopSetPiece(List<SetPiece> pieces, List<Rectangle> loopSubAreas) : base(new List<PlatformFrame>(), new Vector2())
            {
                this.pieces = pieces;
                this.loopSubAreas = loopSubAreas;

                flipable = new Vector2(0, 1);

                Vector2 max = pieces[0].position + pieces[0].bounds;
                Vector2 min = pieces[0].position - pieces[0].bounds;
                foreach (SetPiece sP in pieces)
                {
                    if (sP.position.X - sP.bounds.X < min.X)
                        min = new Vector2(sP.position.X - sP.bounds.X, min.Y);
                    if (sP.position.X + sP.bounds.X > max.X)
                        max = new Vector2(sP.position.X + sP.bounds.X, max.Y);
                    if (sP.position.Y - sP.bounds.Y < min.Y)
                        min = new Vector2(min.X, sP.position.Y - sP.bounds.Y);
                    if (sP.position.Y + sP.bounds.Y < max.Y)
                        max = new Vector2(max.X, sP.position.Y - sP.bounds.Y);
                }
                foreach (SetPiece sP in pieces)
                    sP.position -= (max + min) / 2;
                for (int i = 0; i < loopSubAreas.Count; ++i)
                    loopSubAreas[i] = new Rectangle(loopSubAreas[i].Location - ((max + min) / 2).ToPoint(), loopSubAreas[i].Size);
                bounds = new Vector2(Math.Abs(max.X - min.X) / 2, Math.Abs(max.Y - min.Y) / 2);

                foreach (SetPiece sP in pieces)
                {
                    platforms.AddRange(sP.platforms);
                    sP.platforms.ForEach(pF => pF.relativePos += sP.position);
                }

            }

            public CaveLoopSetPiece(CaveLoopSetPiece sPC) : base(new List<PlatformFrame>(), new Vector2())
            {
                pieces = new List<SetPiece>(sPC.pieces.ConvertAll(sP => new SetPiece(sP)));
                loopSubAreas = new List<Rectangle>(sPC.loopSubAreas);
                foreach (SetPiece sP in pieces)
                    platforms.AddRange(sP.platforms);

                flipable = sPC.flipable;
                position = sPC.position;
                bounds = sPC.bounds;
            }

            override
            public void flip(Vector2 axis)
            {
                foreach (SetPiece sP in pieces)
                {
                    sP.position *= axis;
                    for (int i = 0; i < sP.platforms.Count; ++i)
                    {
                        sP.platforms[i].relativePos *= axis;
                        for (int j = 0; j < sP.platforms[i].connections.Count; ++j)
                            sP.platforms[i].connections[j] *= axis;
                    }
                }
                for (int i = 0; i < loopSubAreas.Count; ++i)
                    loopSubAreas[i] = new Rectangle(loopSubAreas[i].Location * axis.ToPoint(), loopSubAreas[i].Size);
            }

            override
            public List<SetPiece> getSetPieces()
            {
                foreach (SetPiece sP in pieces)
                {
                    sP.platforms.ForEach(pF => pF.relativePos -= sP.position);
                    sP.position += position;
                }
                for (int i = 0; i < loopSubAreas.Count; ++i)
                    loopSubAreas[i] = new Rectangle(loopSubAreas[i].Location + position.ToPoint(), loopSubAreas[i].Size);
                return pieces;
            }

            override
            public SetPiece copy()
            {
                return new CaveLoopSetPiece(this);
            }

            public List<Rectangle> getSubAreas()
            {
                return loopSubAreas;
            }
        }

        private class CaveSetPiece : SetPiece
        {
            private SetPiece topPiece;
            private SetPiece botPiece;

            public CaveSetPiece(SetPiece topPiece, SetPiece botPiece) : base(new List<PlatformFrame>(), new Vector2())
            {
                this.topPiece = topPiece;
                this.botPiece = botPiece;
                platforms.AddRange(topPiece.platforms);
                platforms.AddRange(botPiece.platforms);

                topPiece.platforms.ForEach(pF => pF.relativePos += topPiece.position);
                botPiece.platforms.ForEach(pF => pF.relativePos += botPiece.position);

                foreach (PlatformFrame p in platforms)
                {
                    if (Math.Abs(p.relativePos.X) + p.platformTypeBounds.X + (p.size.X - 1) * PlatformFrame.continuousPlatformXBound > bounds.X)
                        bounds.X = Math.Abs(p.relativePos.X) + p.platformTypeBounds.X + (p.size.X - 1) * PlatformFrame.continuousPlatformXBound;
                    if (Math.Abs(p.relativePos.Y) + p.platformTypeBounds.Y * p.size.Y > bounds.Y)
                        bounds.Y = Math.Abs(p.relativePos.Y) + p.platformTypeBounds.Y * p.size.Y;
                }

                if (topPiece.flipable.X == 1 || botPiece.flipable.X == 1)
                    flipable.X = 1;
                if (topPiece.flipable.Y == 1 || botPiece.flipable.Y == 1)
                    flipable.Y = 1;
            }

            public CaveSetPiece(CaveSetPiece sPC) : base(new List<PlatformFrame>(), new Vector2())
            {
                topPiece = new SetPiece(sPC.topPiece);
                botPiece = new SetPiece(sPC.botPiece);
                platforms.AddRange(topPiece.platforms);
                platforms.AddRange(botPiece.platforms);

                flipable = sPC.flipable;
                position = sPC.position;
                bounds = sPC.bounds;
            }

            override
            public void stretch(int platform, int amount, Direction direction)
            {
                int pixelAmount = amount * 132;

                for (int i = 0; i < platforms[0].connections.Count; ++i)
                    platforms[0].connections[i] = platforms[0].connections[i] + new Vector2(pixelAmount / 2 * Math.Sign(platforms[0].connections[i].X), 0);
                platforms[0].size.X += amount;
                for (int i = 0; i < platforms[1].connections.Count; ++i)
                    platforms[1].connections[i] = platforms[1].connections[i] + new Vector2(pixelAmount / 2 * Math.Sign(platforms[1].connections[i].X), 0);
                platforms[1].size.X += amount;

                position.X += pixelAmount / 2 * (int)direction;
                bounds.X += pixelAmount / 2;

                topPiece.bounds.X += pixelAmount / 2;
                botPiece.bounds.X += pixelAmount / 2;
            }

            override
            public List<SetPiece> getSetPieces()
            {
                topPiece.platforms.ForEach(pF => pF.relativePos -= topPiece.position);
                botPiece.platforms.ForEach(pF => pF.relativePos -= botPiece.position);
                topPiece.position += position;
                botPiece.position += position;
                return new List<SetPiece> { topPiece, botPiece };
            }

            override
            public SetPiece copy()
            {
                return new CaveSetPiece(this);
            }
        }

        private class SubArea
        {
            public SubArea subArea;
            public List<SetPiece> library;
            public int maxPlatformStretch;
            public int maxSpread;
            public int minSpread;
            public int maxBoundDistance;
            public int platformVerticalSpacing;
            private Vector2 bounds;
            public Vector2 Bounds
            {
                get
                {
                    Vector2 ret = bounds;
                    SubArea sA = subArea;
                    while (sA != null)
                    {
                        ret += sA.bounds;
                        sA = sA.subArea;
                    }
                    return ret;
                }
            }

            public SubArea(SubArea subArea, List<SetPiece> library, int maxPlatformStretch, int maxSpread, int minSpread, int platformVerticalSpacing, Vector2 bounds)
            {
                this.subArea = subArea;
                this.library = library;
                this.maxPlatformStretch = maxPlatformStretch;
                this.maxSpread = maxSpread;
                this.minSpread = minSpread;
                this.platformVerticalSpacing = platformVerticalSpacing;
                this.bounds = bounds;
            }
        }

        private abstract class LevelArea
        {
            public SubArea subArea;
            public List<SetPiece> connectionLibrary;
            public SortedVector2List<SetPiece> area;
            public int maxSubAreas;
            public int minSubAreas;
            public int maxBoundDistance;
            public Vector2 bounds;

            public LevelArea(List<SetPiece> connectionLibrary, SubArea subArea, int maxSubAreas, int minSubAreas, int maxBoundDistance, Vector2 bounds)
            {
                this.connectionLibrary = connectionLibrary;
                this.subArea = subArea;
                this.maxSubAreas = maxSubAreas;
                this.minSubAreas = minSubAreas;
                this.maxBoundDistance = maxBoundDistance;
                this.bounds = bounds;

                SubArea sA = subArea;
                while (sA != null)
                {
                    sA.maxBoundDistance = maxBoundDistance;
                    sA = sA.subArea;
                }
            }

            abstract public Vector2 getFirstPlatformPosition(Vector2 bounds);

            abstract public List<Vector2> getEdgeConnections(SortedVector2List<SetPiece> area, Direction direction);

            abstract public SortedVector2List<SetPiece> buildLevelArea(SortedVector2List<SetPiece>[] areas, Random r);

            abstract public List<Rectangle> generateSubAreaDimensions(int numAreas, Random r);
        }

        private class BasicArea : LevelArea
        {
            public BasicArea(List<SetPiece> lib, int maxSubAreas, int minSubAreas, Vector2 bounds) 
                : base(new List<SetPiece> { },
                      new SubArea(null, new List<SetPiece> { lib[0], lib[1], lib[2], lib[3], lib[4], lib[5] }, 5, 3, 3, 150, bounds / 2), 
                      maxSubAreas, minSubAreas, 600, bounds)
            {

            }

            override
            public List<Vector2> getEdgeConnections(SortedVector2List<SetPiece> area, Direction direction)
            {
                List<Vector2> ret = new List<Vector2>();
                return ret;
            }

            override
            public Vector2 getFirstPlatformPosition(Vector2 bounds)
            {
                return new Vector2();
            }


            override
            public SortedVector2List<SetPiece> buildLevelArea(SortedVector2List<SetPiece>[] areas, Random r)
            {
                return null;
            }

            override
            public List<Rectangle> generateSubAreaDimensions(int numAreas, Random r)
            {
                return null;
            }
        }

        private class CaveArea : LevelArea
        {
            public const int maxConnectionsPerSetPiece = 4;
            public const int maxLoops = 2;
            private List<SetPiece>[] sortedConnectionLibrary = new List<SetPiece>[maxConnectionsPerSetPiece + 1];
            private List<SetPiece> areaConnectors = new List<SetPiece>();
            private static Vector2 SUB_AREA_BASE_SIZE = new Vector2(996, 716);
            private static Vector2 SUB_AREA_SIZE = new Vector2(1128, 716);

            public CaveArea(List<SetPiece> lib, int maxSubAreas, int minSubAreas, Vector2 bounds) 
                : base(new List<SetPiece> { lib[12], lib[13], lib[14], lib[15], lib[16], lib[17], lib[18], lib[19], lib[20], lib[21], lib[22], lib[23] }, 
                      new SubArea(
                          new SubArea(
                              new SubArea(null, new List<SetPiece> { lib[6], lib[7], lib[8], lib[9] }, 5, 4, 4, 500, SUB_AREA_BASE_SIZE), 
                              new List<SetPiece> { lib[10] }, 1, 2, 1, 180, new Vector2(0, 0)),
                          new List<SetPiece> { lib[24] }, 0, 1, 1, 0, new Vector2(132, 0)),
                      maxSubAreas, minSubAreas, 500, bounds)
            {
                for (int i = 0; i < maxConnectionsPerSetPiece + 1; ++i)
                    sortedConnectionLibrary[i] = new List<SetPiece>();
                foreach (SetPiece sP in connectionLibrary)
                {
                    int connections = 0;
                    foreach (PlatformFrame pF in sP.platforms)
                        connections += pF.connections.Count;
                    sortedConnectionLibrary[connections].Add(sP);
                }
            }

            override
            public List<Vector2> getEdgeConnections(SortedVector2List<SetPiece> area, Direction direction)
            {
                if (direction != Direction.Left && direction != Direction.Right)
                    return null;
                
                List<Vector2> farthest = new List<Vector2>();
                foreach (KeyValuePair<int, SetPiece> sP in area.Values[(direction == Direction.Left) ? 0 : area.Count - 1])
                    foreach (PlatformFrame pF in sP.Value.platforms)
                        foreach (Vector2 cP in pF.connections)
                            if (farthest.Count == 0)
                                farthest.Add(cP + pF.relativePos + sP.Value.position);
                            else if ((cP.X + pF.relativePos.X + sP.Value.position.X) * (int)direction > farthest[0].X * (int)direction)
                            {
                                farthest.Clear();
                                farthest.Add(cP + pF.relativePos + sP.Value.position);
                            }
                            else if (cP.X + pF.relativePos.X + sP.Value.position.X == farthest[0].X)
                                farthest.Add(cP + pF.relativePos + sP.Value.position);
                return farthest;
            }

            override
            public Vector2 getFirstPlatformPosition(Vector2 bounds)
            {
                return new Vector2(-SUB_AREA_BASE_SIZE.X + 72, 0);
            }

            override
            public SortedVector2List<SetPiece> buildLevelArea(SortedVector2List<SetPiece>[] areas, Random r)
            {
                SortedVector2List<SetPiece> ret = new SortedVector2List<SetPiece>();
                foreach (SortedVector2List<SetPiece> subArea in areas)
                    foreach (SetPiece sP in subArea.GetAllItems())
                        ret.Add(sP.position, sP);
                foreach (SetPiece sP in areaConnectors)
                    ret.Add(sP.position, sP);
                return ret;
            }

            override
            public List<Rectangle> generateSubAreaDimensions(int numAreas, Random r)
            {
                List<Rectangle> subAreas = new List<Rectangle>();
                List<int> connectionBranches = new List<int>();
                // Sub Areas
                int sA = numAreas;
                // End Connections
                int eC = 0;
                // Splitting Connections
                int sC = 0;
                // Total Connection Branches
                int cB = 0;
                while (sA * 2 - 1 != eC + cB)
                {
                    int openedAreas = (sA < cB - sC) ? sA : cB - sC;
                    int maxConnections;
                    if (sA * 2 - 1 - eC - cB > maxConnectionsPerSetPiece)
                        maxConnections = maxConnectionsPerSetPiece;
                    else
                        maxConnections = sA * 2 - 1 - eC - cB;
                    if (maxConnections > sA)
                        maxConnections = sA;
                    if (cB - sC - sA + 1 + maxConnections - 1 > maxLoops)
                        maxConnections = -cB + sC + sA + maxLoops;
                    if ((cB - sC - sA + 1 + maxConnections - 1) * 2 > sC + 1)
                        maxConnections = -cB + sC + sA + (sC + 1) / 2;

                    int minConnections = 0;
                    if (eC == openedAreas - sC && openedAreas + 1 != sA)
                        minConnections = 1;

                    int branch = r.Next(maxConnections - minConnections) + 1 + minConnections;
                    if (branch == 1)
                        ++eC;
                    else
                    {
                        ++sC;
                        cB += branch;
                    }
                    connectionBranches.Add(branch);
                }

                List<int> potentialLoopPoints = new List<int>();
                List<int> potentialLoopStarts = new List<int>();
                List<SetPiece> loopSetPieces = new List<SetPiece>();
                for (int i = 0; i < connectionBranches.Count; ++i)
                    if (connectionBranches[i] >= 2)
                    {
                        potentialLoopPoints.Add(connectionBranches[i]);
                        if (connectionBranches[i] >= 3)
                            potentialLoopStarts.Add(connectionBranches[i]);
                    }
                int potentialLoopPieces = potentialLoopPoints.Count;
                for (int i = 0; i < sA - eC - sC; ++i)
                {
                    // Generating pieces the loop will be made of and selecting start pieces
                    List<SetPiece> placedLoopPieces = new List<SetPiece>();
                    List<Rectangle> placedLoopSubAreas = new List<Rectangle>();
                    List<int> loopBranches = new List<int>();
                    loopBranches.Add(potentialLoopStarts[r.Next(potentialLoopStarts.Count)]);
                    potentialLoopStarts.Remove(loopBranches[loopBranches.Count - 1]);
                    potentialLoopPoints.Remove(loopBranches[loopBranches.Count - 1]);
                    connectionBranches.Remove(loopBranches[loopBranches.Count - 1]);

                    int connectionsInLoop = r.Next(potentialLoopPieces / (sA - eC - sC) - 1) + 1;
                    for (int j = 0; j < connectionsInLoop; ++j)
                    {
                        loopBranches.Add(potentialLoopPoints[r.Next(potentialLoopPoints.Count)]);
                        potentialLoopStarts.Remove(loopBranches[loopBranches.Count - 1]);
                        potentialLoopPoints.Remove(loopBranches[loopBranches.Count - 1]);
                        connectionBranches.Remove(loopBranches[loopBranches.Count - 1]);
                    }

                    SetPiece startPiece;
                    Vector2 startConnection = new Vector2();
                    Vector2 endConnection = new Vector2();

                    List<SetPiece> sameSideConnections = sortedConnectionLibrary[loopBranches[0]].FindAll(sP => sP.connectionPoints.FindAll(v => v.X < 0).Count > 1 || sP.connectionPoints.FindAll(v => v.X > 0).Count > 1);
                    startPiece = sameSideConnections[r.Next(sameSideConnections.Count)].copy();

                    Direction direction;
                    if (startPiece.connectionPoints.FindAll(v => v.X < 0).Count > 1)
                        direction = Direction.Left;
                    else
                        direction = Direction.Right;

                    foreach (PlatformFrame pF in startPiece.platforms)
                        foreach (Vector2 cP in pF.connections)
                            if (cP.X * (int)direction > 1)
                                if (startConnection == new Vector2())
                                    startConnection = cP + pF.relativePos;
                                else
                                    endConnection = cP + pF.relativePos;

                    placedLoopPieces.Add(startPiece);

                    // Creating the loop
                    SetPiece curPiece = startPiece;
                    int[] shortUpDownMap = new int[loopBranches.Count / 2];
                    int[] longUpDownMap = new int[loopBranches.Count / 2];

                    bool top = startConnection.Y < endConnection.Y;
                    for (int j = 1; j < loopBranches.Count; ++j)
                    {
                        int subAreaXBound = (j > loopBranches.Count / 2) ? (loopBranches.Count / 2 * (int)SUB_AREA_SIZE.X * 2 - loopBranches.Count % 2 * 200) / (loopBranches.Count / 2) / 2 : (int)SUB_AREA_SIZE.X;
                        if (startConnection.X > 0)
                            placedLoopSubAreas.Add(new Rectangle(curPiece.position.ToPoint() + startConnection.ToPoint() + new Point(subAreaXBound, 0), new Point(subAreaXBound, 644)));
                        else
                            placedLoopSubAreas.Add(new Rectangle(curPiece.position.ToPoint() + startConnection.ToPoint() + new Point(-subAreaXBound, 0), new Point(subAreaXBound, 644)));
                        curPiece.platforms.ForEach(pF => pF.connections.Remove(startConnection - pF.relativePos));

                        Vector2 nextConnection = new Vector2();
                        List<int> pieces = new List<int>();
                        for (int k = 0; k < sortedConnectionLibrary[loopBranches[j]].Count; ++k)
                            pieces.Add(k);
                        if (j == loopBranches.Count / 2)
                            top = !top;
                        int iter = 0;
                        do
                        {
                            int rand = r.Next(pieces.Count - iter);
                            curPiece = sortedConnectionLibrary[loopBranches[j]][pieces[rand]].copy();
                            pieces[rand] = pieces[pieces.Count - 1 - iter];
                            ++iter;

                            if (j == loopBranches.Count / 2)
                                setConnectionsForLoopPiece(curPiece, top, true, ref nextConnection, ref startConnection);
                            else if (j > loopBranches.Count / 2)
                            {
                                int shortUpDownSum = 0;
                                int totalShortUpDownSum = 0;
                                int longUpDownSum = 0;
                                for (int k = loopBranches.Count / 2 - 1; k >= loopBranches.Count / 2 - 1 - (j - loopBranches.Count / 2) && k >= 0; --k)
                                    shortUpDownSum += shortUpDownMap[k];
                                for (int k = loopBranches.Count / 2 - 1; k >= 0; --k)
                                    totalShortUpDownSum += shortUpDownMap[k];
                                for (int k = 0; k < longUpDownMap.Length; ++k)
                                    longUpDownSum += longUpDownMap[k];

                                // If it needs to go up or down, then not straight
                                bool straight = !(shortUpDownSum + longUpDownSum < 0 && !top || shortUpDownSum + longUpDownSum > 0 && top || shortUpDownSum + longUpDownSum > loopBranches.Count - 1 - j);
                                // If it is right below the path or it needs to go down, then not up
                                bool up = !(shortUpDownSum + longUpDownSum == 0 && !top || shortUpDownSum + longUpDownSum < 0 && !top || shortUpDownSum + longUpDownSum - 1 > loopBranches.Count - 1 - j);
                                // If it is right above the path or it needs to go up, then not down
                                bool down = !(shortUpDownSum + longUpDownSum == 0 && top || shortUpDownSum + longUpDownSum > 0 && top || shortUpDownSum + longUpDownSum + 1 > loopBranches.Count - 1 - j);

                                setConnectionsForLoopPiece(curPiece, top, false, ref nextConnection, ref startConnection);
                                if (!(straight && nextConnection.Y == startConnection.Y || up && startConnection.Y < nextConnection.Y || down && startConnection.Y > nextConnection.Y))
                                    nextConnection = new Vector2();
                                else
                                    longUpDownMap[j - 1 - loopBranches.Count / 2] = Math.Sign(startConnection.Y);
                            }
                            else
                            {
                                setConnectionsForLoopPiece(curPiece, top, false, ref nextConnection, ref startConnection);
                                shortUpDownMap[j - 1] = Math.Sign(startConnection.Y);
                            }
                        }
                        while ((nextConnection == new Vector2() || startConnection == new Vector2()) && iter != sortedConnectionLibrary[loopBranches[j]].Count);

                        curPiece.position = placedLoopSubAreas[placedLoopSubAreas.Count - 1].Location.ToVector2()
                            + new Vector2(-Math.Sign(nextConnection.X) * placedLoopSubAreas[placedLoopSubAreas.Count - 1].Width, 0)
                            - nextConnection;
                        placedLoopPieces.Add(curPiece);
                        curPiece.platforms.ForEach(pF => pF.connections.Remove(nextConnection - pF.relativePos));
                    }

                    placedLoopSubAreas.Add(new Rectangle(
                        (endConnection + startPiece.position + (startConnection + curPiece.position - endConnection - startPiece.position) / 2).ToPoint(),
                        new Point((int)Math.Abs(startConnection.X + curPiece.position.X - endConnection.X - startPiece.position.X) / 2, 644)));

                    startPiece.platforms.ForEach(pF => pF.connections.Remove(endConnection - pF.relativePos));
                    curPiece.platforms.ForEach(pF => pF.connections.Remove(startConnection - pF.relativePos));

                    connectionBranches.Add(-i);
                    int randSpot = r.Next(connectionBranches.Count);
                    int t = connectionBranches[randSpot];
                    connectionBranches[randSpot] = connectionBranches[connectionBranches.Count - 1];
                    connectionBranches[connectionBranches.Count - 1] = t;
                    loopSetPieces.Add(new CaveLoopSetPiece(placedLoopPieces, placedLoopSubAreas));
                }

                // Placing all sub areas and connectors
                subAreas.Add(new Rectangle(-3000, 0, (int)SUB_AREA_SIZE.X, 644));
                int openConnections = 0;
                {
                    int branchIndex = 0;
                    while (branchIndex < connectionBranches.Count && (connectionBranches[branchIndex] == 1 || connectionBranches[branchIndex] <= 0 && loopSetPieces[-connectionBranches[branchIndex]].connectionPoints.Count == 1)
                        && openConnections == 0 && connectionBranches.Count != 1)
                        ++branchIndex;

                    SetPiece nextConnector;
                    if (connectionBranches[branchIndex] > 0)
                    {
                        List<int> pieces = new List<int>();
                        for (int l = 0; l < sortedConnectionLibrary[connectionBranches[branchIndex]].Count; ++l)
                            pieces.Add(l);
                        int pieceIter = 0;
                        do
                        {
                            int rand = r.Next(pieces.Count - pieceIter);
                            nextConnector = sortedConnectionLibrary[connectionBranches[branchIndex]][pieces[rand]].copy();
                            pieces[rand] = pieces[pieces.Count - 1 - pieceIter];
                            ++pieceIter;
                        }
                        while ((nextConnector = placeNextConnector(nextConnector, r, subAreas, areaConnectors, subAreas[subAreas.Count - 1], subAreas[0].X > 0 ? Direction.Left : Direction.Right)) == null
                            && pieceIter != pieces.Count);
                    }
                    else
                        nextConnector = placeNextConnector(loopSetPieces[-connectionBranches[branchIndex]].copy(), r, subAreas, areaConnectors, subAreas[subAreas.Count - 1], subAreas[0].X > 0 ? Direction.Left : Direction.Right);

                    openConnections += nextConnector.connectionPoints.Count;
                    foreach (SetPiece nextConnectorPiece in nextConnector.getSetPieces())
                        areaConnectors.Add(nextConnectorPiece);
                    if (connectionBranches[branchIndex] <= 0)
                        subAreas.AddRange(((CaveLoopSetPiece)nextConnector).getSubAreas());
                    connectionBranches.RemoveAt(branchIndex);
                }

                if (placeSubAreaAndConnector(openConnections, subAreas, connectionBranches, loopSetPieces, numAreas, r))
                    return subAreas;
                return null;
            }

            private bool placeSubAreaAndConnector(int openConnections, List<Rectangle> subAreas, List<int> connectionBranches, List<SetPiece> loopSetPieces, int numAreas, Random r)
            {
                if (connectionBranches.Count == 0)
                    return true;
                for (int i = 0; i < areaConnectors.Count; ++i)
                {
                    SetPiece sP = areaConnectors[i];
                    foreach (PlatformFrame pF in sP.platforms)
                        for (int j = 0; j < pF.connections.Count; ++j)
                        {
                            Vector2 cP = pF.connections[j];
                            --openConnections;
                            if (cP.X > 0)
                                subAreas.Add(new Rectangle(sP.position.ToPoint() + pF.relativePos.ToPoint() + cP.ToPoint() + new Point((int)SUB_AREA_SIZE.X, 0), new Point((int)SUB_AREA_SIZE.X, 644)));
                            else
                                subAreas.Add(new Rectangle(sP.position.ToPoint() + pF.relativePos.ToPoint() + cP.ToPoint() + new Point(-(int)SUB_AREA_SIZE.X, 0), new Point((int)SUB_AREA_SIZE.X, 644)));

                            int branchIndex = 0;
                            while (branchIndex < connectionBranches.Count)
                            {
                                while (connectionBranches.Count != 1 && (connectionBranches[branchIndex] == 1 || connectionBranches[branchIndex] <= 0 && loopSetPieces[-connectionBranches[branchIndex]].connectionPoints.Count == 1)
                                    && openConnections == 0)
                                    ++branchIndex;

                                if (branchIndex < connectionBranches.Count)
                                {
                                    SetPiece nextConnector;
                                    if (connectionBranches[branchIndex] > 0)
                                    {
                                        List<int> pieces = new List<int>();
                                        for (int k = 0; k < sortedConnectionLibrary[connectionBranches[branchIndex]].Count; ++k)
                                            pieces.Add(k);
                                        int pieceIter = 0;
                                        do
                                        {
                                            int rand = r.Next(pieces.Count - pieceIter);
                                            nextConnector = sortedConnectionLibrary[connectionBranches[branchIndex]][pieces[rand]].copy();
                                            pieces[rand] = pieces[pieces.Count - 1 - pieceIter];
                                            ++pieceIter;

                                            nextConnector = placeNextConnector(nextConnector, r, subAreas, areaConnectors, subAreas[subAreas.Count - 1], cP.X > 0 ? Direction.Right : Direction.Left);
                                            if (nextConnector != null)
                                            {
                                                int subAreasCount = subAreas.Count;
                                                int areaConnectorsCount = areaConnectors.Count;
                                                List<int> connectionBranchesCopy = new List<int>(connectionBranches);
                                                List<Vector2> pFConnectionsCopy = new List<Vector2>(pF.connections);
                                                pF.connections.Remove(cP);

                                                openConnections += nextConnector.connectionPoints.Count;
                                                foreach (SetPiece nextConnectorPiece in nextConnector.getSetPieces())
                                                    areaConnectors.Add(nextConnectorPiece);
                                                if (connectionBranches[branchIndex] <= 0)
                                                    subAreas.AddRange(((CaveLoopSetPiece)nextConnector).getSubAreas());
                                                connectionBranches.RemoveAt(branchIndex);

                                                if (placeSubAreaAndConnector(openConnections, subAreas, connectionBranches, loopSetPieces, numAreas, r))
                                                    return true;

                                                connectionBranches = connectionBranchesCopy;
                                                if (subAreas.Count - subAreasCount > 0)
                                                    subAreas.RemoveRange(subAreasCount, subAreas.Count - subAreasCount);
                                                if (areaConnectors.Count - areaConnectorsCount > 0)
                                                    areaConnectors.RemoveRange(areaConnectorsCount, areaConnectors.Count - areaConnectorsCount);
                                                openConnections -= nextConnector.connectionPoints.Count;
                                                pF.connections = pFConnectionsCopy;
                                            }
                                        }
                                        while (pieceIter != pieces.Count);
                                    }
                                    else
                                    {
                                        nextConnector = placeNextConnector(loopSetPieces[-connectionBranches[branchIndex]].copy(), r, subAreas, areaConnectors, subAreas[subAreas.Count - 1], cP.X > 0 ? Direction.Right : Direction.Left);
                                        if (nextConnector != null)
                                        {
                                            int subAreasCount = subAreas.Count;
                                            int areaConnectorsCount = areaConnectors.Count;
                                            List<int> connectionBranchesCopy = new List<int>(connectionBranches);
                                            List<Vector2> pFConnectionsCopy = new List<Vector2>(pF.connections);
                                            pF.connections.Remove(cP);

                                            openConnections += nextConnector.connectionPoints.Count;
                                            foreach (SetPiece nextConnectorPiece in nextConnector.getSetPieces())
                                                areaConnectors.Add(nextConnectorPiece);
                                            if (connectionBranches[branchIndex] <= 0)
                                                subAreas.AddRange(((CaveLoopSetPiece)nextConnector).getSubAreas());
                                            connectionBranches.RemoveAt(branchIndex);

                                            if (placeSubAreaAndConnector(openConnections, subAreas, connectionBranches, loopSetPieces, numAreas, r))
                                                return true;

                                            connectionBranches = connectionBranchesCopy;
                                            if (subAreas.Count - subAreasCount > 0)
                                                subAreas.RemoveRange(subAreasCount, subAreas.Count - subAreasCount);
                                            if (areaConnectors.Count - areaConnectorsCount > 0)
                                                areaConnectors.RemoveRange(areaConnectorsCount, areaConnectors.Count - areaConnectorsCount);
                                            pF.connections = pFConnectionsCopy;
                                            openConnections -= nextConnector.connectionPoints.Count;
                                        }
                                    }
                                    ++branchIndex;
                                }
                            }
                            subAreas.RemoveAt(subAreas.Count - 1);
                            ++openConnections;
                        }
                }
                return false;
            }

            private void setConnectionsForLoopPiece(SetPiece curPiece, bool top, bool sameSideConnection, ref Vector2 nextConnection, ref Vector2 startConnection)
            {
                Vector2 nConnec = new Vector2();
                Vector2 sConnec = new Vector2();
                foreach (PlatformFrame pF in curPiece.platforms)
                    foreach (Vector2 cP in pF.connections)
                        if ((cP.X + pF.relativePos.X) * startConnection.X < 0)
                            if (nConnec == new Vector2())
                                nConnec = cP + pF.relativePos;
                            else if (top && cP.Y + pF.relativePos.Y > nextConnection.Y || !top && cP.Y + pF.relativePos.Y < nextConnection.Y)
                                nConnec = cP + pF.relativePos;
                foreach (PlatformFrame pF in curPiece.platforms)
                    foreach (Vector2 cP in pF.connections)
                        if (((cP.X + pF.relativePos.X) * nConnec.X < 0 && !sameSideConnection || (cP.X + pF.relativePos.X) * nConnec.X > 0 && sameSideConnection)
                            && cP + pF.relativePos != nConnec)
                            if (sConnec == new Vector2())
                                sConnec = cP + pF.relativePos;
                            else if (top && cP.Y + pF.relativePos.Y > sConnec.Y || !top && cP.Y + pF.relativePos.Y < sConnec.Y)
                                sConnec = cP + pF.relativePos;
                if (sConnec != new Vector2() && nConnec != new Vector2())
                {
                    nextConnection = nConnec;
                    startConnection = sConnec;
                }
            }

            private SetPiece placeNextConnector(SetPiece nextConnector, Random r, List<Rectangle> areas, List<SetPiece> areaConnectors, Rectangle baseArea, Direction connectionSide)
            {
                bool placed = false;
                List<PlatformFrame> piecePlatforms = new List<PlatformFrame>(nextConnector.platforms);
                while (piecePlatforms.Count > 0 && placed == false)
                {
                    int pieceIndex = r.Next(piecePlatforms.Count);
                    PlatformFrame platform = piecePlatforms[pieceIndex];
                    piecePlatforms.RemoveAt(pieceIndex);

                    List<Vector2> pieceConnections = new List<Vector2>(platform.connections);
                    while (pieceConnections.Count > 0 && placed == false)
                    {
                        placed = true;
                        pieceIndex = r.Next(pieceConnections.Count);
                        Vector2 connection = pieceConnections[pieceIndex];
                        pieceConnections.RemoveAt(pieceIndex);

                        Vector2 flipAxis = new Vector2(1);
                        if (connection.X * (int)connectionSide > 0)
                            flipAxis.X = -1;
                        if (flipAxis != new Vector2(1))
                            if (flipAxis.X * nextConnector.flipable.X == -1)
                                placed = false;
                            else
                            {
                                nextConnector.flip(flipAxis);
                                connection *= flipAxis;
                            }

                        if (connectionSide > 0)
                            nextConnector.position = baseArea.Location.ToVector2() + new Vector2(baseArea.Width, 0) - connection - platform.relativePos;
                        else
                            nextConnector.position = baseArea.Location.ToVector2() + new Vector2(-baseArea.Width, 0) - connection - platform.relativePos;

                        foreach (SetPiece placedPiece in areaConnectors)
                            if (withinBounds(placedPiece, nextConnector, -144))
                                placed = false;
                        foreach (Rectangle placedSubArea in areas)
                            if (withinBounds(placedSubArea, nextConnector, -144))
                                placed = false;

                        if (placed)
                        {
                            nextConnector.platforms.ForEach(pF => pF.connections.Remove(connection + platform.relativePos - pF.relativePos));
                            return nextConnector;
                        }
                        else if(flipAxis.X * nextConnector.flipable.X != -1)
                            nextConnector.flip(flipAxis);
                    }
                }
                return null;
            }
        }

        private List<SetPiece> library = new List<SetPiece>();
        private SortedVector2List<SetPiece> level = new SortedVector2List<SetPiece>();
        private int a = new Random().Next();
        private Random r;

        public LevelGenerator(float jumpVel, float moveVel, float gravity)
        {
            r = new Random(1581083272);
            Debug.Print("level seed: " + a);
            loadSetPieces();

            CaveArea area = new CaveArea(library, 5, 2, new Vector2(4000, 4000));
            //LevelArea area = new BasicArea(library, 1, 1, new Vector2(4000, 4000));
            int subAreas = r.Next(area.maxSubAreas - area.minSubAreas) + area.minSubAreas;
            SortedVector2List<SetPiece>[] subAreaLevels = new SortedVector2List<SetPiece>[subAreas];
            List<Rectangle> subAreaDimenions = area.generateSubAreaDimensions(subAreas, r);
            for (int i = 0; i < subAreas; ++i)
                subAreaLevels[i] = new SortedVector2List<SetPiece>();
            for (int i = 0; i < subAreas; ++i)
            {
                populatSubAreas(area.subArea, subAreaLevels[i], area.getFirstPlatformPosition(area.subArea.Bounds));
                foreach (SetPiece sP in subAreaLevels[i].GetAllItems())
                    sP.position += subAreaDimenions[i].Location.ToVector2();
            }

            /*edge = new SetPiece(library[12]);
            edge.position = area.getEdgeConnections(subAreaLevels[0], Direction.Right)[0] - edge.platforms[0].relativePos - edge.platforms[0].connections[0];
            edge.position.X = 1128 - edge.platforms[0].relativePos.X - edge.platforms[0].connections[0].X - 6;
            subAreaLevels[0].Add(edge.position, edge);*/

            level = area.buildLevelArea(subAreaLevels, r);

            SetPiece stair = library[1].copy();
            stair.position = new Vector2(-4500, -100);
            level.Add(stair.position, stair);

            stair = library[1].copy();
            stair.flip(new Vector2(-1, 1));
            stair.position = new Vector2(-4500, 500);
            level.Add(stair.position, stair);

            stair = library[1].copy();
            stair.position = new Vector2(-4500, 1100);
            level.Add(stair.position, stair);

            stair = library[1].copy();
            stair.flip(new Vector2(-1, 1));
            stair.position = new Vector2(-4500, 1700);
            level.Add(stair.position, stair);

            stair = library[1].copy();
            stair.position = new Vector2(-4500, 2300);
            level.Add(stair.position, stair);

            stair = library[1].copy();
            stair.flip(new Vector2(-1, 1));
            stair.position = new Vector2(-4500, 2900);
            level.Add(stair.position, stair);

            stair = library[1].copy();
            stair.position = new Vector2(-4500, 3500);
            level.Add(stair.position, stair);

            /*for (int i = 13; i < 24; ++i) {
                SetPiece edge = new SetPiece(library[i]);
                edge.position = new Vector2(-3000 + (i - 13) * 890, 3200);
                level.Add(edge.position, edge);
            }*/

        }

        public List<Platform> getPlatforms(Vector2 center, Dictionary<string, Sprite> sprites)
        {
            List<Platform> platforms = new List<Platform>();
            foreach (KeyValuePair<int, SortedList<int, SetPiece>> sL in level.list)
                foreach (KeyValuePair<int, SetPiece> sP in sL.Value)
                    foreach (PlatformFrame pF in sP.Value.platforms)
                        if (pF.size == new Vector2(1))
                            platforms.Add(new Platform(center + sP.Value.position + pF.relativePos, sprites[pF.type].IndividualSpriteSize, sprites[pF.type], LevelThemes.Cosmos, PlatformSubTheme.Stone, null));
                        else
                            platforms.Add(new PlatformCollection(center + sP.Value.position + pF.relativePos, (int)pF.size.X, (int)pF.size.Y, sprites[pF.type], LevelThemes.Cosmos, PlatformSubTheme.Stone, null));
            return platforms;
        }

        private void populatSubAreas(SubArea subArea, SortedVector2List<SetPiece> area, Vector2 start)
        {
            List<SetPiece> startingPieces;
            Queue<SetPiece> queue = new Queue<SetPiece>();
            if (subArea.subArea != null)
            {
                populatSubAreas(subArea.subArea, area, start);
                startingPieces = new List<SetPiece>(area.GetAllItems());
            }
            else
            {
                SetPiece startPiece;
                List<int> pieces = new List<int>();
                for (int k = 0; k < subArea.library.Count; ++k)
                    pieces.Add(k);
                int iter = 0;
                do
                {
                    int rand = r.Next(pieces.Count - iter);
                    startPiece = subArea.library[pieces[rand]].copy();
                    startPiece.position = start;
                    pieces[rand] = pieces[pieces.Count - 1 - iter];
                    ++iter;
                }
                while (!withinAreaBounds(subArea.Bounds, startPiece) && iter != pieces.Count);

                startingPieces = startPiece.getSetPieces();
                foreach (SetPiece sP in startingPieces)
                    area.Add(sP.position, sP);
            }

            foreach (SetPiece startPiece in startingPieces)
            {
                queue.Enqueue(startPiece);
                SetPiece curPiece = null;
                int spread = r.Next(subArea.maxSpread - subArea.minSpread) + subArea.minSpread;
                for (int i = spread; i >= 1; --i)
                {
                    for (int j = 0; j < Factorial(spread) / Factorial(i - 1) && queue.Count > 0; ++j)
                    {
                        if (j % i == 0)
                            curPiece = queue.Dequeue();
                        if (curPiece.connectionPoints.Count > 0)
                        {
                            SetPiece next;
                            List<int> pieces = new List<int>();
                            for (int k = 0; k < subArea.library.Count; ++k)
                                pieces.Add(k);
                            int iter = 0;
                            bool placed;
                            do
                            {
                                int rand = r.Next(pieces.Count - iter);
                                next = subArea.library[pieces[rand]].copy();
                                pieces[rand] = pieces[pieces.Count - 1 - iter];
                                ++iter;
                            }
                            while (!(placed = placeSetPiece(curPiece, next, subArea, area)) && iter != pieces.Count);

                            if (placed)
                            {
                                List<SetPiece> nextPieces = next.getSetPieces();
                                foreach (SetPiece sP in nextPieces)
                                {
                                    area.Add(sP.position, sP);
                                    queue.Enqueue(sP);
                                }
                            }
                            else
                                j += i - j % i;
                        }
                        else
                            j += i - j % i;
                    }
                }
                queue.Clear();
            }
        }

        private int Factorial(int i)
        {
            if (i <= 1)
                return 1;
            return i * Factorial(i - 1);
        }

        private int Summation(int i)
        {
            if (i == 1)
                return 1;
            return i + Summation(i - 1);
        }

        private void loadSetPieces()
        {
            // 0
            library.Add(new SetPiece(
                new List<PlatformFrame> {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, 0), new Vector2(5, 1),
                        new List<Vector2> {
                            new Vector2(-436, 0),
                            new Vector2(436, 0),
                            new Vector2(-168, -100),
                            new Vector2(168, -100)
                        })
                },
                new Vector2(0, 1)));

            // 1
            library.Add(new SetPiece(
                new List<PlatformFrame> {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(122, -200), new Vector2(1, 1),
                        new List<Vector2> {
                            new Vector2(172, 0),
                            new Vector2(-172, -100)
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-122, 0), new Vector2(1, 1),
                        new List<Vector2> {
                            new Vector2(-172, 0)
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(122, 200), new Vector2(1, 1),
                        new List<Vector2> {
                            new Vector2(-172, 100),
                            new Vector2(172, 0)
                        })
                },
                new Vector2(0, 0)));

            // 2
            library.Add(new SetPiece(
                new List<PlatformFrame> {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-222, 100), new Vector2(1, 1),
                        new List<Vector2> {
                            //new Vector2(0, 100),
                            new Vector2(-172, 0)
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, -100), new Vector2(1, 1),
                        new List<Vector2> {
                            new Vector2(-122, -100),
                            new Vector2(122, -100)
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(222, 100), new Vector2(1, 1),
                        new List<Vector2> {
                            //new Vector2(0, 100),
                            new Vector2(172, 0)
                        })
                },
                new Vector2(0, 0)));

            // 3
            library.Add(new SetPiece(
                new List<PlatformFrame> {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-72, 100), new Vector2(1, 1),
                        new List<Vector2> {
                            new Vector2(122, 100),
                            new Vector2(-172, 0)
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(72, -100), new Vector2(1, 1),
                        new List<Vector2> {
                            new Vector2(-122, -100),
                            new Vector2(172, 0),
                        })
                },
                new Vector2(0, 0)));

            // 4
            library.Add(new SetPiece(
                new List<PlatformFrame> {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-222, 0), new Vector2(1, 1),
                        new List<Vector2> {
                            new Vector2(72, -150),
                            new Vector2(72, 150),
                            new Vector2(-172, 0)
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(222, 0), new Vector2(1, 1),
                        new List<Vector2> {
                            new Vector2(-72, -150),
                            new Vector2(-72, 150),
                            new Vector2(172, 0),
                        })
                },
                new Vector2(0, 0)));

            // 5
            library.Add(new SetPiece(
                new List<PlatformFrame> {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, 0), new Vector2(1, 1),
                        new List<Vector2> {
                            new Vector2(172, 0),
                            new Vector2(-172, 0),
                            new Vector2(172, -150),
                            new Vector2(-172, -150),
                            new Vector2(172, 150),
                            new Vector2(-172, 150)
                        })
                },
                new Vector2(0, 0)));

            // 6
            library.Add(new CaveSetPiece(
                new SetPiece(
                    new List<PlatformFrame> {
                        new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 0), new Vector2(4, 2),
                            new List<Vector2> {
                                new Vector2(-264, 0),
                                new Vector2(264, 0),
                                new Vector2(-264, -72),
                                new Vector2(264, -72),
                                new Vector2(-264, -144),
                                new Vector2(264, -144),
                                new Vector2(0, -144),
                                new Vector2(80, -144),
                                new Vector2(-80, -144)
                            })
                    },
                    new Vector2(0, 1),
                    new Vector2(0, 500)),
                new SetPiece(
                    new List<PlatformFrame> {
                        new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 0), new Vector2(4, 2),
                            new List<Vector2> {
                            })
                    },
                    new Vector2(0, 1),
                    new Vector2(0, -500))
            ));

            // 7
            library.Add(new CaveSetPiece(
                new SetPiece(
                    new List<PlatformFrame> {
                        new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 0), new Vector2(2, 2),
                            new List<Vector2> {
                                new Vector2(-132, 0),
                                new Vector2(132, 0),
                                new Vector2(-132, -72),
                                new Vector2(132, -72),
                                new Vector2(-132, -144),
                                new Vector2(132, -144),
                                new Vector2(0, -144),
                                new Vector2(50, -144),
                                new Vector2(-50, -144)
                            })
                    },
                    new Vector2(0, 1),
                    new Vector2(0, 425)),
                new SetPiece(
                    new List<PlatformFrame> {
                        new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 0), new Vector2(2, 2),
                            new List<Vector2> {
                            })
                    },
                    new Vector2(0, 1),
                    new Vector2(0, -425))
            ));

            // 8
            library.Add(new CaveSetPiece(
                new SetPiece(
                    new List<PlatformFrame> {
                        new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 0), new Vector2(1, 2),
                            new List<Vector2> {
                                new Vector2(-66, 0),
                                new Vector2(66, 0),
                                new Vector2(-66, -72),
                                new Vector2(66, -72),
                                new Vector2(-66, -144),
                                new Vector2(66, -144),
                                new Vector2(0, -144)
                            })
                    },
                    new Vector2(0, 1),
                    new Vector2(0, 360)),
                new SetPiece(
                    new List<PlatformFrame> {
                        new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 0), new Vector2(1, 2),
                            new List<Vector2> {
                            })
                    },
                    new Vector2(0, 1),
                    new Vector2(0, -360))
            ));

            // 9
            library.Add(new CaveSetPiece(
                new SetPiece(
                    new List<PlatformFrame> {
                        new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 0), new Vector2(1, 2),
                            new List<Vector2> {
                                new Vector2(-66, 0),
                                new Vector2(66, 0),
                                new Vector2(-66, -72),
                                new Vector2(66, -72),
                                new Vector2(-66, -144),
                                new Vector2(66, -144)
                            })
                    },
                    new Vector2(0, 1),
                    new Vector2(0, 324)),
                new SetPiece(
                    new List<PlatformFrame> {
                        new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 0), new Vector2(1, 2),
                            new List<Vector2> {
                            })
                    },
                    new Vector2(0, 1),
                    new Vector2(0, -324))
            ));

            // 10
            library.Add(new SetPiece(
                new List<PlatformFrame> {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, 0), new Vector2(1, 1),
                        new List<Vector2> {
                            new Vector2(162, 255),
                            new Vector2(-162, 255),
                            new Vector2(162, 205),
                            new Vector2(-162, 205),
                            new Vector2(162, 100),
                            new Vector2(-162, 100),
                            new Vector2(115, 0),
                            new Vector2(-115, 0),
                            new Vector2(122, -50),
                            new Vector2(-150, -50),
                            new Vector2(122, -100),
                            new Vector2(-150, -100)
                        })
                },
                new Vector2(0, 1)));

            // 11
            library.Add(new SetPiece(
                new List<PlatformFrame>
                {
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-330, -360), new Vector2(1, 5),
                        new List<Vector2>()),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-132, -432), new Vector2(2, 4),
                        new List<Vector2>()),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(198, -648), new Vector2(3, 1),
                        new List<Vector2>()),

                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-140, 220), new Vector2(1, 1),
                        new List<Vector2>()),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(192, 0), new Vector2(1, 1),
                        new List<Vector2>()),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(220, 400), new Vector2(1, 1),
                        new List<Vector2>()),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(330, 0), new Vector2(1, 2),
                        new List<Vector2>{
                            new Vector2(66, -144)
                        }),
                    
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-264, 576), new Vector2(2, 2),
                        new List<Vector2>{
                            new Vector2(-132, -72)
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(132, 648), new Vector2(4, 1),
                        new List<Vector2>{
                            new Vector2(264, -72)
                        })
                },
                new Vector2(0, 1)));

            // 12
            library.Add(new SetPiece(
                new List<PlatformFrame>
                {
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 0), new Vector2(1, 8),
                        new List<Vector2>{
                            new Vector2(-66, 0)
                        })
                },
                new Vector2(0, 1)));

            // 13
            library.Add(new SetPiece(
                new List<PlatformFrame>
                {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, -636), new Vector2(6, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-204, -540), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-66, -396), new Vector2(1, 3),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(66, -468), new Vector2(1, 2),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(204, -540), new Vector2(1, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(330, 36), new Vector2(1, 3),
                        new List<Vector2>{
                            new Vector2(66, -608)
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(198, 324), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(66, 372), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-186, 108), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(198, -12), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-330, 36), new Vector2(1, 3),
                        new List<Vector2>{
                            new Vector2(-66, -608)
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 612), new Vector2(6, 1),
                        new List<Vector2>{
                            new Vector2(396, -40),
                            new Vector2(-396, -40)
                        })
                },
                new Vector2(1, 1)));

            // 14
            library.Add(new SetPiece(
                new List<PlatformFrame>
                {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, -636), new Vector2(6, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, -540), new Vector2(4, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, -396), new Vector2(3, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(330, 36), new Vector2(1, 3),
                        new List<Vector2>{
                            new Vector2(66, -608)
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(150, 84), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(198, 36), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-330, 36), new Vector2(1, 3),
                        new List<Vector2>{
                            new Vector2(-66, -608)
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-150, 84), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-198, 36), new Vector2(1, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 612), new Vector2(6, 1),
                        new List<Vector2>{
                            new Vector2(-396, -40),
                            new Vector2(396, -40)
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 468), new Vector2(3, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 324), new Vector2(2, 1),
                        new List<Vector2>{
                        })
                },
                new Vector2(1, 1)));

            // 15
            library.Add(new SetPiece(
                new List<PlatformFrame>
                {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, -636), new Vector2(6, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-198, -108), new Vector2(3, 1),
                        new List<Vector2>{
                            new Vector2(-198, -464)
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(330, 252), new Vector2(1, 6),
                        new List<Vector2>{
                            new Vector2(66, -824)
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(198, 324), new Vector2(1, 5),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-0, 468), new Vector2(2, 3),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-264, 612), new Vector2(2, 1),
                        new List<Vector2>{
                            new Vector2(-144, -40)
                        })
                },
                new Vector2(1, 1)));

            // 16
            library.Add(new SetPiece(
                new List<PlatformFrame>
                {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, -636), new Vector2(6, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-330, 36), new Vector2(1, 3),
                        new List<Vector2>{
                            new Vector2(-66, -608)
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 252), new Vector2(1, 4),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(330, -180), new Vector2(1, 6),
                        new List<Vector2>{
                        }),


                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-24, 108), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-306, 252), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-24, 396), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(24, 108), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(306, 252), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(24, 396), new Vector2(1, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 612), new Vector2(6, 1),
                        new List<Vector2>{
                            new Vector2(-396, -40),
                            new Vector2(396, -40)
                        }),
                },
                new Vector2(1, 1)));

            // 17
            library.Add(new SetPiece(
                new List<PlatformFrame>
                {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, -636), new Vector2(6, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(198, 36), new Vector2(3, 3),
                        new List<Vector2>{
                            new Vector2(-198, -608)
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-330, 180), new Vector2(1, 5),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-198, 276), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-66, -12), new Vector2(1, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 612), new Vector2(6, 1),
                        new List<Vector2>{
                            new Vector2(-396, -40),
                            new Vector2(396, -40)
                        }),
                },
                new Vector2(1, 1)));

            // 18
            library.Add(new SetPiece(
                new List<PlatformFrame>
                {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, -636), new Vector2(6, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(330, 36), new Vector2(1, 3),
                        new List<Vector2>{
                            new Vector2(66, -608)
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-330, -180), new Vector2(1, 6),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-162, 420), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(162, 276), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-162, 132), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(162, -12), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-162, -156), new Vector2(1, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 612), new Vector2(6, 1),
                        new List<Vector2>{
                            new Vector2(-396, -40),
                            new Vector2(396, -40)
                        }),
                },
                new Vector2(1, 1)));

            // 19
            library.Add(new SetPiece(
                new List<PlatformFrame>
                {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, -636), new Vector2(6, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-330, 36), new Vector2(1, 3),
                        new List<Vector2>{
                            new Vector2(-66, -608)
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-198, -36), new Vector2(1, 2),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-66, -108), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(66, -60), new Vector2(1, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(330, 252), new Vector2(1, 6),
                        new List<Vector2>{
                            new Vector2(66, -824)
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(198, 396), new Vector2(1, 4),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(66, 468), new Vector2(1, 3),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-66, 540), new Vector2(1, 2),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-264, 612), new Vector2(2, 1),
                        new List<Vector2>{
                            new Vector2(-132, -40)
                        })
                },
                new Vector2(1, 1)));

            // 20
            library.Add(new SetPiece(
                new List<PlatformFrame>
                {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, -636), new Vector2(6, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-132, -540), new Vector2(4, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(330, 36), new Vector2(1, 3),
                        new List<Vector2>{
                            new Vector2(66, -608)
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(198, 108), new Vector2(1, 2),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(66, 36), new Vector2(1, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-198, 276), new Vector2(1, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-198, -396), new Vector2(3, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-264, -252), new Vector2(2, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-330, 180), new Vector2(1, 5),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 612), new Vector2(6, 1),
                        new List<Vector2>{
                            new Vector2(396, -40)
                        })
                },
                new Vector2(1, 1)));

            // 21
            library.Add(new SetPiece(
                new List<PlatformFrame>
                {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, -636), new Vector2(6, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-330, -180), new Vector2(1, 6),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, 420), new Vector2(2, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(-132, 132), new Vector2(2, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, -156), new Vector2(2, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(264, 180), new Vector2(2, 5),
                        new List<Vector2>{
                            new Vector2(132, -752)
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 612), new Vector2(6, 1),
                        new List<Vector2>{
                            new Vector2(-396, -40)
                        })
                },
                new Vector2(1, 1)));

            // 22
            library.Add(new SetPiece(
                new List<PlatformFrame>
                {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, -636), new Vector2(6, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-264, 180), new Vector2(2, 5),
                        new List<Vector2>{
                            new Vector2(-132, -752)
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, -108), new Vector2(2, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 468), new Vector2(2, 1),
                        new List<Vector2>{  
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(330, -180), new Vector2(1, 6),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(132, 180), new Vector2(2, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 612), new Vector2(6, 1),
                        new List<Vector2>{
                            new Vector2(396, -40)
                        })
                },
                new Vector2(1, 1)));

            // 23
            library.Add(new SetPiece(
                new List<PlatformFrame>
                {
                    new PlatformFrame(Platform.HORIZONTAL_PLAT, new Vector2(0, -636), new Vector2(6, 1),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-330, 36), new Vector2(1, 3),
                        new List<Vector2>{
                            new Vector2(-66, -608)
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(-132, -36), new Vector2(2, 2),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(264, 324), new Vector2(1, 3),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(198, 396), new Vector2(1, 2),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(132, 468), new Vector2(1, 1),
                        new List<Vector2>{
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(330, -36), new Vector2(1, 8),
                        new List<Vector2>{
                        }),

                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 612), new Vector2(6, 1),
                        new List<Vector2>{
                            new Vector2(-396, -40)
                        })
                },
                new Vector2(1, 1)));

            // 24
            library.Add(new SetPiece(
                new List<PlatformFrame>
                {
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 384), new Vector2(1, 3),
                        new List<Vector2>{
                            new Vector2(66, 0),
                            new Vector2(66, 36),
                            new Vector2(66, 72),
                            new Vector2(66, 96),
                            new Vector2(66, 132),
                            new Vector2(66, 168),
                            new Vector2(66, -36),
                            new Vector2(66, -72),
                            new Vector2(66, -96),
                            new Vector2(66, -132),
                            new Vector2(66, -168),
                            new Vector2(66, -204),
                            new Vector2(66, -240),
                            new Vector2(66, -276),
                            new Vector2(66, -312),
                            new Vector2(66, -348),
                            new Vector2(66, -384),
                            new Vector2(66, -420),
                            new Vector2(66, -456),
                            new Vector2(66, -492),
                        }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, 624), new Vector2(1, 1),
                        new List<Vector2>{ }),
                    new PlatformFrame(Platform.SQUARE_PLAT, new Vector2(0, -480), new Vector2(1, 3),
                        new List<Vector2>{ })
                },
                new Vector2(0, 1)));

        }

        private bool placeSetPiece(SetPiece basePiece, SetPiece newPiece, SubArea subArea, SortedVector2List<SetPiece> area)
        {

            List<PlatformFrame> basePiecePlatforms = new List<PlatformFrame>(basePiece.platforms);
            while (basePiecePlatforms.Count > 0)
            {
                int pieceIndex = r.Next(basePiecePlatforms.Count);
                PlatformFrame basePiecePlatform = basePiecePlatforms[pieceIndex];
                basePiecePlatforms.RemoveAt(pieceIndex);

                List<Vector2> basePieceConnections = new List<Vector2>(basePiecePlatform.connections);
                while (basePieceConnections.Count > 0)
                {
                    pieceIndex = r.Next(basePieceConnections.Count);
                    Vector2 baseConnection = basePieceConnections[pieceIndex];
                    basePieceConnections.RemoveAt(pieceIndex);

                    List<PlatformFrame> newPiecePlatforms = new List<PlatformFrame>(newPiece.platforms);
                    while (newPiecePlatforms.Count > 0)
                    {
                        pieceIndex = r.Next(newPiecePlatforms.Count);
                        PlatformFrame newPiecePlatform = newPiecePlatforms[pieceIndex];
                        newPiecePlatforms.RemoveAt(pieceIndex);

                        List<Vector2> newPieceConnections = new List<Vector2>(newPiecePlatform.connections);
                        while (newPieceConnections.Count > 0)
                        {
                            bool clearToPlace = true;

                            pieceIndex = r.Next(newPieceConnections.Count);
                            Vector2 newConnection = newPieceConnections[pieceIndex];
                            newPieceConnections.RemoveAt(pieceIndex);

                            if (newConnection.X == 0 && baseConnection.X == 0)
                                clearToPlace = false;

                            Vector2 flipAxis = new Vector2(1);
                            if (((newConnection.X == 0) ? newPiecePlatform.relativePos.X : newConnection.X) * ((baseConnection.X == 0) ? basePiecePlatform.relativePos.X : baseConnection.X) > 0)
                                flipAxis.X = -1;
                            if (((newConnection.Y == 0) ? newPiecePlatform.relativePos.Y : newConnection.Y) * ((baseConnection.Y == 0) ? basePiecePlatform.relativePos.Y : baseConnection.Y) > 0)
                                flipAxis.Y = -1;
                            if (flipAxis != new Vector2(1))
                                if (flipAxis.X * newPiece.flipable.X == -1 || flipAxis.Y * newPiece.flipable.Y == -1)
                                    clearToPlace = false;
                                else
                                {
                                    newPiece.flip(flipAxis);
                                    newConnection *= flipAxis;
                                }
                            newPiece.position = basePiece.position + baseConnection + basePiecePlatform.relativePos - newConnection - newPiecePlatform.relativePos;

                            Vector2 newPieceMinPoint = new Vector2(newPiece.position.X - newPiece.bounds.X, newPiece.position.Y - newPiece.bounds.Y);

                            if (!withinAreaBounds(subArea.Bounds, newPiece))
                                clearToPlace = false;

                            foreach (PlatformFrame bPF in basePiece.platforms)
                                foreach (PlatformFrame nPF in newPiece.platforms)
                                    if (Vector2.Distance((bPF.relativePos + basePiece.position) * new Vector2(1, 0.5f), (nPF.relativePos + newPiece.position) * new Vector2(1, 0.5f)) < bPF.size.X * 66 + nPF.size.X * 66
                                        && Math.Abs(bPF.relativePos.Y + basePiece.position.Y - nPF.relativePos.Y - newPiece.position.Y) - bPF.size.Y * bPF.platformTypeBounds.Y - nPF.size.Y * nPF.platformTypeBounds.Y < subArea.platformVerticalSpacing)
                                        clearToPlace = false;

                            int x = SortedVector2List<SortedList<int, SetPiece>>.binarySearch(area.list, (int)newPieceMinPoint.X - subArea.maxBoundDistance, 1);
                            int j = (x == -1) ? area.Count : x;
                            for (; j < area.Count && newPiece.position.X + newPiece.bounds.X + subArea.maxBoundDistance > area.Keys[j] && clearToPlace; ++j)
                            {
                                int y = SortedVector2List<SetPiece>.binarySearch(area.Values[j], (int)newPieceMinPoint.Y - subArea.maxBoundDistance, 1);
                                for (int k = (y == -1) ? area.Values[j].Count : y; k < area.Values[j].Count && newPiece.position.Y + newPiece.bounds.Y + subArea.maxBoundDistance > area.Values[j].Keys[k] && clearToPlace; ++k)
                                    if (withinBounds(newPiece, area.Values[j].Values[k], subArea.platformVerticalSpacing) && area.Values[j].Values[k] != basePiece)
                                        clearToPlace = false;
                            }

                            if (!clearToPlace && !(flipAxis.X * newPiece.flipable.X == -1 || flipAxis.Y * newPiece.flipable.Y == -1))
                                newPiece.flip(flipAxis);

                            if (clearToPlace)
                            {
                                int canStretchL = 0;
                                int canStretchR = 0;
                                int maxPlatformStretch = (subArea.maxBoundDistance - newPiece.bounds.X < subArea.maxPlatformStretch * (132 / 2)) ? (int)(subArea.maxBoundDistance - newPiece.bounds.X) / 66 : subArea.maxPlatformStretch;
                                 canStretchL = calculateMaxStrechForPiece(
                                    SortedVector2List<SetPiece>.binarySearch(area, (int)(newPiece.position.X - newPiece.bounds.X), -1),
                                    area, maxPlatformStretch, subArea.maxBoundDistance, Direction.Left, newPiece, basePiece, subArea.Bounds);
                                canStretchR = calculateMaxStrechForPiece(
                                    SortedVector2List<SetPiece>.binarySearch(area, (int)(newPiece.position.X + newPiece.bounds.X), 1),
                                    area, maxPlatformStretch, subArea.maxBoundDistance, Direction.Right, newPiece, basePiece, subArea.Bounds);

                                int canStretch;
                                if (newConnection.X + newPiecePlatform.relativePos.X < -newPiece.bounds.X)
                                    canStretch = canStretchR;
                                else if (newConnection.X + newPiecePlatform.relativePos.X > newPiece.bounds.X)
                                    canStretch = canStretchL;
                                else
                                    canStretch = (canStretchR + canStretchL > maxPlatformStretch) ? maxPlatformStretch : canStretchL + canStretchR;

                                if (canStretch > 0)
                                {
                                    int stretch = r.Next(Summation(canStretch + 1)) + 1;
                                    int sum = 0;
                                    for (int i = 1; ; sum += i, ++i)
                                        if (sum >= stretch)
                                        {
                                            stretch = canStretch + 2 - i;
                                            break;
                                        }

                                    if (stretch > 0)
                                    {
                                        List<int> platforms = new List<int>();
                                        for (int i = 0; i < newPiece.platforms.Count; ++i)
                                            platforms.Add(i);
                                        List<int> platformsToStretch = new List<int>();
                                        do
                                        {
                                            int i = r.Next(platforms.Count);
                                            if (platforms.Count > 0)
                                            {
                                                platforms[i] = platforms[platforms.Count - 1];
                                                platforms.RemoveAt(platforms.Count - 1);
                                            }
                                            platformsToStretch.Add(i);
                                        } while (platforms.Count > 0 && r.Next(2) % 2 == 0 && platformsToStretch.Count < stretch);

                                        int left = 0;
                                        int right = 0;
                                        for (int i = 0; i < platformsToStretch.Count; ++i)
                                            if (newPiece.platforms[platformsToStretch[i]].relativePos.X < newConnection.X + newPiecePlatform.relativePos.X)
                                                ++left;
                                            else
                                                ++right;

                                        int stretchL = 0;
                                        int stretchR = 0;

                                        if (canStretchL <= stretch / platformsToStretch.Count * left)
                                            stretchR = stretch - canStretchL;
                                        else if (canStretchR <= stretch / platformsToStretch.Count * right)
                                            stretchL = stretch - canStretchR;
                                        else
                                        {
                                            stretchL = stretch / platformsToStretch.Count * left;
                                            stretchR = stretch / platformsToStretch.Count * right;

                                            int extra = stretch % platformsToStretch.Count;
                                            if (canStretchL - stretchL < extra)
                                                stretchR += extra;
                                            else if (canStretchR - stretchR < extra)
                                                stretchL += extra;
                                            else if (r.Next(2) % 2 == 0)
                                                stretchR += extra;
                                            else
                                                stretchL += extra;
                                        }

                                        for (int i = 0; i < platformsToStretch.Count; ++i)
                                            if (newPiece.platforms[platformsToStretch[i]].relativePos.X < newConnection.X + newPiecePlatform.relativePos.X)
                                                newPiece.stretch(platformsToStretch[i],
                                                    (i != platformsToStretch.Count - 1) ? stretchL / platformsToStretch.Count : stretchL / platformsToStretch.Count + stretchL % platformsToStretch.Count,
                                                    Direction.Left);
                                            else
                                                newPiece.stretch(platformsToStretch[i],
                                                    (i != platformsToStretch.Count - 1) ? stretchR / platformsToStretch.Count : stretchR / platformsToStretch.Count + stretchR % platformsToStretch.Count,
                                                    Direction.Right);

                                        //if (newPiece.position + newConnection + newPiecePlatform.relativePos != basePiece.position + baseConnection + basePiecePlatform.relativePos)
                                          //  newPiece.position = basePiece.position + baseConnection + basePiecePlatform.relativePos + newPiecePlatform.relativePos - newConnection;
                                    }
                                }

                                basePiecePlatform.connections.Remove(baseConnection);
                                newPiecePlatform.connections.Remove(newConnection);

                                x = SortedVector2List<SetPiece>.binarySearch(area, (int)newPieceMinPoint.X - subArea.maxBoundDistance, 1);
                                for (j = (x == -1) ? area.Count : x; j < area.Count && newPiece.position.X + newPiece.bounds.X + subArea.maxBoundDistance > area.Keys[j] && clearToPlace; ++j)
                                {
                                    int y = SortedVector2List<SetPiece>.binarySearch(area.Values[j], (int)newPieceMinPoint.Y - subArea.maxBoundDistance, 1);
                                    for (int k = (y == -1) ? area.Values[j].Count : y; k < area.Values[j].Count && newPiece.bounds.Y + newPiece.position.Y + subArea.maxBoundDistance > area.Values[j].Keys[k] && clearToPlace; ++k)
                                    {
                                        foreach (PlatformFrame p in area.Values[j].Values[k].platforms)
                                            p.connections.RemoveAll(v => withinBounds(v + p.relativePos + area.Values[j].Values[k].position, newPiece, true));
                                        foreach (PlatformFrame p in newPiece.platforms)
                                            p.connections.RemoveAll(v => withinBounds(v + p.relativePos + newPiece.position, area.Values[j].Values[k], true));
                                    }
                                }
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private int calculateMaxStrechForPiece(int start, SortedVector2List<SetPiece> area, int maxPlatformStretch, int maxBoundDistance, Direction direction, SetPiece newPiece, SetPiece basePiece, Vector2 areaBounds)
        {
            int canStretch = 0;
            for (int i = 1; i < maxPlatformStretch && canStretch != -1; ++i)
            {
                if ((newPiece.position.X + (int)direction * (newPiece.bounds.X + 132 * i)) * (int)direction < areaBounds.X * (int)direction)
                {
                    int j = start;
                    for (; j < area.Count && j >= 0 && canStretch != -1
                        && (newPiece.position.X + (int)direction * (newPiece.bounds.X + maxBoundDistance + 132 * i)) * (int)direction > area.Keys[j] * (int)direction;
                        j += (int)direction)
                    {
                        int y = SortedVector2List<SetPiece>.binarySearch(area.Values[j], (int)(newPiece.position.Y - newPiece.bounds.Y - maxBoundDistance), 1);
                        for (int k = (y == -1) ? area.Values[j].Count : y;
                            k < area.Values[j].Count && newPiece.position.Y + newPiece.bounds.Y + maxBoundDistance > area.Values[j].Keys[k] && canStretch != -1;
                            ++k)
                            if (withinBounds(newPiece.position + (int)direction * (newPiece.bounds + new Vector2(132 * i, 0)), area.Values[j].Values[k], true) // New Piece bottom corner in other setPiece
                                || withinBounds(newPiece.position + (int)direction * (new Vector2(newPiece.bounds.X, -newPiece.bounds.Y) - new Vector2(132 * i, 0)), area.Values[j].Values[k], true) // New Piece top corner in other setPiece
                                || withinBounds(area.Values[j].Values[k].position + (int)direction * (area.Values[j].Values[k].bounds + new Vector2(132 * i, 0)), newPiece, true) // Set Piece bottom corner in New Piece
                                || withinBounds(area.Values[j].Values[k].position + (int)direction * (new Vector2(area.Values[j].Values[k].bounds.X, -area.Values[j].Values[k].bounds.Y) - new Vector2(132 * i, 0)), newPiece, true)) // Set Piece top corner in New Piece
                                canStretch = -1;
                            else
                                canStretch = i;
                    }
                    if ((j == area.Count || j == -1) && canStretch != -1)
                        canStretch = i;
                }
            }
            return (canStretch == -1) ? 0 : canStretch;
        }

        private static bool withinAreaBounds(Vector2 area, SetPiece sP)
        {
            return -area.X < sP.position.X - sP.bounds.X && -area.Y < sP.position.Y - sP.bounds.Y && area.X > sP.position.X + sP.bounds.X && area.Y > sP.position.Y + sP.bounds.Y;
        }

        private static bool withinBounds(Rectangle area, SetPiece sP, int verticalSpacing, bool includingEdges = false)
        {
            if (includingEdges)
                return Math.Abs(area.X - sP.position.X) <= area.Width + sP.bounds.X && Math.Abs(area.Y - sP.position.Y) <= area.Height + sP.bounds.Y + verticalSpacing;
            else
                return Math.Abs(area.X - sP.position.X) < area.Width + sP.bounds.X && Math.Abs(area.Y - sP.position.Y) < area.Height + sP.bounds.Y + verticalSpacing;
        }

        private static bool withinBounds(SetPiece sP1, SetPiece sP2, int verticalSpacing, bool includingEdges = false)
        {
            if (includingEdges)
                return Math.Abs(sP1.position.X - sP2.position.X) <= sP1.bounds.X + sP2.bounds.X && Math.Abs(sP1.position.Y - sP2.position.Y) <= sP1.bounds.Y + sP2.bounds.Y + verticalSpacing;
            else
                return Math.Abs(sP1.position.X - sP2.position.X) < sP1.bounds.X + sP2.bounds.X && Math.Abs(sP1.position.Y - sP2.position.Y) < sP1.bounds.Y + sP2.bounds.Y + verticalSpacing;
        }

        private static bool withinBounds(Vector2 point, SetPiece sP, bool includingEdges = false)
        {
            if (includingEdges)
                return point.X <= sP.position.X + sP.bounds.X && point.X >= sP.position.X - sP.bounds.X && point.Y <= sP.position.Y + sP.bounds.Y && point.Y >= sP.position.Y - sP.bounds.Y;
            else
                return point.X < sP.position.X + sP.bounds.X && point.X > sP.position.X - sP.bounds.X && point.Y < sP.position.Y + sP.bounds.Y && point.Y > sP.position.Y - sP.bounds.Y;
        }

        
    }

}

public class Platform : GameObject
{
    public static string HORIZONTAL_PLAT = "HorizontalPlatform";
    public static string VERTICAL_PLAT = "VerticalPlatform";
    public static string SQUARE_PLAT = "SquarePlatform";
    public static string PIXEL_PLAT = "PixelPlatform";
    public static string HORIZONTAL_DECOR = "HorizontalDecoration";

    private LevelThemes spriteTheme;
    private PlatformSubTheme spriteSubTheme;
    protected Sprite decoration;

    public Platform(Vector2 position, Vector2 size, Sprite sprite, LevelThemes theme, PlatformSubTheme subTheme, Sprite decoration) : base(position, size, sprite, CollisionTypes.Obstacle)
    {
        collisionType = CollisionTypes.Obstacle;
        collisionArea.Add(new Vector2(-size.X / 2, -size.Y / 2));
        collisionArea.Add(new Vector2(size.X / 2, -size.Y / 2));
        collisionArea.Add(new Vector2(size.X / 2, size.Y / 2));
        collisionArea.Add(new Vector2(-size.X / 2, size.Y / 2));

        spriteTheme = theme;
        spriteSubTheme = subTheme;

        this.decoration = decoration;
    }

    public List<Platform> splitPlatform()
    {
        List<Platform> ret = new List<Platform>();
        Vector2 platforms = new Vector2((float)Math.Ceiling(size.X / (Level.MAX_PLATFORM_BOUND * 2)), (float)Math.Ceiling(size.Y / (Level.MAX_PLATFORM_BOUND * 2)));
        float xPos = 0;
        for (int i = 0; i < platforms.X; ++i)
        {
            float yPos = 0;
            for (int j = 0; j < platforms.Y; ++j)
            {
                Vector2 platSize = new Vector2(
                    (i == platforms.X - 1) ? size.X - xPos : Level.MAX_PLATFORM_BOUND * 2,
                    (j == platforms.Y - 1) ? size.Y - yPos : Level.MAX_PLATFORM_BOUND * 2);
                if (sprite.Name == HORIZONTAL_PLAT || sprite.Name == SQUARE_PLAT)
                    platSize.X = (float)Math.Floor((platSize.X - 144) / 132) * 132 + 144;
                ret.Add(new PlatformCollection(Position - size / 2 + new Vector2(xPos, yPos) + platSize / 2, platSize, Sprite, spriteTheme, spriteSubTheme, decoration));
                yPos += platSize.Y;
                if (j == platforms.Y - 1)
                    xPos += platSize.X - 12;
            }
        }
        return ret;
    }

    override
    public void collide(CollidableObj obj, List<Vector2>[] axis)
    {
        for (int i = 0; i < axis[0].Count; ++i)
        {
            obj.addVelocity(-obj.Velocity * new Vector2(Math.Abs(axis[0][i].X), Math.Abs(axis[0][i].Y)));
            obj.addPosition(-axis[1][i]);
            if (obj.collisionType == CollisionTypes.Enemy && axis[0][i].Y > 0 && ((GameObject)obj).states[(int)TypesOfStates.Movement] == ObjectStates.Airborne)
                ((GameObject)obj).states[(int)TypesOfStates.Movement] = ObjectStates.Idle;
        }
    }

    protected int getPlatformSheetIndex()
    {
        return (int)spriteTheme * 3 + (int)spriteSubTheme;
    }

    protected void drawDecoration(SpriteBatch sb, Vector2 origin)
    {
        decoration.drawSpriteSheet(sb, Position - new Vector2(0, sprite.IndividualSpriteSize.Y / 2 + decoration.IndividualSpriteSize.Y / 2), origin, Direction.Right, (int)spriteTheme, 0);
    }
    protected void drawDecoration(SpriteBatch sb, Vector2 origin, Vector2 platformPos)
    {
        decoration.drawSpriteSheet(sb, platformPos - new Vector2(0, sprite.IndividualSpriteSize.Y / 2 + decoration.IndividualSpriteSize.Y / 2), origin, Direction.Right, (int)spriteTheme, 0);
    }

    virtual public List<Vector2> getConnectivityNodeCoordinates()
    {
        return new List<Vector2> { new Vector2(Position.X - size.X / 2, Position.Y - size.Y / 2), new Vector2(Position.X + size.X / 2, Position.Y - size.Y / 2) };
    }

    override
    public void draw(SpriteBatch sb, Vector2 origin)
    {
        sprite.drawSpriteSheet(sb, Position, origin, Direction.Right, getPlatformSheetIndex(), 0);
        if (decoration != null)
            drawDecoration(sb, origin);
    }
}

public class PlatformCollection : Platform
{
    private Vector2 numberOfSprites;
    // NumSprites * SpriteSize - (NumSprites - 1) * 12 = TotalSize = SpriteSize + (NumSprites - 1) * (SpriteSize - 12)
    public PlatformCollection(Vector2 position, Vector2 minSize, Sprite sprite, LevelThemes theme, PlatformSubTheme type, Sprite decoration) : 
        base(position, 
            new Vector2(sprite.IndividualSpriteSize.X, 0) 
            + new Vector2(
                (float)Math.Ceiling((minSize.X - sprite.IndividualSpriteSize.X) / (sprite.IndividualSpriteSize.X - 12)), 
                (float)Math.Ceiling(minSize.Y / sprite.IndividualSpriteSize.Y)) 
            * (sprite.IndividualSpriteSize - new Vector2(12, 0)), 
            sprite, theme, type, decoration)
    {
        numberOfSprites = new Vector2(
            (float)Math.Ceiling((minSize.X - sprite.IndividualSpriteSize.X) / (sprite.IndividualSpriteSize.X - 12)) + 1, 
            (float)Math.Ceiling(minSize.Y / sprite.IndividualSpriteSize.Y));
    }
    public PlatformCollection(Vector2 position, int xSprites, int ySprites, Sprite sprite, LevelThemes theme, PlatformSubTheme type, Sprite decoration) : 
        base(position, new Vector2(sprite.IndividualSpriteSize.X * xSprites - 12 * (xSprites - 1), sprite.IndividualSpriteSize.Y * ySprites), sprite, theme, type, decoration)
    {
        numberOfSprites = new Vector2(xSprites, ySprites);
    }

    override
    public List<Vector2> getConnectivityNodeCoordinates()
    {
        List<Vector2> ret = new List<Vector2>();
        for (int i = 0; i < numberOfSprites.X; ++i)
            ret.Add(new Vector2(Position.X - size.X / 2 + i * (sprite.IndividualSpriteSize.X - 12), Position.Y - size.Y / 2));
        ret.Add(new Vector2(Position.X + size.X / 2, Position.Y - size.Y / 2));
        return ret;
    }

    override
    public void draw(SpriteBatch sb, Vector2 origin)
    {
        for (int i = 0; i < numberOfSprites.X; ++i)
            for (int j = 0; j < numberOfSprites.Y; ++j)
            {
                Vector2 position = Position - size / 2 + new Vector2((sprite.IndividualSpriteSize.X - 12) * i, sprite.IndividualSpriteSize.Y * j) + sprite.IndividualSpriteSize / 2;
                sprite.drawSpriteSheet(sb, position, origin, Direction.Right, getPlatformSheetIndex(), 0);
                if (decoration != null)
                    drawDecoration(sb, origin, position);
            }
    }
}

