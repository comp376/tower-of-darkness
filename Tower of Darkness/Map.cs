using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tower_of_Darkness {
    class Map {

        private List<TileSet> tileSets;
        private List<Texture2D> textures;

        private string mapName;

        public Map(string mapName) {
            tileSets = new List<TileSet>();
            textures = new List<Texture2D>();

            this.mapName = mapName;
            tilesetBuilder(mapName);
        }

        private void tilesetBuilder(string mapName) {
            
        }
    }
}
