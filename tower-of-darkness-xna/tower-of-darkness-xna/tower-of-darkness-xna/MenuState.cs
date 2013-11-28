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
        KeyboardState oldState;
        private const int NUM_MENU_ITEMS = 3;
        private Texture2D menuBackground;
        private Texture2D menuSelector;
        private Texture2D howToPlayBackground;
        private Texture2D creditsScreen;
        private Vector2 menuSelectorPosition;
        private int menuSelectorIndex = 0;
        private float menuTimer = 0;
        private float menuInterval = 100;

        private bool howToPlay = false;
        private float howToPlayTimer = 0;
        private float howToPlayInterval = 1000;

        private bool isCredits;

        public MenuState(ContentManager Content, int PreferredBackBufferWidth, int PreferredBackBufferHeight, string startingMapName, Character character, bool isCredits)
            : base(Content) {
            this.PreferredBackBufferWidth = PreferredBackBufferWidth;
            this.PreferredBackBufferHeight = PreferredBackBufferHeight;
            this.startingMapName = startingMapName;
            this.character = character;
            this.isCredits = isCredits;
            LoadContent();
        }

        public override void LoadContent() {
            menuBackground = Content.Load<Texture2D>("sprites/menuscreen");
            howToPlayBackground = Content.Load<Texture2D>("sprites/howtoplay");
            menuSelector = Content.Load<Texture2D>("sprites/menu_selector");
            creditsScreen = Content.Load<Texture2D>("sprites/creditscreen");
            menuSelectorPosition = new Vector2(70, 320);
            oldState = Keyboard.GetState();
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
            KeyboardState newState = Keyboard.GetState();
            howToPlayTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (newState.IsKeyDown(Keys.Enter))
            {
                if (!oldState.IsKeyDown(Keys.Enter))
                {
                    
                    if (howToPlayTimer >= howToPlayInterval) {
                        howToPlay = false;
                        howToPlayTimer = 0;
                        menuTimer = -500;
                    }
                    
                }
            }

            oldState = newState;
        }

        private void UpdateCredits(GameTime gameTime) {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) || Keyboard.GetState().IsKeyDown(Keys.Enter)) {
                menuTimer = -100;
                isCredits = false;
            }
        }

        public override void Update(GameTime gameTime) {
            if (isCredits) {
                UpdateCredits(gameTime);
            }else if (!howToPlay) {
                UpdateMenu(gameTime);
            } else {
                UpdateHowToPlay(gameTime);
            }

           

        }

        public override void Draw(GameTime gameTime, SpriteBatch batch) {
            if (isCredits) {
                batch.Begin();
                batch.Draw(creditsScreen, new Vector2(), Color.White);
                batch.End();
            }else if (!howToPlay) {
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
