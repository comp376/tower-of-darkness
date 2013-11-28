using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace tower_of_darkness_xna {
    class MenuState : GameState {

        private int PreferredBackBufferWidth;
        private int PreferredBackBufferHeight;
        private string startingMapName;
        private Character character;

        private const int NUM_MENU_ITEMS = 3;
        private Texture2D menuBackground;
        private Texture2D menuSelector;
        private Texture2D howToPlayBackground;
        private Vector2 menuSelectorPosition;
        private int menuSelectorIndex = 0;
        private float menuTimer = 0;
        private float menuInterval = 100;

        private bool howToPlay = false;

        public MenuState(ContentManager Content, int PreferredBackBufferWidth, int PreferredBackBufferHeight, string startingMapName, Character character)
            : base(Content) {
            this.PreferredBackBufferWidth = PreferredBackBufferWidth;
            this.PreferredBackBufferHeight = PreferredBackBufferHeight;
            this.startingMapName = startingMapName;
            this.character = character;
            LoadContent();
        }

        public override void LoadContent() {
            menuBackground = Content.Load<Texture2D>("sprites/menuscreen");
            howToPlayBackground = Content.Load<Texture2D>("sprites/howtoplay");
            menuSelector = Content.Load<Texture2D>("sprites/menu_selector");
            menuSelectorPosition = new Vector2(70, 320);
        }

        public override void UnloadContent() {
            //throw new NotImplementedException();
        }

        private void UpdateMenu(GameTime gameTime) {
            //throw new NotImplementedException();
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
                            Texture2D characterSpriteSheet = Content.Load<Texture2D>("sprites/character2");
                            SpriteFont font = Content.Load<SpriteFont>("fonts/spriteFont");
                            character = new Character(characterSpriteSheet, 3, 2, 32, 64, Content, font);
                            Game1.currentGameState = new LevelState(Content, PreferredBackBufferWidth, PreferredBackBufferHeight, startingMapName, character);
                            break;
                        case 1:
                            howToPlay = true;
                            break;
                        case 2:     //Exit
                            Game1.exitGame = true;
                            break;
                    }
                } if (kbs.IsKeyDown(Keys.Escape)) {
                    Game1.exitGame = true;
                }
            }
            menuSelectorPosition = new Vector2(70, 320 + menuSelectorIndex * 34);
        }

        public void UpdateHowToPlay(GameTime gameTime) {

        }

        public override void Update(GameTime gameTime) {
            if (!howToPlay) {
                UpdateMenu(gameTime);
            } else {
                UpdateHowToPlay(gameTime);
            }

           

        }

        public override void Draw(GameTime gameTime, SpriteBatch batch) {
            if (!howToPlay) {
                batch.Begin();
                batch.Draw(menuBackground, new Vector2(), Color.White);
                batch.Draw(menuSelector, menuSelectorPosition, Color.White);
                batch.End();
            } else {
                batch.Begin();
                batch.Draw(howToPlayBackground, new Vector2(), Color.White);
                batch.End();
            }

            
        }
    }
}
