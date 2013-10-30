using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Drawing;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Tower_of_Darkness {
    class MapTest {
        private ContentManager Content;
        private GraphicsDevice graphics;
        private const string EXT = ".tmx";

        private List<TileSet> tileSets;
        private List<Texture2D> textures;

        private string mapName;
        private XElement element;
        private int mapWidth;
        private int mapHeight;
        private int tileWidth;
        private int tileHeight;
        private Microsoft.Xna.Framework.Rectangle mapRect;

        public MapTest(string mapName, ContentManager Content, GraphicsDevice graphics) {
            this.Content = Content;
            this.graphics = graphics;
            Content.RootDirectory = "Content\\maps";
            tileSets = new List<TileSet>();
            textures = new List<Texture2D>();

            this.mapName = mapName;
            tilesetBuilder(mapName);
            //mapBuilder();
            //mapRect = new Microsoft.Xna.Framework.Rectangle(0, 0, (mapWidth * tileWidth), (mapHeight * tileHeight));
        }

        public void Draw(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Color color) {
            spriteBatch.Draw(textures[0], mapRect, color);
            //spriteBatch.Draw(textures[1], mapRect, color);
            //spriteBatch.Draw(textures[2], mapRect, color);
        }

        private void mapBuilder() {
            foreach (XElement lElement in element.Descendants("layer")) {
                string layerName = lElement.Attribute("name").Value;
                int layerWidth = Convert.ToInt32(lElement.Attribute("width").Value);
                int layerHeight = Convert.ToInt32(lElement.Attribute("height").Value);
                int[] tiles = new int[layerWidth * layerHeight];
                int tileLength = 0;
                XElement dElement = lElement.Element("data");
                foreach (XElement tElement in dElement.Descendants("tile")) {
                    int gid = Convert.ToInt32(tElement.Attribute("gid").Value);
                    if (gid > 0) {
                        tiles[tileLength] = gid;
                    }
                    tileLength++;
                }

                int[,] tileCoordinates = new int[mapWidth,mapHeight];
                for(int tileX = 0; tileX < mapWidth; tileX++){
                    for(int tileY = 0; tileY < mapHeight; tileY++){
                        tileCoordinates[tileX,tileY] = tiles[(tileX+(tileY*mapWidth))];
                    }
                }

                //pixmap stuff
                //Pixmap layerPixmap = new Pixmap(mapWidth * tileWidth, mapHeight * tileHeight, Pixmap.Format.RGBA8888);
                Bitmap layerImage = new Bitmap(mapWidth * tileWidth, mapHeight * tileHeight, System.Drawing.Imaging.PixelFormat.Canonical);

                for (int spriteForX = 0; spriteForX < mapWidth; spriteForX++) {
                    for (int spriteForY = 0; spriteForY < mapHeight; spriteForY++) {
                        int tileGid = tileCoordinates[spriteForX, spriteForY];
                        TileSet currentTileset = null;
                        foreach (TileSet ts in tileSets) {
                            if (tileGid >= ts.firstgid - 1 && tileGid <= ts.lastgid) {
                                currentTileset = ts;
                                break;
                            }
                        }
                        int destY = spriteForY * tileWidth;
                        int destX = spriteForX * tileWidth;

                        tileGid -= currentTileset.firstgid - 1;
                        int sourceY = (int)Math.Ceiling((double)tileGid / (double)currentTileset.tileAmountWidth) - 1;
                        int sourceX = tileGid - (currentTileset.tileAmountWidth * sourceY) - 1;

                        //layerPixmap.drawPixmap(currentTileset.pixmap, sourceX * tileWidth, sourceY * tileWidth, tileWidth, tileHeight, destX, destY, tileWidth, tileHeight);
                        //layerData = setImageData(sourceX * tileWidth, sourceY * tileWidth, tileWidth, tileHeight, destX, destY, tileWidth, tileHeight, mapWidth, mapHeight);
                    }
                }
                //textures.Add(layerTexture);
                //textures.add(new Texture(layerPixmap));
            }
        }

        //private Color[] setImageData(int srcx, int srcy, int srcWidth, int srcHeight, int dstx, int dsty, int dstWidth, int dstHeight, int mapWidth, int mapHeight) {
        //    //Console.WriteLine("srcx: " + srcx + ", " + "srcy: " + srcy + ", " + "srcWidth: " + srcWidth + ", " + "srcHeight: " + srcHeight + ", " + "dstx: " + dstx + ", " + "dsty: " + dsty + ", " + "dstWidth: " + dstWidth + ", " + "dstHeight: " + dstHeight + ", " + "mapWidth: " + mapWidth + ", " + "mapHeight: " + mapHeight);
        //    int width = dstWidth * mapWidth;
        //    int height = dstHeight * mapHeight;

        //}

        private void tilesetBuilder(string mapName) {
            Stream stream = TitleContainer.OpenStream("Content\\maps\\" + mapName + EXT);
            XDocument doc = XDocument.Load(stream);

            element = doc.Root;
            mapWidth = Convert.ToInt32(element.Attribute("width").Value);
            mapHeight = Convert.ToInt32(element.Attribute("height").Value);
            tileWidth = Convert.ToInt32(element.Attribute("tilewidth").Value);
            tileHeight = Convert.ToInt32(element.Attribute("tileheight").Value);

            foreach(XElement tsElement in element.Elements("tileset")){
                int imageWidth = Convert.ToInt32(tsElement.Element("image").Attribute("width").Value);
                int imageHeight = Convert.ToInt32(tsElement.Element("image").Attribute("height").Value);
                int firstGid = Convert.ToInt32(tsElement.Attribute("firstgid").Value);
                string tilesetName = tsElement.Attribute("name").Value;
                int tilesetTileWidth = Convert.ToInt32(tsElement.Attribute("tilewidth").Value);
                int tilesetTileHeight = Convert.ToInt32(tsElement.Attribute("tileheight").Value);
                String tilesetImagePath = tsElement.Element("image").Attribute("source").Value;
                tileSets.Add(new TileSet(firstGid, tilesetName, tilesetTileWidth, tilesetTileHeight, tilesetImagePath, imageWidth, imageHeight));
            }

            foreach (TileSet ts in tileSets) {
                Texture2D texture = Content.Load<Texture2D>(stripExtension(ts.source));
                MemoryStream ms = new MemoryStream();
                texture.SaveAsPng(ms, texture.Width, texture.Height);
                Bitmap bmp = new Bitmap(ms);
            }
        }

        private string stripExtension(string file) {
            int period = file.IndexOf('.');
            return file.Substring(0, period);
        }
    }
}
