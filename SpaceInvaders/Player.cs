/*
 * Author: Shon Verch
 * File Name: Player.cs
 * Project Name: SpaceInvaders
 * Creation Date: 02/05/2019
 * Modified Date: 02/24/2019
 * Description: The player controller class.
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceInvaders.Engine;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace SpaceInvaders
{
    /// <summary>
    /// The player controller class.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// The amount of pixels to spawn above the horizontal boundary line.
        /// </summary>
        public const int VerticalSpawnOffset = 29;

        /// <summary>
        /// The amount of lives that a <see cref="Player"/> starts with.
        /// </summary>
        public const int DefaultLives = 3;

        /// <summary>
        /// The maximum amount of lives that a <see cref="Player"/> can have.
        /// </summary>
        public const int MaxLives = 4;

        /// <summary>
        /// The horizontal speed in pixels per second.
        /// </summary>
        private const int HorizontalSpeed = 200;

        /// <summary>
        /// The amount of time, in seconds, that the death animation lasts for.
        /// </summary>
        private const float DeathAnimationDuration = 1;

        /// <summary>
        /// The animation rate of the death animation, in seconds.
        /// </summary>
        private const float DeathAnimationRate = 0.10f;

        /// <summary>
        /// The score of this <see cref="Player"/>.
        /// </summary>
        public int Score { get; set; } = 0;

        /// <summary>
        /// The number of lives that this <see cref="Player"/> has remaining.
        /// </summary>
        public int Lives
        {
            get => lives;
            set
            {
                if (value == lives) return;
                int oldLives = lives;
                lives = MathHelper.Clamp(value, 0, MaxLives);

                // If lives increased, we don't want to start the death animation.
                if (oldLives <= lives) return;

                GameplayScreen gameplayScreen = MainGame.Context.GetGameScreen<GameplayScreen>(GameScreenType.Gameplay);
                if (lives > 0)
                {
                    gameplayScreen.Freeze(DeathAnimationDuration);
                }
                else
                {
                    gameplayScreen.TriggerGameover();
                }

                isDeathAnimation = true;
                deathAnimationFrame = false;
                deathAnimationTimer = DeathAnimationDuration;
                deathAnimationFrameTimer = DeathAnimationRate;
                playerDeathSoundEffectInstance.Play();
            }
        }

        /// <summary>
        /// The position of this <see cref="Player"/>.
        /// </summary>
        public Vector2 Position => boundingRectangle.Position;

        /// <summary>
        /// The texture of this <see cref="Player"/>.
        /// </summary>
        public Texture2D Texture { get; }

        /// <summary>
        /// The starting position of this <see cref="Player"/>.
        /// </summary>
        private Vector2 startingPosition;

        /// <summary>
        /// The maximum horizontal coordinate that the player cannot go beyond.
        /// </summary>
        private readonly float maxHorizontalCoordinate;

        /// <summary>
        /// The bounding <see cref="RectangleF"/> of this <see cref="Player"/>.
        /// </summary>
        private RectangleF boundingRectangle;

        private int lives = DefaultLives;

        private bool isDeathAnimation;
        private bool deathAnimationFrame;
        private float deathAnimationTimer;
        private float deathAnimationFrameTimer;

        private readonly SoundEffectInstance playerDeathSoundEffectInstance;

        /// <summary>
        /// Initializes a new <see cref="Player"/>.
        /// </summary>
        public Player()
        {
            Texture = MainGame.Context.MainTextureAtlas["player"];

            float positionY = GameplayScreen.HorizontalBoundaryY - Texture.Height * MainGame.ResolutionScale - VerticalSpawnOffset;
            startingPosition = new Vector2(0, positionY);
            boundingRectangle = new RectangleF(startingPosition.X, startingPosition.Y,
                Texture.Width * MainGame.ResolutionScale, Texture.Height * MainGame.ResolutionScale);

            maxHorizontalCoordinate = GameplayScreen.HorizontalBoundaryEnd.X - Texture.Width * MainGame.ResolutionScale;
            playerDeathSoundEffectInstance = MainGame.Context.Content.Load<SoundEffect>("Audio/explosion").CreateInstance();
            playerDeathSoundEffectInstance.Volume = 0.5f;
        }

        /// <summary>
        /// Initializes the horizontal position of this <see cref="Player"/>.
        /// <remarks>
        /// The <see cref="BarrierGroup"/> initialization requires the vertical position of this <see cref="Player"/>;
        /// however, this <see cref="Player"/> position initialization requires the horizontal positon of the <see cref="BarrierGroup"/>.
        /// Hence, we initialize the vertical position of this <see cref="Player"/>, then the <see cref="BarrierGroup"/>, and finally,
        /// we can initialize the horizontal position of this <see cref="Player"/>.
        /// </remarks>
        /// </summary>
        public void InitializeHorizontalPosition()
        {
            float centeringOffset = Texture.Width * MainGame.ResolutionScale * 0.5f;
            float positionX = MainGame.Context.GetGameScreen<GameplayScreen>(GameScreenType.Gameplay).BarrierGroup[0].Rectangle.Centre.X - centeringOffset;
            startingPosition.X = positionX;
            boundingRectangle.X = positionX;
        }

        /// <summary>
        /// Update the player.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            if (isDeathAnimation)
            {
                deathAnimationFrameTimer -= deltaTime;
                if (deathAnimationFrameTimer <= 0)
                {
                    deathAnimationFrameTimer = DeathAnimationRate;
                    deathAnimationFrame = !deathAnimationFrame;
                }

                deathAnimationTimer -= deltaTime;
                if (deathAnimationTimer <= 0)
                {
                    isDeathAnimation = false;
                    if (lives > 0)
                    {
                        boundingRectangle.Position = startingPosition;
                    }
                }
            }

            GameplayScreen gameplayScreen = MainGame.Context.GetGameScreen<GameplayScreen>(GameScreenType.Gameplay);

            if (gameplayScreen.IsFrozen) return;
            HandleMovement(deltaTime);
            if (Input.GetKey(Keys.Space))
            {
                gameplayScreen.ProjectileController.CreatePlayerProjectile();
            }
        }

        /// <summary>
        /// Update the movement.
        /// </summary>
        /// <param name="deltaTime"></param>
        private void HandleMovement(float deltaTime)
        {
            float velocity = 0;
            if (Input.GetKey(Keys.A))
            {
                velocity = -1;
            }

            if (Input.GetKey(Keys.D))
            {
                velocity = 1;
            }

            float newX = MathHelper.Clamp(boundingRectangle.X + velocity * HorizontalSpeed * deltaTime, GameplayScreen.HorizontalBoundaryStart.X, maxHorizontalCoordinate);
            boundingRectangle.X = newX;
        }

        /// <summary>
        /// Render the player.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            int frameIndex = deathAnimationFrame ? 2 : 1;

            // Show the death animation sprites if we are currently playing the death animation,
            // or if we have no lives left.
            bool useDeathSprites = isDeathAnimation || lives <= 0;
            Texture2D texture = useDeathSprites ? MainGame.Context.MainTextureAtlas[$"player_death_{frameIndex}"] : Texture;
            spriteBatch.Draw(texture, boundingRectangle.Position, null, ColourHelpers.PureGreen, 0, Vector2.Zero, MainGame.ResolutionScale, SpriteEffects.None, 0.5f);
        }

        /// <summary>
        /// Determines whether this <see cref="Player"/> is intersecting with the specified <see cref="RectangleF"/>.
        /// </summary>
        /// <param name="rectangle">The <see cref="RectangleF"/>.</param>
        /// <returns>A boolean value indicating whether an intersection occured.</returns>
        public bool Intersects(RectangleF rectangle) => boundingRectangle.Intersects(rectangle);
    }
}
