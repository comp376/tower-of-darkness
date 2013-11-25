﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace tower_of_darkness_xna {
    class Character : NPC {
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
        private int talkInterval = 2000;

        //Lighting
        private Color lightColor;
        private Texture2D lightTexture;
        private Texture2D lanternTexture;
        private Vector2 lanternPosition;
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

        public Character(Texture2D spriteSheet, int xNumberOfFrames, int yNumberOfFrames, int spriteWidth, int spriteHeight, ContentManager Content)
            : base(spriteSheet, xNumberOfFrames, yNumberOfFrames, spriteWidth, spriteHeight) {
            this.Content = Content;
            LoadContent();
            objectRectangle = new Rectangle(objectRectangle.X + 64, objectRectangle.Y + 64, objectRectangle.Width, objectRectangle.Height - 8); //8 to offset tight jumps
        }

        public void LoadContent() {
            lightColor = new Color(120, 120, 90);
            lightTexture = Content.Load<Texture2D>("sprites/light3");
            lanternTexture = Content.Load<Texture2D>("sprites/lantern");
            lanternPosition = new Vector2(objectRectangle.X, objectRectangle.Y);
            lightPosition = new Vector2(objectRectangle.X, objectRectangle.Y);
            currentLightSize = lowerBoundary;
        }

        public void Update(GameTime gameTime, Rectangle mapRect, ref Rectangle mapView, ref List<Rectangle> cRectangles, ref List<Transition> transitions, ref List<Rectangle> ladders, ref List<Breakable> breakables, ref List<NPC> npcs, ref List<Enemy> enemies) {
            jump(ref cRectangles, ref mapView, ref mapRect, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies);
            move(gameTime, mapRect, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies);
            gravity(ref cRectangles, ref mapView, ref mapRect, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies);
            animate(gameTime);
            hitTransition(transitions, mapView);
            lanternSwinging(gameTime);
            talk(gameTime, ref npcs);
        }

        public override void Draw(SpriteBatch spriteBatch, Color color) {
            lanternPosition = new Vector2(objectRectangle.X, objectRectangle.Y);
            lightPosition = new Vector2(objectRectangle.X, objectRectangle.Y);
            SpriteEffects walkingDirection = SpriteEffects.None;
            switch (movementStatus) {
                case MovementStatus.Left:
                    walkingDirection = SpriteEffects.FlipHorizontally;
                    break;
            } if (walkingDirection == SpriteEffects.None) {
                lanternPosition.X += 32;
                lightPosition.X += ((lightTexture.Width + currentLightSize) / 2) - spriteWidth - 45;
            }
            lanternPosition.Y += 32;
            lightPosition.Y -= ((lightTexture.Height - currentLightSize) / 2) - spriteHeight - 9;
            spriteBatch.Draw(lanternTexture, lanternPosition, null, Color.White, degreeToRadian(lanternAngle), new Vector2(lanternTexture.Width / 2, lanternTexture.Height / 2), 1, walkingDirection, 0);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            spriteBatch.Draw(lightTexture, new Rectangle((int)(lightPosition.X), (int)(lightPosition.Y - (currentLightSize * 6)), (int)(lightTexture.Width + (currentLightSize * 15)), (int)(lightTexture.Height + (currentLightSize * 15))), new Rectangle(0, 0, lightTexture.Width, lightTexture.Height), lightColor, degreeToRadian(lanternAngle), new Vector2(lightTexture.Width / 2, 0), walkingDirection, 0);
            base.Draw(spriteBatch, color);
        }

        private void talk(GameTime gameTime, ref List<NPC> npcs) {
            talkTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (talkTimer >= talkInterval) {
                KeyboardState kbs = Keyboard.GetState();
                if (kbs.IsKeyDown(Keys.Up)) {
                    foreach (NPC npc in npcs) {
                        if (npc.objectRectangle.Intersects(objectRectangle)) {
                            npc.showText = true;
                            talkTimer = 0;
                        }
                    }
                } else {
                    foreach (NPC npc in npcs) {
                        npc.showText = false;
                    }
                }
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

        private void gravity(ref List<Rectangle> cRectangles, ref Rectangle mapView, ref Rectangle mapRect, ref List<Transition> transitions, ref List<Rectangle> ladders, ref List<Breakable> breakables, ref List<NPC> npcs, ref List<Enemy> enemies) {
            if (climbing)
                return;
            int middleY = mapView.Height / 2;
            if (!collides(cRectangles, MovementStatus.Fall) && !collides(ref breakables, MovementStatus.Fall)) {
                if (mapInView(mapView, mapRect, 0, GRAVITY_SPEED, MovementStatus.Fall)) {
                    if (objectRectangle.Y < middleY) {
                        objectRectangle.Y += GRAVITY_SPEED;
                    } else {
                        scroll(MovementStatus.Fall, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies);
                    }
                } else {
                    objectRectangle.Y += GRAVITY_SPEED;
                }
                falling = true;
            } else
                falling = false;
        }

        private void jump(ref List<Rectangle> cRectangles, ref Rectangle mapView, ref Rectangle mapRect, ref List<Transition> transitions, ref List<Rectangle> ladders, ref List<Breakable> breakables, ref List<NPC> npcs, ref List<Enemy> enemies) {
            int middleY = mapView.Height / 2;
            KeyboardState kbs = Keyboard.GetState();
            if (jumping) {
                if (collides(cRectangles, MovementStatus.Jump) || collides(ref breakables, MovementStatus.Jump)) {
                    jumping = false;
                } else {
                    if (mapInView(mapView, mapRect, 0, (int)jumpingHeight, MovementStatus.Jump)) {
                        if (objectRectangle.Y > middleY)
                            objectRectangle.Y += (int)jumpingHeight;
                        else {
                            scroll(MovementStatus.Jump, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies);
                        }
                    } else
                        objectRectangle.Y += (int)jumpingHeight;
                    //jumpingHeight += JUMPING_INCREMENT;
                    float apex = JUMPING_HEIGHT / JUMPING_INCREMENT;
                    //Console.WriteLine((startingY - apex) + ", " + (startingY - spriteHeight));
                    if (objectRectangle.Y < (startingY - spriteHeight)) {
                        jumping = false;
                    }
                }
            } else {
                if (kbs.IsKeyDown(Keys.Space) && !falling) {
                    jumping = true;
                    jumpingHeight = -JUMPING_HEIGHT;
                    startingY = objectRectangle.Y;
                }
            }
        }

        private void move(GameTime gameTime, Rectangle mapRect, ref Rectangle mapView, ref List<Rectangle> cRectangles, ref List<Transition> transitions, ref List<Rectangle> ladders, ref List<Breakable> breakables, ref List<NPC> npcs, ref List<Enemy> enemies) {
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
                                    scroll(movementStatus, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies);
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
                                    scroll(movementStatus, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies);
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
                                        scroll(movementStatus, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies);
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
                                        scroll(movementStatus, ref mapView, ref cRectangles, ref transitions, ref ladders, ref breakables, ref npcs, ref enemies);
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

        private void scroll(MovementStatus movementStatus, ref Rectangle mapView, ref List<Rectangle> cRectangles, ref List<Transition> transitions, ref List<Rectangle> ladders, ref List<Breakable> breakables, ref List<NPC> npcs, ref List<Enemy> enemies) {
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
                    collision = true;
                    r.isTouched = isTouched;
                }
            }
            return collision;
        }

        private void hitTransition(List<Transition> transitions, Rectangle mapView) {
            foreach (Transition t in transitions) {
                if (objectRectangle.Intersects(t.tRect)) {
                    Game1.currentGameState = new LevelState(Content, mapView.Width, mapView.Height, t.nextMapName, this, t);
                    break;
                }
            }
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
