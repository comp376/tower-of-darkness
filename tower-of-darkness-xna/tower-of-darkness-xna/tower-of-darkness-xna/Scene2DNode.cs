using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace tower_of_darkness_xna {
    public class Scene2DNode {
        public Texture2D texture;
        public Vector2 worldPosition;
        public Vector2 startingPosition;
        public string type;
        private const int MAX_HOVER_HEIGHT = 20;
        public bool consumed = false;

        enum hoverDirections {
            Up,
            Down,
        }
        hoverDirections hoverDirection;


        public int TextureWidth { get { return texture.Width; } }
        public int TextureHeight { get { return texture.Height; } }

        //public Vector2 Position {
        //    get { return worldPosition; }
        //    set { worldPosition = value; }
        //}

        //public Vector2 StartingPosition {
        //    get { return startingPosition; }
        //    set { startingPosition = value; }
        //}

        public Scene2DNode(Texture2D texture, Vector2 position, String type) {
            this.texture = texture;
            this.worldPosition = position;
            this.startingPosition = position;
            this.type = type;
            this.hoverDirection = hoverDirections.Up;
        }

        //Regular Draw.
        public void Draw(SpriteBatch spriteBatch, Vector2 drawPosition) {
            spriteBatch.Draw(texture, drawPosition, Color.White);
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(texture, new Vector2(worldPosition.X, worldPosition.Y), Color.White);
        }

        public void Draw(SpriteBatch spriteBatch, Color colorEssence, Color other){ 
            Color color; 
            if (type == "essence" || type == "super essence") 
                color = colorEssence; else color = other; 
            spriteBatch.Draw(texture, new Vector2(worldPosition.X, worldPosition.Y), color); 
        }

        //This draw function is used for spinning textures.
        public void Draw(SpriteBatch spriteBatch, Vector2 drawPosition, float angle) {
            spriteBatch.Draw(texture, drawPosition, null, Color.White, angle, new Vector2(texture.Width / 2, texture.Height / 2), 1, SpriteEffects.None, 1);
        }

        public void hover() {
            if (this.hoverDirection == hoverDirections.Up) {
                if (this.worldPosition.Y > (this.startingPosition.Y - MAX_HOVER_HEIGHT)) {
                    this.worldPosition.Y -= 0.3f;
                } else {
                    this.hoverDirection = hoverDirections.Down;
                }
            } else if (this.hoverDirection == hoverDirections.Down) {//going down
                if (this.startingPosition.Y > this.worldPosition.Y) {
                    this.worldPosition.Y += 0.3f;
                } else {
                    this.hoverDirection = hoverDirections.Up;
                }
            }
        }

    }
}
