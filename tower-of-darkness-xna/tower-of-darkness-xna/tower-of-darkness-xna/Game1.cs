using System;
using System.Collections.Generic;
using System.Linq;
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

        private float ambient = 0.5f;
        private Color ambientColor = new Color(255, 235, 119);
        private Character character;
        private Texture2D light;
        private Texture2D lanternTexture;
        private Texture2D grassTexture;
        private Texture2D keyTexture;

        private List<Scene2DNode> nodeList;
        private Map map;
        //private Map map;
        private Rectangle mapView;

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
            mapView = graphics.GraphicsDevice.Viewport.Bounds;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            


            Texture2D characterSpriteSheet = Content.Load<Texture2D>("character2");

            grassTexture = Content.Load<Texture2D>("grass");
            keyTexture = Content.Load<Texture2D>("key");

            List<Scene2DNode> nodeList;
            light = Content.Load<Texture2D>("light");
            light = Content.Load<Texture2D>("light2");
            lanternTexture = Content.Load<Texture2D>("lantern");
            character = new Character(characterSpriteSheet, 3, 1, 32, 64, new Vector2(200, graphics.PreferredBackBufferHeight - 96), light, ambient, ambientColor, lanternTexture);
            loadLevel1Content();
            
        }

        private void loadLevel1Content() {
            nodeList = new List<Scene2DNode>();
            map = Content.Load<Map>("test");
            foreach (Tileset ts in map.Tilesets) {
                foreach (Tile t in ts.Tiles) {
                    if (t.Properties["type"].Value == "floor")
                    {
                        Console.WriteLine("Floor created.");
                    }
                }
            }
            Scene2DNode myKey = new Scene2DNode(keyTexture, new Vector2(graphics.PreferredBackBufferWidth - (keyTexture.Width * 2), graphics.PreferredBackBufferHeight - (keyTexture.Height * 2)), "key");
            nodeList.Add(myKey);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            KeyboardState keys = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Rectangle delta = mapView;
            if (keys.IsKeyDown(Keys.Down))
                delta.Y += Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 8);
            if (keys.IsKeyDown(Keys.Up))
                delta.Y -= Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 8);
            if (keys.IsKeyDown(Keys.Right))
                delta.X += Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 8);
            if (keys.IsKeyDown(Keys.Left))
                delta.X -= Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 8);

            if (map.Bounds.Contains(delta))
                mapView = delta;


            // TODO: Add your update logic here
            character.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            Color drawColor = new Color(ambientColor.R / 255f * ambient, ambientColor.G / 255f * ambient, ambientColor.B / 255f * ambient);
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            map.Draw(spriteBatch, mapView);
            character.Draw(spriteBatch, new Color(40, 40, 40));
            //map.Draw(spriteBatch, new Color(40, 40, 40));


            //foreach (Scene2DNode node in nodeList) {
            //    if (node.getNodeType() == "key")
            //        node.hover();

            //    node.Draw(spriteBatch);
            //}
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
