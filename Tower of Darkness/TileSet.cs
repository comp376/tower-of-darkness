using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tower_of_Darkness {
    class TileSet {
        public int firstgid;
        public int lastgid;
        public string name;
        public int tileWidth;
        public int tileHeight;
        public string source;
        public int imageWidth;
        public int imageHeight;
        //public Pixmap pixmap;
        public int tileAmountWidth;

        public TileSet(int firstgid, String name, int tileWidth,
            int tileHeight, String source, int imageWidth, int imageHeight) {
            this.firstgid = firstgid;
            this.name = name;
            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            this.source = source;
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
            tileAmountWidth = (int)Math.Floor((double)imageWidth / (double)tileWidth);
            lastgid = tileAmountWidth * (imageHeight / tileHeight) + firstgid - 1;
        }
    }
}
