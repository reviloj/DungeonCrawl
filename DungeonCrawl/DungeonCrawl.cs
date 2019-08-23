using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Collections.Generic;
using System;

namespace DC
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class DungeonCrawl : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        Stack<GameState> gameStates = new Stack<GameState>();
        Game game;
        Texture2D control;
        SpriteFont verdana36;

        public static bool debugVar = false;
        public static int graphDebugVar = 5;
        public static int graphDebugVar2 = 1;

        private static float defaultAspectRatio = (float)16 / 9;
        public static Vector2 DefaultResolution { get; private set; } = new Vector2(1280, 720);
        public static float ResolustionScale { get; private set; }
        public static Vector2 Resolution { get { return DefaultResolution * ResolustionScale; } }
        public static GameTime gameTime = new GameTime();
        // 16 milliseconds per update is for 60fps
        public static int FPS { get; } = 60;
        // Time per update in monogame with FPS (milliseconds)
        public static double timePerUpdate { get; } = 1d / FPS * 1000; // 16.6667; 
        public static int TicksPerFrame { get; } = 166667; // (int)(1d / FPS * 10000000);

        public DungeonCrawl()
        {
            TargetElapsedTime = TimeSpan.FromSeconds(1d / FPS);

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            
            game = new Game();
            gameStates.Push(game);
            gameStates.Peek().loadContent(textures);
            ((Game)gameStates.Peek()).generateLevel(0);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            control = Content.Load<Texture2D>("ball");
            verdana36 = Content.Load<SpriteFont>("font");

            textures.Add("DrawingPixel", new Texture2D(GraphicsDevice, 1, 1));
            textures["DrawingPixel"].SetData(new[] { Color.White });

            textures.Add("Hero", Content.Load<Texture2D>("AdventurerSpriteSheet"));
            textures.Add("GoldSwordHero", Content.Load<Texture2D>("GoldSwordAdventurerSpriteSheet"));
            textures.Add("Platform", Content.Load<Texture2D>("platform"));
            textures.Add("Chort",Content.Load<Texture2D>("ChortSpriteSheet"));
            textures.Add("BaseSwordTN", Content.Load<Texture2D>("BaseSwordTN"));
            textures.Add("GoldSwordTN", Content.Load<Texture2D>("GoldSwordTN"));
            textures.Add("SquarePlatform", Content.Load<Texture2D>("SquarePlatforms"));
            textures.Add("HorizontalPlatform", Content.Load<Texture2D>("HorizontalPlatforms"));
            textures.Add("VerticalPlatform", Content.Load<Texture2D>("VerticalPlatforms"));
            textures.Add("HorizontalDecoration", Content.Load<Texture2D>("HorizontalDecoration"));
            textures.Add("EmptyChest", Content.Load<Texture2D>("EmptyChestSpriteSheet"));
            textures.Add("DarkForestBG", Content.Load<Texture2D>("DarkForestLayers"));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            textures["DrawingPixel"].Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();


            GameState newState = gameStates.Peek().update((float)gameTime.ElapsedGameTime.Ticks / TicksPerFrame);
            if (newState == gameStates.Peek())
                gameStates.Pop();
            else if (newState != null)
            {
                gameStates.Push(newState);
                gameStates.Peek().loadContent(textures);
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if(ResolustionScale == 0)
            {
                float xResScale = GraphicsDevice.Viewport.Bounds.Width / DefaultResolution.X;
                float yResScale = GraphicsDevice.Viewport.Bounds.Height / DefaultResolution.Y;
                ResolustionScale = (xResScale < yResScale) ? xResScale : yResScale;
            }
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null);

            gameStates.Peek().draw(spriteBatch);

            foreach (KeyValuePair<int, SortedList<int, AIManager.ConnectivityNode>> nodeList in game.aiManager.connectivityGraph)
                foreach (KeyValuePair<int, AIManager.ConnectivityNode> node in nodeList.Value)
                {
                    spriteBatch.Draw(control, (node.Value.position - new Vector2(control.Width, control.Height) / 4 + (-game.hero.Position + game.calculateHeroScreenPos())) * DC.DungeonCrawl.ResolustionScale, //node.Value.position * ResolustionScale,
                    new Rectangle(0, 0, control.Width, control.Height), Color.White, 0.0f, new Vector2(0, 0), 1,
                    SpriteEffects.None, 0.0f);
                    foreach (AIManager.ConnectivityPath path in node.Value.connections)
                    {
                        bool jumpPath = path.jumpPath && path.jumpableStrengths[graphDebugVar] && path.minXVel(graphDebugVar, game.gravity) <= graphDebugVar2;
                        bool straightPath = path.straightPath && path.minXVel(0, game.gravity) <= graphDebugVar2;
                        if (jumpPath || straightPath)
                        {
                            Vector2 edge = (path.endNode.position - new Vector2(control.Width, control.Height) / 2 + (-game.hero.Position + game.calculateHeroScreenPos())) * DC.DungeonCrawl.ResolustionScale
                                - (path.startNode.position - new Vector2(control.Width, control.Height) / 2 + (-game.hero.Position + game.calculateHeroScreenPos())) * DC.DungeonCrawl.ResolustionScale;
                            //path.node2.position - path.node1.position;
                            // calculate angle to rotate line
                            float angle =
                                (float)Math.Atan2(edge.Y, edge.X);

                            Color c;
                            if (path.startNode.position.Y < path.endNode.position.Y)
                                c = new Color(Color.Blue, 50);
                            else
                                c = new Color(Color.Red, 50);
                            spriteBatch.Draw(textures["DrawingPixel"],
                                new Rectangle(// rectangle defines shape of line and position of start of line
                                    (int)((node.Value.position + (-game.hero.Position + game.calculateHeroScreenPos())) * DC.DungeonCrawl.ResolustionScale).X,
                                    (int)((node.Value.position + (-game.hero.Position + game.calculateHeroScreenPos())) * DC.DungeonCrawl.ResolustionScale).Y,
                                    (int)edge.Length(), //sb will strech the texture to fill this rectangle
                                    (straightPath && !jumpPath) ? 7 : (straightPath) ? 3 : 1), //width of line, change this to make thicker line
                                null,
                                c, //colour of line
                                angle,     //angle of line (calulated above)
                                new Vector2(0, 0), // point in line about which to rotate
                                SpriteEffects.None,
                                0);

                        }
                    }
                }

            /*if (game.Enemies != null && game.Enemies.Count > 0 && game.Enemies[0].ai.path != null) {
                Vector2 pos = game.Enemies[0].ai.path.position;
                spriteBatch.Draw(control, (pos - new Vector2(control.Width, control.Height) / 4 + (-game.hero.Position + game.calculateHeroScreenPos())) * DC.DungeonCrawl.ResolustionScale, //node.Value.position * ResolustionScale,
                        new Rectangle(0, 0, control.Width, control.Height), Color.White, 0.0f, new Vector2(0, 0), 1,
                        SpriteEffects.None, 0.0f);
            }*/

            spriteBatch.Draw(control, new Vector2(70, 600) * ResolustionScale,
                new Rectangle(0, 0, control.Width, control.Height), Color.White, 0.0f, new Vector2(0, 0), ResolustionScale * new Vector2((float)2, (float)2),
                SpriteEffects.None, 0.0f);
            spriteBatch.Draw(control, new Vector2(195, 600) * ResolustionScale,
                new Rectangle(0, 0, control.Width, control.Height), Color.White, 0.0f, new Vector2(0, 0), ResolustionScale * new Vector2((float)2, (float)2),
                SpriteEffects.None, 0.0f);
            spriteBatch.Draw(control, new Vector2(1134, 600) * ResolustionScale,
                new Rectangle(0, 0, control.Width, control.Height), Color.White, 0.0f, new Vector2(0, 0), ResolustionScale * new Vector2((float)1.5, (float)1.5),
                SpriteEffects.None, 0.0f);
            spriteBatch.Draw(control, new Vector2(1009, 600) * ResolustionScale,
                new Rectangle(0, 0, control.Width, control.Height), Color.White, 0.0f, new Vector2(0, 0), ResolustionScale * new Vector2((float)1.5, (float)1.5),
                SpriteEffects.None, 0.0f);
            spriteBatch.Draw(control, new Vector2(1070, 520) * ResolustionScale,
                new Rectangle(0, 0, control.Width, control.Height), Color.White, 0.0f, new Vector2(0, 0), ResolustionScale * new Vector2((float)1.5, (float)1.5),
                SpriteEffects.None, 0.0f);
            spriteBatch.Draw(control, new Vector2(1070, 50) * ResolustionScale,
                new Rectangle(0, 0, control.Width, control.Height), Color.White, 0.0f, new Vector2(0, 0), ResolustionScale * new Vector2((float)1.5, (float)1.5),
                SpriteEffects.None, 0.0f);
            spriteBatch.Draw(control, new Vector2(970, 50) * ResolustionScale,
                new Rectangle(0, 0, control.Width, control.Height), Color.White, 0.0f, new Vector2(0, 0), ResolustionScale * new Vector2((float)1.5, (float)1.5),
                SpriteEffects.None, 0.0f);
            spriteBatch.Draw(control, new Vector2(870, 50) * ResolustionScale,
                new Rectangle(0, 0, control.Width, control.Height), Color.White, 0.0f, new Vector2(0, 0), ResolustionScale * new Vector2((float)1.5, (float)1.5),
                SpriteEffects.None, 0.0f);
            /*spriteBatch.Draw(control, new Vector2(400, 400) * ResolustionScale,
                new Rectangle(0, 0, control.Width, control.Height), Color.White, 0.0f, new Vector2(0, 0), 1,
                SpriteEffects.None, 0.0f);*/
            spriteBatch.DrawString(verdana36, "~" + game.hero.Velocity.Y + "!!" + DC.DungeonCrawl.graphDebugVar + "~~~~" + graphDebugVar2, new Vector2(50, 275), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
