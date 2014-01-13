﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using LuaInterface;

namespace TheVillainsRevenge
{
    class GameScreen
    {
        public static int test;
        Texture2D texture;
        Texture2D debug;
        Player spieler = new Player(40, 1000);
        Hero hero = new Hero(0, 0);
        Princess princess = new Princess();
        Map karte = new Map();
        Camera camera = new Camera();
        GUI gui = new GUI();
        ParallaxPlane background_0 = new ParallaxPlane("background_0");
        ParallaxPlane background_1 = new ParallaxPlane("background_1");
        ParallaxPlane background_2 = new ParallaxPlane("background_2");
        ParallaxPlane background_3 = new ParallaxPlane("background_3");
        CloudPlane clouds_1 = new CloudPlane(1);
        CloudPlane clouds_2 = new CloudPlane(2);
        CloudPlane clouds_3 = new CloudPlane(3);
        Sound bgmusic = new Sound("sounds/Level_1/background");
        RenderTarget2D renderScreen;
        RenderTarget2D renderSpine;
        RenderTarget2D renderForeground;
        RenderTarget2D renderBackground0;
        RenderTarget2D renderBackground1;
        RenderTarget2D renderBackground2;
        RenderTarget2D renderBackground3;
        RenderTarget2D renderHud;
        SpriteFont font;
        bool levelend = false;
        Effect coverEyes;
        Effect outline;
        Effect smash;
        public static int slow = 0;
        double slowTime;
        public static Lua LuaKI = new Lua();

        //KIDaten
        public int getPoints(string w)
        {
            if (w == "Spieler")
                return spieler.kicheck.Count()-1;
            else
                return hero.kicheck.Count()-1;
        }
        public int getPointID(int s,string w)
        {
            if (w == "Spieler")
                return spieler.kicheck.ElementAt(s).id;
            else
                return hero.kicheck.ElementAt(s).id;
        }
        public int getPointTime(int s,string w)
        {
            if (w == "Spieler")
                return spieler.kicheck.ElementAt(s).time;
            else
                return hero.kicheck.ElementAt(s).time;
        }
        public void removePoint(int s)
        {
            if (hero.kicheck.Count() != 0)
            {
                hero.kicheck.RemoveAt(s);
            }
        }
        public void addPoint(int s, int t)
        {
            bool geht = true;
            if (hero.kicheck.Count() != 0)
            {
                if (hero.kicheck.ElementAt(hero.kicheck.Count() - 1).id == s)
                {
                    geht = false;
                }

            }
            if(geht)
                hero.kicheck.Add(new KICheck(t, s));
        }

        public GameScreen()
        {
            texture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            texture.SetData<Color>(new Color[] { Color.White });
            LuaKI.RegisterFunction("getPoints", this, this.GetType().GetMethod("getPoints"));
            LuaKI.RegisterFunction("getPointID", this, this.GetType().GetMethod("getPointID"));
            LuaKI.RegisterFunction("getPointTime", this, this.GetType().GetMethod("getPointTime"));
            LuaKI.RegisterFunction("addPoint", this, this.GetType().GetMethod("addPoint"));
            LuaKI.RegisterFunction("removePoint", this, this.GetType().GetMethod("removePoint")); 
        }

        public void Load(ContentManager Content)
        {
            renderScreen = new RenderTarget2D(Game1.graphics.GraphicsDevice, 1920, 1080);
            renderSpine = new RenderTarget2D(Game1.graphics.GraphicsDevice, 1920, 1080);
            renderForeground = new RenderTarget2D(Game1.graphics.GraphicsDevice, 1920, 1080);
            renderBackground0 = new RenderTarget2D(Game1.graphics.GraphicsDevice, 1920, 1080);
            renderBackground1 = new RenderTarget2D(Game1.graphics.GraphicsDevice, 1920, 1080);
            renderBackground2 = new RenderTarget2D(Game1.graphics.GraphicsDevice, 1920, 1080);
            renderBackground3 = new RenderTarget2D(Game1.graphics.GraphicsDevice, 1920, 1080);
            renderHud = new RenderTarget2D(Game1.graphics.GraphicsDevice, 1920, 1080);
            font = Content.Load<SpriteFont>("fonts/schrift");
            spieler.Load(Content, Game1.graphics);
            hero.Load(Content, Game1.graphics);
            princess.Load(Content, Game1.graphics);
            karte.Load(Content);
            karte.Generate();
            background_0.Load(Content, Convert.ToInt32((double)Game1.luaInstance["planeTilesBackground0"]));
            background_1.Load(Content, Convert.ToInt32((double)Game1.luaInstance["planeTilesBackground1"]));
            background_2.Load(Content, Convert.ToInt32((double)Game1.luaInstance["planeTilesBackground2"]));
            background_3.Load(Content, Convert.ToInt32((double)Game1.luaInstance["planeTilesBackground3"]));
            clouds_1.Load(Content, "clouds_1", karte, camera);
            clouds_2.Load(Content, "clouds_2", karte, camera);
            clouds_3.Load(Content, "clouds_3", karte, camera);
            gui.Load(Content);
            coverEyes = Content.Load<Effect>("CoverEyes");
            outline = Content.Load<Effect>("Outline");
            smash = Content.Load<Effect>("Smash");
            if (Game1.sound)
            {
                bgmusic.Load(Content);
            }
            debug = Content.Load<Texture2D>("sprites/Level_1/Planes/background_0_debug");
        }
        public void Save(int checkpointX)
        {
            spieler.Save(checkpointX);
            hero.Save();
            princess.Save();
            foreach (Enemy enemy in karte.enemies)
            {
                enemy.Save();
            }
            foreach (Trigger trigger in karte.triggers)
            {
                trigger.Save();
            }
            foreach (MovingBlock mblock in karte.mblocks)
            {
                mblock.Save();
            }
        }
        public void Reset()
        {
            if (spieler.lifes != 0)
            {
                spieler.Reset();
                hero.Reset();
                princess.Reset();
                foreach (Enemy enemy2 in karte.enemies)
                {
                    enemy2.Reset();
                }
                foreach (Trigger trigger in karte.triggers)
                {
                    trigger.Reset(karte.blocks);
                }
                foreach (MovingBlock mblock in karte.mblocks)
                {
                    mblock.Reset();
                }
            }
        }

        public int Update(GameTime gameTime, ContentManager Content)
        {
            if (!levelend)
            {
                spieler.Update(gameTime, karte);
                //--------------------Map--------------------
                karte.Update(gameTime, spieler.cbox.box);
                for (int i = 0; i < karte.objects.Count(); i++)
                {
                    Obj obj = karte.objects.ElementAt(i);
                    obj.Update(gameTime, karte);
                    if (obj.box.Intersects(hero.cbox.box))
                    {
                        hero.slowtime += 10;
                        karte.objects.Remove(obj);
                    }
                    else if (obj.type == 2)
                    {
                        foreach (Block block in karte.blocks)
                        {
                            if (obj.box.Intersects(block.cbox))
                            {
                                karte.objects.Remove(obj);
                                break;
                            }
                        }
                    }
                }
                foreach (Enemy enemy in karte.enemies)
                {
                    enemy.Update(gameTime, karte,hero.position);
                    if (enemy.position.X < -enemy.cbox.box.Width || enemy.position.Y < -enemy.cbox.box.Height || enemy.position.X > karte.size.X || enemy.position.Y > karte.size.Y)
                    {
                        karte.enemies.Remove(enemy);
                        break;
                    }
                    if (spieler.cbox.box.Intersects(enemy.cbox.box)&&enemy.type == 1)
                    {
                        spieler.getHit();
                        Reset();
                        break;
                    }
                    if(enemy.type == 2&&hero.cbox.box.Intersects(enemy.cbox.box))
                    {
                        karte.enemies.Remove(enemy);
                        break;
                    }
                }
                foreach (Item item in karte.items)
                {
                    if (spieler.cbox.box.Intersects(item.cbox))
                    {
                        if (item.type == "herz")
                        {
                            if (spieler.lifes != 4)
                                spieler.lifes++;
                            karte.items.Remove(item);
                        }
                        else if (item.type == "zeit")
                        {
                            if (spieler.item1 != 0)
                                spieler.item2 = spieler.item1;
                            spieler.item1 = 1;
                            karte.items.Remove(item);
                        }
                        else if (item.type == "banana")
                        {
                            if (spieler.item1 != 0)
                                spieler.item2 = spieler.item1;
                            spieler.item1 = 2;
                            karte.items.Remove(item);
                        }
                        else if (item.type == "monkey")
                        {
                            if (spieler.item1 != 0)
                                spieler.item2 = spieler.item1;
                            spieler.item1 = 3;
                            karte.items.Remove(item);
                        }
                        break;
                    }
                }
                foreach (Checkpoint cpoint in karte.checkpoints)
                {
                    if (spieler.position.X > cpoint.x && spieler.checkpoint.X < cpoint.x)
                    {
                        //TODO: Speichern aller dynamischen Objekte in der Welt um diesen Zustand bei zurücksetzen an Checkpoint exakt zu rekonstruieren.
                        if (cpoint.end)
                        {
                            spieler.spine.anim("idle", 1, false, gameTime);
                            levelend = true;
                        }
                        else
                        {
                            Save(cpoint.x);
                        }
                        break;
                    }
                }
                foreach (Trigger trigger in karte.triggers)
                {
                    if (spieler.cbox.box.Intersects(trigger.cbox) && spieler.fall)
                    {
                        trigger.Pushed(karte.blocks);
                        break;
                    }
                }
                //--------------------Hero--------------------
                hero.Update(gameTime, karte, spieler.cbox.box);    
                for (int i = 0; i <karte.kipoints.Count(); i++)
                {
                    KIPoint kipoint = karte.kipoints.ElementAt(i);
                    if (spieler.cbox.box.Intersects(kipoint.cbox))
                    {
                        bool geht = true;
                        if (spieler.kicheck.Count() > 0)
                        {
                            KICheck check = spieler.kicheck.ElementAt(spieler.kicheck.Count()-1);
                            if(check.id == kipoint.id)
                                geht = false;
                        }
                        if (geht)
                        {
                            if (spieler.kicheck.Count() >= 20)
                            {
                                spieler.kicheck.RemoveAt(0);
                            }
                            spieler.kicheck.Add(new KICheck((int)gameTime.TotalGameTime.TotalSeconds, kipoint.id));
                            LuaKI.DoFile("kiscript.txt");
                        }
                    }
                    if (hero.cbox.box.Intersects(kipoint.cbox)&&hero.kicheck.Count() != 0)
                    {
                        if (hero.kicheck.ElementAt(0).id == kipoint.id)
                        {
                            hero.kicheck.RemoveAt(0);
                        }
                    }
                }
                //--------------------Princess--------------------
                princess.Update(gameTime);
                if (spieler.position.Y >= (karte.size.Y))
                {
                    spieler.getHit();
                    Reset();
                }
                if (spieler.cbox.box.Intersects(hero.cbox.box))
                {
                    spieler.lifes = 0;
                }
                //--------------------Camera--------------------
                camera.Update(Game1.graphics, spieler, karte);
                //--------------------Backgrounds--------------------
                background_0.Update(karte, camera);
                background_1.Update(karte, camera);
                background_2.Update(karte, camera);
                background_3.Update(karte, camera);
                clouds_1.Update(karte, gameTime, camera);
                clouds_2.Update(karte, gameTime, camera);
                clouds_3.Update(karte, gameTime, camera);
                //--------------------SlowTimer--------------------
                if (slow != 0)
                {
                    slowTime += gameTime.ElapsedGameTime.TotalSeconds;
                    if (slowTime > slow)
                    {
                        slowTime = 0;
                        slow = 0;
                    }
                }
                //-----Update Shader-----
                coverEyes.Parameters["gameTime"].SetValue(gameTime.TotalGameTime.Milliseconds);
                outline.Parameters["gameTime"].SetValue(gameTime.TotalGameTime.Milliseconds);
                smash.Parameters["gameTime"].SetValue(gameTime.TotalGameTime.Milliseconds);
            }
            if (spieler.lifes != 0)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //----------------------------------------------------------------------
            //----------------------------------------Draw to renderSpine
            //----------------------------------------------------------------------
            Game1.graphics.GraphicsDevice.SetRenderTarget(renderSpine);
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);
            spieler.Draw(gameTime, camera);
            hero.Draw(gameTime, camera);

            //----------------------------------------------------------------------
            //----------------------------------------Draw to renderBackground
            //----------------------------------------------------------------------
            //Background3
            Game1.graphics.GraphicsDevice.SetRenderTarget(renderBackground3);
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.viewportTransform);
                background_3.Draw(spriteBatch, spieler); //Himmel
                clouds_3.Draw(spriteBatch);
            spriteBatch.End();
            //Background2
            Game1.graphics.GraphicsDevice.SetRenderTarget(renderBackground2);
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.viewportTransform);
                background_2.Draw(spriteBatch, spieler); //Berge
                clouds_2.Draw(spriteBatch);
            spriteBatch.End();
            //Background1
            Game1.graphics.GraphicsDevice.SetRenderTarget(renderBackground1);
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.viewportTransform);
                background_1.Draw(spriteBatch, spieler); //Wald
                clouds_1.Draw(spriteBatch);
            spriteBatch.End();
            //Background0
            Game1.graphics.GraphicsDevice.SetRenderTarget(renderBackground0);
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.viewportTransform);
                if (!Game1.debug)
                {
                    background_0.Draw(spriteBatch, spieler); //Bäume
                }
                else
                {
                    spriteBatch.Draw(debug, new Vector2(background_0.position.X, background_0.position.Y), Color.White);
                }
            spriteBatch.End();

            //----------------------------------------------------------------------
            //----------------------------------------Draw to RenderForeground
            //----------------------------------------------------------------------
            Game1.graphics.GraphicsDevice.SetRenderTarget(renderForeground);
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.viewportTransform);
                karte.Draw(spriteBatch,gameTime,camera); //Plattformen & Co
                hero.Draw(gameTime,camera); //Ashbrett
                spriteBatch.Draw(renderSpine, new Vector2(camera.viewport.X, camera.viewport.Y), Color.White); //Bonepuker
                if (Game1.debug) //Boundingboxen
                {
                    spriteBatch.Draw(texture, spieler.cbox.box, null, Color.White);
                    spriteBatch.Draw(texture, hero.cbox.box, null, Color.White);
                    for (int i = 0; i < karte.enemies.Count(); i++)
                    {
                        Enemy enemy = karte.enemies.ElementAt(i);
                        spriteBatch.Draw(texture, enemy.cbox.box, null, Color.White);
                    }
                    spriteBatch.Draw(texture, hero.kicollide, null, Color.Red);
                      
                }
            spriteBatch.End();

            //----------------------------------------------------------------------
            //----------------------------------------Draw to renderHud
            //----------------------------------------------------------------------
            Game1.graphics.GraphicsDevice.SetRenderTarget(renderHud);
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                if (levelend)
                {
                    spriteBatch.DrawString(font, "Finished level!", new Vector2((Game1.resolution.X / 2) - 200, (Game1.resolution.Y / 2) - 200), Color.Black, 0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 0f);
                }
                if (Game1.debug)
                {
                    //for (int i = 0; i <= 62; i++)
                    //{
                    spriteBatch.DrawString(font, "" + (spieler.spine.CurrentAnimTime(gameTime)), new Vector2(Game1.resolution.X - 300, 150), Color.White);
                    //}
                    spriteBatch.DrawString(font, "bg0.tex[0]: " + (background_0.texture[0].Name), new Vector2(Game1.resolution.X - 300, 170), Color.White);
                    spriteBatch.DrawString(font, "Player: " + (spieler.position.X + " " + spieler.position.Y), new Vector2(Game1.resolution.X - 300, 190), Color.White);
                    spriteBatch.DrawString(font, "Hero: " + (hero.position.X + " " + hero.position.Y), new Vector2(Game1.resolution.X - 300, 210), Color.White);
                    spriteBatch.DrawString(font, "Camera: " + (camera.viewport.X + " " + camera.viewport.Y + " " + camera.viewport.Width + " " + camera.viewport.Height), new Vector2(Game1.resolution.X - 300, 230), Color.White);
                    spriteBatch.DrawString(font, "Skeleton: " + (spieler.spine.skeleton.X + " " + spieler.spine.skeleton.Y), new Vector2(Game1.resolution.X - 300, 250), Color.White);
                    spriteBatch.DrawString(font, "RageMeter: " + princess.rageMeter, new Vector2(Game1.resolution.X - 300, 290), Color.White);
                    Slot bb = spieler.spine.skeleton.FindSlot("bonepuker");
                    spriteBatch.DrawString(font, "bb-bonepuker: " + spieler.spine.bounds.BoundingBoxes.FirstOrDefault(), new Vector2(Game1.resolution.X - 300, 310), Color.White);
                    spriteBatch.DrawString(font, "SlowTime: " + slow + " Vergangen: " + slowTime, new Vector2(Game1.resolution.X - 300, 330), Color.White);
                    spriteBatch.DrawString(font, "KIState: " + hero.kistate, new Vector2(Game1.resolution.X - 300, 350), Color.White);
                    for (int i = 0; i < spieler.kicheck.Count(); i++)
                    {
                        KICheck kicheck = spieler.kicheck.ElementAt(i);
                        spriteBatch.DrawString(font, "ID: " + kicheck.id + " Time: " + kicheck.time, new Vector2(Game1.resolution.X - 300, 390 + i * 20), Color.White);
                    }
                    for (int i = 0; i < hero.kicheck.Count(); i++)
                    {
                        KICheck kicheck = hero.kicheck.ElementAt(i);
                        spriteBatch.DrawString(font, "ID: " + kicheck.id + " Time: " + kicheck.time, new Vector2(Game1.resolution.X - 400, 390 + i * 20), Color.White);
                    }
                }
                gui.Draw(spriteBatch, spieler.lifes, spieler.position, hero.position, karte.size, spieler.item1, spieler.item2);
            spriteBatch.End();

            //----------------------------------------------------------------------
            //----------------------------------------Draw to renderScreen
            //----------------------------------------------------------------------
            //-----Background-----
            Game1.graphics.GraphicsDevice.SetRenderTarget(renderScreen);
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, outline); //-----[Shader]-----Outline
            //Background3
            outline.Parameters["lineSize"].SetValue(10);
            outline.Parameters["lineBrightness"].SetValue(4);
            spriteBatch.Draw(renderBackground3, Vector2.Zero, Color.White);
            //Background2
            outline.Parameters["lineSize"].SetValue(15);
            outline.Parameters["lineBrightness"].SetValue(3);
            spriteBatch.Draw(renderBackground2, Vector2.Zero, Color.White);
            //Background1
            outline.Parameters["lineSize"].SetValue(20);
            outline.Parameters["lineBrightness"].SetValue(2);
            spriteBatch.Draw(renderBackground1, Vector2.Zero, Color.White);
            //Background0
            outline.Parameters["lineSize"].SetValue(20);
            outline.Parameters["lineBrightness"].SetValue(0);
            spriteBatch.Draw(renderBackground0, Vector2.Zero, Color.White);
            spriteBatch.End();
            //-----Foreground-----
            if (princess.beating || spieler.smash) //-----[Shader]-----Smash
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, smash);
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            }
            spriteBatch.Draw(renderForeground, Vector2.Zero, Color.White);
            spriteBatch.End();

            //----------------------------------------------------------------------
            //----------------------------------------Draw to Screen
            //----------------------------------------------------------------------
            Game1.graphics.GraphicsDevice.SetRenderTarget(null);
            Game1.graphics.GraphicsDevice.Clear(Color.Black);
            
            //-----renderTarget-----
            if (princess.coverEyes) //-----[Shader]-----CoverEyes
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, coverEyes, camera.screenTransform);
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.screenTransform);
            }
            spriteBatch.Draw(renderScreen, new Vector2(), Color.White);
            spriteBatch.End();

            //-----HUD-----
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.screenTransform);
            spriteBatch.Draw(renderHud, new Vector2(), Color.White);
            spriteBatch.End();
            
        }
    }
}
