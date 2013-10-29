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
        private const float LIGHT_CHANGE = 0.05f;
        private const float BOUNDARY_CHANGE = 0.05f;
        private const float ANGLE_CHANGE = 15.0f;

        private bool isMoving;
        private int xCurrentFrame = 0;
        private int yCurrentFrame = 0;
        private float frameTimer = 0;
        private float frameInterval = 100;
        private Texture2D lightTexture;
        private float ambient;
        private Color ambientColor;
        private Vector2 lightPosition;
        private float lightTimer = 0;
        private float lightInterval = 50;
        private float currentLightSize;
        private LightDirection lightDir;
        private float LOWER_BOUNDARY = 1.0f;
        private float UPPER_BOUNDARY = 1.0f;

        private Texture2D lanternTexture;
        private Vector2 lanternPosition;
        private float lanternTimer = 0;
        private float lanternInterval = 100;
        private LanternSwing lanternSwing;
        private float lanternAngle = 0f;
        private float BACKWARDS_BOUNDARY = 30;
        private float FORWARDS_BOUNDARY = -30;

        public Character(Texture2D spriteSheet, int xNumberOfFrames, int yNumberOfFrames, int spriteWidth, int spriteHeight, Vector2 objectPosition, Texture2D lightTexture, float ambient, Color ambientColor, Texture2D lanternTexture)
            : base(spriteSheet, xNumberOfFrames, yNumberOfFrames, spriteWidth, spriteHeight, objectPosition) {
            this.lightTexture = lightTexture;
            this.ambient = ambient;
            this.ambientColor = ambientColor;
            lightPosition = new Vector2(objectPosition.X, objectPosition.Y - 8);
            lightDir = LightDirection.Increasing;
            currentLightSize = LOWER_BOUNDARY;
            this.lanternTexture = lanternTexture;
            lanternPosition = objectPosition;
            lanternSwing = LanternSwing.Forwards;
        }

        public void Update(GameTime gameTime) {
            move();
            pulse(gameTime);
            lanternSwinging(gameTime);

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
                LOWER_BOUNDARY += BOUNDARY_CHANGE;
                UPPER_BOUNDARY += BOUNDARY_CHANGE;
            } if (kbs.IsKeyDown(Keys.OemPlus) || kbs.IsKeyDown(Keys.Add)) {
                LOWER_BOUNDARY -= BOUNDARY_CHANGE;
                UPPER_BOUNDARY -= BOUNDARY_CHANGE;
            }

            System.Diagnostics.Debug.WriteLine(currentLightSize);
        }

        private void pulse(GameTime gameTime) {
            lightTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (lightTimer >= lightInterval) {
                if (lightDir == LightDirection.Increasing) {
                    if (currentLightSize < UPPER_BOUNDARY)
                        currentLightSize += LIGHT_CHANGE;
                    else
                        lightDir = LightDirection.Decreasing;
                } if (lightDir == LightDirection.Decreasing) {
                    if (currentLightSize > LOWER_BOUNDARY)
                        currentLightSize -= LIGHT_CHANGE;
                    else
                        lightDir = LightDirection.Increasing;
                }
                lightTimer = 0;
            }
        }

        private void lanternSwinging(GameTime gameTime) {
            lanternTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (lanternTimer >= lanternInterval) {
                if (lanternSwing == LanternSwing.Forwards) {
                    if (lanternAngle > FORWARDS_BOUNDARY) {
                        lanternAngle -= ANGLE_CHANGE;
                    } else {
                        lanternSwing = LanternSwing.Backwards;
                    }
                } if (lanternSwing == LanternSwing.Backwards) {
                    if (lanternAngle < BACKWARDS_BOUNDARY) {
                        lanternAngle += ANGLE_CHANGE;
                    } else {
                        lanternSwing = LanternSwing.Forwards;
                    }
                }
                lanternTimer = 0;
            }
        }

        private void move() {
            KeyboardState kbs = Keyboard.GetState();
            if (kbs.IsKeyDown(Keys.Up) || kbs.IsKeyDown(Keys.Down) || kbs.IsKeyDown(Keys.Left) || kbs.IsKeyDown(Keys.Right)) {
                isMoving = true;
                if (kbs.IsKeyDown(Keys.Up)) {
                    objectPosition.Y -= MOVE_SPEED;
                }
                if (kbs.IsKeyDown(Keys.Down)) {
                    objectPosition.Y += MOVE_SPEED;
                }
                if (kbs.IsKeyDown(Keys.Left)) {
                    objectPosition.X -= MOVE_SPEED;
                }
                if (kbs.IsKeyDown(Keys.Right)) {
                    objectPosition.X += MOVE_SPEED;
                }

            }

            if (kbs.IsKeyUp(Keys.Up) && kbs.IsKeyUp(Keys.Down) && kbs.IsKeyUp(Keys.Left) && kbs.IsKeyUp(Keys.Right)) {
                isMoving = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Color color) {
            Rectangle sourceRect = new Rectangle(spriteWidth * xCurrentFrame, spriteHeight * yCurrentFrame, spriteWidth, spriteHeight);
            spriteBatch.Draw(spriteSheet, objectPosition, sourceRect, color);
            lanternPosition = objectPosition;
            lanternPosition.X += 48;
            lanternPosition.Y += 32;
            spriteBatch.Draw(lanternTexture, lanternPosition, new Rectangle(0, 0, lanternTexture.Width, lanternTexture.Height), Color.White, degreeToRadian(0), new Vector2(lanternTexture.Width / 2, lanternTexture.Height / 2), 1, SpriteEffects.None, 0); //scale float
            //draw fire
            spriteBatch.End();
            Color drawColor = new Color(ambientColor.R / 255f * ambient, ambientColor.G / 255f * ambient, ambientColor.B / 255f * ambient);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            lightPosition = objectPosition;
            lightPosition.X += ((lightTexture.Width  + currentLightSize )/ 2) - spriteWidth - 10;
            lightPosition.Y = ((lightTexture.Height - currentLightSize) / 2) - spriteHeight - 85;
            spriteBatch.Draw(lightTexture, new Rectangle((int)lightPosition.X, (int)(lightPosition.Y - (currentLightSize * 6)), (int)(lightTexture.Width + (currentLightSize * 15)), (int)(lightTexture.Height + (currentLightSize * 15))), new Rectangle(0, 0, lightTexture.Width, lightTexture.Height), drawColor, degreeToRadian(0), new Vector2(lightTexture.Width / 2, 0), SpriteEffects.None, 0);
        }

        private float degreeToRadian(float angle) {
            return (float)Math.PI * angle / 180.0f;
        }
    }

    enum LightDirection {
        Increasing, Decreasing
    }

    enum LanternSwing {
        Forwards, Backwards
    }
}
