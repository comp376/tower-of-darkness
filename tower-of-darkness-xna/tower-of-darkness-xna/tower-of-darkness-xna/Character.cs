using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FuncWorks.XNA.XTiled;

namespace tower_of_darkness_xna {
    class Character : Object {

        //readonly Vector2 gravity = new Vector2(0, -9.8f);
        private const int MOVE_SPEED = 3;
        private const int GRAVITY_SPEED = 2;
        private const float LIGHT_CHANGE = 0.05f;
        private const float BOUNDARY_CHANGE = 0.05f;
        private const float ANGLE_CHANGE = 0.5f;

        private bool isMoving;
        private SpriteEffects walkingDirection = SpriteEffects.None;
        private int xCurrentFrame = 0;
        private int yCurrentFrame = 0;
        private float frameTimer = 0;
        private float frameInterval = 200;
        private Texture2D lightTexture;
        private float ambient;
        private Color ambientColor;
        private Vector2 lightPosition;
        private float lightTimer = 0;
        private float lightInterval = 50;
        private float currentLightSize;
        private LightDirection lightDir;
        private float LOWER_BOUNDARY = 1.0f;
        private float UPPER_BOUNDARY = 1.0f;
        public int keyCount;

        private Texture2D lanternTexture;
        private Vector2 lanternPosition;
        private float lanternTimer = 0;
        private float lanternInterval = 5;
        private LanternSwing lanternSwing;
        private float lanternAngle = 0f;
        private float BACKWARDS_BOUNDARY = 10;
        private float FORWARDS_BOUNDARY = -10;

        private bool jumping;
        private float startY, jumpspeed = 0;

        public Character(Texture2D spriteSheet, int xNumberOfFrames, int yNumberOfFrames, int spriteWidth, int spriteHeight, Vector2 objectPosition, Texture2D lightTexture, float ambient, Color ambientColor, Texture2D lanternTexture, GraphicsDeviceManager graphics)
            : base(spriteSheet, xNumberOfFrames, yNumberOfFrames, spriteWidth, spriteHeight, objectPosition) {
            this.lightTexture = lightTexture;
            this.ambient = ambient;
            this.ambientColor = ambientColor;
            lightPosition = new Vector2(objectPosition.X, objectPosition.Y - 8);
            lightDir = LightDirection.Increasing;
            currentLightSize = LOWER_BOUNDARY;
            this.lanternTexture = lanternTexture;
            lanternPosition = objectPosition;
            lanternSwing = LanternSwing.Forwards;
            keyCount = 0;
            this.jumping = false;
            this.jumpspeed = 0;
            this.startY = graphics.PreferredBackBufferHeight - 128;
        }
        
        public bool Collides(TileData tile)
        {
            // check if we collide with a tile.
            if (this.objectPosition.X + (this.spriteWidth) > tile.Target.X &&
                    this.objectPosition.X < tile.Target.X + (32) && //These two 32's should probably be changed for the tile width, accessible from map.tilewidth/map.tileheight
                    this.objectPosition.Y + (this.spriteHeight) > tile.Target.Y &&
                    this.objectPosition.Y < tile.Target.Y + (32))
                return true;
            else
                return false;
        }

        public bool Collides(Scene2DNode node)
        {
            // check if two sprites intersect
            if (this.objectPosition.X + (this.spriteWidth) > node.worldPosition.X &&
                    this.objectPosition.X < node.worldPosition.X + (node.TextureWidth) &&
                    this.objectPosition.Y + (this.spriteHeight) > node.worldPosition.Y &&
                    this.objectPosition.Y < node.worldPosition.Y + (node.TextureWidth))
                return true;
            else
                return false;
        }

        public bool Collides(NPC npc)
        {
            if (this.objectPosition.X + (this.spriteWidth) > npc.objectPosition.X &&
                                this.objectPosition.X < npc.objectPosition.X + (npc.spriteWidth) &&
                                this.objectPosition.Y + (this.spriteHeight) > npc.objectPosition.Y &&
                                this.objectPosition.Y < npc.objectPosition.Y + (npc.spriteWidth))
                return true;
            else
                return false;
        }
        
        public void Update(GameTime gameTime, List<Rectangle> cRectangles) {
            
            gravity(cRectangles);
            move(cRectangles);
            pulse(gameTime);
            lanternSwinging(gameTime);

            if (isMoving) {
                frameTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (frameTimer >= frameInterval) {
                    if (xCurrentFrame < xNumberOfFrames - 1) {
                        xCurrentFrame++;
                    } else {
                        xCurrentFrame = 0;
                    }

                    if (yCurrentFrame < yNumberOfFrames - 1) {
                        yCurrentFrame++;
                    } else {
                        yCurrentFrame = 0;
                    }
                    frameTimer = 0;
                }
            }
            KeyboardState kbs = Keyboard.GetState();
            if (kbs.IsKeyDown(Keys.OemMinus) || kbs.IsKeyDown(Keys.Subtract)) {
                LOWER_BOUNDARY += BOUNDARY_CHANGE;
                UPPER_BOUNDARY += BOUNDARY_CHANGE;
            } if (kbs.IsKeyDown(Keys.OemPlus) || kbs.IsKeyDown(Keys.Add)) {
                LOWER_BOUNDARY -= BOUNDARY_CHANGE;
                UPPER_BOUNDARY -= BOUNDARY_CHANGE;
            }
        }

        private void gravity(List<Rectangle> cRectangles) {
            Rectangle playerRect = new Rectangle((int)objectPosition.X, (int)objectPosition.Y, spriteWidth, spriteHeight);
            Rectangle tempRec = playerRect;
            tempRec.Y += MOVE_SPEED;
            bool collisionFree = true;
            foreach (Rectangle r in cRectangles) {
                if (tempRec.Intersects(r)) {
                    collisionFree = false;
                    break;
                }
            }
            if (collisionFree) {
                objectPosition.Y += GRAVITY_SPEED;
            }
        }

        private void pulse(GameTime gameTime) {
            lightTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (lightTimer >= lightInterval) {
                if (lightDir == LightDirection.Increasing) {
                    if (currentLightSize < UPPER_BOUNDARY)
                        currentLightSize += LIGHT_CHANGE;
                    else
                        lightDir = LightDirection.Decreasing;
                } if (lightDir == LightDirection.Decreasing) {
                    if (currentLightSize > LOWER_BOUNDARY)
                        currentLightSize -= LIGHT_CHANGE;
                    else
                        lightDir = LightDirection.Increasing;
                }
                lightTimer = 0;
            }
        }

        private void lanternSwinging(GameTime gameTime) {
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

        private void move(List<Rectangle> cRectangles) {
            KeyboardState kbs = Keyboard.GetState();

            Rectangle playerRect = new Rectangle((int)objectPosition.X, (int)objectPosition.Y, spriteWidth, spriteHeight);

            if (jumping) {
                this.objectPosition.Y += jumpspeed;
                jumpspeed += 0.25f;
                foreach(Rectangle r in cRectangles){
                    if (r.Intersects(playerRect)) {
                        jumping = false;
                        this.objectPosition.Y += -8;
                    }
                }
            } else {
                if (kbs.IsKeyDown(Keys.Space)) {
                    jumping = true;
                    jumpspeed = -6;
                }
            }

            if (kbs.IsKeyDown(Keys.Down) || kbs.IsKeyDown(Keys.Left) || kbs.IsKeyDown(Keys.Right)) {
                isMoving = true;
                
                if (kbs.IsKeyDown(Keys.Down)) {
                    Rectangle tempRec = playerRect;
                    tempRec.Y += MOVE_SPEED;
                    bool collisionFree = true;
                    foreach(Rectangle r in cRectangles){
                        if (tempRec.Intersects(r)) {
                            collisionFree = false;
                            break;
                        }
                    }
                    if(collisionFree)
                        objectPosition.Y += MOVE_SPEED;
                }
                if (kbs.IsKeyDown(Keys.Left)) {
                    Rectangle tempRec = playerRect;
                    tempRec.X -= MOVE_SPEED;
                    bool collisionFree = true;
                    foreach (Rectangle r in cRectangles) {
                        if (tempRec.Intersects(r)) {
                            collisionFree = false;
                            break;
                        }
                    }
                    walkingDirection = SpriteEffects.FlipHorizontally;
                    if(collisionFree)
                      objectPosition.X -= MOVE_SPEED;
                }
                if (kbs.IsKeyDown(Keys.Right)) {
                    Rectangle tempRec = playerRect;
                    tempRec.X += MOVE_SPEED;
                    bool collisionFree = true;
                    foreach (Rectangle r in cRectangles) {
                        if (tempRec.Intersects(r)) {
                            collisionFree = false;
                            break;
                        }
                    }
                    walkingDirection = SpriteEffects.None;
                    if (collisionFree)
                        objectPosition.X += MOVE_SPEED;
                }

            }

            if (kbs.IsKeyUp(Keys.Up) && kbs.IsKeyUp(Keys.Down) && kbs.IsKeyUp(Keys.Left) && kbs.IsKeyUp(Keys.Right)) {
                isMoving = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Color color) {
            Rectangle sourceRect = new Rectangle(spriteWidth * xCurrentFrame, spriteHeight * yCurrentFrame, spriteWidth, spriteHeight);
            spriteBatch.Draw(spriteSheet, objectPosition, sourceRect, color, 0, new Vector2(), 1, walkingDirection, 0);
            lanternPosition = objectPosition;
            if(walkingDirection == SpriteEffects.None)
                lanternPosition.X += 32;
            lanternPosition.Y += 32;
            spriteBatch.Draw(lanternTexture, lanternPosition, new Rectangle(0, 0, lanternTexture.Width, lanternTexture.Height), Color.White, degreeToRadian(lanternAngle), new Vector2(lanternTexture.Width / 2, lanternTexture.Height / 2), 1, walkingDirection, 0); //scale float
            //draw fire
            spriteBatch.End();
            Color drawColor = new Color(ambientColor.R / 255f * ambient, ambientColor.G / 255f * ambient, ambientColor.B / 255f * ambient);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            lightPosition = objectPosition;
            if(walkingDirection == SpriteEffects.None)
                lightPosition.X += ((lightTexture.Width + currentLightSize) / 2) - spriteWidth - 45;
            lightPosition.Y -= ((lightTexture.Height - currentLightSize) / 2) - spriteHeight - 9;
            spriteBatch.Draw(lightTexture, new Rectangle((int)(lightPosition.X), (int)(lightPosition.Y - (currentLightSize * 6)), (int)(lightTexture.Width + (currentLightSize * 15)), (int)(lightTexture.Height + (currentLightSize * 15))), new Rectangle(0, 0, lightTexture.Width, lightTexture.Height), drawColor, degreeToRadian(lanternAngle), new Vector2(lightTexture.Width / 2, 0), walkingDirection, 0);
        }

        private float degreeToRadian(float angle) {
            return (float)Math.PI * angle / 180.0f;
        }
    }

    enum LightDirection {
        Increasing, Decreasing
    }

    enum LanternSwing {
        Forwards, Backwards
    }

}
