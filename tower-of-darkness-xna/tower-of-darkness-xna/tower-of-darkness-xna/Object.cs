using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tower_of_darkness_xna {
    abstract class Object {

        protected Texture2D spriteSheet;
        protected int xNumberOfFrames;
        protected int yNumberOfFrames;
        protected int spriteWidth;
        protected int spriteHeight;
        protected Vector2 objectPosition;

        protected Object(Texture2D spriteSheet, int xNumberOfFrames, int yNumberOfFrames, int spriteWidth, int spriteHeight, Vector2 objectPosition) {
            this.spriteSheet = spriteSheet;
            this.xNumberOfFrames = xNumberOfFrames;
            this.yNumberOfFrames = yNumberOfFrames;
            this.spriteWidth = spriteWidth;
            this.spriteHeight = spriteHeight;
            this.objectPosition = objectPosition;
        }

        public abstract void Draw(SpriteBatch spriteBatch, Color color);
    }
}
