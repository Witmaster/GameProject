using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameProject
{
    /// <summary>
    /// A class for a burger
    /// </summary>
    public class Burger
    {
        #region Fields

        // graphic and drawing info
        Texture2D sprite;
        Rectangle drawRectangle;

        // burger stats
        int health = 100;

        // shooting support
        bool canShoot = true;
        int elapsedCooldownTime = 0;

        // sound effect
        SoundEffect shootSound;


        #endregion

        #region Constructors

        /// <summary>
        ///  Constructs a burger
        /// </summary>
        /// <param name="contentManager">the content manager for loading content</param>
        /// <param name="spriteName">the sprite name</param>
        /// <param name="x">the x location of the center of the burger</param>
        /// <param name="y">the y location of the center of the burger</param>
        /// <param name="shootSound">the sound the burger plays when shooting</param>
        public Burger(ContentManager contentManager, string spriteName, int x, int y,
            SoundEffect shootSound)
        {
            LoadContent(contentManager, spriteName, x, y);
            this.shootSound = shootSound;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collision rectangle for the burger
        /// </summary>
        public Rectangle CollisionRectangle
        {
            get { return drawRectangle; }
        }

        public int Health
        {
            get {return health;}
            set { if (value < 101 && value >= 0) health = value; else if (value < 0) health = 0; }
        }
        #endregion

        #region Private properties

        /// <summary>
        /// Gets and sets the x location of the center of the burger
        /// </summary>
        private int X
        {
            get { return drawRectangle.Center.X; }
            set
            {
                drawRectangle.X = value - drawRectangle.Width / 2;

                // clamp to keep in range
                if (drawRectangle.X < 0)
                {
                    drawRectangle.X = 0;
                }
                else if (drawRectangle.X > GameConstants.WINDOW_WIDTH - drawRectangle.Width)
                {
                    drawRectangle.X = GameConstants.WINDOW_WIDTH - drawRectangle.Width;
                }
            }
        }

        /// <summary>
        /// Gets and sets the y location of the center of the burger
        /// </summary>
        private int Y
        {
            get { return drawRectangle.Center.Y; }
            set
            {
                drawRectangle.Y = value - drawRectangle.Height / 2;

                // clamp to keep in range
                if (drawRectangle.Y < 0)
                {
                    drawRectangle.Y = 0;
                }
                else if (drawRectangle.Y > GameConstants.WINDOW_HEIGHT - drawRectangle.Height)
                {
                    drawRectangle.Y = GameConstants.WINDOW_HEIGHT - drawRectangle.Height;
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Updates the burger's location based on mouse. Also fires 
        /// french fries as appropriate
        /// </summary>
        /// <param name="gameTime">game time</param>
        /// <param name="mouse">the current state of the mouse</param>
        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            // burger should only respond to input if it still has health
            if (this.health>0)
            {
                // move burger using mouse
                if (keyboard.IsKeyDown(Keys.W))
                    this.drawRectangle.Y -= GameConstants.BURGER_MOVEMENT_AMOUNT;
                if (keyboard.IsKeyDown(Keys.S))
                    this.drawRectangle.Y += GameConstants.BURGER_MOVEMENT_AMOUNT;
                if (keyboard.IsKeyDown(Keys.A))
                    this.drawRectangle.X -= GameConstants.BURGER_MOVEMENT_AMOUNT;
                if (keyboard.IsKeyDown(Keys.D))
                    this.drawRectangle.X += GameConstants.BURGER_MOVEMENT_AMOUNT;
                // clamp burger in window
                if (this.drawRectangle.X<0)
                { this.drawRectangle.X = 0; }
                if (this.drawRectangle.X+this.drawRectangle.Width>GameConstants.WINDOW_WIDTH)
                { this.drawRectangle.X = GameConstants.WINDOW_WIDTH - this.drawRectangle.Width; }
                if (this.drawRectangle.Y<0)
                { this.drawRectangle.Y = 0; }
                if (this.drawRectangle.Y+this.drawRectangle.Height>GameConstants.WINDOW_HEIGHT)
                {this.drawRectangle.Y=GameConstants.WINDOW_HEIGHT-this.drawRectangle.Height;}
                // update shooting allowed
                if (canShoot == false)
                {
                    elapsedCooldownTime += gameTime.ElapsedGameTime.Milliseconds;
                    if (elapsedCooldownTime>=GameConstants.BURGER_COOLDOWN_MILLISECONDS||
                        keyboard.IsKeyUp(Keys.Space))
                    {
                        canShoot =true;
                        elapsedCooldownTime = 0;
                    }
                }
                // timer concept (for animations) introduced in Chapter 7

                // shoot if appropriate
                if (keyboard.IsKeyDown(Keys.Space)&&canShoot==true)
                {
                    canShoot = false;
                    Game1.AddProjectile(new Projectile(ProjectileType.FrenchFries,
                        Game1.GetProjectileSprite(ProjectileType.FrenchFries),
                        this.drawRectangle.X + this.drawRectangle.Width/2,
                        this.drawRectangle.Y + this.drawRectangle.Height / 2-GameConstants.FRENCH_FRIES_PROJECTILE_OFFSET,
                        GameConstants.FRENCH_FRIES_PROJECTILE_SPEED));
                    this.shootSound.Play();
                }
            }
        }

        /// <summary>
        /// Draws the burger
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to use</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.sprite, this.drawRectangle, Color.White);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Loads the content for the burger
        /// </summary>
        /// <param name="contentManager">the content manager to use</param>
        /// <param name="spriteName">the name of the sprite for the burger</param>
        /// <param name="x">the x location of the center of the burger</param>
        /// <param name="y">the y location of the center of the burger</param>
        private void LoadContent(ContentManager contentManager, string spriteName,
            int x, int y)
        {
            // load content and set remainder of draw rectangle
            sprite = contentManager.Load<Texture2D>(spriteName);
            drawRectangle = new Rectangle(x - sprite.Width / 2,
                y - sprite.Height / 2, sprite.Width,
                sprite.Height);
        }

        #endregion
    }
}
