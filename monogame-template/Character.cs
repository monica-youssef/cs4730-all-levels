//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Audio;

//namespace monogame_template
//{
//    class Character
//    {
//        Texture2D texture;

//        Vector2 position;
//        public Vector2 velocity;
//        public Texture2D kirby;
//        public Texture2D kirby_idle_right;
//        public Texture2D kirby_idle_left;
//        public Texture2D kirby_walking_right;
//        public Texture2D kirby_walking_left;
//        public Texture2D kirby_jumping_right;
//        public Texture2D kirby_jumping_left;
//        public Texture2D kirby_screaming;
//        public Texture2D kirby_scream_right;
//        public Texture2D kirby_scream_left;

//        public bool hasJumped;

//        public Rectangle rectangle;

//        public Character(Texture2D newTexture, Vector2 newPosition)
//        {
//            texture = newTexture;
//            position = newPosition;
//            hasJumped = true;            
//        }

//        public void Update(GameTime gameTime)
//        {
//            position += velocity;
//            rectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);


//            if (Keyboard.GetState().IsKeyDown(Keys.Right)) velocity.X = 3f;
//            else if(Keyboard.GetState().IsKeyDown(Keys.Left)) velocity.X = -3f; else velocity.X = 0f;

//            if (Keyboard.GetState().IsKeyDown(Keys.Space) && hasJumped == false)
//            {
//                position.Y -= 10f;
//                velocity.Y = -5f;
//                hasJumped = true;
//            }

//                float i = 1;
//                velocity.Y += 0.15f * i;

//        }

//        public void Draw(SpriteBatch spriteBatch)
//        {
//            spriteBatch.Draw(texture, rectangle, Color.White);
//        }
//    }
//}
