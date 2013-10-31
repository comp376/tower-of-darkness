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
        private Texture2D background;
        private Texture2D light;
        private Texture2D lanternTexture;
        private Texture2D grassTexture;
        private Texture2D keyTexture;

        private SoundEffect pickUpKey;
        private SoundEffectInstance pickUpKeyInstance;

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
            pickUpKey = Content.Load<SoundEffect>("pop");
            pickUpKeyInstance = pickUpKey.CreateInstance();

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

            background = Content.Load<Texture2D>("background");
            map = Content.Load<Map>("test");

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
            Console.WriteLine("Map is: " + map.Height + " tiles high");
            Console.WriteLine("Map is: " + map.Width + " tiles wide");
            Scene2DNode myKey = new Scene2DNode(keyTexture, new Vector2(475,125), "key");
            nodeList.Add(myKey);

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

            for (int i = 0; i < nodeList.Count; i++)
            {
                if (character.Collides(nodeList[i]))
                {
                    if (nodeList[i].getNodeType() == "key")
                    {
                        nodeList.RemoveAt(i);
                        character.keyCount++;
                        pickUpKeyInstance.Play();
                    }
                }
            }

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
            spriteBatch.Draw(background, new Vector2(), Color.White);
            map.Draw(spriteBatch, mapView);
            character.Draw(spriteBatch, new Color(40, 40, 40));


            foreach (Scene2DNode node in nodeList) {
                if (node.getNodeType() == "key")
                    node.hover();

                node.Draw(spriteBatch);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
