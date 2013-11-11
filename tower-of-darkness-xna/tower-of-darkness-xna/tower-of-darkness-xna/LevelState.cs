using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FuncWorks.XNA.XTiled;

namespace tower_of_darkness_xna {
    class LevelState : GameState {

        private bool DEBUG = true;

        private const int BACKGROUND_LAYER = 0;
        private const int FOREGROUND_LAYER = 1;
        private const int TOP_LAYER = 2;
        private Color OPAQUE_COLOR = new Color(25, 25, 25);
        private Color BACKGROUND_COLOR = new Color(100, 100, 100);

        private Texture2D backgroundTexture;
        private Map map;
        private Rectangle mapView;
        private Rectangle mapRect;
        private string mapName;
        private Character character;
        private List<Rectangle> cRectangles;
        private List<Transition> transitions;
        private List<NPC> npcs;

        //debug
        private Texture2D collision;
        private Texture2D transition;

        public LevelState(ContentManager Content, int PreferredBackBufferWidth, int PreferredBackBufferHeight, string mapName, Character character)
            : base(Content) {
                this.mapView = new Rectangle(0, 0, PreferredBackBufferWidth, PreferredBackBufferHeight);
                this.mapName = mapName;
                this.character = character;
                LoadContent();
        }

        public LevelState(ContentManager Content, int PreferredBackBufferWidth, int PreferredBackBufferHeight, string mapName, Character character, Transition transition)
            : this(Content, PreferredBackBufferWidth, PreferredBackBufferHeight, mapName, character) {
            int xChange = transition.xChange;
            int yChange = transition.yChange;

            //Move camera
            mapView.X += xChange;
            mapView.Y += yChange;

            //Place player location
            character.objectRectangle.X = transition.xPlayer;
            character.objectRectangle.Y = transition.yPlayer;

            //Move collision rectangles
            for (int i = 0; i < cRectangles.Count; i++) {
                cRectangles[i] = new Rectangle(cRectangles[i].X - xChange, cRectangles[i].Y - yChange, cRectangles[i].Width, cRectangles[i].Height);
            }

            //Move transition rectangles
            for (int i = 0; i < transitions.Count; i++) {
                transitions[i].tRect = new Rectangle(transitions[i].tRect.X - xChange, transitions[i].tRect.Y - yChange, transitions[i].tRect.Width, transitions[i].tRect.Height);
            }

            //Set player direction
            character.movementStatus = (MovementStatus)transition.direction;
        }

        public override void LoadContent() {
            backgroundTexture = Content.Load<Texture2D>("sprites/background");
            map = Content.Load<Map>("maps/" + mapName);
            modifyLayerOpacity();
            mapRect = new Rectangle(0, 0, map.Width * map.TileWidth, map.Height * map.TileHeight);
            loadCollisionRectangles();
            loadTransitionRectangles();
            loadNPCs();
            //loadMapObjects();
            loadMapInfo();

            //debug
            collision = Content.Load<Texture2D>("debug/collision");
            transition = Content.Load<Texture2D>("debug/transition");
        }

        private void modifyLayerOpacity() {
            foreach (TileLayer tl in map.TileLayers) {
                tl.OpacityColor = OPAQUE_COLOR;
            }
        }

        private void loadMapInfo() {
            if (map.ObjectLayers["Load"] == null)
                return;
            foreach (MapObject mo in map.ObjectLayers["Load"].MapObjects) {
                if (mo.Name == "Spawn") {
                    int xChange = (int)mo.Properties["xChange"].AsInt32 * 2;
                    int yChange = (int)mo.Properties["yChange"].AsInt32 * 2;

                    //Move camera first
                    mapView.X += xChange / 2;   //Divide by 2 to account for one side difference only
                    mapView.Y += yChange / 2;

                    //Place player location
                    character.objectRectangle.X = mo.Bounds.X - xChange;
                    character.objectRectangle.Y = mo.Bounds.Y - yChange;

                    //Move collision rectangles
                    for (int i = 0; i < cRectangles.Count; i++) {
                        cRectangles[i] = new Rectangle(cRectangles[i].X - xChange, cRectangles[i].Y - yChange, cRectangles[i].Width, cRectangles[i].Height);
                    }

                    //Move transition rectangles
                    for (int i = 0; i < transitions.Count; i++) {
                        transitions[i].tRect = new Rectangle(transitions[i].tRect.X - xChange, transitions[i].tRect.Y - yChange, transitions[i].tRect.Width, transitions[i].tRect.Height);
                    }

                    character.movementStatus = (MovementStatus)mo.Properties["direction"].AsInt32;
                }
            }
        } 

        private void loadCollisionRectangles() {
            cRectangles = new List<Rectangle>();
            int tileSize = map.TileWidth / 2;   //Not sure why dividing by 2
            foreach (TileData[] td in map.TileLayers[FOREGROUND_LAYER].Tiles) {
                foreach (TileData t in td) {
                    if (t != null) {
                        //Console.WriteLine(t.Target);
                        Rectangle cRect = t.Target;
                        cRect.X -= tileSize;
                        cRect.Y -= tileSize;
                        cRectangles.Add(cRect);
                    }
                }
            }
        }

        private void loadTransitionRectangles(){
            if (map.ObjectLayers["Transition"] == null)
                return;
            transitions = new List<Transition>();
            int tileSize = map.TileWidth / 2;   //Not sure why dividing by 2
            foreach (MapObject mo in map.ObjectLayers["Transition"].MapObjects) {
                Rectangle tRect = mo.Bounds;
                int direction = (int)mo.Properties["d"].AsInt32;
                int xChange = (int)mo.Properties["cx"].AsInt32;
                int yChange = (int)mo.Properties["cy"].AsInt32;
                int xPlayer = (int)mo.Properties["x"].AsInt32;
                int yPlayer = (int)mo.Properties["y"].AsInt32;
                Transition t = new Transition(mo.Name, tRect, direction, xChange, yChange, xPlayer, yPlayer);
                transitions.Add(t);
            }
        }

        private void loadNPCs() {
            if (map.ObjectLayers["NPC"] == null)
                return;
            npcs = new List<NPC>();
            foreach (MapObject mo in map.ObjectLayers["NPC"].MapObjects) {
                string spritesheetName = mo.Properties["spritesheet"].Value;
                Texture2D npcSpriteSheet = Content.Load<Texture2D>("sprites/" + spritesheetName);
                int xNumberOfFrames = (int)mo.Properties["xFrames"].AsInt32;
                int yNumberOfFrames = (int)mo.Properties["yFrames"].AsInt32;
                Rectangle npcRect = mo.Bounds;
                NPC npc = new NPC(npcSpriteSheet, xNumberOfFrames, yNumberOfFrames, npcRect.Width, npcRect.Height);
                npc.objectRectangle = npcRect; //also provides npc(x,y) 
                npcs.Add(npc);
            }
        }

        public override void UnloadContent() {
            //throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime) {
            character.Update(gameTime, mapRect, ref mapView, ref cRectangles, ref transitions);
        }

        public override void Draw(GameTime gameTime, SpriteBatch batch) {
            batch.Begin();
            batch.Draw(backgroundTexture, new Vector2(), BACKGROUND_COLOR);
            map.DrawLayer(batch, BACKGROUND_LAYER, mapView, 0);
            map.DrawLayer(batch, FOREGROUND_LAYER, mapView, 0);
            character.Draw(batch, Color.White);
            map.DrawLayer(batch, TOP_LAYER, mapView, 0);

            if (DEBUG) {
                foreach (Rectangle r in cRectangles) {
                    batch.Draw(collision, r, OPAQUE_COLOR);
                }
                foreach (Transition t in transitions) {
                    batch.Draw(transition, t.tRect, OPAQUE_COLOR);
                }
            }
            batch.End();
        }
    }
}
