using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using FuncWorks.XNA.XTiled;

namespace tower_of_darkness_xna {
    class LevelState : GameState {

        private bool DEBUG = false;
        private bool PAUSE_SCREEN = false;

        SoundEffect pickUp;
        KeyboardState oldState;

        private const int BACKGROUND_LAYER = 0;
        private const int LADDER_LAYER = 1;
        private const int FOREGROUND_LAYER = 2;
        private const int TOP_LAYER = 3;
        private Color STARTING_COLOR = new Color(200, 200, 200);
        private float LIGHT_CHANGE = 5.0f;
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
        private List<Dim> dims;
        private List<Light> lights;
        private int dimsCount = 0;
        private bool[] visited = new bool[21];
        private bool isSet;
        private Random rand;
        private Tuple<int, int> enemyMoveTuple = new Tuple<int, int>(50, 100);
        private Tuple<int, int> enemyDirectionTuple = new Tuple<int, int>(4000, 6000);

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
        private Texture2D bookTexture;
        private Texture2D lightTexture;

        //pause content
        private const int NUM_PAUSE_ITEMS = 2;
        private Texture2D pauseBackground;
        private Texture2D pauseSelector;
        private Texture2D lanternKeyTexture;
        private Texture2D bookKeyTexture;
        private Vector2 pauseSelectorPosition;
        private int pauseSelectorIndex = 0;
        private float pauseSelectTimer = 0;
        private float pauseSelectInterval = 100;
        private float pausePlayTimer = 1000;
        private float pausePlayInterval = 1000;

        public LevelState(ContentManager Content, int PreferredBackBufferWidth, int PreferredBackBufferHeight, string mapName, Character character)
            : base(Content) 
        {
            rand = new Random();
            this.mapView = new Rectangle(0, 0, PreferredBackBufferWidth, PreferredBackBufferHeight);
            this.mapName = mapName;
            this.character = character;
            for (int i = 0; i < 21; i++)
            {
                visited[i] = false;
            }
            LoadContent(); 
        }

        public LevelState(ContentManager Content, int PreferredBackBufferWidth, int PreferredBackBufferHeight, string mapName, Character character, Transition transition)
        :base(Content){
            rand = new Random();
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

            //Move lights
            for (int i = 0; i < lights.Count; i++) {
                lights[i].lRect = new Rectangle(lights[i].lRect.X - xChange, lights[i].lRect.Y - yChange, lights[i].lRect.Width, lights[i].lRect.Height);
            }

            Console.WriteLine("been to this map before? : " + visited[(int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32]);
            if (visited[(int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32])
            {
                visited[(int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32] = true;
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

            if (!visited[1])
            {
                //Move dim rectangles
                for (int i = 0; i < dims.Count; i++)
                {
                    dims[i].dRect = new Rectangle(dims[i].dRect.X - xChange, dims[i].dRect.Y - yChange, dims[i].dRect.Width, dims[i].dRect.Height);
                }
            }
            
                
            //Set player direction
            character.movementStatus = (MovementStatus)transition.direction;
        }

        public override void LoadContent() {
            backgroundTexture = Content.Load<Texture2D>("sprites/background");
            keyTexture = Content.Load<Texture2D>("sprites/key");
            essenceTexture = Content.Load<Texture2D>("sprites/essence");
            superEssenceTexture = Content.Load<Texture2D>("sprites/superEssence");
            bookTexture = Content.Load<Texture2D>("sprites/book");
            font = Content.Load<SpriteFont>("fonts/spriteFont");
            lightTexture = Content.Load<Texture2D>("sprites/light3");
            map = Content.Load<Map>("maps/" + mapName);
            if (!visited[(int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32 + 1])
            {
                if (mapName != "forest")
                    modifyLayerOpacity(OPAQUE_COLOR, alpha);
            }

            mapRect = new Rectangle(0, 0, map.Width * map.TileWidth, map.Height * map.TileHeight);
            loadCollisionRectangles();
            loadTransitionRectangles();
            loadLadderRectangles();
            loadBreakables();
            loadObjects();
            loadNPCs();
            loadEnemies();
            loadDimRectangles();
            loadLights();
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
            pauseSelector = Content.Load<Texture2D>("sprites/pause_selector");
            pauseSelectorPosition = new Vector2(128, 150);
            lanternKeyTexture = Content.Load<Texture2D>("sprites/lantern_key_item");
            bookKeyTexture = Content.Load<Texture2D>("sprites/book_key_item");

            pickUp = Content.Load<SoundEffect>("audio/pop2");
            oldState = Keyboard.GetState();
        }

        private void modifyLayerOpacity(Color c, float a) {
            if (!visited[(int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32 + 1])
            {
                foreach (TileLayer tl in map.TileLayers)
                {
                    tl.OpacityColor = c * a;
                }
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

                    //Move lights
                    for (int i = 0; i < lights.Count; i++) {
                        lights[i].lRect = new Rectangle(lights[i].lRect.X - xChange, lights[i].lRect.Y - yChange, lights[i].lRect.Width, lights[i].lRect.Height);
                    }

                    if (!visited[(int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32])
                    {
                        Console.WriteLine("Moving map "+(int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32 +" objects");
                        //Move Objects
                        for (int i = 0; i < objects.Count; i++)
                        {
                            Console.WriteLine("Moving some objects");
                            objects[i] = new Scene2DNode(objects[i].texture, new Vector2(objects[i].worldPosition.X - xChange, objects[i].worldPosition.Y - yChange), objects[i].type);
                        }
                    }

                    if (!visited[1])
                    {
                        //Move dim rectangles
                        for (int i = 0; i < dims.Count; i++)
                        {
                            dims[i].dRect = new Rectangle(dims[i].dRect.X - xChange, dims[i].dRect.Y - yChange, dims[i].dRect.Width, dims[i].dRect.Height);
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
                string curMap = mo.Properties["map"].Value.ToString();
                Transition t = new Transition(mo.Name, curMap, tRect, direction, xChange, yChange, xPlayer, yPlayer);
                transitions.Add(t);
            }
        }

        private void loadDimRectangles() {
            dims = new List<Dim>();
            if (map.ObjectLayers["Dim"] == null)
                return;
            int tileSize = map.TileWidth / 2;   //Not sure why dividing by 2
            int id = 1;
            foreach (MapObject mo in map.ObjectLayers["Dim"].MapObjects) {
                Rectangle dRect = mo.Bounds;
                Dim dim = new Dim(id, dRect);
                dims.Add(dim);
                id++;
            }
            dimsCount = dims.Count;
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
                        else if (mo.Properties["Type"].Value == "lantern")
                        {
                            Scene2DNode node = new Scene2DNode(character.lanternTexture, new Vector2(mo.Bounds.X, mo.Bounds.Y), "lantern");
                            objects.Add(node);
                        }
                        else if (mo.Properties["Type"].Value == "book")
                        {
                            Scene2DNode node = new Scene2DNode(bookTexture, new Vector2(mo.Bounds.X, mo.Bounds.Y), "book");
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
                string quest = mo.Properties["questAdvance"].Value;
                Texture2D npcSpriteSheet = Content.Load<Texture2D>("sprites/" + spritesheetName);
                int xNumberOfFrames = (int)mo.Properties["xFrames"].AsInt32;
                int yNumberOfFrames = (int)mo.Properties["yFrames"].AsInt32;
                int id = (int)mo.Properties["id"].AsInt32;
                Console.WriteLine("Making npc with id:" + id);
                Rectangle npcRect = mo.Bounds;
                string text = mo.Properties["text"].Value.ToString();
                NPC n = new NPC(npcSpriteSheet, xNumberOfFrames, yNumberOfFrames, npcRect.Width, npcRect.Height, text, spritesheetName, quest, font, id);
                Console.WriteLine(n.ToString());
                n.objectRectangle = npcRect; //also provides npc(x,y) 
                npcs.Add(n);
            }
        }

        private void loadLights() {
            lights = new List<Light>();
            if (map.ObjectLayers["Light"] == null)
                return;
            foreach (MapObject mo in map.ObjectLayers["Light"].MapObjects) {
                Rectangle lightRect = mo.Bounds;
                Light light = new Light(lightTexture, lightRect);
                lights.Add(light);
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
                //TODO
                int enemyMoveInterval = rand.Next(enemyMoveTuple.Item1, enemyMoveTuple.Item2);
                int enemyDirectionInterval = rand.Next(enemyDirectionTuple.Item1, enemyDirectionTuple.Item2);
                int startingDirection = rand.Next(0, 2);
                Enemy e = new Enemy(enemySpriteSheet, xNumberOfFrames, yNumberOfFrames, enemyRect.Width, enemyRect.Height, hits, spritesheetName, font, enemyMoveInterval, enemyDirectionInterval, startingDirection);
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
            KeyboardState newState = Keyboard.GetState();
            pausePlayTimer += gameTime.ElapsedGameTime.Milliseconds;
            if(character.goToMainMenu)
                Game1.currentGameState = new MenuState(Content, Game1.WIDTH, Game1.HEIGHT, Game1.STARTING_MAP_NAME, character, true);

            if (pausePlayTimer >= pausePlayInterval) {
                if (newState.IsKeyDown(Keys.Escape)) {
                    if (!oldState.IsKeyDown(Keys.Escape))
                    {
                        PAUSE_SCREEN = true;
                    }
                }
            }
            character.Update(gameTime, mapRect, (int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies, ref objects, ref dims, ref lights);
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
            for (int i = 0; i < dims.Count; i++) {
                if (dims[i].isPassed) {
                    Color newForestColor = new Color(STARTING_COLOR.R - (int)(LIGHT_CHANGE * dims[i].id), STARTING_COLOR.G - (int)(LIGHT_CHANGE * dims[i].id), STARTING_COLOR.B - (int)(LIGHT_CHANGE * dims[i].id));
                    if (dims[i].id == dimsCount)
                        modifyLayerOpacity(OPAQUE_COLOR, alpha);
                    else
                        modifyLayerOpacity(newForestColor, alpha);
                    dims.RemoveAt(i);
                }
            }
            foreach (Enemy e in enemies) {
                e.Update(gameTime, cRectangles, breakables, transitions);
            }
            if (character.isBossDead) {
                for (int i = 0; i < map.TileLayers["Breakable"].Tiles.Length; i++) {
                    for (int j = 0; j < map.TileLayers["Breakable"].Tiles[i].Length; j++) {
                        if (map.TileLayers["Breakable"].Tiles[i][j] != null) {
                            map.TileLayers["Breakable"].Tiles[i][j] = null;
                        }
                    }
                }
                for (int i = 0; i < breakables.Count; i++) {
                    breakables.RemoveAt(i);
                }
            }
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].consumed)
                {
                    pickUp.Play();
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

            if (map.ObjectLayers["Visited"] == null)
                return; 

            /*if ((int)map.ObjectLayers["Visited"].Properties["mapId"].AsInt32 == 0)
            {
                
            }*/

            oldState = newState;
        }

        private void UpdatePause(GameTime gameTime) {
            pauseSelectTimer += gameTime.ElapsedGameTime.Milliseconds;
            KeyboardState kbs = Keyboard.GetState();
            if (pauseSelectTimer >= pauseSelectInterval) {
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
                        case 1:         //Go to menu
                            Game1.currentGameState = new MenuState(Content, Game1.WIDTH, Game1.HEIGHT, Game1.STARTING_MAP_NAME, character, false);
                            pauseSelectorIndex = 0;
                            pauseSelectTimer = 0;
                            break;
                    }
                }
                if (kbs.IsKeyDown(Keys.Escape)) {
                    if (!oldState.IsKeyDown(Keys.Escape))
                    {
                        PAUSE_SCREEN = false;
                        pauseSelectorIndex = 0;
                        pauseSelectTimer = 0;
                        pausePlayTimer = 0;
                    }
                }
                pauseSelectorPosition = new Vector2(128, 150 + pauseSelectorIndex * 30);
            }
            Console.WriteLine(PAUSE_SCREEN);
            oldState = kbs;
        }

        public override void Draw(GameTime gameTime, SpriteBatch batch) {
            batch.Begin();
            batch.Draw(backgroundTexture, new Vector2(), BACKGROUND_COLOR * alpha);
            map.Draw(batch, mapView);
            character.Draw(batch, Color.White);
            foreach (NPC npc in npcs) {
                npc.Draw(batch, Color.White * alpha);
            }
            foreach (Enemy e in enemies) {
                e.Draw(batch, Color.White * alpha);
            }
            foreach (Scene2DNode node in objects){
                node.Draw(batch, Color.White * alpha);
            }
            foreach (Light l in lights) {
                l.Draw(batch);
            }

            //Debug
            if (DEBUG) {
                foreach (Rectangle r in cRectangles) {
                    batch.Draw(collision, r, Color.White);
                }
                foreach (Transition t in transitions) {
                    batch.Draw(transition, t.tRect, Color.White);
                }
                foreach (Rectangle r in ladders) {
                    batch.Draw(ladder, r, Color.White);
                }
                foreach (Breakable r in breakables) {
                    batch.Draw(breakable, r.bRect, Color.White);
                }
                foreach (NPC n in npcs) {
                    batch.Draw(npc, n.objectRectangle, Color.White);
                }
                foreach (Enemy e in enemies) {
                    batch.Draw(enemy, e.objectRectangle, Color.White);
                }
                batch.Draw(charDebug, character.objectRectangle, Color.White);
                batch.DrawString(font, "MAP: " + mapName, new Vector2(mapView.Width - 144, 0), Color.White, 0, new Vector2(), 1.1f, SpriteEffects.None, 0);
                string fps = (1 / (float)gameTime.ElapsedGameTime.TotalSeconds).ToString();
                batch.DrawString(font, "FPS: " + fps, new Vector2(mapView.Width - 144, 16), Color.White, 0, new Vector2(), 1.1f, SpriteEffects.None, 0);
                batch.DrawString(font, "KEYS: " + character.keyCount / 3, new Vector2(mapView.Width - 144, 32), Color.White, 0, new Vector2(), 1.1f, SpriteEffects.None, 0);
            }

            batch.End();    //Stops additive blending from player drawing batch
            batch.Begin();
            batch.Draw(keyTexture, new Vector2(), Color.White);
            batch.DrawString(font, "x" + character.keyCount / 3, new Vector2(32,8), Color.White, 0, new Vector2(), 1.1f, SpriteEffects.None, 0);

            //Pause
            if (PAUSE_SCREEN) {
                batch.Draw(pauseBackground, new Vector2(100, 60), Color.White);
                if (character.lanternPickedUp)
                    batch.Draw(lanternKeyTexture, new Vector2(327, 262), Color.White);
                if(character.bookPickedUp)
                    batch.Draw(bookKeyTexture, new Vector2(441,262), Color.White);
                batch.DrawString(font, (character.keyCount / 3).ToString(), new Vector2(560, 155), Color.White, 0, new Vector2(), 1.25f, SpriteEffects.None, 0);
                batch.DrawString(font, character.enemiesKilled.ToString(), new Vector2(560, 185), Color.White, 0, new Vector2(), 1.25f, SpriteEffects.None, 0);
                batch.Draw(pauseSelector, pauseSelectorPosition, Color.White);
            }
            batch.End();
            
        }
    }
}
