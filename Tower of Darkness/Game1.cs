#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace Tower_of_Darkness {
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

        private List<Scene2DNode> nodeList;

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

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Texture2D characterSpriteSheet = Content.Load<Texture2D>("character");
            grassTexture = Content.Load<Texture2D>("grass");

            List<Scene2DNode> nodeList = new List<Scene2DNode>();
            light = Content.Load<Texture2D>("light");
            lanternTexture = Content.Load<Texture2D>("lantern");
            character = new Character(characterSpriteSheet, 3, 1, 64, 64, new Vector2(50, 50), light, ambient, ambientColor, lanternTexture);
            loadLevel1Content();
        }

        private void loadLevel1Content(){
            nodeList = new List<Scene2DNode>();
            drawGround();
        }

        private void drawGround(){
            for (int i = 0; i < graphics.PreferredBackBufferWidth; i += 32){
                Scene2DNode node = new Scene2DNode(grassTexture, new Vector2(i,graphics.PreferredBackBufferHeight-grassTexture.Height), "grass");
                nodeList.Add(node);
            }
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

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
            character.Draw(spriteBatch, new Color(40, 40, 40));

            
            foreach (Scene2DNode node in nodeList){
                node.Draw(spriteBatch);
            }
            spriteBatch.End();




            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            //spriteBatch.Draw(light, new Rectangle((int)lightPosition.X, (int)lightPosition.Y, light.Width, light.Height), drawColor);
            //spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
