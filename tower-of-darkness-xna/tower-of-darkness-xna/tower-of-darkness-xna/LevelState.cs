using System;
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
        private Color ITEM_COLOR = new Color(255, 255, 255);
        private float alpha = 0.9f;
        private Color BACKGROUND_COLOR = new Color(255, 255, 255, 1);

        private Texture2D backgroundTexture;
        private Map map;
        private Rectangle mapView;
        private Rectangle mapRect;
        private string mapName;
        private Character character;
        private List<Rectangle> cRectangles;
        private List<Transition> transitions;
        private List<Rectangle> ladders;
        private List<Breakable> breakables;
        private List<Scene2DNode> objects;
        private bool[] visited = new bool[13];

        private List<NPC> npcs;
        private List<Enemy> enemies;
        private SpriteFont font;

        //debug
        private Texture2D collision;
        private Texture2D transition;
        private Texture2D ladder;
        private Texture2D charDebug;
        private Texture2D breakable;
        private Texture2D npc;
        private Texture2D enemy;
        private Texture2D keyTexture;
        private Texture2D essenceTexture;
        private Texture2D superEssenceTexture;

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
            : base(Content) 
        {
            this.mapView = new Rectangle(0, 0, PreferredBackBufferWidth, PreferredBackBufferHeight);
            this.mapName = mapName;
            this.character = character;
            for (int i = 0; i < 13; i++)
            {
                visited[i] = false;
            }
            LoadContent(); 
        }

        public LevelState(ContentManager Content, int PreferredBackBufferWidth, int PreferredBackBufferHeight, string mapName, Character character, Transition transition)
        :base(Content){
            this.mapView = new Rectangle(0, 0, PreferredBackBufferWidth, PreferredBackBufferHeight);
            this.mapName = mapName;
            this.character = character;
            

            int xChange = transition.xChange;
            int yChange = transition.yChange;
            LoadContent();
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

            //Move ladder rectangles
            for (int i = 0; i < ladders.Count; i++) {
                ladders[i] = new Rectangle(ladders[i].X - xChange, ladders[i].Y - yChange, ladders[i].Width, ladders[i].Height);
            }

            //Move breakable rectangles
            for (int i = 0; i < breakables.Count; i++) {
                breakables[i].bRect = new Rectangle(breakables[i].bRect.X - xChange, breakables[i].bRect.Y - yChange, breakables[i].bRect.Width, breakables[i].bRect.Height);
            }

            //Move npcs
            for (int i = 0; i < npcs.Count; i++) {
                npcs[i].objectRectangle = new Rectangle(npcs[i].objectRectangle.X - xChange, npcs[i].objectRectangle.Y - yChange, npcs[i].objectRectangle.Width, npcs[i].objectRectangle.Height);
            }


            if (visited[(int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32])
            {
                
                //Move Objects
                for (int i = 0; i < objects.Count; i++)
                {
                    objects[i] = new Scene2DNode(objects[i].texture, new Vector2(objects[i].worldPosition.X - xChange, objects[i].worldPosition.Y - yChange), objects[i].type);
                }
            }
            
            //Move enemies
            for (int i = 0; i < enemies.Count; i++) {
                enemies[i].objectRectangle = new Rectangle(enemies[i].objectRectangle.X - xChange, enemies[i].objectRectangle.Y - yChange, enemies[i].objectRectangle.Width, enemies[i].objectRectangle.Height);
            }
                
            //Set player direction
            character.movementStatus = (MovementStatus)transition.direction;
        }

        public override void LoadContent() {
            backgroundTexture = Content.Load<Texture2D>("sprites/background");
            keyTexture = Content.Load<Texture2D>("sprites/key");
            essenceTexture = Content.Load<Texture2D>("sprites/essence");
            superEssenceTexture = Content.Load<Texture2D>("sprites/superEssence");
            font = Content.Load<SpriteFont>("fonts/spriteFont");
            map = Content.Load<Map>("maps/" + mapName);
            modifyLayerOpacity();
            mapRect = new Rectangle(0, 0, map.Width * map.TileWidth, map.Height * map.TileHeight);
            loadCollisionRectangles();
            loadTransitionRectangles();
            loadLadderRectangles();
            loadBreakables();
            loadObjects();
            loadNPCs();
            loadEnemies();
            loadMapInfo();

            //debug
            collision = Content.Load<Texture2D>("debug/collision");
            transition = Content.Load<Texture2D>("debug/transition");
            ladder = Content.Load<Texture2D>("debug/ladder");
            charDebug = Content.Load<Texture2D>("debug/char");
            breakable = Content.Load<Texture2D>("debug/ladder");
            npc = Content.Load<Texture2D>("debug/char");
            enemy = Content.Load<Texture2D>("debug/char");

            //pause
            pauseBackground = Content.Load<Texture2D>("sprites/pausescreen");
            pauseSelector = Content.Load<Texture2D>("sprites/menu_selector");
            pauseSelectorPosition = new Vector2(128, 150);
        }

        private void modifyLayerOpacity() {
            if(DEBUG)
                OPAQUE_COLOR = new Color(100, 100, 100);

            foreach (TileLayer tl in map.TileLayers) {
                tl.OpacityColor = OPAQUE_COLOR * alpha;
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

                    //Move ladder rectangles
                    for (int i = 0; i < ladders.Count; i++) {
                        ladders[i] = new Rectangle(ladders[i].X - xChange, ladders[i].Y - yChange, ladders[i].Width, ladders[i].Height);
                    }

                    //Move breakable rectangles
                    for (int i = 0; i < breakables.Count; i++) {
                        breakables[i].bRect = new Rectangle(breakables[i].bRect.X - xChange, breakables[i].bRect.Y - yChange, breakables[i].bRect.Width, breakables[i].bRect.Height);
                    }

                    //Move npcs
                    for (int i = 0; i < npcs.Count; i++) {
                        npcs[i].objectRectangle = new Rectangle(npcs[i].objectRectangle.X - xChange, npcs[i].objectRectangle.Y - yChange, npcs[i].objectRectangle.Width, npcs[i].objectRectangle.Height);
                    }

                    if (!visited[(int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32])
                    {
                        //Move Objects
                        for (int i = 0; i < objects.Count; i++)
                        {
                            Console.WriteLine("Moving some objects");
                            objects[i] = new Scene2DNode(objects[i].texture, new Vector2(objects[i].worldPosition.X - xChange, objects[i].worldPosition.Y - yChange), objects[i].type);
                        }
                    }
                    character.movementStatus = (MovementStatus)mo.Properties["direction"].AsInt32;
                }
            }
        } 

        private void loadCollisionRectangles() {
            cRectangles = new List<Rectangle>();
            if (map.TileLayers["Foreground"] == null)
                return;
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
            if (map.TileLayers["Ladder"] == null)
                return;
            int tileSize = map.TileWidth / 2;   //Not sure why dividing by 2
            foreach (TileData[] td in map.TileLayers["Ladder"].Tiles) {
                foreach (TileData t in td) {
                    if (t != null) {
                        Rectangle lRect = t.Target;
                        lRect.X -= tileSize;
                        lRect.Y -= tileSize;
                        ladders.Add(lRect);
                    }
                }
            }
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

        private void loadBreakables() {
            breakables = new List<Breakable>();
            if (map.TileLayers["Breakable"] == null)
                return;
            int tileSize = map.TileWidth / 2;   //Not sure why dividing by 2
            for (int i = 0; i < map.TileLayers["Breakable"].Tiles.Length; i++) {
                for (int j = 0; j < map.TileLayers["Breakable"].Tiles[i].Length; j++) {
                    if (map.TileLayers["Breakable"].Tiles[i][j] != null) {
                        Rectangle lRect = map.TileLayers["Breakable"].Tiles[i][j].Target;
                        lRect.X -= tileSize;
                        lRect.Y -= tileSize;
                        Breakable b = new Breakable(lRect, i, j, "breakable");
                        breakables.Add(b);
                    }
                }
            }
            if (map.TileLayers["Doors"] == null)
                return;
            for (int i = 0; i < map.TileLayers["Doors"].Tiles.Length; i++)
            {
                for (int j = 0; j < map.TileLayers["Doors"].Tiles[i].Length; j++)
                {
                    if (map.TileLayers["Doors"].Tiles[i][j] != null)
                    {
                        Rectangle lRect = map.TileLayers["Doors"].Tiles[i][j].Target;
                        lRect.X -= tileSize;
                        lRect.Y -= tileSize;
                        Breakable b = new Breakable(lRect, i, j, "door");
                        breakables.Add(b);
                    }
                }
            }
        }

        private void loadObjects()
        {
            //for (int i = 0; i < MAP_COUNT; i++)
           // {
            //    Console.WriteLine(theMapObjects[i].Count);
           // }
            objects = new List<Scene2DNode>();
            if (map.ObjectLayers["Objects"] != null)
            {
                if (character.theMapObjects[(int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32].Contains(character.emptyNode))
                {
                    int tileSize = map.TileWidth / 2;
                    foreach (MapObject mo in map.ObjectLayers["Objects"].MapObjects)
                    {
                        if (mo.Properties["Type"].Value == "key")
                        {
                            Scene2DNode node = new Scene2DNode(keyTexture, new Vector2(mo.Bounds.X, mo.Bounds.Y), "key");
                            objects.Add(node);
                        }
                        else if (mo.Properties["Type"].Value == "essence")
                        {
                            Scene2DNode node = new Scene2DNode(essenceTexture, new Vector2(mo.Bounds.X, mo.Bounds.Y), "essence");
                            objects.Add(node);
                        }
                        else if (mo.Properties["Type"].Value == "super essence")
                        {
                            Scene2DNode node = new Scene2DNode(superEssenceTexture, new Vector2(mo.Bounds.X, mo.Bounds.Y), "super essence");
                            objects.Add(node);
                        }
                    }
                    visited[(int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32] = true;
                    character.theMapObjects[(int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32].Remove(character.emptyNode);
                    character.theMapObjects[(int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32] = objects;
                }
                else
                {
                    objects = character.theMapObjects[(int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32];
                }
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
                string text = mo.Properties["text"].Value;
                NPC n = new NPC(npcSpriteSheet, xNumberOfFrames, yNumberOfFrames, npcRect.Width, npcRect.Height, text, spritesheetName, font);
                Console.WriteLine(n.ToString());
                n.objectRectangle = npcRect; //also provides npc(x,y) 
                npcs.Add(n);
            }
        }

        private void loadEnemies() {
            enemies = new List<Enemy>();
            if (map.ObjectLayers["Enemy"] == null)
                return;
            foreach (MapObject mo in map.ObjectLayers["Enemy"].MapObjects) {
                string spritesheetName = mo.Properties["spritesheet"].Value;
                Texture2D enemySpriteSheet = Content.Load<Texture2D>("sprites/" + spritesheetName);
                int xNumberOfFrames = (int)mo.Properties["xFrames"].AsInt32;
                int yNumberOfFrames = (int)mo.Properties["yFrames"].AsInt32;
                Rectangle enemyRect = mo.Bounds;
                int hits = (int)mo.Properties["hits"].AsInt32;
                Enemy e = new Enemy(enemySpriteSheet, xNumberOfFrames, yNumberOfFrames, enemyRect.Width, enemyRect.Height, hits, spritesheetName);
                Console.WriteLine(e.ToString());
                e.objectRectangle = enemyRect;
                enemies.Add(e);
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
            character.Update(gameTime, mapRect, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies, ref objects);
            for (int i = 0; i < breakables.Count; i++) {
                breakables[i].Update(gameTime);
                if (breakables[i].isBroken && breakables[i].type == "breakable") {
                    map.TileLayers["Breakable"].Tiles[breakables[i].i][breakables[i].j] = null;
                    breakables.RemoveAt(i);
                }else if (breakables[i].isBroken && breakables[i].type == "door")
                {
                    map.TileLayers["Doors"].Tiles[breakables[i].i][breakables[i].j] = null;
                    breakables.RemoveAt(i);
                }
            }
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].consumed)
                {
                    objects.RemoveAt(i);
                }
            }
            foreach (NPC npc in npcs) {
                npc.Update(gameTime);
            }
            foreach (Scene2DNode node in objects)
            {
                node.hover();
            }
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
            batch.Draw(backgroundTexture, new Vector2(), BACKGROUND_COLOR * alpha);
            map.Draw(batch, mapView);
            character.Draw(batch, Color.White);
            foreach (NPC npc in npcs) {
                npc.Draw(batch, OPAQUE_COLOR * alpha);
            }
            foreach (Enemy e in enemies) {
                e.Draw(batch, OPAQUE_COLOR * alpha);
            }
            foreach (Scene2DNode node in objects){
                node.Draw(batch, ITEM_COLOR, OPAQUE_COLOR * alpha);
            }

            //Debug
            if (DEBUG) {
                foreach (Rectangle r in cRectangles) {
                    batch.Draw(collision, r, OPAQUE_COLOR);
                }
                foreach (Transition t in transitions) {
                    batch.Draw(transition, t.tRect, OPAQUE_COLOR);
                }
                foreach (Rectangle r in ladders) {
                    batch.Draw(ladder, r, OPAQUE_COLOR);
                }
                foreach (Breakable r in breakables) {
                    batch.Draw(breakable, r.bRect, OPAQUE_COLOR);
                }
                foreach (NPC n in npcs) {
                    batch.Draw(npc, n.objectRectangle, OPAQUE_COLOR);
                }
                foreach (Enemy e in enemies) {
                    batch.Draw(enemy, e.objectRectangle, OPAQUE_COLOR);
                }
                batch.Draw(charDebug, character.objectRectangle, OPAQUE_COLOR * alpha);
                batch.DrawString(font, "MAP: " + mapName, new Vector2(mapView.Width - 144, 0), Color.White, 0, new Vector2(), 1.1f, SpriteEffects.None, 0);
                string fps = (1 / (float)gameTime.ElapsedGameTime.TotalSeconds).ToString();
                batch.DrawString(font, "FPS: " + fps, new Vector2(mapView.Width - 144, 16), Color.White, 0, new Vector2(), 1.1f, SpriteEffects.None, 0);
                batch.DrawString(font, "KEYS: " + character.keyCount / 2, new Vector2(mapView.Width - 144, 32), Color.White, 0, new Vector2(), 1.1f, SpriteEffects.None, 0);
            }

            batch.End();    //Stops additive blending from player drawing batch
            batch.Begin();
            batch.Draw(keyTexture, new Vector2(), Color.White);
            batch.DrawString(font, "x" + character.keyCount / 2, new Vector2(32,8), Color.White, 0, new Vector2(), 1.1f, SpriteEffects.None, 0);

            //Pause
            if (PAUSE_SCREEN) {
                batch.Draw(pauseBackground, new Vector2(100, 60), Color.White);
                batch.Draw(pauseSelector, pauseSelectorPosition, Color.White);
            }
            batch.End();
            
        }
    }
}
