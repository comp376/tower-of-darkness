using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tower_of_darkness_xna {
    abstract class Object {

        protected const int STARTING_FRAME = 0;
        protected Texture2D spriteSheet;
        protected int xNumberOfFrames;
        protected int yNumberOfFrames;
        public int spriteWidth;
        public int spriteHeight;
        //public Vector2 objectPosition;
        public Rectangle objectRectangle;
        public SpriteFont font;
        public int id;

        protected Object(Texture2D spriteSheet, int xNumberOfFrames, int yNumberOfFrames, int spriteWidth, int spriteHeight, SpriteFont font)
        {
            this.spriteSheet = spriteSheet;
            this.xNumberOfFrames = xNumberOfFrames;
            this.yNumberOfFrames = yNumberOfFrames;
            this.spriteWidth = spriteWidth;
            this.spriteHeight = spriteHeight;
            this.objectRectangle = new Rectangle(0, 0, spriteWidth, spriteHeight);
            this.font = font;
        }

        protected Object(Texture2D spriteSheet, int xNumberOfFrames, int yNumberOfFrames, int spriteWidth, int spriteHeight, SpriteFont font, int id)
        {
            this.spriteSheet = spriteSheet;
            this.xNumberOfFrames = xNumberOfFrames;
            this.yNumberOfFrames = yNumberOfFrames;
            this.spriteWidth = spriteWidth;
            this.spriteHeight = spriteHeight;
            this.objectRectangle = new Rectangle(0, 0, spriteWidth, spriteHeight);
            this.font = font;
            this.id = id;
        }

        public abstract void Draw(SpriteBatch spriteBatch, Color color);
    }
}
