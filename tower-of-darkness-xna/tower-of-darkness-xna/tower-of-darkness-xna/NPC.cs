﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tower_of_darkness_xna {
    class NPC : Object{

        private const int MOVE_SPEED = 2;

        private SpriteEffects walkingDirection = SpriteEffects.None;
        private int xCurrentFrame = 0;
        private int yCurrentFrame = 0;

        private float moveTimer = 0;
        private float moveInterval = 500;
        private float directionTimer = 0;
        private float directionInterval = 1000;
        private Random rand;

        public NPC(Texture2D spriteSheet, int xNumberOfFrames, int yNumberOfFrames, int spriteWidth, int spriteHeight, Vector2 objectPosition, SpriteEffects walkingDirection, float directionInterval)
            : base(spriteSheet, xNumberOfFrames, yNumberOfFrames, spriteWidth, spriteHeight, objectPosition) {
                this.walkingDirection = walkingDirection;
                this.directionInterval = directionInterval;
            rand = new Random();
        }

        public void Update(GameTime gameTime) {
            
            move(gameTime);
        }

        private void move(GameTime gameTime) {
            directionTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (directionTimer >= directionInterval) {
                int i = rand.Next(0, 2);
                if (i == 0)
                    walkingDirection = SpriteEffects.FlipHorizontally;
                else
                    walkingDirection = SpriteEffects.None;
                directionTimer = 0;
            }

            moveTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (moveTimer >= moveInterval) {
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
                switch (walkingDirection) {
                    case SpriteEffects.FlipHorizontally:
                        objectPosition.X -= MOVE_SPEED;
                        break;
                    case SpriteEffects.None:
                        objectPosition.X += MOVE_SPEED;
                        break;
                }
                moveTimer = 0;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Color color) {
            Rectangle sourceRect = new Rectangle(spriteWidth * xCurrentFrame, spriteHeight * yCurrentFrame, spriteWidth, spriteHeight);
            spriteBatch.Draw(spriteSheet, objectPosition, sourceRect, color, 0, new Vector2(), 1, walkingDirection, 0);
        }
    }
}