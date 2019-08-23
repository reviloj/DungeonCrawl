
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

using System.Diagnostics;
using System;
using System.Collections.Generic;

public class Game : GameState
{
    public const int ALL_ENEMY_MIN_JUMP_STRENGTH = 9;
    public const int ALL_ENEMY_MAX_JUMP_STRENGTH = 12;
    public const int MEDIUM_ENEMY_HEIGHT = 120;
    public const int MEDIUM_ENEMY_WIDTH = 80;
    public const int LARGE_ENEMY_HEIGHT = 180;
    public const int LARGE_ENEMY_WIDTH = 160;


    private Vector2 BASE_HERO_SCREEN_POS = new Vector2(640, 375);
    private int difficulty;
    public Level level;
    public Hero hero;
    private Stopwatch frameTime = new Stopwatch();
    private Dictionary<string, Background> backgrounds = new Dictionary<string, Background>();
    public AIManager aiManager;
    public float gravity = 0.25f;
    public List<Enemy> Enemies { get; } = new List<Enemy>();

    private int iiii = 0;

    public Game() : base()
    {

    }

    override
    public void loadContent(Dictionary<string, Texture2D> textures)
    {
        sprites.Add("Hero", new Sprite("Hero", textures["Hero"], new Vector2(200, 148)));
        sprites.Add("GoldSwordHero", new Sprite("GoldSwordHero", textures["GoldSwordHero"], new Vector2(200, 148)));
        sprites.Add("Platform", new Sprite("Platform", textures["Platform"]));
        sprites.Add("Chort", new Sprite("Chort", textures["Chort"], new Vector2(80, 120)));
        sprites.Add("BaseSwordTN", new Sprite("BaseSwordTN", textures["BaseSwordTN"]));
        sprites.Add("GoldSwordTN", new Sprite("GoldSwordTN", textures["GoldSwordTN"]));
        sprites.Add("SquarePlatform", new Sprite("SquarePlatform", textures["SquarePlatform"], new Vector2(144, 144)));
        sprites.Add("HorizontalPlatform", new Sprite("HorizontalPlatform", textures["HorizontalPlatform"], new Vector2(144, 48)));
        sprites.Add("VerticalPlatform", new Sprite("VerticalPlatform", textures["VerticalPlatform"], new Vector2(48, 144)));
        sprites.Add("HorizontalDecoration", new Sprite("HorizontalDecoration", textures["HorizontalDecoration"], new Vector2(144, 24)));
        sprites.Add("EmptyChest", new Sprite("EmptyChest", textures["EmptyChest"], new Vector2(80, 80)));
        backgrounds.Add("DarkForestBG", new Background("DarkForestBG", textures["DarkForestBG"], new Vector2(1856, 1044), 9));
    }

    public void generateLevel(int difficulty)
    {
        level = new Level(new Vector2(12000, 6000), difficulty, sprites, backgrounds, 0.15f);
        hero = new Hero(new Vector2(250, -300), getSprite("Hero"), getSprite("BaseSwordTN"), new List<Stat>(), new List<Effect>());
        Enemies.Add(new Chort(new Vector2(1000, -300), sprites["Chort"], new List<Stat>(), new List<Effect>()));
        //Enemies.Add(new Chort(new Vector2(700, -520), sprites["Chort"], new List<Stat>(), new List<Effect>()));
        //Enemies.Add(new Chort(new Vector2(700, -100), sprites["Chort"], new List<Stat>(), new List<Effect>()));
        //Enemies.Add(new Chort(new Vector2(250, -420), sprites["Chort"], new List<Stat>(), new List<Effect>()));
        aiManager = new AIManager(level.Platforms, gravity, Enemies);
    }

    private void applyGravity(float timeDisplacement)
    {
        float gravVel = gravity * timeDisplacement;
        hero.addVelocity(new Vector2(0, gravVel));
        foreach (Character enemy in Enemies)
            enemy.addVelocity(new Vector2(0, gravVel));
        foreach (Item item in level.Items)
            item.addVelocity(new Vector2(0, gravVel));
    }

    private void doCollisionChecks(float timeDisplacement)
    {
        level.Platforms.ForAllInRange(hero.Position, hero.size + new Vector2(Level.MAX_PLATFORM_BOUND),
            delegate (Platform p)
            {
                hero.handleItemCollisions(p, timeDisplacement);
                hero.handleCollision(p, timeDisplacement);
                return true;
            });
        foreach (Item item in level.Items)
            level.Platforms.ForAllInRange(item.Position, item.size + new Vector2(Level.MAX_PLATFORM_BOUND),
                delegate (Platform p)
                {
                    item.handleCollision(p, timeDisplacement);
                    return true;
                });

        foreach (Character enemy in Enemies)
        {
            hero.handleItemCollisions(enemy, timeDisplacement);
            hero.handleCollision(enemy, timeDisplacement);
            level.Platforms.ForAllInRange(enemy.Position, enemy.size + new Vector2(Level.MAX_PLATFORM_BOUND),
                delegate (Platform p)
                {
                    enemy.handleCollision(p, timeDisplacement);
                    return true;
                });
        }
    }

    private bool onScreen(GameObject obj)
    {
        float x = (obj.Position.X - hero.Position.X + calculateHeroScreenPos().X) * DC.DungeonCrawl.ResolustionScale;
        float y = (obj.Position.Y - hero.Position.Y + calculateHeroScreenPos().Y) * DC.DungeonCrawl.ResolustionScale;
        return x > 0 && x < DC.DungeonCrawl.Resolution.X && y > 0 && y < DC.DungeonCrawl.Resolution.Y;
    }

    private Rectangle getDrawingArea()
    {
        return new Rectangle((hero.Position - calculateHeroScreenPos()).ToPoint(), DC.DungeonCrawl.Resolution.ToPoint());
    }

    private Vector2 getScreenPos(Vector2 vec)
    {
        return vec - hero.Position + calculateHeroScreenPos();
    }

    private void updateEnemiesAI(float timeDisplacement, SortedVector2List<Platform> platforms)
    {
        foreach (Enemy c in Enemies)
            c.ai.update(timeDisplacement, platforms);
    }

    private void updateEnemies(float timeDisplacement)
    {
        for (int i = Enemies.Count - 1; i >= 0; --i)
        {
            Enemies[i].update(timeDisplacement);
            if (Enemies[i].calculateStat(StatTypes.Health) == 0)
            {
                Enemies.Remove(Enemies[i]);
                continue;
            }
        }
    }

    override
    public GameState update(float timeDisplacement)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        TouchCollection touchCollection = TouchPanel.GetState();
        List<PlayerActions> actions = new List<PlayerActions>();
        if (touchCollection.Count > 0)
            foreach (TouchLocation tl in touchCollection)
                /*if (tl.State == TouchLocationState.Released
                    && tl.Position.X > 870 * DC.DungeonCrawl.ResolustionScale && tl.Position.X < 966 * DC.DungeonCrawl.ResolustionScale
                    && tl.Position.Y > 50 * DC.DungeonCrawl.ResolustionScale && tl.Position.Y < 146 * DC.DungeonCrawl.ResolustionScale)
                {
                    DC.DungeonCrawl.debugVar = true;
                }
                else*/if (tl.State == TouchLocationState.Released
                            && tl.Position.X > 970 * DC.DungeonCrawl.ResolustionScale && tl.Position.X < 1066 * DC.DungeonCrawl.ResolustionScale
                            && tl.Position.Y > 50 * DC.DungeonCrawl.ResolustionScale && tl.Position.Y < 146 * DC.DungeonCrawl.ResolustionScale)
                {
                    DC.DungeonCrawl.graphDebugVar2++;
                    if (DC.DungeonCrawl.graphDebugVar2 > 10)
                        DC.DungeonCrawl.graphDebugVar2 = 1;
                }
                else 
                if (tl.State == TouchLocationState.Released
                            && tl.Position.X > 1070 * DC.DungeonCrawl.ResolustionScale && tl.Position.X < 1166 * DC.DungeonCrawl.ResolustionScale
                            && tl.Position.Y > 50 * DC.DungeonCrawl.ResolustionScale && tl.Position.Y < 146 * DC.DungeonCrawl.ResolustionScale)
                    //return new Map(draw, level, hero);
                {
                    DC.DungeonCrawl.graphDebugVar++;
                    if (DC.DungeonCrawl.graphDebugVar > Game.ALL_ENEMY_MAX_JUMP_STRENGTH)
                        DC.DungeonCrawl.graphDebugVar = 5;
                }
                else if (tl.Position.X > 70 * DC.DungeonCrawl.ResolustionScale && tl.Position.X < 198 * DC.DungeonCrawl.ResolustionScale
                    && tl.Position.Y > 600 * DC.DungeonCrawl.ResolustionScale && tl.Position.Y < 728 * DC.DungeonCrawl.ResolustionScale)
                    actions.Add(PlayerActions.MoveLeft);
                else if (tl.Position.X > 195 * DC.DungeonCrawl.ResolustionScale && tl.Position.X < 323 * DC.DungeonCrawl.ResolustionScale
                    && tl.Position.Y > 600 * DC.DungeonCrawl.ResolustionScale && tl.Position.Y < 728 * DC.DungeonCrawl.ResolustionScale)
                    actions.Add(PlayerActions.MoveRight);
                else if (tl.Position.X > 1134 * DC.DungeonCrawl.ResolustionScale && tl.Position.X < 1230 * DC.DungeonCrawl.ResolustionScale
                    && tl.Position.Y > 600 * DC.DungeonCrawl.ResolustionScale && tl.Position.Y < 696 * DC.DungeonCrawl.ResolustionScale)
                    actions.Add(PlayerActions.Jump);
                else if (tl.Position.X > 1009 * DC.DungeonCrawl.ResolustionScale && tl.Position.X < 1105 * DC.DungeonCrawl.ResolustionScale
                    && tl.Position.Y > 600 * DC.DungeonCrawl.ResolustionScale && tl.Position.Y < 696 * DC.DungeonCrawl.ResolustionScale)
                    actions.Add(PlayerActions.Attack);
                else if (tl.Position.X > 1070 * DC.DungeonCrawl.ResolustionScale && tl.Position.X < 1166 * DC.DungeonCrawl.ResolustionScale
                    && tl.Position.Y > 520 * DC.DungeonCrawl.ResolustionScale && tl.Position.Y < 616 * DC.DungeonCrawl.ResolustionScale)
                    actions.Add(PlayerActions.Skill1);
                else
                {
                    foreach (Item item in level.Items)
                        if (tl.State == TouchLocationState.Released
                            && onScreen(item) && hero.hasCollision(item, timeDisplacement) != null
                            && tl.Position.X / DC.DungeonCrawl.ResolustionScale > getScreenPos(item.Position).X - item.size.X / 2 && tl.Position.X / DC.DungeonCrawl.ResolustionScale < getScreenPos(item.Position).X + item.size.X / 2
                            && tl.Position.Y / DC.DungeonCrawl.ResolustionScale > getScreenPos(item.Position).Y - item.size.Y / 2 && tl.Position.Y / DC.DungeonCrawl.ResolustionScale < getScreenPos(item.Position).Y + item.size.Y / 2)
                        {
                            hero.swapItem(item, ItemSlots.Weapon, level);
                            break;
                        }
                    foreach (Chest chest in level.Chests)
                        if (tl.State == TouchLocationState.Released && !chest.Open
                            && onScreen(chest) && hero.hasCollision(chest, timeDisplacement) != null
                            && tl.Position.X / DC.DungeonCrawl.ResolustionScale > getScreenPos(chest.Position).X - chest.size.X / 2 && tl.Position.X / DC.DungeonCrawl.ResolustionScale < getScreenPos(chest.Position).X + chest.size.X / 2
                            && tl.Position.Y / DC.DungeonCrawl.ResolustionScale > getScreenPos(chest.Position).Y - chest.size.Y / 2 && tl.Position.Y / DC.DungeonCrawl.ResolustionScale < getScreenPos(chest.Position).Y + chest.size.Y / 2)
                        {
                            chest.open();
                            break;
                        }
                }
        if (iiii % 30 == 0)
            Debug.Print("touchCommands:" + sw.ElapsedMilliseconds);

        hero.react(actions);

        if (iiii % 30 == 0)
            Debug.Print("heroreact:" + sw.ElapsedMilliseconds);

        updateEnemiesAI(timeDisplacement, level.Platforms);
        aiManager.updateAIPaths(level.Platforms, hero, gravity);

        if (iiii % 30 == 0)
            Debug.Print("aiUpdatae:" + sw.ElapsedMilliseconds);

        applyGravity(timeDisplacement);

        if (iiii % 30 == 0)
            Debug.Print("gravity:" + sw.ElapsedMilliseconds);

        doCollisionChecks(timeDisplacement);

        if (iiii % 30 == 0)
            Debug.Print("collisionchecks:" + sw.ElapsedMilliseconds);

        level.update(timeDisplacement, hero, aiManager, gravity);
        if (iiii % 30 == 0)
            Debug.Print("levelUpdate:" + sw.ElapsedMilliseconds);
        hero.update(timeDisplacement);
        if (iiii % 30 == 0)
            Debug.Print("heroUpdate:" + sw.ElapsedMilliseconds);
        updateEnemies(timeDisplacement);
        if (iiii % 30 == 0)
            Debug.Print("enemiesupdate:" + sw.ElapsedMilliseconds);
        if (iiii % 30 == 0)
            Debug.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

        return null;
    }

    public Vector2 calculateHeroScreenPos()
    {
        return BASE_HERO_SCREEN_POS + new Vector2(
            (hero.Position.X - hero.size.X / 2 < BASE_HERO_SCREEN_POS.X - hero.size.X / 2) 
                ? -(BASE_HERO_SCREEN_POS.X - hero.Position.X) 
                : (hero.Position.X + hero.size.X / 2 > level.Size.X - (DC.DungeonCrawl.DefaultResolution.X - BASE_HERO_SCREEN_POS.X - hero.size.X / 2)) 
                    ? hero.Position.X - (level.Size.X - (DC.DungeonCrawl.DefaultResolution.X - BASE_HERO_SCREEN_POS.X)) 
                    : 0, 
            (hero.Position.Y > -BASE_HERO_SCREEN_POS.Y) 
                ? (BASE_HERO_SCREEN_POS.Y + hero.Position.Y) / 1.5f * ((BASE_HERO_SCREEN_POS.Y + hero.Position.Y) / BASE_HERO_SCREEN_POS.Y) 
                : 0);
    }

    override
    public void draw(SpriteBatch sb)
    {
        Vector2 origin = -hero.Position + calculateHeroScreenPos();
        level.drawLevelBack(sb, origin, getDrawingArea());
        hero.draw(sb, origin);
        foreach (Character enemy in Enemies)
            enemy.draw(sb, origin);
        level.drawLevelFront(sb, origin, getDrawingArea());
    }
}