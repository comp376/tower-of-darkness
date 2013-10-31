using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using FuncWorks.XNA.XTiled;

namespace tower_of_darkness_xna {
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private bool npcText;
        private int xMove = 784; // Zoning
        private int xMove1 = 0; // Scrolling

        private GameState gameState = GameState.Menu;

        private string NPC_ONE_STRING = "ABCDEFGHIJKLMNOPQRTSTUVXYZ\nABCDEFGHIJKLMNOPQRTSTUVXYZ";
        private string NPC_TWO_STRING = "Sup, #2 here";

        private const int NUM_NPCS = 2;
        private float ambient = 0.8f;
        private Color ambientColor = new Color(255, 235, 119);
        private Character character;
        private Texture2D background;
        private Texture2D light;
        private Texture2D lanternTexture;
        private Texture2D grassTexture;
        private Texture2D keyTexture;
        private Tuple<int, int> npcDirectionInterval = new Tuple<int, int>(500, 3000);
        private Random rand;
        private SpriteFont font;
        private string text;

        private SoundEffect pickUpKey;
        private SoundEffectInstance pickUpKeyInstance;

        private List<Scene2DNode> nodeList;
        private List<NPC> npcs;
        public Map currentMap;
        private Map map;
        private Rectangle mapView;

        //Menu vars
        private const int NUM_MENU_ITEMS = 2;
        private Texture2D menuBackground;
        private Texture2D menuSelector;
        private Vector2 menuSelectorPosition;
        private int menuSelectorIndex = 0;
        private float menuTimer = 100;
        private float menuInterval = 100;

        //Pause vars
        private const int NUM_PAUSE_ITEMS = 3;
        private Texture2D pauseBackground;
        private Texture2D pauseSelector;
        private Vector2 pauseSelectorPosition;
        private int pauseSelectorIndex = 0;
        private float pauseSelectTimer = 100;
        private float pauseSelectInterval = 100;
        private float pausePlayTimer = 1000;
        private float pausePlayInterval = 1000;

        private Texture2D filter;

        public Game1()
            : base() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here
            //graphics.ToggleFullScreen();

            mapView = new Rectangle(0, 0, 784, 480);

            pickUpKey = Content.Load<SoundEffect>("pop");
            pickUpKeyInstance = pickUpKey.CreateInstance();

            //mapView = graphics.GraphicsDevice.Viewport.Bounds;
            
            rand = new Random();
            filter = Content.Load<Texture2D>("filter");
            base.Initialize();
        }

        private void loadPlayingContent() {
            background = Content.Load<Texture2D>("background");

           

            text = " ";
            npcText = false;
            map = Content.Load<Map>("test");
            currentMap = map;
            Texture2D characterSpriteSheet = Content.Load<Texture2D>("character2");
            Texture2D npcSpriteSheet = Content.Load<Texture2D>("npc");

            grassTexture = Content.Load<Texture2D>("grass");
            keyTexture = Content.Load<Texture2D>("key");
            font = Content.Load<SpriteFont>("spriteFont");

            List<Scene2DNode> nodeList;
            npcs = new List<NPC>();
            NPC npc = new NPC(npcSpriteSheet, 3, 1, 32, 64, new Vector2(400, graphics.PreferredBackBufferHeight - 128), SpriteEffects.None, rand.Next(npcDirectionInterval.Item1, npcDirectionInterval.Item2), font, NPC_ONE_STRING);
            NPC npcTwo = new NPC(npcSpriteSheet, 3, 1, 32, 64, new Vector2(300, graphics.PreferredBackBufferHeight - 128), SpriteEffects.None, rand.Next(npcDirectionInterval.Item1, npcDirectionInterval.Item2), font, NPC_TWO_STRING);
            npcs.Add(npc);
            npcs.Add(npcTwo);
            light = Content.Load<Texture2D>("light");
            light = Content.Load<Texture2D>("light2");
            lanternTexture = Content.Load<Texture2D>("lantern");
            character = new Character(characterSpriteSheet, 3, 1, 32, 64, new Vector2(200, graphics.PreferredBackBufferHeight - 128), light, ambient, ambientColor, lanternTexture, graphics);
            loadLevel1Content();
        }

        private void loadPauseContent() {
            pauseBackground = Content.Load<Texture2D>("pausescreen");
            pauseSelector = Content.Load<Texture2D>("menu_selector");
            pauseSelectorPosition = new Vector2(128, 150);
        }

        private void loadMenuContent() {
            menuBackground = Content.Load<Texture2D>("menuscreen");
            menuSelector = Content.Load<Texture2D>("menu_selector");
            menuSelectorPosition = new Vector2(70, 320);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            loadMenuContent();

            loadPauseContent();
            //loadPlayingContent();  //We can split these per level. : Joel
            
        }

        private void loadLevel1Content() {
            nodeList = new List<Scene2DNode>();
   
            map = Content.Load<Map>("test2");

            Console.WriteLine("Map is: " + map.Height + " tiles high");
            Console.WriteLine("Map is: " + map.Width + " tiles wide");
            Scene2DNode myKey = new Scene2DNode(keyTexture, new Vector2(475,125), "key");
            nodeList.Add(myKey);

            //Test to get every tile data.
            /*
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                        //Console.WriteLine(x + "---" + y + ": " + map.TileLayers[0].Tiles[x][y].SourceID);
                }
            }
            */
            

            int[] toBeRemoved;//saves index of node(s) to be removed.
            toBeRemoved = new int[nodeList.Count];
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        private void updatePlaying(GameTime gameTime) {
            KeyboardState keys = Keyboard.GetState();
            Console.WriteLine(xMove);
            //Rectangle delta = mapView;
            //if (keys.IsKeyDown(Keys.Down))
            //    delta.Y += Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 8);
            //if (keys.IsKeyDown(Keys.Up))
            //    delta.Y -= Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 8);
            //if (keys.IsKeyDown(Keys.Right))
            //    delta.X += Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 8);
            //if (keys.IsKeyDown(Keys.Left))
            //    delta.X -= Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 8);

            //if (map.Bounds.Contains(delta))
            //    mapView = delta;

           character.Update(gameTime);

            //Scrolling
            /*
           if (keys.IsKeyDown(Keys.Right))
           {
               xMove1 += 4;
               mapView = new Rectangle(xMove1, 0, 784, 480);
           }
           else if (keys.IsKeyDown(Keys.Left))
           {
               xMove1 -= 4;
               mapView = new Rectangle(xMove1, 0, 784, 480);
           }

           if (xMove1 <= 0)
           {
               xMove1  = 0;
               mapView = new Rectangle(xMove1, 0, 784, 480);
           }
            */
            //For more zoning to different areas
            
           if (character.objectPosition.X >= graphics.GraphicsDevice.Viewport.Bounds.Right)
           {
               mapView = new Rectangle(0 + xMove, 0, 784, 480);
                character.objectPosition.X = graphics.GraphicsDevice.Viewport.Bounds.Left;
           }

         if (character.objectPosition.X < graphics.GraphicsDevice.Viewport.Bounds.Left)
           {
               mapView = new Rectangle(0, 0, 784, 480);
               character.objectPosition.X = graphics.GraphicsDevice.Viewport.Bounds.Right;
           }

            /* //Testing Boundaries. Will need to variable for each zone so we know which areas we can zone to
         if (character.objectPosition.X <= 4)
         {
             character.objectPosition.X = 4;
         }
             */
             

            // TODO: Add your update logic here
            pausePlayTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (pausePlayTimer >= pausePlayInterval) {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                    gameState = GameState.Pause;
                    pausePlayTimer = 0;
                    pauseSelectTimer = -300;
                }
            }
            character.Update(gameTime);

            

            for (int i = 0; i < nodeList.Count; i++) {
                if (character.Collides(nodeList[i])) {
                    if (nodeList[i].getNodeType() == "key") {
                        nodeList.RemoveAt(i);
                        character.keyCount++;
                        pickUpKeyInstance.Play();
                    }
                }
            }

            foreach (NPC n in npcs) {
                if (character.Collides(n)) {

                    if (keys.IsKeyDown(Keys.Up)) {
                        //text = "Testing this out";
                        //npcText = true;
                        n.showText = true;
                    }
                } else {
                    n.showText = false;
                }
            
                n.Update(gameTime);
            }

        }

        private void updateMenu(GameTime gameTime) {
            menuTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (menuTimer >= menuInterval) {
                KeyboardState kbs = Keyboard.GetState();
                if (kbs.IsKeyDown(Keys.Up)) {
                    if (menuSelectorIndex > 0) {
                        menuSelectorIndex--;
                    } else {
                        menuSelectorIndex = NUM_MENU_ITEMS - 1;
                    }
                    menuTimer = 0;
                } if (kbs.IsKeyDown(Keys.Down)) {
                    if (menuSelectorIndex < NUM_MENU_ITEMS - 1) {
                        menuSelectorIndex++;
                    } else {
                        menuSelectorIndex = 0;
                    }
                    menuTimer = 0;
                } if (kbs.IsKeyDown(Keys.Space) || kbs.IsKeyDown(Keys.Enter)) {
                    switch (menuSelectorIndex) {
                        case 0:     //New Game
                            loadPlayingContent();
                            gameState = GameState.Playing;
                            break;
                        case 1:     //Exit
                            Exit();
                            break;
                    }
                } if (kbs.IsKeyDown(Keys.Escape)) {
                    Exit();
                }
            }
            menuSelectorPosition = new Vector2(70, 320 + menuSelectorIndex * 34);
        }


        private void updatePause(GameTime gameTime) {
            pauseSelectTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (pauseSelectTimer >= pauseSelectInterval) {
                KeyboardState kbs = Keyboard.GetState();
                if (kbs.IsKeyDown(Keys.Up)) {
                    if (pauseSelectorIndex > 0) {
                        pauseSelectorIndex--;
                    } else {
                        pauseSelectorIndex = NUM_PAUSE_ITEMS - 1;
                    }
                    pauseSelectTimer = 0;
                }
                if (kbs.IsKeyDown(Keys.Down)) {
                    if (pauseSelectorIndex < NUM_PAUSE_ITEMS - 1) {
                        pauseSelectorIndex++;
                    } else {
                        pauseSelectorIndex = 0;
                    }
                    pauseSelectTimer = 0;
                }
                if (kbs.IsKeyDown(Keys.Space) || kbs.IsKeyDown(Keys.Enter)) {
                    switch (pauseSelectorIndex) {
                        case 0:         //Continue
                            gameState = GameState.Playing;
                            break;
                        case 2:         //Exit
                            gameState = GameState.Menu;
                            menuTimer = -300;
                            break;
                    }
                    pauseSelectorIndex = 0;
                    pauseSelectTimer = 0;
                }
                if (kbs.IsKeyDown(Keys.Escape)) {
                    gameState = GameState.Playing;
                    pauseSelectorIndex = 0;
                    pauseSelectTimer = 0;
                    pausePlayTimer = -300;
                }

                pauseSelectorPosition = new Vector2(128, 150 + pauseSelectorIndex * 30);
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            switch (gameState) {
                case GameState.Menu:
                    updateMenu(gameTime);
                    break;
                case GameState.Playing:
                    updatePlaying(gameTime);
                    break;
                case GameState.Pause:
                    updatePause(gameTime);
                    break;
            }
            base.Update(gameTime);
        }

        private void drawPlaying(GameTime gameTime) {
            //GraphicsDevice.Clear(Color.Black);

            Color drawColor = new Color(ambientColor.R / 255f * ambient, ambientColor.G / 255f * ambient, ambientColor.B / 255f * ambient);
            spriteBatch.Begin();
            spriteBatch.Draw(background, new Vector2(), Color.White);
            map.Draw(spriteBatch, mapView);
            character.Draw(spriteBatch, new Color(50, 50, 50));
            foreach (NPC n in npcs) {
                n.Draw(spriteBatch, new Color(50, 50, 50));

                
            }
            foreach (Scene2DNode node in nodeList) {
                if (node.getNodeType() == "key")
                    node.hover();
                node.Draw(spriteBatch);
            }

            //if (npcText == true)
            //{
            //    spriteBatch.DrawString(font, text, new Vector2(300, 300), Color.White);
            //}
            spriteBatch.End();
        }

        private void drawMenu(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            spriteBatch.Draw(menuBackground, new Vector2(), Color.White);
            spriteBatch.Draw(menuSelector, menuSelectorPosition, Color.White);
            spriteBatch.End();
        }

        private void drawPause(GameTime gameTime) {
            spriteBatch.Begin();
            spriteBatch.Draw(pauseBackground, new Vector2(100, 60), Color.White);
            spriteBatch.Draw(pauseSelector, pauseSelectorPosition, Color.White);
            spriteBatch.End();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {

            switch (gameState) {
                case GameState.Menu:
                    drawMenu(gameTime);
                    break;
                case GameState.Playing:
                    drawPlaying(gameTime);
                    break;
                case GameState.Pause:
                    drawPause(gameTime);
                    break;
            }
            
            base.Draw(gameTime);
        }
    }
}
