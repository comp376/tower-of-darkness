using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Tower_of_Darkness
{
    public class Scene2DNode
    {
        private Texture2D texture;
        private Vector2 worldPosition;
        private string type;

        public int TextureWidth { get { return texture.Width; } }
        public int TextureHeight { get { return texture.Height; } }

        public Vector2 Position{
            get { return worldPosition; }
            set { worldPosition = value; }
        }

        public Scene2DNode(Texture2D texture, Vector2 position, String type){
            this.texture = texture;
            this.worldPosition = position;
            this.type = type;
        }

        //Regular Draw.
        public void Draw(SpriteBatch spriteBatch, Vector2 drawPosition)
        {
            spriteBatch.Draw(texture, drawPosition, Color.White);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Vector2(this.Position.X, this.Position.Y), Color.White);
        }

        //This draw function is used for spinning textures.
        public void Draw(SpriteBatch renderer, Vector2 drawPosition, float angle){
            renderer.Draw(texture, drawPosition, null, Color.White, angle, new Vector2(texture.Width/2, texture.Height/2), 1, SpriteEffects.None, 1);
        }

        public string getNodeType(){
            return this.type;
        }

    }
}