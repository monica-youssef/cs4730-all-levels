using System;
using System.Linq;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using TiledCS;
using System.Collections.Generic;



// https://www.youtube.com/watch?v=ZLxIShw-7ac
// https://www.youtube.com/watch?v=MMO70m4R2eE
// https://github.com/MonoGame/MonoGame.Samples/blob/3.8.1/Platformer2D/Platformer2D.Core/Game/Player.cs
// https://www.youtube.com/watch?v=CV8P9aq2gQo
// https://www.youtube.com/watch?v=ZLxIShw-7ac
// https://www.youtube.com/watch?v=TlHSNjeND9s


namespace monogame_template;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public Texture2D kirby;
    public Texture2D kirby_idle_right;
    public Texture2D kirby_idle_left;
    public Texture2D kirby_walking_right;
    public Texture2D kirby_walking_left;
    public Texture2D kirby_jumping_right;
    public Texture2D kirby_jumping_left;
    public Texture2D kirby_screaming;
    public Texture2D kirby_scream_right;
    public Texture2D kirby_scream_left;
    public Texture2D passable;
    public Texture2D notpassable;
    public Texture2D box;
    public Texture2D coin;
    public Texture2D heart;
    public Texture2D gem;

    // create a layer to check for ground
    private TiledLayer groundLayer;
    private TiledLayer exitLayer;
    private TiledLayer coinLayer;

    private Rectangle[] standingAnimation = new Rectangle[4];
    private Rectangle[] walkingAnimation = new Rectangle[9];
    private Rectangle[] jumpingAnimation = new Rectangle[9];
    private Rectangle[] longScreamAnimation = new Rectangle[4];
    private Rectangle[] passableRect = new Rectangle[1];
    private Rectangle[] notpassableRect = new Rectangle[1];
    private Rectangle[] boxRect = new Rectangle[1];


    int animSpeed;
    int animationFrame;

    bool onPlatform = false;
    bool hitGround;

    private float timer = 0f;
    private int threshold = 150;
    private int previousAnimationIndex = 0;
    private int currentAnimationIndex = 1;

    bool hasJumped;
    bool hasScreamed;
    int totalCoins = 0;

    int playerDirection;

    SpriteEffects effect = SpriteEffects.FlipHorizontally;

    public Vector2 playerPosition = new Vector2(0, 428);
    public Vector2 playerVelocity;

    public Vector2 boxPosition = new Vector2(335, 100);
    public Vector2 boxVelocity;

    public static bool IsTouchingLeft(Rectangle r1, Rectangle r2, Vector2 vel)
    {
        return r1.Right + vel.X > r2.Left &&
          r1.Left < r2.Left &&
          r1.Bottom > r2.Top &&
          r1.Top < r2.Bottom;
    }
    public static bool IsTouchingRight(Rectangle r1, Rectangle r2, Vector2 vel)
    {
        return r1.Left + vel.X < r2.Right &&
          r1.Right > r2.Right &&
          r1.Bottom > r2.Top &&
          r1.Top < r2.Bottom;
    }

    public static bool IsTouchingTop(Rectangle r1, Rectangle r2, Vector2 vel)
    {
        return r1.Bottom + vel.Y > r2.Top &&
          r1.Top < r2.Top &&
          r1.Right > r2.Left &&
          r1.Left < r2.Right;
    }

    public static bool IsTouchingBottom(Rectangle r1, Rectangle r2, Vector2 vel)
    {
        return r1.Top + vel.Y < r2.Bottom &&
          r1.Bottom > r2.Bottom &&
          r1.Right > r2.Left &&
          r1.Left < r2.Right;
    }

    private Point GameBounds = new Point(640, 360); //window resolution

    // TILED
    private TiledMap map;
    private Dictionary<int, TiledTileset> tilesets;
    private Texture2D tilesetTexture;

    [Flags]
    enum Trans
    {
        None = 0,
        Flip_H = 1 << 0,
        Flip_V = 1 << 1,
        Flip_D = 1 << 2,

        Rotate_90 = Flip_D | Flip_H,
        Rotate_180 = Flip_H | Flip_V,
        Rotate_270 = Flip_V | Flip_D,

        Rotate_90AndFlip_H = Flip_H | Flip_V | Flip_D,
    }

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

    }

    protected override void Initialize()
    {

        base.Initialize();
        hasJumped = true;
        playerDirection = 2;
        onPlatform = false;


    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        kirby = Content.Load<Texture2D>("kirby_sprite_sheet");
        kirby_idle_right = Content.Load<Texture2D>("kirby_idle_right");
        kirby_idle_left = Content.Load<Texture2D>("kirby_idle_left");
        kirby_walking_right = Content.Load<Texture2D>("kirby_walking_right");
        kirby_walking_left = Content.Load<Texture2D>("kirby_walking_left");
        kirby_jumping_right = Content.Load<Texture2D>("kirby_jumping_right");
        kirby_jumping_left = Content.Load<Texture2D>("kirby_jumping_left");
        kirby_scream_right = Content.Load<Texture2D>("kirby_long_scream");
        kirby_scream_left = Content.Load<Texture2D>("kirby_long_scream_left");
        passable = Content.Load<Texture2D>("passable2");
        notpassable = Content.Load<Texture2D>("notpassable2");
        box = Content.Load<Texture2D>("box2");
        coin = Content.Load<Texture2D>("coin");
        heart = Content.Load<Texture2D>("heart");
        gem = Content.Load<Texture2D>("gem");


        timer = 0;
        animSpeed = 200;
        animationFrame = 0;

        //idle animation
        for (int i = 0; i < 4; i++)
        {
            standingAnimation[i] = new Rectangle(i * 28, 0, 27, 27);
        }

        //walking animation
        for (int i = 0; i < 9; i++)
        {
            walkingAnimation[i] = new Rectangle(i * 24, 0, 22, 23);
        }

        //jumping animation
        for (int i = 0; i < 9; i++)
        {
            jumpingAnimation[i] = new Rectangle(i * 23, 0, 23, 24);
        }

        //screaming animation
        for (int i = 0; i < 4; i++)
        {
            longScreamAnimation[i] = new Rectangle(i * 25, 0, 24, 26);
        }

        //passable rectangle
        passableRect[0] = new Rectangle(0, 0, 112, 16);

        //not passable rectangle
        notpassableRect[0] = new Rectangle(0, 0, 64, 16);

        //box rectangle
        boxRect[0] = new Rectangle(0, 0, 32, 32);

        //tiled info
        map = new TiledMap(Content.RootDirectory + "/map4.tmx");
        tilesets = map.GetTiledTilesets(Content.RootDirectory + "/");
        tilesetTexture = Content.Load<Texture2D>("merge_signs");
        groundLayer = map.Layers.First(l => l.name == "Ground");
        exitLayer = map.Layers.First(l => l.name == "Exit");
        coinLayer = map.Layers.First(l => l.name == "Coin");

        Reset();
    }


    protected override void Update(GameTime gameTime)
    {
        Rectangle passableRectCol = new Rectangle(300, 400, 96, 4);
        Rectangle notpassableRectCol = new Rectangle(700, 386, notpassable.Width, notpassable.Height);
        Rectangle kirbyRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, 15, 28);
        Rectangle boxRectCol = new Rectangle((int)boxPosition.X, (int)boxPosition.Y, box.Width, box.Height);


        playerPosition += playerVelocity;
        boxPosition += boxVelocity;

        // box has gravity pulling it down
        float n = 1;
        boxVelocity.Y += 0.3f * n;

        base.Update(gameTime);
        var kstate = Keyboard.GetState();

        // TILED
        hitGround = false;
        foreach (var obj in groundLayer.objects)
        {
            var objRect = new Rectangle((int)obj.x, (int)obj.y, (int)obj.width, (int)obj.height);
            // can access as either a Rectangle or the direct obj calls
            bool xoverlap = (playerPosition.X < objRect.Right) && (playerPosition.X + 24 > objRect.Left);
            // a little wiggle room in these calcs in case the player is falling fast enough to skip through
            bool yoverlap = (playerPosition.Y + 24 - obj.y < 2) && (playerPosition.Y + 24 - obj.y > -2);

            if (xoverlap && yoverlap)
            {
                hitGround = true;
                // once a collision has been detected, no need to check the other objects
                break;
            }
        }

        //EXIT CONDITION
        foreach (var obj in exitLayer.objects)
        {
            var objRect = new Rectangle((int)obj.x, (int)obj.y, (int)obj.width, (int)obj.height);
            bool xoverlap = (playerPosition.X < objRect.Right) && (playerPosition.X > objRect.Left);
            bool yoverlap = (playerPosition.Y > objRect.Top) && (playerPosition.Y < objRect.Bottom);

            if (xoverlap && yoverlap)
            {
                Exit();
            }
        }

        //COIN LAYERS
        foreach (TiledObject obj in coinLayer.objects)
        {
            
            var objRect = new Rectangle((int)obj.x, (int)obj.y, (int)obj.width, (int)obj.height);
            bool xoverlap = (playerPosition.X < objRect.Right) && (playerPosition.X > objRect.Left);
            bool yoverlap = (playerPosition.Y > objRect.Top) && (playerPosition.Y < objRect.Bottom);

            if (xoverlap && yoverlap)
            {
                obj.x = 800;
                obj.y = 800;
                totalCoins += 1;
                break;
            }
        }

        // BOX COLLISION

        //box sits on platform
        if (IsTouchingTop(boxRectCol, passableRectCol, boxVelocity))
        {
            boxVelocity.Y = 0f;

        }

        // when you are on the right side of the box
        if (IsTouchingLeft(boxRectCol, kirbyRect, boxVelocity))
        {
            boxVelocity.X -= 2f;
        }
        if (boxPosition.X + 100 > playerPosition.X && hasScreamed && playerDirection == 1)
        {
            boxVelocity.X -= 4f;
        }


        // when you are on the left side of the box
        if (IsTouchingRight(boxRectCol, kirbyRect, boxVelocity))
        {
                boxVelocity.X += 2f;
        }
        if (boxPosition.X - 100 < playerPosition.X && hasScreamed && playerDirection == 2)
        {
            boxVelocity.X += 4f;
        }



        //box stops on the ground
        if (boxPosition.Y >= 420)
            boxVelocity.Y = 0f;



        // MOVEMENT

        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            playerVelocity.X = 3f;
            playerDirection = 2;
        }

        else if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            playerDirection = 1;
            playerVelocity.X = -3f;
        }

        else playerVelocity.X = 0f;


        // JUMPING

        if ((Keyboard.GetState().IsKeyDown(Keys.W)) && hasJumped == false)
        {
            playerPosition.Y -= 10f;
            playerVelocity.Y = -5f;
            hasJumped = true;
        }

        if (hasJumped == true)
        {
            float i = 1;
            playerVelocity.Y += 0.15f * i;
        }

        if (playerPosition.Y >= 428)
        {
            hasJumped = false;
            hitGround = true;
        }

        if (hitGround == true)
        {
            playerVelocity.Y = 0f;
            hasJumped = false;
        }
        if (hitGround == false)
        {
            hasJumped = true;

        }

        if (Keyboard.GetState().IsKeyDown(Keys.Space))
        {
            hasScreamed = true;
        }   

        if (playerPosition.Y >= 428)
            hasJumped = false;

        if (hasJumped == false)
            playerVelocity.Y = 0f;
        else
            playerDirection = 0;



        // TIMER

        if (timer > threshold)
        {
            if (currentAnimationIndex == standingAnimation.Length - 1)
            {
                currentAnimationIndex = 0;
                hasScreamed = false;
            }
            else
            {
                currentAnimationIndex += 1;
            }
            timer = 0;
        }
        else
        {
            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        }

        base.Update(gameTime);
    }



    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        // TILED MAP
        var tileLayers = map.Layers.Where(x => x.type == TiledLayerType.TileLayer);
        foreach (var layer in tileLayers)
        {
            for (var y = 0; y < layer.height; y++)
            {
                for (var x = 0; x < layer.width; x++)
                {
                    // Assuming the default render order is used which is from right to bottom
                    var index = (y * layer.width) + x;
                    var gid = layer.data[index]; // The tileset tile index
                    var tileX = x * map.TileWidth;
                    var tileY = y * map.TileHeight;

                    // Gid 0 is used to tell there is no tile set
                    if (gid == 0)
                    {
                        continue;
                    }

                    // Helper method to fetch the right TieldMapTileset instance
                    // This is a connection object Tiled uses for linking the correct tileset to the 
                    // gid value using the firstgid property
                    var mapTileset = map.GetTiledMapTileset(gid);

                    // Retrieve the actual tileset based on the firstgid property of the connection object 
                    // we retrieved just now
                    var tileset = tilesets[mapTileset.firstgid];

                    // Use the connection object as well as the tileset to figure out the source rectangle
                    var rect = map.GetSourceRect(mapTileset, tileset, gid);

                    // Create destination and source rectangles
                    var source = new Rectangle(rect.x, rect.y, rect.width, rect.height);
                    var destination = new Rectangle(tileX, tileY, map.TileWidth, map.TileHeight);

                    // You can use the helper methods to get information to handle flips and rotations
                    Trans tileTrans = Trans.None;
                    if (map.IsTileFlippedHorizontal(layer, x, y)) tileTrans |= Trans.Flip_H;
                    if (map.IsTileFlippedVertical(layer, x, y)) tileTrans |= Trans.Flip_V;
                    if (map.IsTileFlippedDiagonal(layer, x, y)) tileTrans |= Trans.Flip_D;

                    SpriteEffects effects = SpriteEffects.None;
                    double rotation = 0f;
                    switch (tileTrans)
                    {
                        case Trans.Flip_H: effects = SpriteEffects.FlipHorizontally; break;
                        case Trans.Flip_V: effects = SpriteEffects.FlipVertically; break;

                        case Trans.Rotate_90:
                            rotation = Math.PI * .5f;
                            destination.X += map.TileWidth;
                            break;

                        case Trans.Rotate_180:
                            rotation = Math.PI;
                            destination.X += map.TileWidth;
                            destination.Y += map.TileHeight;
                            break;

                        case Trans.Rotate_270:
                            rotation = Math.PI * 3 / 2;
                            destination.Y += map.TileHeight;
                            break;

                        case Trans.Rotate_90AndFlip_H:
                            effects = SpriteEffects.FlipHorizontally;
                            rotation = Math.PI * .5f;
                            destination.X += map.TileWidth;
                            break;

                        default:
                            break;
                    }

                    // Render sprite at position tileX, tileY using the rect
                    _spriteBatch.Draw(tilesetTexture, destination, source, Color.White,
                        (float)rotation, Vector2.Zero, effects, 0);
                }
            }
        }

        //draw box
        _spriteBatch.Draw(box, boxPosition, boxRect[0], Color.White);

        //coin total
        if (totalCoins >= 1)
            _spriteBatch.Draw(coin, new Vector2(0, 0), Color.White);
        if (totalCoins >= 2)
            _spriteBatch.Draw(heart, new Vector2(40, 0), Color.White);
        if (totalCoins >= 3)
            _spriteBatch.Draw(gem, new Vector2(80, 0), Color.White);

        //kirby is walking left
        if (playerDirection == 1 && Keyboard.GetState().IsKeyDown(Keys.A))
            _spriteBatch.Draw(kirby_walking_left, playerPosition, walkingAnimation[currentAnimationIndex], Color.White);

        //kirby is walking right
        else if (playerDirection == 2 && Keyboard.GetState().IsKeyDown(Keys.D))
            _spriteBatch.Draw(kirby_walking_right, playerPosition, walkingAnimation[currentAnimationIndex], Color.White);

        //kirby screaming to the right
        else if (playerDirection == 1 && hasScreamed)
            _spriteBatch.Draw(kirby_scream_left, playerPosition, longScreamAnimation[currentAnimationIndex], Color.White);
        else if (hasScreamed)
            _spriteBatch.Draw(kirby_scream_right, playerPosition, longScreamAnimation[currentAnimationIndex], Color.White);
        //kirby is jumping 
        else if (hasJumped)
            _spriteBatch.Draw(kirby_jumping_right, playerPosition, jumpingAnimation[currentAnimationIndex], Color.White);

        //kirby is idle left
        else if (playerDirection == 1 && playerVelocity.X == 0f && (Keyboard.GetState().IsKeyUp(Keys.A) || Keyboard.GetState().IsKeyUp(Keys.W)))
            _spriteBatch.Draw(kirby_idle_left, playerPosition, standingAnimation[currentAnimationIndex], Color.White);

        //kirby is idle right
        else if (playerDirection == 2 &&  playerVelocity.X == 0f && (Keyboard.GetState().IsKeyUp(Keys.D) || Keyboard.GetState().IsKeyUp(Keys.W)))
            _spriteBatch.Draw(kirby_idle_right, playerPosition, standingAnimation[currentAnimationIndex], Color.White);

        else
            _spriteBatch.Draw(kirby_idle_right, playerPosition, standingAnimation[currentAnimationIndex], Color.White);

        base.Draw(gameTime);
        _spriteBatch.End();
    }

    private void Reset()
    {
        if (kirby_idle_right == null)
        {   //create texture to draw with if it does not exist
            kirby_idle_right = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            kirby_idle_right.SetData<Color>(new Color[] { Color.White });
        }
    }

}

static class platformHelper
{
    const int penetrationMargin = 1;
    public static bool isOnTopOf(this Rectangle r1, Rectangle r2)
    {
        return (r1.Bottom >= r2.Top - penetrationMargin &&
            r1.Bottom <= r2.Top + 1 &&
            r1.Right >= r2.Left + 5 &&
            r1.Left <= r2.Right - 5
            );
    }
}



