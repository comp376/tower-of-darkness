using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tower_of_darkness_xna {

    //should fix this class. was originally made for 4-directional type npc's
    class NPC : Object {
        //Animation
        protected int xCurrentFrame = 0;
        protected int yCurrentFrame = 0;
        protected int frameTimer = 0;
        protected int frameInterval = 80;
        public bool isMoving;
        public MovementStatus movementStatus;

        public NPC(Texture2D spriteSheet, int xNumberOfFrames, int yNumberOfFrames, int spriteWidth, int spriteHeight)
            : base(spriteSheet, xNumberOfFrames, yNumberOfFrames, spriteWidth, spriteHeight) {

        }

        public void Update(GameTime gameTime) {


        }

        protected void animate(GameTime gameTime) {
            frameTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (frameTimer >= frameInterval) {
                if (xCurrentFrame < xNumberOfFrames - 1) {
                    xCurrentFrame++;
                } else {
                    xCurrentFrame = 0;
                }

                frameTimer = 0;
            }
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
    }
}
