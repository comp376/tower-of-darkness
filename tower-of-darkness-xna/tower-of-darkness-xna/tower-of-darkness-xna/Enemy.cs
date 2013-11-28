using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tower_of_darkness_xna {
    class Enemy : Object {
        private int MOVE_SPEED = 4;
        private const int GRAVITY_SPEED = 4;

        protected int xCurrentFrame = 0;
        protected int yCurrentFrame = 0;
        protected int frameTimer = 0;
        protected int frameInterval = 80;
        public bool isMoving;
        public MovementStatus movementStatus;

        private bool falling = false;
        private int moveTimer = 0;
        private int moveInterval;// = 1000;
        private int changeDirectionTimer = 0;
        private int changeDirectionInterval;// = 5000;

        private Color enemyColor = Color.White;

        public bool hit = false;
        public int hits;
        private string spritesheetName;
        private const int MAX_HOVER_HEIGHT = 500;
        private bool isBoss = false;

        private float hoverTimer = 0;
        private float hoverInterval = 100;
        private int startingY;

        private float hitTimer = 0;
        private float hitInterval = 10;
        private float flashColorTimer = 0;
        private float flashColorInterval = 100;

        enum HoverDirections {
            Up,
            Down,
        }
        HoverDirections hoverDirection;

        public Enemy(Texture2D spriteSheet, int xNumberOfFrames, int yNumberOfFrames, int spriteWidth, int spriteHeight, int hits, string spritesheetName, SpriteFont font, int moveInterval, int changeDirectionInterval, int startingDirection)
            : base(spriteSheet, xNumberOfFrames, yNumberOfFrames, spriteWidth, spriteHeight, font) {
                hoverDirection = HoverDirections.Up;
                startingY = objectRectangle.Y;
                this.hits = hits;
                this.spritesheetName = spritesheetName;
                this.moveInterval = moveInterval;
                this.changeDirectionInterval = changeDirectionInterval;
                switch (startingDirection) {
                    case 0:
                        movementStatus = MovementStatus.Left;
                        break;
                    case 1:
                        movementStatus = MovementStatus.Right;
                        break;
                }
            
                if (spritesheetName == "enemy2")
                    MOVE_SPEED = 8;

                if (spritesheetName == "boss")
                    isBoss = true;
        }

        public void Update(GameTime gameTime, List<Rectangle> cRectangles, List<Breakable> breakables) {
            changeMoveDirection(gameTime);
            animate(gameTime);
            gravity(cRectangles, breakables);
            move(gameTime, cRectangles, breakables);
            hover(gameTime);
            hitting(gameTime);
        }

        private void hitting(GameTime gameTime) {
            if (hit == true) {
                hitTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (hitTimer >= hitInterval) {
                    if (enemyColor != Color.Red) {
                        hits--;
                        enemyColor = Color.Red;
                        flashColorTimer = 0;
                    } else {
                        flashColorTimer += gameTime.ElapsedGameTime.Milliseconds;
                        if (flashColorTimer >= flashColorInterval) {
                            enemyColor = Color.White;
                            hitTimer = 0;
                            flashColorTimer = 0;
                            hit = false;
                        }
                    }
                }
            }
        }

        private void hover(GameTime gameTime) {
            if (!isBoss)
                return;
            hoverTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (hoverTimer >= hoverInterval) {
                if (hoverDirection == HoverDirections.Up) {
                    if (objectRectangle.Y > (startingY - MAX_HOVER_HEIGHT)) {
                        objectRectangle.Y -= 1;
                    } else {
                        hoverDirection = HoverDirections.Down;
                    }
                } else if (hoverDirection == HoverDirections.Down) {
                    if (startingY > objectRectangle.Y) {
                        objectRectangle.Y += 1;
                    } else {
                        hoverDirection = HoverDirections.Up;
                    }
                }
                hoverTimer = 0;
            }

        }

        private void changeMoveDirection(GameTime gameTime){
            if (!falling) {
                changeDirectionTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (changeDirectionTimer >= changeDirectionInterval) {
                    switch (movementStatus) {
                        case MovementStatus.Left:
                            movementStatus = MovementStatus.Right;
                            break;
                        case MovementStatus.Right:
                            movementStatus = MovementStatus.Left;
                            break;
                    }
                    changeDirectionTimer = 0;
                }
            }
        }

        private void animate(GameTime gameTime) {
            if (!falling) {
                frameTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (frameTimer >= frameInterval) {
                    if (xCurrentFrame < xNumberOfFrames - 1) {
                        xCurrentFrame++;
                    } else {
                        xCurrentFrame = 0;
                    }

                    frameTimer = 0;
                }
            }
        }

        private void move(GameTime gameTime, List<Rectangle> cRectangles, List<Breakable> breakables) {
            moveTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (moveTimer >= moveInterval && !falling) {
                switch (movementStatus) {
                    case MovementStatus.Left:
                        if (!collides(cRectangles, movementStatus) && !collides(breakables, movementStatus)) {
                            objectRectangle.X -= MOVE_SPEED;
                        } else {
                            switch (movementStatus) {
                                case MovementStatus.Left:
                                    movementStatus = MovementStatus.Right;
                                    break;
                                case MovementStatus.Right:
                                    movementStatus = MovementStatus.Left;
                                    break;
                            }
                        }
                        break;
                    case MovementStatus.Right:
                        if (!collides(cRectangles, movementStatus) && !collides(breakables, movementStatus)) {
                            objectRectangle.X += MOVE_SPEED;
                        } else {
                            switch (movementStatus) {
                                case MovementStatus.Left:
                                    movementStatus = MovementStatus.Right;
                                    break;
                                case MovementStatus.Right:
                                    movementStatus = MovementStatus.Left;
                                    break;
                            }
                        }
                        break;
                }

                moveTimer = 0;
            }
        }

        private void gravity(List<Rectangle> cRectangles, List<Breakable> breakables) {
            if (!collides(cRectangles, MovementStatus.Fall) && !collides(breakables, MovementStatus.Fall)) {
                objectRectangle.Y += GRAVITY_SPEED;
                falling = true;
            } else {
                falling = false;
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

        private bool collides(List<Breakable> breakables, MovementStatus movementStatus) {
            bool collision = false;
            Rectangle tempRect = objectRectangle;
            if (movementStatus == MovementStatus.Left)
                tempRect.X -= MOVE_SPEED;
            if (movementStatus == MovementStatus.Right)
                tempRect.X += MOVE_SPEED;
            if (movementStatus == MovementStatus.Fall)
                tempRect.Y += GRAVITY_SPEED;
            if (movementStatus == MovementStatus.Up)
                tempRect.Y += MOVE_SPEED;
            if (movementStatus == MovementStatus.Down)
                tempRect.Y += -MOVE_SPEED;
            foreach (Breakable b in breakables) {
                if (tempRect.Intersects(b.bRect)) {
                    collision = true;
                }
            }
            return collision;
        }

        public override void Draw(SpriteBatch spriteBatch, Color color) {
            int x = 0, y = 0;
            getSourceRect(ref x, ref y);
            Rectangle sourceRect = new Rectangle(x, y, spriteWidth, spriteHeight);
            SpriteEffects flip = SpriteEffects.None;
            switch (movementStatus) {
                case MovementStatus.Left:
                    flip = SpriteEffects.FlipHorizontally;
                    break;
            }
            spriteBatch.Draw(spriteSheet, objectRectangle, sourceRect, enemyColor, 0f, new Vector2(), flip, 0);

        }

        private void getSourceRect(ref int x, ref int y) {
            x = xCurrentFrame * spriteWidth;
            
            switch (movementStatus) {
                case MovementStatus.None:
                    y = (int)MovementStatus.Right * spriteHeight;
                    break;
                default:
                    //y = (int)movementStatus * spriteHeight;
                    y = 0;
                    break;
            }

        }

        public override string ToString() {
            return "ENEMY: spritesheet: [" + spritesheetName + "]"
                 + ", hits: [" + hits + "]"
                 + ", xFrames: [" + xNumberOfFrames + "]"
                 + ", yFrames: [" + yNumberOfFrames + "]";
        }
    }
}
