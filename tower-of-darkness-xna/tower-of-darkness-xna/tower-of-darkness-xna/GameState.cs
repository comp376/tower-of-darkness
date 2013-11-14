using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace tower_of_darkness_xna {
    public abstract class GameState {

        protected ContentManager Content;

        protected GameState(ContentManager Content) {
            this.Content = Content;
        }

        public abstract void LoadContent();

        public abstract void UnloadContent();

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime, SpriteBatch batch);
    }
}
