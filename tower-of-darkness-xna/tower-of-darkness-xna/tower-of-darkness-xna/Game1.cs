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
        public static int WIDTH = 800;
        public static int HEIGHT = 480;

        public static string STARTING_MAP_NAME = "tower1";

        public static GameState currentGameState;
        public static bool exitGame = false;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Character character;

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
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
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