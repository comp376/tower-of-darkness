using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Tower_of_Darkness {
    class Map {

        private ContentManager Content;

        private List<TileSet> tileSets;
        private List<Texture2D> textures;

        private string mapName;
        //private Element element;
        private int mapWidth;
        private int mapHeight;
        private int tileWidth;
        private int tileHeight;

        public Map(string mapName, ContentManager Content) {
            this.Content = Content;
            tileSets = new List<TileSet>();
            textures = new List<Texture2D>();

            this.mapName = mapName;
            tilesetBuilder(mapName);
        }

        private void tilesetBuilder(string mapName) {
            //XmlReader xml = new XmlReader();
            //try {
                
            //}
            string xml = Content.Load<string>(mapName);

        }
    }
}
