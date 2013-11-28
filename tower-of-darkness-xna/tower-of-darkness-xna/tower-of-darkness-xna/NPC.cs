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
        public string text;
        private string spritesheetName;
        private SpriteFont font;
        private bool isNPC = false;
        public bool showText = false;

        public string questAdvance;
        public bool wizardSpokenTo = false;

        public bool lanternPickedUp = false;
        

        //character inherited constructor
        public NPC(Texture2D spriteSheet, int xNumberOfFrames, int yNumberOfFrames, int spriteWidth, int spriteHeight, SpriteFont font)
            : base(spriteSheet, xNumberOfFrames, yNumberOfFrames, spriteWidth, spriteHeight, font) {
        }

        //npc constructor
        public NPC(Texture2D spriteSheet, int xNumberOfFrames, int yNumberOfFrames, int spriteWidth, int spriteHeight, string text, string spritesheetName, string quest, SpriteFont font, int id)
            : base(spriteSheet, xNumberOfFrames, yNumberOfFrames, spriteWidth, spriteHeight, font, id) {
                this.text = text;
                this.spritesheetName = spritesheetName;
                this.font = font;
                this.questAdvance = quest;
                isNPC = true;
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
            if (isNPC && showText){
                Vector2 fontOrigin = font.MeasureString(text) / 2;
                Vector2 fontPosition = new Vector2(objectRectangle.X, objectRectangle.Y - 16);
                spriteBatch.DrawString(font, text, fontPosition, Color.White, 0, fontOrigin, 0.75f, SpriteEffects.None, 0);     
            }//Show text?
        }

        private void getSourceRect(ref int x, ref int y) {
            if (isMoving) {
                x = xCurrentFrame * spriteWidth;
            } else {
                x = STARTING_FRAME * spriteWidth;
            }

            if (!isNPC) {
                if (lanternPickedUp) {
                    y = 0 * spriteHeight;
                } else {
                    y = 1 * spriteHeight;
                }
            } else {
                y = 0 * spriteHeight;
            }

            //switch (movementStatus) {
            //    case MovementStatus.None:
            //        y = (int)MovementStatus.Right * spriteHeight;
            //        break;
            //    default:
            //        //y = (int)movementStatus * spriteHeight;
            //        y = 0;
            //        break;
            //}
            ///*
            // if (yNumberOfFrames == 2){
            //    if (lanternPickedUp)
            //        y = (int)MovementStatus.Left * spriteHeight;
            //    else
            //        y = (int)MovementStatus.Right * spriteHeight;
            //}else
            //    y = (int)MovementStatus.Right * spriteHeight;
            //  */

        }

        public override string ToString() {
            return "NPC: spritesheet: [" + spritesheetName + "]"
                 + ", text: [" + text + "]"
                 + ", xFrames: [" + xNumberOfFrames + "]"
                 + ", yFrames: [" + yNumberOfFrames + "]";
        }
    }
}
