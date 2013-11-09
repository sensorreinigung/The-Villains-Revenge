﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TheVillainsRevenge
{
     class Map
    {
        //Lade Textur
        Texture2D mapTexture;
        Texture2D background;
        public Vector2 size = new Vector2(1920 * 2, 1080 * 2);
        //Erstelle Blocks als List
        public List<Block> blocks = new List<Block>();
        public void Load(ContentManager Content)
        {
            //Lade Textur, einmal ausgeführt
            mapTexture = Content.Load<Texture2D>("sprites/tiles");
            background = Content.Load<Texture2D>("sprites/background");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //Gehe alle Blöcke durch
            spriteBatch.Draw(background, Vector2.Zero, new Rectangle(0, 0, (int)size.X, (int)size.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1.0f);
            foreach (Block block in blocks)
            {
                //Zeichne die Blöcke anhand der Daten der Blöcke
                spriteBatch.Draw(mapTexture, block.pos, block.cuttexture, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
            }
        }

        public void Generate()
        {
            //generiere Das Level (erzeuge neue Objekte in der List)
            //Iteration
            for (int i = 0; i <= 20; i++)
            {
                blocks.Add(new Block(new Vector2(i*48, Game1.resolution.Y-48), "block"));
            }
        }
    }
}
