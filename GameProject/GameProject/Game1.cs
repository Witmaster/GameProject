using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GameProject
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // game objects. Using inheritance would make this
        // easier, but inheritance isn't a GDD 1200 topic
        Burger burger;
        List<TeddyBear> bears = new List<TeddyBear>();
        static List<Projectile> projectiles = new List<Projectile>();
        List<Explosion> explosions = new List<Explosion>();

        // projectile and explosion sprites. Saved so they don't have to
        // be loaded every time projectiles or explosions are created
        static Texture2D frenchFriesSprite;
        static Texture2D teddyBearProjectileSprite;
        static Texture2D explosionSpriteStrip;

        // scoring support
        int score = 0;
        string scoreString = GameConstants.SCORE_PREFIX + 0;
                
        // health support
        string healthString = GameConstants.HEALTH_PREFIX + 
            GameConstants.BURGER_INITIAL_HEALTH;
        bool burgerDead = false;
        // text display support
        static SpriteFont font;

        // sound effects
        SoundEffect burgerDamage;
        SoundEffect burgerDeath;
        SoundEffect burgerShot;
        SoundEffect explosion;
        SoundEffect teddyBounce;
        SoundEffect teddyShot;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set resolution
            graphics.PreferredBackBufferWidth = GameConstants.WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = GameConstants.WINDOW_HEIGHT;
           // IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            RandomNumberGenerator.Initialize();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load audio content
            burgerDamage=Content.Load<SoundEffect>("burgerdamage");
            burgerDeath=Content.Load<SoundEffect>("burgerdeath");
            burgerShot = Content.Load<SoundEffect>("burgershot");
            explosion = Content.Load<SoundEffect>("explosionsound");
            teddyBounce = Content.Load<SoundEffect>("teddybounce");
            teddyShot = Content.Load<SoundEffect>("teddyshot");

            // load sprite font 
            font=Content.Load<SpriteFont>("Arial20");
            // load projectile and explosion sprites
            teddyBearProjectileSprite = Content.Load<Texture2D>("teddybearprojectile");
            frenchFriesSprite = Content.Load<Texture2D>("frenchfries");
            explosionSpriteStrip = Content.Load<Texture2D>("explosion");
            // add initial game objects
            burger = new Burger(Content, "burger", graphics.PreferredBackBufferWidth / 2, (int)(graphics.PreferredBackBufferHeight*0.875), burgerShot); 
            
            for (; bears.Count < GameConstants.MAX_BEARS; )
            {
                SpawnBear();
            }
            // set initial health and score strings
            healthString = GameConstants.HEALTH_PREFIX + burger.Health.ToString();
            scoreString = GameConstants.SCORE_PREFIX + score.ToString();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // get current mouse state and update burger
            burger.Update(gameTime, Keyboard.GetState());
            // update other game objects
            foreach (TeddyBear bear in bears)
            {
                bear.Update(gameTime);
            }
            foreach (Projectile projectile in projectiles)
            {
                projectile.Update(gameTime);
            }
            foreach (Explosion explosion in explosions)
            {
                explosion.Update(gameTime);
            }

            // check and resolve collisions between teddy bears
            for (int bearOne = 0; bearOne < bears.Count;bearOne++ )
            {
                for (int bearTwo=bearOne+1; bearTwo<bears.Count;bearTwo++)
                {
                    if (bears[bearOne].Active&&bears[bearTwo].Active)
                    {
                        CollisionResolutionInfo resolution = CollisionUtils.CheckCollision(
                            gameTime.ElapsedGameTime.Milliseconds,
                            GameConstants.WINDOW_WIDTH,
                            GameConstants.WINDOW_HEIGHT,
                            bears[bearOne].Velocity,
                            bears[bearOne].DrawRectangle,
                            bears[bearTwo].Velocity,
                            bears[bearTwo].DrawRectangle);
                        if (resolution!=null)
                        {
                            if (resolution.FirstOutOfBounds)
                            {
                                bears[bearOne].Active = false;
                            } 
                            else
                            {
                                bears[bearOne].DrawRectangle = resolution.FirstDrawRectangle;
                                bears[bearOne].Velocity = resolution.FirstVelocity;
                                teddyBounce.Play();
                            }
                            if (resolution.SecondOutOfBounds)
                            {
                                bears[bearTwo].Active = false;
                            }
                            else
                            {
                                bears[bearTwo].DrawRectangle = resolution.SecondDrawRectangle;
                                bears[bearTwo].Velocity = resolution.SecondVelocity;
                                teddyBounce.Play();
                            }
                        }
                    }
                }
            }
                // check and resolve collisions between burger and teddy bears
            foreach(TeddyBear teddy in bears)
            {
                if (teddy.Active)
                    if(teddy.CollisionRectangle.Intersects(burger.CollisionRectangle))
                    {
                        explosions.Add(new Explosion(explosionSpriteStrip,
                            teddy.DrawRectangle.Center.X,
                            teddy.DrawRectangle.Center.Y));
                        explosion.Play();
                        burgerDamage.Play();
                        teddy.Active = false;
                        burger.Health = burger.Health - GameConstants.BEAR_DAMAGE;
                        healthString = GameConstants.HEALTH_PREFIX + burger.Health.ToString();
                        CheckBurgerKill();
                    }
                
            }
                // check and resolve collisions between burger and projectiles
            foreach(Projectile shot in projectiles)
            {
                if(shot.Active&&shot.Type==ProjectileType.TeddyBear)
                    if(shot.CollisionRectangle.Intersects(burger.CollisionRectangle))
                    {
                        shot.Active = false;
                        burger.Health -= GameConstants.TEDDY_BEAR_PROJECTILE_DAMAGE;
                        healthString = GameConstants.HEALTH_PREFIX + burger.Health.ToString();
                        burgerDamage.Play();
                        CheckBurgerKill();
                    }
            }
                // check and resolve collisions between teddy bears and projectiles
                foreach (TeddyBear teddy in bears)
                {
                    if (teddy.Active)
                    {
                        foreach (Projectile shot in projectiles)
                        {
                            if (shot.Type == ProjectileType.FrenchFries && shot.Active)
                            {
                                if (teddy.CollisionRectangle.Intersects(shot.CollisionRectangle))
                                {
                                    explosions.Add(new Explosion(explosionSpriteStrip,
                                      teddy.DrawRectangle.Center.X,
                                    teddy.DrawRectangle.Center.Y));
                                    score += GameConstants.BEAR_POINTS;
                                    scoreString = GameConstants.SCORE_PREFIX + score.ToString();
                                    explosion.Play();
                                    teddy.Active = false;
                                    shot.Active = false;
                                }
                            }
                        }
                    }
                }
            // clean out inactive teddy bears and add new ones as necessary
            for (int i = bears.Count - 1; i >= 0; i--)
            {
                if (!bears[i].Active)
                {
                    bears.RemoveAt(i);
                    //we only care to have all five bears up
                    //according to what is said in assingment so...
                    while(bears.Count<GameConstants.MAX_BEARS)
                    {
                        SpawnBear();
                    }
                }
            }
            // clean out inactive projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--) 
            {
                if (!projectiles[i].Active)
                {
                    projectiles.RemoveAt(i);
                }
            }
            // clean out finished explosions
            for (int i = explosions.Count - 1; i >= 0; i--) 
            {
                if (explosions[i].Finished)
                {
                    explosions.RemoveAt(i);
                }
            }
                base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // draw game objects
            burger.Draw(spriteBatch);
            foreach (TeddyBear bear in bears)
            {
                bear.Draw(spriteBatch);
            }
            foreach (Projectile projectile in projectiles)
            {
                projectile.Draw(spriteBatch);
            }
            foreach (Explosion explosion in explosions)
            {
                explosion.Draw(spriteBatch);
            }            
            // draw score and health
            spriteBatch.DrawString(font, healthString, GameConstants.HEALTH_LOCATION, Color.White);
            spriteBatch.DrawString(font, scoreString, GameConstants.SCORE_LOCATION, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Public methods

        /// <summary>
        /// Gets the projectile sprite for the given projectile type
        /// </summary>
        /// <param name="type">the projectile type</param>
        /// <returns>the projectile sprite for the type</returns>
        public static Texture2D GetProjectileSprite(ProjectileType type)
        {
            // replace with code to return correct projectile sprite based on projectile type
            switch(type)
            { 
                case ProjectileType.FrenchFries:
                    { 
                        return frenchFriesSprite;
                        break;
                    }
                case ProjectileType.TeddyBear:
                    { 
                        return teddyBearProjectileSprite;
                        break;
                    }
                default:
                    {
                        return null;
                        break;
                    }
            }
        }

        /// <summary>
        /// Adds the given projectile to the game
        /// </summary>
        /// <param name="projectile">the projectile to add</param>
        public static void AddProjectile(Projectile projectile)
        {
            projectiles.Add(projectile);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Spawns a new teddy bear at a random location
        /// </summary>
        private void SpawnBear()
        {
            // generate random location
            int spawnX = GetRandomLocation(GameConstants.SPAWN_BORDER_SIZE,
                graphics.PreferredBackBufferWidth - GameConstants.SPAWN_BORDER_SIZE*2);
            int spawnY = GetRandomLocation(GameConstants.SPAWN_BORDER_SIZE,
                graphics.PreferredBackBufferHeight - GameConstants.SPAWN_BORDER_SIZE*2);
            // generate random velocity
            float speed = RandomNumberGenerator.NextFloat(0.2f)+0.1f;
            float angle = RandomNumberGenerator.NextFloat((float)Math.PI * 2);
            Vector2 velocity = new Vector2(
                speed*(float)Math.Cos(angle),
                speed*(float)Math.Sin(angle)
                );
            // create new bear
            TeddyBear newBear = new TeddyBear(Content, "teddybear", spawnX, spawnY, velocity,teddyBounce,teddyShot);
            // make sure we don't spawn into a collision
            foreach(Projectile shot in projectiles)
            {
                if(shot.CollisionRectangle.Intersects(newBear.CollisionRectangle))
                {
                    return;
                }
            }
            foreach(TeddyBear teddy in bears)
            {
                if (teddy.CollisionRectangle.Intersects(newBear.CollisionRectangle))
                    {
                        return;
                    }
            }
                if (burger.CollisionRectangle.Intersects(newBear.CollisionRectangle))
                {
                    return;
                }
            // add new bear to list
            bears.Add(newBear);

        }

        /// <summary>
        /// Gets a random location using the given min and range
        /// </summary>
        /// <param name="min">the minimum</param>
        /// <param name="range">the range</param>
        /// <returns>the random location</returns>
        private int GetRandomLocation(int min, int range)
        {
            return min + RandomNumberGenerator.Next(range);
        }

        /// <summary>
        /// Gets a list of collision rectangles for all the objects in the game world
        /// </summary>
        /// <returns>the list of collision rectangles</returns>
        private List<Rectangle> GetCollisionRectangles()
        {
            List<Rectangle> collisionRectangles = new List<Rectangle>();
            collisionRectangles.Add(burger.CollisionRectangle);
            foreach (TeddyBear bear in bears)
            {
                collisionRectangles.Add(bear.CollisionRectangle);
            }
            foreach (Projectile projectile in projectiles)
            {
                collisionRectangles.Add(projectile.CollisionRectangle);
            }
            foreach (Explosion explosion in explosions)
            {
                collisionRectangles.Add(explosion.CollisionRectangle);
            }
            return collisionRectangles;
        }

        /// <summary>
        /// Checks to see if the burger has just been killed
        /// </summary>
        private void CheckBurgerKill()
        {
          if (burger.Health==0&&!burgerDead)
          {
              burgerDead = true;
              burgerDeath.Play();
          }
        }

        #endregion
    }
}
