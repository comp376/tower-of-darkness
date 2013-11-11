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
        private const int GRAVITY_SPEED = 3;
        private bool jumping = false;
        private float jumpingHeight = 0;

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
        }

        public void LoadContent() {
            lightColor = new Color(120, 120, 90);
            lightTexture = Content.Load<Texture2D>("sprites/light3");
            lanternTexture = Content.Load<Texture2D>("sprites/lantern");
            lanternPosition = new Vector2(objectRectangle.X, objectRectangle.Y);
            lightPosition = new Vector2(objectRectangle.X, objectRectangle.Y);
            currentLightSize = lowerBoundary;
        }

        public void Update(GameTime gameTime, Rectangle mapRect, ref Rectangle mapView, ref List<Rectangle> cRectangles, ref List<Transition> transitions) {
            jump(ref cRectangles);
            move(gameTime, mapRect, ref mapView, ref cRectangles, ref transitions);
            gravity(ref cRectangles);
            animate(gameTime);
            hitTransition(transitions, mapView);
            lanternSwinging(gameTime);
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

        private void gravity(ref List<Rectangle> cRectangles) {
            Rectangle tempRect = objectRectangle;
            tempRect.Y += GRAVITY_SPEED;
            bool collisionFree = true;
            Rectangle lastObject = objectRectangle;
            foreach (Rectangle r in cRectangles) {
                lastObject = r;
                if (tempRect.Intersects(r)) {
                    collisionFree = false;
                    break;
                }
            }
            if (collisionFree) {
                objectRectangle.Y += GRAVITY_SPEED;
            } else
                objectRectangle.Y = lastObject.Y - objectRectangle.Height;
        }

        private void jump(ref List<Rectangle> cRectangles) {
            KeyboardState kbs = Keyboard.GetState();
            if (jumping) {
                Rectangle tempRect = objectRectangle;
                tempRect.Y += (int)jumpingHeight;
                foreach (Rectangle r in cRectangles) {
                    if (r.Intersects(tempRect)) {
                        jumpingHeight += 8;
                        break;
                    }
                }

                objectRectangle.Y += (int)jumpingHeight;
                jumpingHeight += 0.25f;
                foreach (Rectangle r in cRectangles) {
                    if (r.Intersects(objectRectangle)) {
                        jumping = false;
                    }
                }
            } else {
                if (kbs.IsKeyDown(Keys.Space)) {
                    jumping = true;
                    jumpingHeight = -8;
                }
            }
        }

        private void move(GameTime gameTime, Rectangle mapRect, ref Rectangle mapView, ref List<Rectangle> cRectangles, ref List<Transition> transitions) {
            int middleX = mapView.Width / 2;
            int middleY = mapView.Height / 2;

            KeyboardState kbs = Keyboard.GetState();
            moveTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (moveTimer >= moveInterval) {
                if (kbs.IsKeyDown(Keys.Left) || kbs.IsKeyDown(Keys.Right)) {
                    isMoving = true;

                    if (kbs.IsKeyDown(Keys.Left)) {
                        movementStatus = MovementStatus.Left;
                        if (!collides(cRectangles, movementStatus)) {
                            if (mapInView(mapView, mapRect, spriteWidth, 0, movementStatus)) {
                                if (objectRectangle.X > middleX) {
                                    objectRectangle.X -= MOVE_SPEED;
                                } else {
                                    scroll(movementStatus, ref mapView, ref cRectangles, ref transitions);
                                }
                            } else if (objectRectangle.X > 0) {
                                objectRectangle.X -= MOVE_SPEED;
                            }
                        }
                    }

                    if (kbs.IsKeyDown(Keys.Right)) {
                        movementStatus = MovementStatus.Right;
                        if (!collides(cRectangles, movementStatus)) {
                            if (mapInView(mapView, mapRect, spriteWidth, 0, movementStatus)) {
                                if (objectRectangle.X < middleX)
                                    objectRectangle.X += MOVE_SPEED;
                                else {
                                    scroll(movementStatus, ref mapView, ref cRectangles, ref transitions);
                                }
                            } else if (objectRectangle.X + spriteWidth < mapView.Width)
                                objectRectangle.X += MOVE_SPEED;
                        }
                    }
                    moveTimer = 0;

                }
            }

            if (kbs.IsKeyUp(Keys.Left) && kbs.IsKeyUp(Keys.Right)) {
                isMoving = false;
            }
        }

        private void scroll(MovementStatus movementStatus, ref Rectangle mapView, ref List<Rectangle> cRectangles, ref List<Transition> transitions) {
            switch (movementStatus) {
                case MovementStatus.Left:
                    mapView.X += -MOVE_SPEED;
                    for (int i = 0; i < cRectangles.Count; i++) {
                        cRectangles[i] = new Rectangle(cRectangles[i].X + MOVE_SPEED, cRectangles[i].Y, cRectangles[i].Width, cRectangles[i].Height);
                    } for (int i = 0; i < transitions.Count; i++) {
                        transitions[i].tRect = new Rectangle(transitions[i].tRect.X + MOVE_SPEED, transitions[i].tRect.Y, transitions[i].tRect.Width, transitions[i].tRect.Height);
                    }
                    break;
                case MovementStatus.Right:
                    mapView.X += MOVE_SPEED;
                    for (int i = 0; i < cRectangles.Count; i++) {
                        cRectangles[i] = new Rectangle(cRectangles[i].X - MOVE_SPEED, cRectangles[i].Y, cRectangles[i].Width, cRectangles[i].Height);
                    } for (int i = 0; i < transitions.Count; i++) {
                        transitions[i].tRect = new Rectangle(transitions[i].tRect.X - MOVE_SPEED, transitions[i].tRect.Y, transitions[i].tRect.Width, transitions[i].tRect.Height);
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
            foreach (Rectangle r in cRectangles) {
                if (tempRect.Intersects(r)) {
                    collision = true;
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
