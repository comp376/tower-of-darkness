using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace tower_of_darkness_xna {
    class Character : NPC {
        private float STARTING_LIGHT_SIZE = 50.0f;
        private float lightTimer = 0;
        private float lightInterval = 5000;
        KeyboardState oldstate;
        private bool godMode = true;

        //Character
        private int moveTimer = 0;
        private int moveInterval = 5;
        private ContentManager Content;
        private const int MOVE_SPEED = 4;
        private const int GRAVITY_SPEED = 4;
        private bool jumping = false;
        private float jumpingHeight = 0;
        private const int JUMPING_HEIGHT = 8;
        private const float JUMPING_INCREMENT = 0.10f;
        private bool falling = false;
        private int startingY = 0;
        private bool climbing = false;
        private int talkTimer = 0;
        private int characterTalkTimer = 0;
        private int talkInterval = 2000;
        private int attackTimer = 0;
        private int attackInterval = 5;
        private int attackSwingInterval = 5;
        private float attackAngleMax = 100f;
        private bool attacking = false;
        private bool swingingRight = false;
        private int enemyHitTimer = 0;
        private int enemyHitInterval = 500;
        private int flashColorTimer = 0;
        private int flashColorInterval = 1000;
        private Color playerColor = Color.White;
        private float apex = 2f;
        private float apexCounter = 0;

        public const int MAP_COUNT = 20;
        KeyboardState oldState;
        public bool wizardSpokenTo = false;
        public string characterWords = "";
        public bool showText = false;
        public bool finishGame = false;
        public bool goToMainMenu = false;
        public bool firstEncounter = true;
        public int encounterCount = 0;
        public List<Scene2DNode>[] theMapObjects = new List<Scene2DNode>[MAP_COUNT];
        public Scene2DNode emptyNode;

        //Lighting
        private Color lightColor;
        private float lightAlpha;
        private Texture2D lightTexture;
        public Texture2D lanternTexture;
        private Vector2 lanternPosition;
        private Rectangle lanternRectangle;
        private Vector2 lightPosition;
        private float lanternTimer = 0;
        private float lanternInterval = 5;
        private LanternSwing lanternSwing;
        private float lanternAngle = 0f;
        private float currentLightSize;
        private float lowerBoundary = 0.9f;
        private float upperBoundary = 1.4f;
        private const float BACKWARDS_BOUNDARY = 10;
        private const float FORWARDS_BOUNDARY = -10;
        private const float ANGLE_CHANGE = 0.5f;

        public bool isBossDead = false;
        
        public bool bookPickedUp = false;

        public int keyCount = 0;
        public bool doorTouched = false;

        public Character(Texture2D spriteSheet, int xNumberOfFrames, int yNumberOfFrames, int spriteWidth, int spriteHeight, ContentManager Content, SpriteFont font)
            : base(spriteSheet, xNumberOfFrames, yNumberOfFrames, spriteWidth, spriteHeight, font) {
            this.Content = Content;
            LoadContent();
            objectRectangle = new Rectangle(objectRectangle.X + 64, objectRectangle.Y + 64, objectRectangle.Width, objectRectangle.Height - 8); //8 to offset tight jumps
            emptyNode = new Scene2DNode(lanternTexture, new Vector2(-100f, -100f), "empty");
            for (int i = 0; i < MAP_COUNT; i++)
            {
                theMapObjects[i] = new List<Scene2DNode>();
                theMapObjects[i].Add(emptyNode);
            }
        }

        public void LoadContent() {
            lightColor = new Color(220, 220, 175);  //239 228 176
            lightAlpha = 0.6f;
            lightTexture = Content.Load<Texture2D>("sprites/light3");
            lanternTexture = Content.Load<Texture2D>("sprites/lantern");
            lanternPosition = new Vector2(objectRectangle.X, objectRectangle.Y);
            lanternRectangle = new Rectangle(objectRectangle.X, objectRectangle.Y, lanternTexture.Width, lanternTexture.Height);
            lightPosition = new Vector2(objectRectangle.X, objectRectangle.Y);
            oldstate = Keyboard.GetState();
            currentLightSize = STARTING_LIGHT_SIZE;
        }

        public void Update(GameTime gameTime, Rectangle mapRect, int mapId, ref Rectangle mapView, ref List<Rectangle> cRectangles, ref List<Transition> transitions, ref List<Rectangle> ladders, ref List<Breakable> breakables, ref List<NPC> npcs, ref List<Enemy> enemies, ref List<Scene2DNode> objects, ref List<Dim> dims, ref List<Light> lights) {
            jump(ref cRectangles, ref mapView, ref mapRect, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies, ref objects, ref dims, ref lights);
            move(gameTime, mapRect, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies, ref objects, ref dims, ref lights);
            gravity(ref cRectangles, ref mapView, ref mapRect, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies, ref objects, ref dims, ref lights);
            animate(gameTime);
            hitTransition(transitions, mapView, mapId);
            lanternSwinging(gameTime);
            talk(gameTime, ref npcs, mapId);
            attackSwing(gameTime);
            attackHit(ref enemies);
            collides(ref objects);
            enemyCollision(gameTime, enemies);
            crossDim(ref dims);
            decreaseLight(gameTime);
            checkDeath(transitions, mapView, mapId);
            checkBossDeath(enemies);

            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.L))
            {
                if (!oldState.IsKeyDown(Keys.L))
                {
                    if (currentLightSize > -17)
                    currentLightSize -= 1f;
            }
            }

            if (newState.IsKeyDown(Keys.K))
            {
                if (!oldState.IsKeyDown(Keys.K)){
                    currentLightSize += 1f;
                }
            }

            if (objectRectangle.Y > 450)
            {
                Console.WriteLine("it appears you've fallen on the map.");
                
            }
            oldState = newState;
        }

        private void checkBossDeath(List<Enemy> enemies) {
            if (!isBossDead) {
                foreach (Enemy e in enemies) {
                    if (e.isBoss) {
                        if (e.hits <= 0) {
                            isBossDead = true;
                        }
                    }
                }
            }
        }

        public void checkDeath(List<Transition> transitions, Rectangle mapView, int mapId){
            if (!godMode)
            {
                if (currentLightSize <= -17f)
                {
                    Console.WriteLine("I'm dead.");
                    foreach (Transition t in transitions)
                    {
                        currentLightSize = 50f;
                        Game1.currentGameState = new LevelState(Content, mapView.Width, mapView.Height, t.nextMapName, this, t);
                        break;
                    }
                }
            }
        }
        public override void Draw(SpriteBatch spriteBatch, Color color) {
            lanternPosition = new Vector2(objectRectangle.X, objectRectangle.Y);
            lanternRectangle = new Rectangle(objectRectangle.X, objectRectangle.Y, lanternTexture.Width, lanternTexture.Height);
            lightPosition = new Vector2(objectRectangle.X, objectRectangle.Y);
            SpriteEffects walkingDirection = SpriteEffects.None;
            switch (movementStatus) {
                case MovementStatus.Left:
                    walkingDirection = SpriteEffects.FlipHorizontally;
                    break;
            } if (walkingDirection == SpriteEffects.None) {
                lanternPosition.X += 32;
                lanternRectangle.X += 32;
                lightPosition.X += ((lightTexture.Width + currentLightSize) / 2) - spriteWidth - 45;
            } else {
                lanternPosition.X -= 32;
                lanternRectangle.X -= 32;
            }
            lanternPosition.Y += 32;
            lanternRectangle.Y += 32;
            lightPosition.Y -= ((lightTexture.Height - currentLightSize) / 2) - spriteHeight - 9;
            if (lanternPickedUp) {
                if(walkingDirection == SpriteEffects.FlipHorizontally)
                    spriteBatch.Draw(lanternTexture, new Rectangle(lanternRectangle.X + 32, lanternRectangle.Y, lanternRectangle.Width, lanternRectangle.Height), null, Color.White, degreeToRadian(lanternAngle), new Vector2(lanternTexture.Width / 2, lanternTexture.Height / 2), walkingDirection, 0);
                else
                    spriteBatch.Draw(lanternTexture, lanternRectangle, null, Color.White, degreeToRadian(lanternAngle), new Vector2(lanternTexture.Width / 2, lanternTexture.Height / 2), walkingDirection, 0);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                spriteBatch.Draw(lightTexture, new Rectangle((int)(lightPosition.X), (int)(lightPosition.Y - (currentLightSize * 6)), (int)(lightTexture.Width + (currentLightSize * 15)), (int)(lightTexture.Height + (currentLightSize * 15))), new Rectangle(0, 0, lightTexture.Width, lightTexture.Height), lightColor * lightAlpha, degreeToRadian(lanternAngle), new Vector2(lightTexture.Width / 2, 0), walkingDirection, 0);
            }

            if (showText) {
                Vector2 fontOrigin = font.MeasureString(characterWords) / 2;
                Vector2 fontPosition = new Vector2(objectRectangle.X, objectRectangle.Y - 16);
                spriteBatch.DrawString(font, characterWords, fontPosition, Color.White, 0, fontOrigin, 0.75f, SpriteEffects.None, 0);
            }
                base.Draw(spriteBatch, playerColor);
        }

        private void decreaseLight(GameTime gameTime) {
            if (lanternPickedUp) {
                lightTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (lightTimer >= lightInterval) {
                    if (currentLightSize > -17)
                        currentLightSize -= 1.0f;
                    lightTimer = 0;
                }
            }
        }

        private void crossDim(ref List<Dim> dims) {
            foreach (Dim d in dims) {
                if (objectRectangle.X > d.dRect.X) {
                    d.isPassed = true;
                }
            }
        }

        private void enemyCollision(GameTime gameTime, List<Enemy> enemies) {
            enemyHitTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (enemyHitTimer >= enemyHitInterval) {
                if (playerColor != Color.Red) {
                    foreach (Enemy e in enemies) {
                        if (e.objectRectangle.Intersects(objectRectangle)) {
                            if (currentLightSize > -17)
                                currentLightSize -= 5.0f;
                            Console.WriteLine("flash player");
                            playerColor = Color.Red;
                            flashColorTimer = 0;
                    
                        }
                    }
                } else {
                    flashColorTimer += gameTime.ElapsedGameTime.Milliseconds;
                    if (flashColorTimer >= flashColorInterval) {
                        playerColor = Color.White;
                        enemyHitTimer = 0;
                        flashColorTimer = 0;
                    }
                }

            }
            

            
        }

        private void attackHit(ref List<Enemy> enemies) {
            for (int i = 0; i < enemies.Count; i++) {
                if (enemies[i].hits < 0)
                    enemies.RemoveAt(i);
            }

            if (attacking) {
                foreach (Enemy e in enemies) {
                    if (e.objectRectangle.Intersects(lanternRectangle)) {
                        e.hit = true;
                    }
                }
            }
        }

        private void blockHit(ref List<Breakable> breakable)
        {

            if (attacking)
            {
                foreach (Breakable b in breakable)
                {
                    
                }

            }
        }

        private void attackSwing(GameTime gameTime) {
            if (!attacking) {
                attackTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (attackTimer >= attackInterval) {
                    KeyboardState kbs = Keyboard.GetState();
                    if (kbs.IsKeyDown(Keys.Q)) {
                        Console.WriteLine("q: " + movementStatus);
                        if (!attacking) {
                            attacking = true;
                            if (movementStatus == MovementStatus.Left) {
                                attackAngleMax *= -1;
                                swingingRight = false;
                            }
                            if (movementStatus == MovementStatus.Right) {
                                attackAngleMax *= -1;
                                swingingRight = true;
                            }
                        }
                        attackTimer = 0;
                    }
                }
            } else {
                attackTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (attackTimer >= attackSwingInterval) {
                    if (!swingingRight) {
                        if (lanternAngle < attackAngleMax) {
                            lanternAngle += attackSwingInterval;
                            attackTimer = 0;
                        }else {
                            lanternAngle = 0;
                            attackTimer = 0;
                            attacking = false;
                        }
                    } else {
                        if (lanternAngle > attackAngleMax) {
                            lanternAngle += -attackSwingInterval;
                            attackTimer = 0;
                        } else {
                            lanternAngle = 0;
                            attackTimer = 0;
                            attacking = false;
                        }
                    }

        }
            }
        }

        private void talk(GameTime gameTime, ref List<NPC> npcs, int mapId) {
            talkTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (talkTimer >= talkInterval) {
                KeyboardState kbs = Keyboard.GetState();
                if (kbs.IsKeyDown(Keys.Up)) {
                    foreach (NPC npc in npcs) {
                        if (npc.objectRectangle.Intersects(objectRectangle)) {
                            if (npc.questAdvance == "lanternPickup")
                                wizardSpokenTo = true;
                            if (npc.id == 7)
                            {
                                finishGame = true;
                                npc.text = "You're vanquished the darkness \nfrom our world.  You've saved us, \nhero.";
                                npc.showText = true;
                                talkTimer = 0;
                                break;
                            }
                            if (npc.id == 0)
                            {//Wizard?
                                if (!lanternPickedUp)
                                {
                                    npc.text = "An evil force has taken over the land.\nTake my lantern and head to the tower.\nSave us.. ";
                                    npc.showText = true;
                                    talkTimer = 0;
                                }
                                else
                                {
                                    npc.text = "There's a magical book hidden in the tower.\nIf you find it, it may help you.";
                                    npc.showText = true;
                                    talkTimer = 0;
                                }
                            }
                            else
                            {

                                Console.WriteLine(bookPickedUp);
                                if (bookPickedUp)
                                {
                                    if (npc.id == 1)
                                    {
                                        npc.text = "You found the book!\nYou're so brave!";
                                        npc.showText = true;
                                        talkTimer = 0;
                                    }
                                    else if (npc.id == 2)
                                    {
                                        npc.text = "I locked myself in here.\nThe tower has been overun\nwith monsters!";
                                        npc.showText = true;
                                        talkTimer = 0;
                                    }
                                    else if (npc.id == 3)
                                    {
                                        npc.text = "I'm glad you're here.\nLooks like there's some magical\nessences lying around..";
                                        npc.showText = true;
                                        talkTimer = 0;
                                    }
                                    else if (npc.id == 4)
                                    {
                                        npc.text = "I saw a blue skeleton higher up\nthe tower. They seem be\nstronger than the white ones.";
                                        npc.showText = true;
                                        talkTimer = 0;
                                    }
                                    else if (npc.id == 5)
                                    {
                                        npc.text = "Thank goodness\nyou're here!\nThere's great evil\n in the next room..";
                                        npc.showText = true;
                                        talkTimer = 0;
                                    }
                                }
                                else
                                {
                                    npc.text = "????";
                                    npc.showText = true;
                                    talkTimer = 0;
                                    if (firstEncounter)
                                        firstEncounter = false;
                                }
                            }
                        }
                    }
                } else {

                    if (showText)
                    {
                        characterTalkTimer = 0;
                        showText = false;
                    }

                    foreach (NPC npc in npcs) {
                        npc.showText = false;
                        if (!firstEncounter && encounterCount == 0){
                            encounterCount++;
                            characterWords = "..I don't understand what they're saying.";
                            talkTimer = 0;
                            showText = true;   
                        }
                        if (npc.id == 7 && finishGame)
                        {
                            goToMainMenu = true;
                        }

                    }
                }

                
            }
        }

        private void lanternSwinging(GameTime gameTime) {
            if (attacking)
                return;
            KeyboardState kbs = Keyboard.GetState();
            lanternTimer += gameTime.ElapsedGameTime.Milliseconds;

            if (kbs.IsKeyUp(Keys.Right) && kbs.IsKeyUp(Keys.Left)) {
                if (lanternAngle > 0) {
                    lanternAngle -= ANGLE_CHANGE;
                }
                if (lanternAngle < 0) {
                    lanternAngle += ANGLE_CHANGE;
                }
            }
            if (lanternTimer >= lanternInterval) {
                if (kbs.IsKeyDown(Keys.Right)) {
                    if (lanternSwing == LanternSwing.Backwards) {
                        if (lanternAngle > FORWARDS_BOUNDARY) {
                            lanternAngle -= ANGLE_CHANGE;
                        } else {
                            lanternSwing = LanternSwing.Forwards;
                        }
                    }
                    if (lanternSwing == LanternSwing.Forwards) {
                        if (lanternAngle < BACKWARDS_BOUNDARY) {
                            lanternAngle += ANGLE_CHANGE;
                        } else {
                            lanternSwing = LanternSwing.Backwards;
                        }
                    }
                }
                if (kbs.IsKeyDown(Keys.Left)) {
                    if (lanternSwing == LanternSwing.Backwards) {
                        if (lanternAngle > FORWARDS_BOUNDARY) {
                            lanternAngle -= ANGLE_CHANGE;
                        } else {
                            lanternSwing = LanternSwing.Forwards;
                        }
                    }
                    if (lanternSwing == LanternSwing.Forwards) {
                        if (lanternAngle < BACKWARDS_BOUNDARY) {
                            lanternAngle += ANGLE_CHANGE;
                        } else {
                            lanternSwing = LanternSwing.Backwards;
                        }
                    }
                }
                lanternTimer = 0;
            }
        }

        private void gravity(ref List<Rectangle> cRectangles, ref Rectangle mapView, ref Rectangle mapRect, ref List<Transition> transitions, ref List<Rectangle> ladders, ref List<Breakable> breakables, ref List<NPC> npcs, ref List<Enemy> enemies, ref List<Scene2DNode> objects, ref List<Dim> dims, ref List<Light> lights) {
            if (climbing)
                return;
            int middleY = mapView.Height / 2;
            if (!collides(cRectangles, MovementStatus.Fall) && !collides(ref breakables, MovementStatus.Fall)) {
                if (mapInView(mapView, mapRect, 0, GRAVITY_SPEED, MovementStatus.Fall)) {
                    if (objectRectangle.Y < middleY - 5) {
                        objectRectangle.Y += GRAVITY_SPEED;
                    } else {
                        scroll(MovementStatus.Fall, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies, ref objects, ref dims, ref lights);
                    }
                } else {
                    objectRectangle.Y += GRAVITY_SPEED;
                }
                falling = true;
            } else
                falling = false;
        }

        private void jump(ref List<Rectangle> cRectangles, ref Rectangle mapView, ref Rectangle mapRect, ref List<Transition> transitions, ref List<Rectangle> ladders, ref List<Breakable> breakables, ref List<NPC> npcs, ref List<Enemy> enemies, ref List<Scene2DNode> objects, ref List<Dim> dims, ref List<Light> lights) {
            double middleY = mapView.Height / 2;
            //Console.WriteLine(jumping + " and " + falling);
            KeyboardState kbs = Keyboard.GetState();
            if (climbing)
                apexCounter = 0;

            if (jumping && !climbing) {
                if (collides(cRectangles, MovementStatus.Jump) || collides(ref breakables, MovementStatus.Jump)) {
                    jumping = false;
                    apexCounter = 0;
                } 
                else 
                {
                    apexCounter += JUMPING_INCREMENT;//Going up!
                    if (mapInView(mapView, mapRect, 0, (int)jumpingHeight, MovementStatus.Jump))
                    {
                        if (objectRectangle.Y <= middleY)//Are we scrolling?
                        {
                            scroll(MovementStatus.Jump, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies, ref objects, ref dims, ref lights);
                        }
                        else
                        {
                            objectRectangle.Y += (int)jumpingHeight;//Not scrolling.  Increment.
                        }

                    }
                    else
                    {
                        objectRectangle.Y += (int)jumpingHeight;//Not scrolling.  Increment.
                    }

                    if (apexCounter > apex)//Max jump height?
                        {
                            jumping = false;
                            apexCounter = 0f;
                        }

                }
            } else {
                if (kbs.IsKeyDown(Keys.Space) && !falling)
                {
                    if (!oldState.IsKeyDown(Keys.Space) && !falling)
                    {
                         jumping = true;
                         jumpingHeight = -JUMPING_HEIGHT;
                    }
                }    
            }

            oldState = kbs;
        }

        private void move(GameTime gameTime, Rectangle mapRect, ref Rectangle mapView, ref List<Rectangle> cRectangles, ref List<Transition> transitions, ref List<Rectangle> ladders, ref List<Breakable> breakables, ref List<NPC> npcs, ref List<Enemy> enemies, ref List<Scene2DNode> objects, ref List<Dim> dims, ref List<Light> lights) {
            int middleX = mapView.Width / 2;
            int middleY = mapView.Height / 2;
            KeyboardState kbs = Keyboard.GetState();
            moveTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (moveTimer >= moveInterval) {
                if (!collides(ladders, MovementStatus.Up))
                    climbing = false;

                if (kbs.IsKeyDown(Keys.Left) || kbs.IsKeyDown(Keys.Right) || kbs.IsKeyDown(Keys.Up) || kbs.IsKeyDown(Keys.Down)) {
                    isMoving = true;

                    if (kbs.IsKeyDown(Keys.Left)) {
                        movementStatus = MovementStatus.Left;
                        if (!collides(cRectangles, movementStatus) && !collides(ref breakables, movementStatus)) {
                            if (mapInView(mapView, mapRect, spriteWidth, 0, movementStatus)) {
                                if (objectRectangle.X > middleX) {
                                    objectRectangle.X -= MOVE_SPEED;
                                } else {
                                    scroll(movementStatus, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies, ref objects, ref dims, ref lights);
                                }
                            } else if (objectRectangle.X > 0) {
                                objectRectangle.X -= MOVE_SPEED;
                            }
                        }
                    }

                    if (kbs.IsKeyDown(Keys.Right)) {
                        movementStatus = MovementStatus.Right;
                        if (!collides(cRectangles, movementStatus) && !collides(ref breakables, movementStatus)) {
                            if (mapInView(mapView, mapRect, spriteWidth, 0, movementStatus)) {
                                if (objectRectangle.X < middleX)
                                    objectRectangle.X += MOVE_SPEED;
                                else {
                                    scroll(movementStatus, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies, ref objects, ref dims, ref lights);
                                }
                            } else if (objectRectangle.X + spriteWidth < mapView.Width)
                                objectRectangle.X += MOVE_SPEED;
                        }
                    }

                    if (kbs.IsKeyDown(Keys.Up)) {
                        movementStatus = MovementStatus.Up;
                        if (!collides(cRectangles, MovementStatus.Jump))
                            if (collides(ladders, movementStatus)) {
                                climbing = true;
                                if (mapInView(mapView, mapRect, 0, spriteHeight, movementStatus)) {
                                    if (objectRectangle.Y > middleY) {
                                        objectRectangle.Y -= MOVE_SPEED;
                                    } else {
                                        scroll(movementStatus, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies, ref objects, ref dims, ref lights);
                                    }
                                } else if (objectRectangle.Y > 0)
                                    objectRectangle.Y -= MOVE_SPEED;
                            }

                    }

                    if (kbs.IsKeyDown(Keys.Down)) {
                        movementStatus = MovementStatus.Down;
                        if (!collides(cRectangles, MovementStatus.Fall))
                            if (collides(ladders, movementStatus)) {
                                climbing = true;
                                if (mapInView(mapView, mapRect, 0, spriteHeight, movementStatus)) {
                                    if (objectRectangle.Y < middleY)
                                        objectRectangle.Y += MOVE_SPEED;
                                    else {
                                        scroll(movementStatus, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies, ref objects, ref dims, ref lights);
                                    }
                                } else if (objectRectangle.Y + MOVE_SPEED < mapView.Height)
                                    objectRectangle.Y += MOVE_SPEED;
                            }
                    }
                    moveTimer = 0;

                }
            }

            if (kbs.IsKeyUp(Keys.Left) && kbs.IsKeyUp(Keys.Right) && kbs.IsKeyUp(Keys.Up) && kbs.IsKeyUp(Keys.Down)) {
                isMoving = false;
            }
        }

        private void scroll(MovementStatus movementStatus, ref Rectangle mapView, ref List<Rectangle> cRectangles, ref List<Transition> transitions, ref List<Rectangle> ladders, ref List<Breakable> breakables, ref List<NPC> npcs, ref List<Enemy> enemies, ref List<Scene2DNode> objects, ref List<Dim> dims, ref List<Light> lights) {
            switch (movementStatus) {
                case MovementStatus.Left:
                    mapView.X += -MOVE_SPEED;
                    for (int i = 0; i < cRectangles.Count; i++) {
                        cRectangles[i] = new Rectangle(cRectangles[i].X + MOVE_SPEED, cRectangles[i].Y, cRectangles[i].Width, cRectangles[i].Height);
                    } for (int i = 0; i < transitions.Count; i++) {
                        transitions[i].tRect = new Rectangle(transitions[i].tRect.X + MOVE_SPEED, transitions[i].tRect.Y, transitions[i].tRect.Width, transitions[i].tRect.Height);
                    } for (int i = 0; i < ladders.Count; i++) {
                        ladders[i] = new Rectangle(ladders[i].X + MOVE_SPEED, ladders[i].Y, ladders[i].Width, ladders[i].Height);
                    } for (int i = 0; i < breakables.Count; i++) {
                        breakables[i].bRect = new Rectangle(breakables[i].bRect.X + MOVE_SPEED, breakables[i].bRect.Y, breakables[i].bRect.Width, breakables[i].bRect.Height);
                    } for (int i = 0; i < npcs.Count; i++) {
                        npcs[i].objectRectangle = new Rectangle(npcs[i].objectRectangle.X + MOVE_SPEED, npcs[i].objectRectangle.Y, npcs[i].objectRectangle.Width, npcs[i].objectRectangle.Height);
                    } for (int i = 0; i < enemies.Count; i++) {
                        enemies[i].objectRectangle = new Rectangle(enemies[i].objectRectangle.X + MOVE_SPEED, enemies[i].objectRectangle.Y, enemies[i].objectRectangle.Width, enemies[i].objectRectangle.Height);
                    } for(int i = 0; i < objects.Count; i++){
                        objects[i].worldPosition = new Vector2(objects[i].worldPosition.X + MOVE_SPEED, objects[i].worldPosition.Y);
                        objects[i].startingPosition = new Vector2(objects[i].startingPosition.X + MOVE_SPEED, objects[i].startingPosition.Y);
                    } for(int i = 0; i < dims.Count; i++){
                        dims[i].dRect = new Rectangle(dims[i].dRect.X + MOVE_SPEED, dims[i].dRect.Y, dims[i].dRect.Width, dims[i].dRect.Height);
                    } for(int i = 0; i < lights.Count; i++){
                        lights[i].lRect = new Rectangle(lights[i].lRect.X + MOVE_SPEED, lights[i].lRect.Y, lights[i].lRect.Width, lights[i].lRect.Height);
                    }
                    break;
                case MovementStatus.Right:
                    mapView.X += MOVE_SPEED;
                    for (int i = 0; i < cRectangles.Count; i++) {
                        cRectangles[i] = new Rectangle(cRectangles[i].X - MOVE_SPEED, cRectangles[i].Y, cRectangles[i].Width, cRectangles[i].Height);
                    } for (int i = 0; i < transitions.Count; i++) {
                        transitions[i].tRect = new Rectangle(transitions[i].tRect.X - MOVE_SPEED, transitions[i].tRect.Y, transitions[i].tRect.Width, transitions[i].tRect.Height);
                    } for (int i = 0; i < ladders.Count; i++) {
                        ladders[i] = new Rectangle(ladders[i].X - MOVE_SPEED, ladders[i].Y, ladders[i].Width, ladders[i].Height);
                    } for (int i = 0; i < breakables.Count; i++) {
                        breakables[i].bRect = new Rectangle(breakables[i].bRect.X - MOVE_SPEED, breakables[i].bRect.Y, breakables[i].bRect.Width, breakables[i].bRect.Height);
                    } for (int i = 0; i < npcs.Count; i++) {
                        npcs[i].objectRectangle = new Rectangle(npcs[i].objectRectangle.X - MOVE_SPEED, npcs[i].objectRectangle.Y, npcs[i].objectRectangle.Width, npcs[i].objectRectangle.Height);
                    } for (int i = 0; i < enemies.Count; i++) {
                        enemies[i].objectRectangle = new Rectangle(enemies[i].objectRectangle.X - MOVE_SPEED, enemies[i].objectRectangle.Y, enemies[i].objectRectangle.Width, enemies[i].objectRectangle.Height);
                    } for(int i = 0; i < objects.Count; i++){
                        objects[i].worldPosition = new Vector2(objects[i].worldPosition.X - MOVE_SPEED, objects[i].worldPosition.Y);
                        objects[i].startingPosition = new Vector2(objects[i].startingPosition.X - MOVE_SPEED, objects[i].startingPosition.Y);
                    } for(int i = 0; i < dims.Count; i++){
                        dims[i].dRect = new Rectangle(dims[i].dRect.X - MOVE_SPEED, dims[i].dRect.Y, dims[i].dRect.Width, dims[i].dRect.Height);
                    } for(int i = 0; i < lights.Count; i++){
                        lights[i].lRect = new Rectangle(lights[i].lRect.X - MOVE_SPEED, lights[i].lRect.Y, lights[i].lRect.Width, lights[i].lRect.Height);
                    }
                    break;
                case MovementStatus.Up:
                    mapView.Y += -MOVE_SPEED;
                    for (int i = 0; i < cRectangles.Count; i++) {
                        cRectangles[i] = new Rectangle(cRectangles[i].X, cRectangles[i].Y + MOVE_SPEED, cRectangles[i].Width, cRectangles[i].Height);
                    } for (int i = 0; i < transitions.Count; i++) {
                        transitions[i].tRect = new Rectangle(transitions[i].tRect.X, transitions[i].tRect.Y + MOVE_SPEED, transitions[i].tRect.Width, transitions[i].tRect.Height);
                    } for (int i = 0; i < ladders.Count; i++) {
                        ladders[i] = new Rectangle(ladders[i].X, ladders[i].Y + MOVE_SPEED, ladders[i].Width, ladders[i].Height);
                    } for (int i = 0; i < breakables.Count; i++) {
                        breakables[i].bRect = new Rectangle(breakables[i].bRect.X, breakables[i].bRect.Y + MOVE_SPEED, breakables[i].bRect.Width, breakables[i].bRect.Height);
                    } for (int i = 0; i < npcs.Count; i++) {
                        npcs[i].objectRectangle = new Rectangle(npcs[i].objectRectangle.X, npcs[i].objectRectangle.Y + MOVE_SPEED, npcs[i].objectRectangle.Width, npcs[i].objectRectangle.Height);
                    } for (int i = 0; i < enemies.Count; i++) {
                        enemies[i].objectRectangle = new Rectangle(enemies[i].objectRectangle.X, enemies[i].objectRectangle.Y + MOVE_SPEED, enemies[i].objectRectangle.Width, enemies[i].objectRectangle.Height);
                    } for(int i = 0; i < objects.Count; i++){
                        objects[i].worldPosition = new Vector2(objects[i].worldPosition.X, objects[i].worldPosition.Y + MOVE_SPEED);
                        objects[i].startingPosition = new Vector2(objects[i].startingPosition.X, objects[i].startingPosition.Y + MOVE_SPEED);
                    } for(int i = 0; i < dims.Count; i++){
                        dims[i].dRect = new Rectangle(dims[i].dRect.X, dims[i].dRect.Y + MOVE_SPEED, dims[i].dRect.Width, dims[i].dRect.Height);
                    } for(int i = 0; i < lights.Count; i++){
                        lights[i].lRect = new Rectangle(lights[i].lRect.X, lights[i].lRect.Y + MOVE_SPEED, lights[i].lRect.Width, lights[i].lRect.Height);
                    }
                    break;
                case MovementStatus.Down:
                    mapView.Y += MOVE_SPEED;
                    for (int i = 0; i < cRectangles.Count; i++) {
                        cRectangles[i] = new Rectangle(cRectangles[i].X, cRectangles[i].Y - MOVE_SPEED, cRectangles[i].Width, cRectangles[i].Height);
                    } for (int i = 0; i < transitions.Count; i++) {
                        transitions[i].tRect = new Rectangle(transitions[i].tRect.X, transitions[i].tRect.Y - MOVE_SPEED, transitions[i].tRect.Width, transitions[i].tRect.Height);
                    } for (int i = 0; i < ladders.Count; i++) {
                        ladders[i] = new Rectangle(ladders[i].X, ladders[i].Y - MOVE_SPEED, ladders[i].Width, ladders[i].Height);
                    } for (int i = 0; i < breakables.Count; i++) {
                        breakables[i].bRect = new Rectangle(breakables[i].bRect.X, breakables[i].bRect.Y - MOVE_SPEED, breakables[i].bRect.Width, breakables[i].bRect.Height);
                    } for (int i = 0; i < npcs.Count; i++) {
                        npcs[i].objectRectangle = new Rectangle(npcs[i].objectRectangle.X, npcs[i].objectRectangle.Y - MOVE_SPEED, npcs[i].objectRectangle.Width, npcs[i].objectRectangle.Height);
                    } for (int i = 0; i < enemies.Count; i++) {
                        enemies[i].objectRectangle = new Rectangle(enemies[i].objectRectangle.X, enemies[i].objectRectangle.Y - MOVE_SPEED, enemies[i].objectRectangle.Width, enemies[i].objectRectangle.Height);
                    } for(int i = 0; i < objects.Count; i++){
                        objects[i].worldPosition = new Vector2(objects[i].worldPosition.X, objects[i].worldPosition.Y - MOVE_SPEED);
                        objects[i].startingPosition = new Vector2(objects[i].startingPosition.X, objects[i].startingPosition.Y - MOVE_SPEED);
                    } for(int i = 0; i < dims.Count; i++){
                        dims[i].dRect = new Rectangle(dims[i].dRect.X, dims[i].dRect.Y - MOVE_SPEED, dims[i].dRect.Width, dims[i].dRect.Height);
                    } for(int i = 0; i < lights.Count; i++){
                        lights[i].lRect = new Rectangle(lights[i].lRect.X, lights[i].lRect.Y - MOVE_SPEED, lights[i].lRect.Width, lights[i].lRect.Height);
                    }
                    break;
                case MovementStatus.Jump:
                    mapView.Y += (int)jumpingHeight;
                    for (int i = 0; i < cRectangles.Count; i++) {
                        cRectangles[i] = new Rectangle(cRectangles[i].X, cRectangles[i].Y - (int)jumpingHeight, cRectangles[i].Width, cRectangles[i].Height);
                    } for (int i = 0; i < transitions.Count; i++) {
                        transitions[i].tRect = new Rectangle(transitions[i].tRect.X, transitions[i].tRect.Y - (int)jumpingHeight, transitions[i].tRect.Width, transitions[i].tRect.Height);
                    } for (int i = 0; i < ladders.Count; i++) {
                        ladders[i] = new Rectangle(ladders[i].X, ladders[i].Y - (int)jumpingHeight, ladders[i].Width, ladders[i].Height);
                    } for (int i = 0; i < breakables.Count; i++) {
                        breakables[i].bRect = new Rectangle(breakables[i].bRect.X, breakables[i].bRect.Y - (int)jumpingHeight, breakables[i].bRect.Width, breakables[i].bRect.Height);
                    } for (int i = 0; i < npcs.Count; i++) {
                        npcs[i].objectRectangle = new Rectangle(npcs[i].objectRectangle.X, npcs[i].objectRectangle.Y - (int)jumpingHeight, npcs[i].objectRectangle.Width, npcs[i].objectRectangle.Height);
                    } for (int i = 0; i < enemies.Count; i++) {
                        enemies[i].objectRectangle = new Rectangle(enemies[i].objectRectangle.X, enemies[i].objectRectangle.Y - (int)jumpingHeight, enemies[i].objectRectangle.Width, enemies[i].objectRectangle.Height);
                    } for(int i = 0; i < objects.Count; i++){
                        objects[i].worldPosition = new Vector2(objects[i].worldPosition.X, objects[i].worldPosition.Y - (int)jumpingHeight);
                        objects[i].startingPosition = new Vector2(objects[i].startingPosition.X, objects[i].startingPosition.Y - (int)jumpingHeight);
                    } for(int i = 0; i < dims.Count; i++){
                        dims[i].dRect = new Rectangle(dims[i].dRect.X, dims[i].dRect.Y - (int)jumpingHeight, dims[i].dRect.Width, dims[i].dRect.Height);
                    } for(int i = 0; i < lights.Count; i++){
                        lights[i].lRect = new Rectangle(lights[i].lRect.X, lights[i].lRect.Y - (int)jumpingHeight, lights[i].lRect.Width, lights[i].lRect.Height);
                    }
                    break;
                case MovementStatus.Fall:
                    mapView.Y += GRAVITY_SPEED;
                    for (int i = 0; i < cRectangles.Count; i++) {
                        cRectangles[i] = new Rectangle(cRectangles[i].X, cRectangles[i].Y - GRAVITY_SPEED, cRectangles[i].Width, cRectangles[i].Height);
                    } for (int i = 0; i < transitions.Count; i++) {
                        transitions[i].tRect = new Rectangle(transitions[i].tRect.X, transitions[i].tRect.Y - GRAVITY_SPEED, transitions[i].tRect.Width, transitions[i].tRect.Height);
                    } for (int i = 0; i < ladders.Count; i++) {
                        ladders[i] = new Rectangle(ladders[i].X, ladders[i].Y - GRAVITY_SPEED, ladders[i].Width, ladders[i].Height);
                    } for (int i = 0; i < breakables.Count; i++) {
                        breakables[i].bRect = new Rectangle(breakables[i].bRect.X, breakables[i].bRect.Y - GRAVITY_SPEED, breakables[i].bRect.Width, breakables[i].bRect.Height);
                    } for (int i = 0; i < npcs.Count; i++) {
                        npcs[i].objectRectangle = new Rectangle(npcs[i].objectRectangle.X, npcs[i].objectRectangle.Y - GRAVITY_SPEED, npcs[i].objectRectangle.Width, npcs[i].objectRectangle.Height);
                    } for (int i = 0; i < enemies.Count; i++) {
                        enemies[i].objectRectangle = new Rectangle(enemies[i].objectRectangle.X, enemies[i].objectRectangle.Y - GRAVITY_SPEED, enemies[i].objectRectangle.Width, enemies[i].objectRectangle.Height);
                    } for(int i = 0; i < objects.Count; i++){
                        objects[i].worldPosition = new Vector2(objects[i].worldPosition.X, objects[i].worldPosition.Y - GRAVITY_SPEED);
                        objects[i].startingPosition = new Vector2(objects[i].startingPosition.X, objects[i].startingPosition.Y - GRAVITY_SPEED);
                    } for(int i = 0; i < dims.Count; i++){
                        dims[i].dRect = new Rectangle(dims[i].dRect.X, dims[i].dRect.Y - GRAVITY_SPEED, dims[i].dRect.Width, dims[i].dRect.Height);
                    } for(int i = 0; i < lights.Count; i++){
                        lights[i].lRect = new Rectangle(lights[i].lRect.X, lights[i].lRect.Y - GRAVITY_SPEED, lights[i].lRect.Width, lights[i].lRect.Height);
                    }
                    break;
                case MovementStatus.None:
                    break;
            }
        }

        private bool collides(List<Rectangle> cRectangles, MovementStatus movementStatus) {
            bool collision = false;
            Rectangle tempRect = objectRectangle;
            if (movementStatus == MovementStatus.Left)
                tempRect.X -= MOVE_SPEED;
            if (movementStatus == MovementStatus.Right)
                tempRect.X += MOVE_SPEED;
            if (movementStatus == MovementStatus.Fall)
                tempRect.Y += GRAVITY_SPEED;
            if (movementStatus == MovementStatus.Jump)
                tempRect.Y += (int)jumpingHeight;
            if (movementStatus == MovementStatus.Up)
                tempRect.Y += MOVE_SPEED;
            if (movementStatus == MovementStatus.Down)
                tempRect.Y += -MOVE_SPEED;
            foreach (Rectangle r in cRectangles) {
                if (tempRect.Intersects(r)) {
                    collision = true;
                }
            }
            return collision;
        }

        private bool collides(ref List<Breakable> cRectangles, MovementStatus movementStatus) {
            bool collision = false;
            bool isTouched = false;
            Rectangle tempRect = objectRectangle;
            if (movementStatus == MovementStatus.Left)
                tempRect.X -= MOVE_SPEED;
            if (movementStatus == MovementStatus.Right)
                tempRect.X += MOVE_SPEED;
            if (movementStatus == MovementStatus.Fall) {
                tempRect.Y += GRAVITY_SPEED;
                isTouched = true;
            }
            if (movementStatus == MovementStatus.Jump)
                tempRect.Y += (int)jumpingHeight;
            if (movementStatus == MovementStatus.Up)
                tempRect.Y += MOVE_SPEED;
            if (movementStatus == MovementStatus.Down)
                tempRect.Y += -MOVE_SPEED;
            foreach (Breakable r in cRectangles) {
                if (tempRect.Intersects(r.bRect)) {
                    switch (r.type){
                        case "breakable":
                            collision = true;
                            r.isTouched = isTouched;
                            break;
                        case "door":
                            Console.WriteLine(keyCount);
                            if (keyCount > 0)
                            {
                                Console.WriteLine("touched");
                                r.isTouched = true;
                                keyCount--;
                                doorTouched = true;
                            }
                            else if(keyCount == 0 && !doorTouched){
                               doorTouched = false;
                                characterWords = "I'll need a key to open this door. ";
                                talkTimer = 0;
                                showText = true;
                            }
                            collision = true;
                            break;
                        default:
                            break;
                    }//End of switch
                }
            }
            return collision;
        }

        private void collides(ref List<Scene2DNode> objects)
        {
            for (int i = 0; i < objects.Count; i++) {
                Rectangle objectRect = new Rectangle((int)objects[i].worldPosition.X, (int)objects[i].worldPosition.Y, objects[i].TextureWidth, objects[i].TextureHeight);
                if (objectRect.Intersects(objectRectangle)) {
                    if (objects[i].type == "key")
                    {
                        keyCount+=3;
                        objects[i].consumed = true;
                    }else if (objects[i].type == "essence")
                    {
                        //Increase lantern power
                        currentLightSize += 5f;
                        objects[i].consumed = true;
                    }else if (objects[i].type == "super essence")
                    {
                        //Give global lighting for a small duration

                        currentLightSize += 20f;
                        objects[i].consumed = true;
                    }
                    else if (objects[i].type == "lantern")
                    {
                        if (wizardSpokenTo)
                        {
                            objects[i].consumed = true;
                            lanternPickedUp = true;
                        }
                        else
                        {
                            characterWords = "I should ask before taking this..";
                            talkTimer = 0;
                            showText = true;
                        }
                    }
                    else if (objects[i].type == "book")
                    {
                        objects[i].consumed = true;
                        bookPickedUp = true;
                        characterWords = "..I feel different.";
                        talkTimer = 0;
                        showText = true;
                    }
                }
            }
        }

        private void hitTransition(List<Transition> transitions, Rectangle mapView, int mapId) {
            foreach (Transition t in transitions) {
                if (mapId == 1)//Town
                {
                    if (lanternPickedUp)
                    {
                        if (objectRectangle.Intersects(t.tRect))
                        {
                            Game1.currentGameState = new LevelState(Content, mapView.Width, mapView.Height, t.nextMapName, this, t);
                            break;
                        }
                    }
                    else//No lantern yet.
                    {
                        if (objectRectangle.Intersects(t.tRect))
                        {
                            if (t.nextMapName == "bridge")
                            {
                                characterWords = "I can't see well.\nI should see if someone\nin that house over\nthere can help me out.";
                                talkTimer = 0;
                                showText = true;
                            }
                            else
                            {
                                Game1.currentGameState = new LevelState(Content, mapView.Width, mapView.Height, t.nextMapName, this, t);
                                break;
                            }
                        }
                    }//End of lanternPickup?
                }else{//Not Town
                    if (objectRectangle.Intersects(t.tRect))
                    {
                        Game1.currentGameState = new LevelState(Content, mapView.Width, mapView.Height, t.nextMapName, this, t);
                        break;
                    }
                }
                
            }//End of transitions
        }

        private bool mapInView(Rectangle mapView, Rectangle mapRect, int xChange, int yChange, MovementStatus movementStatus) {
            bool inView = false;
            mapRect.X += xChange;
            mapRect.Y += yChange;

            switch (movementStatus) {
                case MovementStatus.Left:
                    if (mapView.X > 0)
                        inView = true;
                    break;
                case MovementStatus.Right:
                    if (mapView.X < (mapRect.Width - mapView.Width))
                        inView = true;
                    break;
                case MovementStatus.Jump:
                    if (mapView.Y > 0)
                        inView = true;
                    break;
                case MovementStatus.Fall:
                    if (mapView.Y < (mapRect.Height - mapView.Height))
                        inView = true;
                    break;
                case MovementStatus.Up:
                    if (mapView.Y > 0)
                        inView = true;
                    break;
                case MovementStatus.Down:
                    if (mapView.Y < (mapRect.Height - mapView.Height))
                        inView = true;
                    break;
                case MovementStatus.None:
                    inView = false;
                    break;
            }

            return inView;
        }

        private float degreeToRadian(float angle) {
            return (float)Math.PI * angle / 180.0f;
        }

    }

    enum LanternSwing {
        Forwards, Backwards
    }
}
