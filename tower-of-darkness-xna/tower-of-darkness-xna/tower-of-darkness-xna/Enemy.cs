﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tower_of_darkness_xna {
    class Enemy : Object {
        protected int xCurrentFrame = 0;
        protected int yCurrentFrame = 0;
        protected int frameTimer = 0;
        protected int frameInterval = 80;
        public bool isMoving;
        public MovementStatus movementStatus;

        public int hits;
        private string spritesheetName;

        public Enemy(Texture2D spriteSheet, int xNumberOfFrames, int yNumberOfFrames, int spriteWidth, int spriteHeight, int hits, string spritesheetName, SpriteFont font)
            : base(spriteSheet, xNumberOfFrames, yNumberOfFrames, spriteWidth, spriteHeight, font) {
                this.hits = hits;
                this.spritesheetName = spritesheetName;
        }

        public void Update(GameTime gameTime) {

        }

        public override void Draw(SpriteBatch spriteBatch, Color color) {
            int x = 0, y = 0;
            getSourceRect(ref x, ref y);
            Rectangle sourceRect = new Rectangle(x, y, spriteWidth, spriteHeight);
            SpriteEffects flip = SpriteEffects.None;
            switch (movementStatus) {
                case MovementStatus.Left:
                    flip = SpriteEffects.FlipHorizontally;
                    break;
            }
            spriteBatch.Draw(spriteSheet, objectRectangle, sourceRect, color, 0f, new Vector2(), flip, 0);
        }

        private void getSourceRect(ref int x, ref int y) {
            if (isMoving) {
                x = xCurrentFrame * spriteWidth;
            } else {
                x = STARTING_FRAME * spriteWidth;
            }

            switch (movementStatus) {
                case MovementStatus.None:
                    y = (int)MovementStatus.Right * spriteHeight;
                    break;
                default:
                    //y = (int)movementStatus * spriteHeight;
                    y = 0;
                    break;
            }

        }

        public override string ToString() {
            return "ENEMY: spritesheet: [" + spritesheetName + "]"
                 + ", hits: [" + hits + "]"
                 + ", xFrames: [" + xNumberOfFrames + "]"
                 + ", yFrames: [" + yNumberOfFrames + "]";
        }
    }
}
