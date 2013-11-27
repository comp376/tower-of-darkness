using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace tower_of_darkness_xna {
    class Dim {

        public int id;
        public Rectangle dRect;
        public bool isPassed = false;

        public Dim(int id, Rectangle dRect) {
            this.id = id;
            this.dRect = dRect;
        }

        public void Update(GameTime gameTime) {
            
        }
    }
}
