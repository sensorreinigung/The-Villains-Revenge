﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TheVillainsRevenge
{
    class Player
    {
        //Deine Mutter ist so fett, selbst die Sonne wird von ihr angezogen
        public Vector2 position; //Position
        Vector2 lastPosition; //Position vor vorherigem Update
        public CollisionBox cbox;
        public int speed; //Bewegungsgeschwindigkeit in m/s _/60
        public int airspeed; //Geschwindigkeit bei Sprung & Fall in m/s _/60
        public bool jump = false;
        public bool fall = false;
        public double falltimer;
        public double jumptimer;
        public int jumppower; //Anfangsgeschwindigkeit in m/s _/60
        public int gravitation; //Erdbeschleunigung in (m/s)*(m/s) _/60
        public int lifes;
        public static int startLifes = Convert.ToInt32((double)Game1.luaInstance["playerStartLifes"]);
        public int item1;
        public int item2;
        public bool check = false;
        public Vector2 checkpoint;
        public bool coverEyes = true;
        string animation = "";

        //----------Spine----------
        public SkeletonRenderer skeletonRenderer;
        public Skeleton skeleton;
        public AnimationState animationState;
        public SkeletonBounds bounds = new SkeletonBounds();

        public Player(int x, int y) //Konstruktor, setzt Anfangsposition
        {
            checkpoint = new Vector2(x, y);
            position.X = x;
            position.Y = y;
            lastPosition = position;
            cbox = new CollisionBox(Convert.ToInt32((double)Game1.luaInstance["playerCollisionOffsetX"]), Convert.ToInt32((double)Game1.luaInstance["playerCollisionOffsetY"]), Convert.ToInt32((double)Game1.luaInstance["playerCollisionWidth"]), Convert.ToInt32((double)Game1.luaInstance["playerCollisionHeight"]));
            lifes = startLifes;

        }

        public void Load(ContentManager Content, GraphicsDeviceManager graphics)//Wird im Hauptgame ausgeführt und geladen
        {
            //----------Spine----------
            skeletonRenderer = new SkeletonRenderer(graphics.GraphicsDevice);
            skeletonRenderer.PremultipliedAlpha = true;

            String name = "skeleton";

            Atlas atlas = new Atlas("spine/sprites/" + name + ".atlas", new XnaTextureLoader(graphics.GraphicsDevice));
            SkeletonJson json = new SkeletonJson(atlas);
            json.Scale = (float)Convert.ToInt32((double)Game1.luaInstance["playerScale"])/10; //Für den Fall dass die aktuelle Textur in der Größe von der in Spine verwendeten Textur abweicht.
            skeleton = new Skeleton(json.ReadSkeletonData("spine/sprites/" + name + ".json"));
            skeleton.SetSlotsToSetupPose(); // Without this the skin attachments won't be attached. See SetSkin.

            // Define mixing between animations.
            AnimationStateData animationStateData = new AnimationStateData(skeleton.Data);
            //animationStateData.SetMix("walk", "jump", 0.2f);
            //animationStateData.SetMix("jump", "walk", 0.4f);
            animationState = new AnimationState(animationStateData);

            // Event handling for all animations.
            animationState.Start += Start;
            animationState.End += End;
            animationState.Complete += Complete;
            animationState.Event += Event;

            skeleton.x = position.X;
            skeleton.y = position.Y;
        }

        public void getHit()
        {
            lifes--;
            if (lifes > 0)
            {
                position.X = checkpoint.X;
                position.Y = checkpoint.Y;
                skeleton.x = position.X;
                skeleton.y = position.Y;
                lastPosition = new Vector2(skeleton.X, skeleton.Y);
            }
        }

        public void Update(GameTime gameTime, Map map)
        {
            if (Game1.debug)
            {
                coverEyes = true;
            }
            else
            {
                coverEyes = false;
            }
            speed = Convert.ToInt32((double)Game1.luaInstance["playerSpeed"]);
            airspeed = Convert.ToInt32((double)Game1.luaInstance["playerAirspeed"]);
            jumppower = Convert.ToInt32((double)Game1.luaInstance["playerJumppower"]);
            gravitation = Convert.ToInt32((double)Game1.luaInstance["playerGravitation"]);
            if (Game1.input.enter)
            {
                lifes = Convert.ToInt32((double)Game1.luaInstance["playerLifes"]);
                item1 = Convert.ToInt32((double)Game1.luaInstance["playerItem1"]);
                item2 = Convert.ToInt32((double)Game1.luaInstance["playerItem2"]);
            }

            //Geschwindigkeit festlegen
            int actualspeed = speed; 
            if (jump || fall)
            {
                actualspeed = airspeed;
            }
            //Lade Keyboard-Daten
            if (Game1.input.iteme) 
            {
                int i = item1;
                item1 = item2;
                item2 = i;
            }
            else if (Game1.input.itemq)
            {
                int i = item2;
                item2 = item1;
                item1 = i;
            }
            if (Game1.input.rechts) //Wenn Rechte Pfeiltaste
            {
                Move(actualspeed, 0, map); //Bewege Rechts
                    if (animation != "run")
                    {
                        animationState.SetAnimation(0, "run", true);
                        animation = "run";
                    }
                skeleton.flipX = false;
            }
            else if (Game1.input.links) //Wenn Rechte Pfeiltaste
            {
                Move(-actualspeed, 0, map);//Bewege Links
                if (animation != "run")
                {
                    animationState.SetAnimation(0, "run", true);
                    animation = "run";
                }
                skeleton.flipX = true;
            }
            else
            {
                if (animation != "idle")
                {
                    animationState.SetAnimation(0, "idle", true);
                    animation = "idle";
                }
            }
            if (Game1.input.sprung)
            {
                if (!jump && !fall)
                {
                    Jump(gameTime, map); //Springen!
                }
            }
            if (Game1.input.itemu)
            {
                if (item1 == 1)
                {
                    GameScreen.slow = GameScreen.slow+Convert.ToInt32((double)Game1.luaInstance["itemSlowTime"]);
                }
                item1 = 0;
            }

            //Gravitation
            if (CollisionCheckedVector(0, 1, map.blocks).Y > 0 && !jump)
            {
                if (!fall)
                {
                    fall = true;
                    falltimer = gameTime.TotalGameTime.TotalMilliseconds;
                }
                float t = (float)((gameTime.TotalGameTime.TotalMilliseconds - falltimer) / 1000);
                Move(0, (int)((gravitation * t)), map); //v(t)=-g*t
            }
            else
            {
                fall = false;
            }

            //Sprung fortführen
            if (jump)
            {
                Jump(gameTime, map);
            }
            position.Y = skeleton.Y;
            position.X = skeleton.X;
            if (position.Y >= (map.size.Y)) getHit();
        }

        public void Draw(GameTime gameTime, Camera camera)
        {
            //Player -> Drawposition
            skeleton.X = position.X - camera.viewport.X;
            skeleton.Y = position.Y - camera.viewport.Y;
            //----------Spine----------
            animationState.Update(gameTime.ElapsedGameTime.Milliseconds / 1000f);
            animationState.Apply(skeleton);
            skeleton.UpdateWorldTransform();
            skeletonRenderer.Begin();
            skeletonRenderer.Draw(skeleton);
            skeletonRenderer.End();
            //Player -> Worldposition
            skeleton.X = position.X;
            skeleton.Y = position.Y;
        }

        public void Jump(GameTime gameTime, Map map) //Deine Mudda springt bei Doodle Jump nach unten.
        {
            if (CollisionCheckedVector(0, -1, map.blocks).Y < 0)
            {
                if (!jump)
                {
                //    animationState.SetAnimation(0, "jump", false);
                    jump = true;
                    jumptimer = gameTime.TotalGameTime.TotalMilliseconds;
                }
                float t = (float)((gameTime.TotalGameTime.TotalMilliseconds - jumptimer) / 1000);
                int deltay = (int)(-jumppower + (gravitation * t));
                if (deltay > 0)
                {
                    jump = false;
                    fall = true;
                    falltimer = gameTime.TotalGameTime.TotalMilliseconds;
                } 
                else
                {
                    Move(0, deltay, map); //v(t)=-g*t
                }
            }
            else
            {
                jump = false;
            }
        }

        public void Move(int deltax, int deltay, Map map) //Falls Input, bewegt den Spieler
        {
            Vector2 domove = new Vector2(0, 0);
            domove = CollisionCheckedVector(deltax, deltay, map.blocks);
            skeleton.X += domove.X;
            skeleton.Y += domove.Y;
            position.Y = skeleton.Y;
            position.X = skeleton.X;
            cbox.Update(position);
        }


        Vector2 CollisionCheckedVector(int x, int y, List<Block> list)
        {
            CollisionBox cboxnew = new CollisionBox((int)cbox.offset.X, (int)cbox.offset.Y, cbox.box.Width, cbox.box.Height);
            cboxnew.Update(cbox.position);
            Vector2 move = new Vector2(0, 0);
            int icoll;
            bool stop;
            //Größere Koordinate als Iteration nehmen
            if (Math.Abs(x) > Math.Abs(y))
            {
                icoll = Math.Abs(x);
            }
            else
            {
                icoll = Math.Abs(y);
            }
            //Iteration
            for (int i = 1; i <= icoll; i++)
            {
                stop = false;
                //Box für nächsten Iterationsschritt berechnen
                cboxnew.box.X = this.cbox.box.X + ((x / icoll) * i);
                cboxnew.box.Y = this.cbox.box.Y + ((y / icoll) * i);
                //Gehe die Blöcke der Liste durch
                foreach (Block block in list)
                {
                    //Wenn Kollision vorliegt: Keinen weiteren Block abfragen
                    if (cboxnew.box.Intersects(block.cbox))
                    {
                        stop = true;
                        break;
                    }
                }
                if (stop == true) //Bei Kollision: Kollisionsabfrage mit letztem kollisionsfreien Zustand beenden
                {
                    break;
                }
                else //Kollisionsfreien Fortschritt speichern
                {
                    move.X = cboxnew.box.X - cbox.box.X;
                    move.Y = cboxnew.box.Y - cbox.box.Y;
                }
            }
            return move;
        }
        Vector2 BoundingCheckedVector(int x, int y, List<Block> list)
        {
            position.Y = skeleton.Y;
            position.X = skeleton.X;
            Vector2 move = new Vector2(0, 0);
            int icoll;
            bool stop;
            //Größere Koordinate als Iteration nehmen
            if (Math.Abs(x) > Math.Abs(y))
            {
                icoll = Math.Abs(x);
            }
            else
            {
                icoll = Math.Abs(y);
            }
            //Iteration
            for (int i = 1; i <= icoll; i++)
            {
                stop = false;
                //Box für nächsten Iterationsschritt berechnen
                skeleton.X = position.X + ((x / icoll) * i);
                skeleton.Y = position.Y + ((y / icoll) * i);
                bounds.Update(skeleton, true);
                //Gehe die Blöcke der Liste durch
                foreach (Block block in list)
                {
                    Vector2 ol = new Vector2((float)block.cbox.X, (float)block.cbox.Y);
                    Vector2 or = new Vector2((float)(block.cbox.X + block.cbox.Width), (float)block.cbox.Y);
                    Vector2 ul = new Vector2((float)block.cbox.X, (float)(block.cbox.Y + block.cbox.Height));
                    Vector2 ur = new Vector2((float)(block.cbox.X + block.cbox.Width), (float)(block.cbox.Y + block.cbox.Height));
                    if (bounds.AabbContainsPoint(ol.X, ol.Y) || bounds.AabbContainsPoint(or.X, or.Y) || bounds.AabbContainsPoint(ul.X, ul.Y) || bounds.AabbContainsPoint(ur.X, ur.Y))
                    {
                        check = true;
                        BoundingBoxAttachment colOL = bounds.ContainsPoint(ol.X, ol.Y);
                        BoundingBoxAttachment colOR = bounds.ContainsPoint(or.X, or.Y);
                        BoundingBoxAttachment colUL = bounds.ContainsPoint(ul.X, ul.Y);
                        BoundingBoxAttachment colUR = bounds.ContainsPoint(ur.X, ur.Y);
                        //BoundingBoxAttachment bb = bounds.BoundingBoxes.FirstOrDefault();
                        //if (colOL == bb || colOR == bb || colUL == bb || colUR == bb) //Wenn Kollision vorliegt: Keinen weiteren Block abfragen
                        if (colOL != null || colOR != null || colUL != null || colUR != null) //Wenn Kollision vorliegt: Keinen weiteren Block abfragen
                        {
                            check = true;
                            stop = true;
                            break;
                        }
                    }
                }
                if (stop == true) //Bei Kollision: Kollisionsabfrage mit letztem kollisionsfreien Zustand beenden
                {
                    break;
                }
                else //Kollisionsfreien Fortschritt speichern
                {
                    check = false;
                    move.X = skeleton.X - position.X;
                    move.Y = skeleton.Y - position.Y;
                }
            }
            skeleton.Y = position.Y;
            skeleton.X = position.X;
            return move;
        }

        //----------Spine----------
        public void Start(object sender, StartEndArgs e)
        {
            Console.WriteLine(e.TrackIndex + " " + animationState.GetCurrent(e.TrackIndex) + ": start");
        }

        public void End(object sender, StartEndArgs e)
        {
            Console.WriteLine(e.TrackIndex + " " + animationState.GetCurrent(e.TrackIndex) + ": end");
        }

        public void Complete(object sender, CompleteArgs e)
        {
            Console.WriteLine(e.TrackIndex + " " + animationState.GetCurrent(e.TrackIndex) + ": complete " + e.LoopCount);
        }

        public void Event(object sender, EventTriggeredArgs e)
        {
            Console.WriteLine(e.TrackIndex + " " + animationState.GetCurrent(e.TrackIndex) + ": event " + e.Event);
        }
    }
}
