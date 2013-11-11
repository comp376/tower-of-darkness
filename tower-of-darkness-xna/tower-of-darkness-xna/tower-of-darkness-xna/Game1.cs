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
        //NEW STUFF

        const int WIDTH = 800;
        const int HEIGHT = 480;
        const string STARTING_MAP_NAME = "map1";

        public static GameState currentGameState;
        public static bool exitGame = false;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Character character;

        //OLD STUFF

        private Color OPAQUE_COLOR = new Color(60, 60, 60);

        private List<Rectangle> cRectangles;
        private List<Rectangle> tRectangles;

        
        private bool npcText;
        private int xMove = 0; // Zoning
        private int xMove1 = 0; // Scrolling
        private int yMove = 0;
        private bool isFirstZone;

        private GameStateEnum gameState = GameStateEnum.Menu;

        private string NPC_ONE_STRING = "???";
        private string NPC_TWO_STRING = "???";

        private const int NUM_NPCS = 2;
        private float ambient = 0.8f;
        private Color ambientColor = new Color(255, 235, 119);
        //private OldCharacter character;
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
        private List<OldNPC> npcs;
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
            graphics.PreferredBackBufferWidth = WIDTH;
            graphics.PreferredBackBufferHeight = HEIGHT;
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

            //mapView = new Rectangle(0, 0, 784, 480);

            //pickUpKey = Content.Load<SoundEffect>("pop");
            //pickUpKeyInstance = pickUpKey.CreateInstance();

            ////mapView = graphics.GraphicsDevice.Viewport.Bounds;

            //rand = new Random();
            //filter = Content.Load<Texture2D>("filter");
            base.Initialize();
        }

        //private void loadPlayingContent(String mapName) {
        //    background = Content.Load<Texture2D>("background");

        //    text = " ";
        //    npcText = false;
        //    map = Content.Load<Map>(mapName);
        //    modifyLayerOpacity();
        //    loadCollisionRectangles();
        //    loadTransitionRectangles();
        //    currentMap = map;
        //    Texture2D characterSpriteSheet = Content.Load<Texture2D>("character2");
        //    Texture2D npcSpriteSheet = Content.Load<Texture2D>("npc");
             
        //    grassTexture = Content.Load<Texture2D>("grass");
        //    keyTexture = Content.Load<Texture2D>("key");
        //    font = Content.Load<SpriteFont>("spriteFont");

        //    List<Scene2DNode> nodeList;
        //    npcs = new List<OldNPC>();
        //    OldNPC npc = new OldNPC(npcSpriteSheet, 3, 1, 32, 64, new Vector2(400, graphics.PreferredBackBufferHeight - 128), SpriteEffects.None, rand.Next(npcDirectionInterval.Item1, npcDirectionInterval.Item2), font, NPC_ONE_STRING);
        //    OldNPC npcTwo = new OldNPC(npcSpriteSheet, 3, 1, 32, 64, new Vector2(300, graphics.PreferredBackBufferHeight - 128), SpriteEffects.None, rand.Next(npcDirectionInterval.Item1, npcDirectionInterval.Item2), font, NPC_TWO_STRING);
        //    npcs.Add(npc);
        //    npcs.Add(npcTwo);
        //    light = Content.Load<Texture2D>("light");
        //    //light = Content.Load<Texture2D>("light2");
        //    lanternTexture = Content.Load<Texture2D>("lantern");
        //    character = new OldCharacter(characterSpriteSheet, 3, 1, 32, 64, new Vector2(200, 320), light, ambient, ambientColor, lanternTexture, graphics);

        //    switch (mapName)
        //    {
        //        case "map1":  
        //            loadLevel1Content();
        //            break;
        //        case "tower1":
        //            loadTower1Content();
        //            break;
        //        default:
        //            loadLevel1Content();
        //            break;
        //    }

        //}

        private void loadCollisionRectangles() {
            cRectangles = new List<Rectangle>();
            ObjectLayer ol = map.ObjectLayers["Collision"];
            foreach (MapObject mo in ol.MapObjects) {
                //Console.WriteLine(mo.Bounds.ToString());
                cRectangles.Add(mo.Bounds);
            }
        }

        private void loadTransitionRectangles() {
            tRectangles = new List<Rectangle>();
            ObjectLayer ol = map.ObjectLayers["Transition"];
            foreach (MapObject mo in ol.MapObjects) {
                tRectangles.Add(mo.Bounds);
            }
        }

        private void modifyLayerOpacity() {
            foreach(TileLayer tl in map.TileLayers){
                tl.OpacityColor = OPAQUE_COLOR;
            }
        }

        private void loadPauseContent() {
            pauseBackground = Content.Load<Texture2D>("pausescreen");
            pauseSelector = Content.Load<Texture2D>("menu_selector");
            pauseSelectorPosition = new Vector2(128, 150);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Texture2D characterSpriteSheet = Content.Load<Texture2D>("sprites/character2");
            character = new Character(characterSpriteSheet, 3, 1, 32, 64, Content);
            currentGameState = new MenuState(Content, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, STARTING_MAP_NAME, character);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        //private void updatePlaying(GameTime gameTime) {
        //    KeyboardState keys = Keyboard.GetState();
        //    //Rectangle delta = mapView;
        //    //if (keys.IsKeyDown(Keys.Down))
        //    //    delta.Y += Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 8);
        //    //if (keys.IsKeyDown(Keys.Up))
        //    //    delta.Y -= Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 8);
        //    //if (keys.IsKeyDown(Keys.Right))
        //    //    delta.X += Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 8);
        //    //if (keys.IsKeyDown(Keys.Left))
        //    //    delta.X -= Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 8);

        //    //if (map.Bounds.Contains(delta))
        //    //    mapView = delta;

        //    //Scrolling

        //    //if (keys.IsKeyDown(Keys.Right)) {
        //    //    xMove1 += 4;
        //    //    mapView = new Rectangle(xMove1, 0, 784, 480);
        //    //} else if (keys.IsKeyDown(Keys.Left)) {
        //    //    xMove1 -= 4;
        //    //    mapView = new Rectangle(xMove1, 0, 784, 480);
        //    //}

        //    //if (xMove1 <= 0) {
        //    //    xMove1 = 0;
        //    //    mapView = new Rectangle(xMove1, 0, 784, 480);
        //    //}

        //    //For more zoning to different areas

        //    if (character.objectPosition.X >= graphics.GraphicsDevice.Viewport.Bounds.Right) {
        //        xMove += 784;
        //        if (xMove == 2352)
        //            yMove += 960;

        //        mapView = new Rectangle(xMove, yMove, 784, 480);
        //        character.objectPosition.X = graphics.GraphicsDevice.Viewport.Bounds.Left;
        //        foreach (Scene2DNode s2dn in nodeList) {
        //            s2dn.worldPosition.X += xMove;
        //        } foreach (OldNPC npc in npcs) {
        //            npc.objectPosition.X += xMove;
        //        } for(int i = 0; i < cRectangles.Count; i++){
        //            cRectangles[i] = new Rectangle(cRectangles[i].X - xMove, cRectangles[i].Y + yMove, cRectangles[i].Width, cRectangles[i].Height);
        //        } for (int i = 0; i < tRectangles.Count; i++) {
        //            tRectangles[i] = new Rectangle(tRectangles[i].X - xMove, tRectangles[i].Y + yMove, tRectangles[i].Width, tRectangles[i].Height);
        //        }

                
        //    }

        //    if (xMove > 0)
        //    {

        //        if (character.objectPosition.X < graphics.GraphicsDevice.Viewport.Bounds.Left)
        //        {
                    
        //        character.objectPosition.X = graphics.GraphicsDevice.Viewport.Bounds.Right;
        //            foreach (Scene2DNode s2dn in nodeList)
        //            {
        //            s2dn.worldPosition.X -= xMove;
        //            } foreach (OldNPC npc in npcs)
        //            {
        //            npc.objectPosition.X -= xMove;
        //            }
                    
        //            for (int i = 0; i < cRectangles.Count; i++)
        //            {
        //            cRectangles[i] = new Rectangle(cRectangles[i].X + xMove, cRectangles[i].Y, cRectangles[i].Width, cRectangles[i].Height);
        //        } for (int i = 0; i < tRectangles.Count; i++) {
        //            tRectangles[i] = new Rectangle(tRectangles[i].X + xMove, tRectangles[i].Y, tRectangles[i].Width, tRectangles[i].Height);
        //        }

        //            xMove -= 784;
        //            mapView = new Rectangle(xMove, 0, 784, 480);
        //        }
        //    }

        //    if (character.objectPosition.X <= 4 && xMove == 0)
        //    {
        //        character.objectPosition.X = 4;
        //    }

        //    //if (character.objectPosition.X >= 776 && xMove == 784)
        //    //{
        //    //    character.objectPosition.X = 776;
        //    //}
        //    /* //Testing Boundaries. Will need to variable for each zone so we know which areas we can zone to
        //    if (character.objectPosition.X <= 4)
        //    {
        //         character.objectPosition.X = 4;
        //    }
        //     */


        //    // TODO: Add your update logic here
        //    pausePlayTimer += gameTime.ElapsedGameTime.Milliseconds;
        //    if (pausePlayTimer >= pausePlayInterval) {
        //        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) {
        //            gameState = GameStateEnum.Pause;
        //            pausePlayTimer = 0;
        //            pauseSelectTimer = -300;
        //        }
        //    }
        //    character.Update(gameTime, cRectangles);

        //    Rectangle playerRect = new Rectangle((int)character.objectPosition.X, (int)character.objectPosition.Y, character.spriteWidth, character.spriteHeight);
        //    //foreach (Rectangle r in cRectangles) {
        //    for(int i = 0; i < tRectangles.Count; i++){
        //        if (tRectangles[i].Intersects(playerRect)) {
        //            Console.WriteLine(map.ObjectLayers["Transition"].MapObjects[i].Name);
        //            loadPlayingContent(map.ObjectLayers["Transition"].MapObjects[i].Name);
        //            //mapView = new Rectangle(0, 0, 784, 480);
        //            //foreach(map.ObjectLayers["Transition"].MapObjects[i].Properties.Values){

        //            //}
        //            ////Dictiontary<string, string> 
        //                //map.ObjectLayers["Transition"].MapObjects[i].Properties.Values

                    
        //            xMove = 0;
        //            //map = Content.Load<Map>(map.ObjectLayers["Transition"].MapObjects[i].Name);
        //            loadCollisionRectangles();
        //            loadTransitionRectangles();
                    
                    
        //            //tRectangles = new List<Rectangle>();
        //            //ObjectLayer ol = map.ObjectLayers["Transition"];
        //            //foreach (MapObject mo in ol.MapObjects) {
        //            //    tRectangles.Add(mo.Bounds);
        //            //}
        //        }
        //    }



        //    for (int i = 0; i < nodeList.Count; i++) {
        //        if (character.Collides(nodeList[i])) {
        //            if (nodeList[i].getNodeType() == "key") {
        //                nodeList.RemoveAt(i);
        //                character.keyCount++;
        //                pickUpKeyInstance.Play();
        //            }
        //        }
        //    }

        //    foreach (OldNPC n in npcs) {
        //        if (character.Collides(n)) {

        //            if (keys.IsKeyDown(Keys.Up)) {
        //                //text = "Testing this out";
        //                //npcText = true;
        //                n.showText = true;
        //            }
        //        } else {
        //            n.showText = false;
        //        }

        //        n.Update(gameTime);
        //    }

        //}

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
                            gameState = GameStateEnum.Playing;
                            break;
                        case 2:         //Exit
                            gameState = GameStateEnum.Menu;
                            menuTimer = -300;
                            break;
                    }
                    pauseSelectorIndex = 0;
                    pauseSelectTimer = 0;
                }
                if (kbs.IsKeyDown(Keys.Escape)) {
                    gameState = GameStateEnum.Playing;
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
            currentGameState.Update(gameTime);
            if (exitGame) this.Exit();
            base.Update(gameTime);
        }

        //private void drawPlaying(GameTime gameTime) {
        //    //GraphicsDevice.Clear(Color.Black);

        //    Color drawColor = new Color(ambientColor.R / 255f * ambient, ambientColor.G / 255f * ambient, ambientColor.B / 255f * ambient);
        //    spriteBatch.Begin();
        //    spriteBatch.Draw(background, new Vector2(), Color.White);
        //    map.Draw(spriteBatch, mapView);
        //    character.Draw(spriteBatch, drawColor);
        //    foreach (OldNPC n in npcs) {
        //        n.Draw(spriteBatch, OPAQUE_COLOR);
        //    }
        //    foreach (Scene2DNode node in nodeList) {
        //        if (node.getNodeType() == "key")
        //            node.hover();
        //        node.Draw(spriteBatch);
        //    }

        //    //if (npcText == true)
        //    //{
        //    //    spriteBatch.DrawString(font, text, new Vector2(300, 300), Color.White);
        //    //}
        //    spriteBatch.End();
        //}

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
            GraphicsDevice.Clear(Color.Black);
            currentGameState.Draw(gameTime, spriteBatch);
            base.Draw(gameTime);
        }

    }
}
