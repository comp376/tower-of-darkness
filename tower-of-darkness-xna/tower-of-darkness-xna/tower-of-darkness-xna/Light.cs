using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tower_of_darkness_xna {
    class Light {
        private const float STARTING_LIGHT_SIZE = 10.0f;

        private Texture2D lightTexture;
        private float currentLightSize; //10?
        public Rectangle lRect;
        private Vector2 lightPosition;
        private Color lightColor;
        private float lightAlpha;

        public Light(Texture2D lightTexture, Rectangle lRect) {
            this.lightTexture = lightTexture;
            this.lRect = lRect;
            currentLightSize = STARTING_LIGHT_SIZE;
            lightPosition = new Vector2(lRect.X - (lightTexture.Width / 2) + 16, lRect.Y - (lightTexture.Height / 2) + 16);
            lightColor = new Color(220, 220, 175);
            lightAlpha = 0.5f;
        }

        public void Draw(SpriteBatch batch) {
            batch.Draw(lightTexture, lightPosition, null, lightColor * lightAlpha);
        }
    }
}
