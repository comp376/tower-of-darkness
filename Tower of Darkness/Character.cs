using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tower_of_Darkness {
    class Character : Object {

        private const int MOVE_SPEED = 2;

        private bool isMoving;
        private int xCurrentFrame = 0;
        private int yCurrentFrame = 0;
        private float frameTimer = 0;
        private float frameInterval = 100;
        private Texture2D lightTexture;
        private float ambient;
        private Color ambientColor;
        private Vector2 lightPosition;
        private float LIGHT_SIZE = 1f;

        public Character(Texture2D spriteSheet, int xNumberOfFrames, int yNumberOfFrames, int spriteWidth, int spriteHeight, Vector2 objectPosition, Texture2D lightTexture, float ambient, Color ambientColor)
            : base(spriteSheet, xNumberOfFrames, yNumberOfFrames, spriteWidth, spriteHeight, objectPosition) {
            this.lightTexture = lightTexture;
            this.ambient = ambient;
            this.ambientColor = ambientColor;
            lightPosition = new Vector2(objectPosition.X, objectPosition.Y - 8);
        }

        public void Update(GameTime gameTime) {
            move();

            if (isMoving) {
                frameTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (frameTimer >= frameInterval) {
                    if (xCurrentFrame < xNumberOfFrames - 1) {
                        xCurrentFrame++;
                    } else {
                        xCurrentFrame = 0;
                    }

                    if (yCurrentFrame < yNumberOfFrames - 1) {
                        yCurrentFrame++;
                    } else {
                        yCurrentFrame = 0;
                    }
                    frameTimer = 0;
                }
            }
            KeyboardState kbs = Keyboard.GetState();
            if (kbs.IsKeyDown(Keys.OemMinus) || kbs.IsKeyDown(Keys.Subtract)) {
                if (LIGHT_SIZE < 1.5f)
                    LIGHT_SIZE += 0.1f;
            } if (kbs.IsKeyDown(Keys.OemPlus) || kbs.IsKeyDown(Keys.Add)) {
                if (LIGHT_SIZE > 0.5f)
                    LIGHT_SIZE -= 0.1f;
            }

            System.Diagnostics.Debug.WriteLine(LIGHT_SIZE);
        }

        private void move() {
            KeyboardState kbs = Keyboard.GetState();
            if (kbs.IsKeyDown(Keys.Up) || kbs.IsKeyDown(Keys.Down) || kbs.IsKeyDown(Keys.Left) || kbs.IsKeyDown(Keys.Right)) {
                isMoving = true;
                if (kbs.IsKeyDown(Keys.Up))
                    objectPosition.Y -= MOVE_SPEED;
                if (kbs.IsKeyDown(Keys.Down))
                    objectPosition.Y += MOVE_SPEED;
                if (kbs.IsKeyDown(Keys.Left))
                    objectPosition.X -= MOVE_SPEED;
                if (kbs.IsKeyDown(Keys.Right))
                    objectPosition.X += MOVE_SPEED;

            }

            if (kbs.IsKeyUp(Keys.Up) && kbs.IsKeyUp(Keys.Down) && kbs.IsKeyUp(Keys.Left) && kbs.IsKeyUp(Keys.Right)) {
                isMoving = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Color color) {
            Rectangle sourceRect = new Rectangle(spriteWidth * xCurrentFrame, spriteHeight * yCurrentFrame, spriteWidth, spriteHeight);
            spriteBatch.Draw(spriteSheet, objectPosition, sourceRect, color);
            spriteBatch.End();
            Color drawColor = new Color(ambientColor.R / 255f * ambient, ambientColor.G / 255f * ambient, ambientColor.B / 255f * ambient);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            lightPosition = objectPosition;
            lightPosition.Y = objectPosition.Y - ((lightTexture.Height / LIGHT_SIZE) / 2);
            lightPosition.X = objectPosition.X - ((lightTexture.Width / LIGHT_SIZE) / 2) + spriteWidth;
            spriteBatch.Draw(lightTexture, new Rectangle((int)lightPosition.X, (int)lightPosition.Y, (int)(lightTexture.Width / LIGHT_SIZE), (int)(lightTexture.Height / LIGHT_SIZE)), drawColor);
        }

    }
}
