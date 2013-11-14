﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FuncWorks.XNA.XTiled;

namespace tower_of_darkness_xna {
    class LevelState : GameState {

        private bool DEBUG = true;
        private bool PAUSE_SCREEN = false;

        private const int BACKGROUND_LAYER = 0;
        private const int LADDER_LAYER = 1;
        private const int FOREGROUND_LAYER = 2;
        private const int TOP_LAYER = 3;
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
        private List<Rectangle> ladders;
        private List<NPC> npcs;

        //debug
        private Texture2D collision;
        private Texture2D transition;
        private Texture2D ladder;
        private Texture2D charDebug;

        //pause content
        private const int NUM_PAUSE_ITEMS = 3;
        private Texture2D pauseBackground;
        private Texture2D pauseSelector;
        private Vector2 pauseSelectorPosition;
        private int pauseSelectorIndex = 0;
        private float pauseSelectTimer = 0;
        private float pauseSelectInterval = 100;
        private float pausePlayTimer = 1000;
        private float pausePlayInterval = 1000;


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
            loadLadderRectangles();
            //loadNPCs();
            //loadMapObjects();
            loadMapInfo();

            //debug
            collision = Content.Load<Texture2D>("debug/collision");
            transition = Content.Load<Texture2D>("debug/transition");
            ladder = Content.Load<Texture2D>("debug/collision");
            charDebug = Content.Load<Texture2D>("debug/transition");

            //pause
            pauseBackground = Content.Load<Texture2D>("sprites/pausescreen");
            pauseSelector = Content.Load<Texture2D>("sprites/menu_selector");
            pauseSelectorPosition = new Vector2(128, 150);
        }

        private void modifyLayerOpacity() {
            if(DEBUG)
                OPAQUE_COLOR = new Color(100, 100, 100);

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
            foreach (TileData[] td in map.TileLayers["Foreground"].Tiles) {
                foreach (TileData t in td) {
                    if (t != null) {
                        Rectangle cRect = t.Target;
                        cRect.X -= tileSize;
                        cRect.Y -= tileSize;
                        cRectangles.Add(cRect);
                    }
                }
            }
        }

        private void loadLadderRectangles() {
            ladders = new List<Rectangle>();
            //int tileSize = map.TileWidth / 2;   //Not sure why dividing by 2
            //foreach (TileData[] td in map.TileLayers["Ladder"].Tiles) {
            //    foreach (TileData t in td) {
            //        if (t != null) {
            //            Rectangle lRect = t.Target;
            //            lRect.X -= tileSize;
            //            lRect.Y -= tileSize;
            //            ladders.Add(lRect);
            //        }
            //    }
            //}
        }

        private void loadTransitionRectangles(){
            transitions = new List<Transition>();
            if (map.ObjectLayers["Transition"] == null)
                return;
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
            npcs = new List<NPC>();
            if (map.ObjectLayers["NPC"] == null)
                return;            
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
            if (PAUSE_SCREEN)
                UpdatePause(gameTime);
            else
                UpdatePlaying(gameTime);
        }

        private void UpdatePlaying(GameTime gameTime) {
            pausePlayTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (pausePlayTimer >= pausePlayInterval) {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                    PAUSE_SCREEN = true;
                }
            }
            character.Update(gameTime, mapRect, ref mapView, ref cRectangles, ref transitions, ref ladders);
        }

        private void UpdatePause(GameTime gameTime) {
            pauseSelectTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (pauseSelectTimer >= pauseSelectInterval) {
                KeyboardState kbs = Keyboard.GetState();
                if (kbs.IsKeyDown(Keys.Up)) {
                    if (pauseSelectorIndex > 0) {
                        pauseSelectorIndex--;
                    } else {
                        pauseSelectorIndex = NUM_PAUSE_ITEMS - 1;
                    }
                    pauseSelectTimer = 0;
                }
                if (kbs.IsKeyDown(Keys.Down)) {
                    if (pauseSelectorIndex < NUM_PAUSE_ITEMS - 1) {
                        pauseSelectorIndex++;
                    } else {
                        pauseSelectorIndex = 0;
                    }
                    pauseSelectTimer = 0;
                }
                if (kbs.IsKeyDown(Keys.Space) || kbs.IsKeyDown(Keys.Enter)) {
                    switch (pauseSelectorIndex) {
                        case 0:         //Continue
                            PAUSE_SCREEN = false;
                            pauseSelectorIndex = 0;
                            pauseSelectTimer = 0;
                            pausePlayTimer = 0;
                            break;
                        case 2:         //Go to menu
                            Game1.currentGameState = new MenuState(Content, Game1.WIDTH, Game1.HEIGHT, Game1.STARTING_MAP_NAME, character);
                            pauseSelectorIndex = 0;
                            pauseSelectTimer = 0;
                            break;
                    }
                }
                if (kbs.IsKeyDown(Keys.Escape)) {
                    PAUSE_SCREEN = false;
                    pauseSelectorIndex = 0;
                    pauseSelectTimer = 0;
                    pausePlayTimer = 0;
                }
                pauseSelectorPosition = new Vector2(128, 150 + pauseSelectorIndex * 30);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch batch) {
            batch.Begin();
            batch.Draw(backgroundTexture, new Vector2(), BACKGROUND_COLOR);
            map.DrawLayer(batch, 1, mapView, 0);
            map.DrawLayer(batch, 2, mapView, 0);
            //map.DrawLayer(batch, LADDER_LAYER, mapView, 0);
            character.Draw(batch, Color.White);
            map.DrawLayer(batch, 0, mapView, 0);

            //Debug
            if (DEBUG) {
                foreach (Rectangle r in cRectangles) {
                    batch.Draw(collision, r, OPAQUE_COLOR);
                }
                foreach (Transition t in transitions) {
                    batch.Draw(transition, t.tRect, OPAQUE_COLOR);
                }
                //foreach (Rectangle r in ladders) {
                //    batch.Draw(ladder, r, OPAQUE_COLOR);
                //}
                batch.Draw(charDebug, character.objectRectangle, OPAQUE_COLOR);
            }

            batch.End();    //Stops additive blending from player drawing batch
            batch.Begin();
            //Pause
            if (PAUSE_SCREEN) {
                batch.Draw(pauseBackground, new Vector2(100, 60), Color.White);
                batch.Draw(pauseSelector, pauseSelectorPosition, Color.White);
            }
            batch.End();
            
        }
    }
}
