using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace tower_of_darkness_xna {
    class Breakable {

        public Rectangle bRect;
        private float breakTimer = 0;
        private float breakInterval = 500;
        public bool isTouched = false;
        public bool isBroken = false;
        public string type;
        public int i;
        public int j;

        public Breakable(Rectangle bRect, int i, int j, string type) {
            this.bRect = bRect;
            this.i = i;
            this.j = j;
            this.type = type;
        }

        public void Update(GameTime gameTime) {
            if (!isBroken && isTouched && this.type == "breakable") {
                breakTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (breakTimer >= breakInterval) {
                    isBroken = true;
                }
            }
            else if (!isBroken && isTouched && this.type == "door")
            {
                isBroken = true;
            }
        }

    }
}
