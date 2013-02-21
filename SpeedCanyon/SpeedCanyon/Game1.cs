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

namespace SpeedCanyon
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {

        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;

        // Model stuff
        ModelManager _modelManager;

        Skybox _skybox;

        // Sun Theta = 300, Phi = 25
        // x = cos(25)*cos(300)
        // y = sin(25)
        // z = cos(25)*sin(300)
        const double DtoR = Math.PI / 180;
        Vector3 _lightDirection = new Vector3(
            (float)(Math.Cos(25 * DtoR) * Math.Cos(300 * DtoR)), 
            (float)Math.Sin(25 * DtoR), 
            (float)(Math.Cos(25 * DtoR) * Math.Sin(300 * DtoR)));

        public Vector3 LightDirection
        {
            get { return _lightDirection; }
            private set { _lightDirection = value; }
        }

        // Camera
        public Camera Camera { get; protected set; }

        // Random
        public Random Rnd { get; protected set; }

        // Shot variables
        float _shotSpeed = 10;
        int _shotDelay = 300;
        int _shotCountdown = 0;

        // Crosshair
        Texture2D _crosshairTexture;

        // Audio
        bool _muted = false;
        AudioEngine _audioEngine;
        WaveBank _waveBank;
        SoundBank _soundBank;
        Cue _trackCue;

        FadeBox _fadeBox;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Rnd = new Random();
            _skybox = new Skybox(this, "Textures/terra");

            // Set preferred resolution
            _graphics.PreferredBackBufferWidth = 960;
            _graphics.PreferredBackBufferHeight = 540;

            // If not running debug, run in full screen
#if !DEBUG
    //graphics.IsFullScreen = true;
#endif
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _skybox.DrawOrder = 0;
            Components.Add(_skybox);

            // Initialize Camera
            Camera = new Camera(this,
                new Vector3(0, 0, 50),
                Vector3.Zero,
                Vector3.Up);
            Components.Add(Camera);

            // Initialize model manager
            _modelManager = new ModelManager(this);
            _modelManager.DrawOrder = 2;
            Components.Add(_modelManager);

            _fadeBox = new FadeBox(this);
            _fadeBox.Initialize();
            _fadeBox.FadeIn();


            base.Initialize();
            
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the crosshair texture
            _crosshairTexture = Content.Load<Texture2D>(@"textures\crosshair");

            // Load sounds and play initial sounds
            _audioEngine = new AudioEngine(@"Content\Audio\GameAudio.xgs");
            _waveBank = new WaveBank(_audioEngine, @"Content\Audio\Wave Bank.xwb");
            _soundBank = new SoundBank(_audioEngine, @"Content\Audio\Sound Bank.xsb");

            // Play the soundtrack
            _trackCue = _soundBank.GetCue("Tracks");
            PlayCue(_trackCue);
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
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // See if the player has fired a shot
            FireShots(gameTime);


            base.Update(gameTime);


            _fadeBox.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);



            // Set suitable renderstates for drawing a 3D model
            //GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);

            _spriteBatch.Begin();

            _spriteBatch.Draw(_crosshairTexture,
                new Vector2((Window.ClientBounds.Width / 2)
                    - (_crosshairTexture.Width / 2),
                    (Window.ClientBounds.Height / 2)
                    - (_crosshairTexture.Height / 2)),
                    Color.White);

            _spriteBatch.End();

            // Set suitable renderstates for drawing a 3D model
            //GraphicsDevice.BlendState = BlendState.AlphaBlend;
            _fadeBox.Draw(gameTime);
        }


        protected void FireShots(GameTime gameTime)
        {
            if (_shotCountdown <= 0)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    _shotDelay = 150;
                }
                else
                {
                    _shotDelay = 300;
                }

                // Did player press space bar or left mouse button?
                if (Keyboard.GetState().IsKeyDown(Keys.Space) ||
                    Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    // Add left shot to the model manager
                    //modelManager.AddShot(
                    //    camera._cameraPosition + new Vector3(0, -5, 0),
                    //    (camera.GetCameraDirection + new Vector3(-0.1f, 0, 0)) * shotSpeed);

                    // Add center shot to the model manager
                    _modelManager.AddShot(
                        Camera.CameraPosition + new Vector3(0, -5, 0),
                        Camera.GetCameraDirection * _shotSpeed);

                    // Add right shot to the model manager
                    //modelManager.AddShot(
                    //    camera._cameraPosition + new Vector3(0, -5, 0),
                    //    (camera.GetCameraDirection + new Vector3(0.1f,0,0)) * shotSpeed);

                    // Play shot audio
                    PlayCue("Shot");

                    // Reset the shot countdown
                    _shotCountdown = _shotDelay;
                }
            }
            else
                _shotCountdown -= gameTime.ElapsedGameTime.Milliseconds;
        }

        public void PlayCue(string name)
        {
            if(!_muted)
                _soundBank.PlayCue(name);
        }

        public void PlayCue(Cue cue)
        {
            if (!_muted)
                cue.Play();
        }
    }
}
