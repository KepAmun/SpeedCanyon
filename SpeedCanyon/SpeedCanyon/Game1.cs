using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TerrainRuntime;

namespace SpeedCanyon
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        Terrain _terrain;

        public BoundingSphereRenderer BoundingSphereRenderer { get; private set; }

        Tank _tank;
        Tank _tank2;
        Tank _tank3;

        TankControllerHuman _playerControl;

        IndexBuffer _ib;
        VertexBuffer _vb;
        BasicEffect _be;

        Texture2D _desertTexture;

        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;

        List<Bullet> _bullets;

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

        Texture2D _crosshairTexture;
        Texture2D _warningLightTexture;

        // Audio
        AudioEngine _audioEngine;
        WaveBank _waveBank;
        SoundBank _soundBank;
        Cue _trackCue;

        FadeBox _fadeBox;

        bool _muted = false;

        bool _paused = false;
        bool _pauseKeyReleased = true;
        bool _pausePending = false;
        TimeSpan _lastPausedTime = TimeSpan.FromSeconds(0);
        TimeSpan _totalPausedTime = TimeSpan.FromSeconds(0);


        ////////////////////////////////////////////////////// Lab 7
        private const float BOUNDARY = 16.0f;
        private Vector3[] _bezierA = new Vector3[4]; // route 1
        private Vector3[] _newbezierA = new Vector3[4]; // route 2
        private Vector3[] _bezierB = new Vector3[4]; // route 3
        private Vector3[] _newbezierB = new Vector3[4]; // route 4

        // define jet route times and identifiers
        private float[] _keyFrameTime = new float[4];
        private float _tripTime = 0.0f;
        private const float TOTAL_TRIP_TIME = 4.8f * 2 + 2.8f * 2;
        private const int NUM_KEYFRAMES = 4;
        // track ship jet position and orientation
        Vector3 _currentPosition, _previousPosition;
        float _Yrotation;

        // jet model objects
        Model _jetModel;
        Matrix[] _jetMatrix;
        //////////////////////////////////////////////////////

        public Game1()
        {
            _muted = true;

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Rnd = new Random();
            _skybox = new Skybox(this, "Textures/terra");

            // Set preferred resolution
            _graphics.PreferredBackBufferWidth = 960;
            _graphics.PreferredBackBufferHeight = 540;

            _bullets = new List<Bullet>();

            _tank = new Tank(this, Vector3.Zero,0,Color.Black);
            _tank2 = new Tank(this, new Vector3(10, 0, 10), -(MathHelper.PiOver4 + MathHelper.PiOver2), Color.Green);
            _tank3 = new Tank(this, new Vector3(10, 0, -10), MathHelper.PiOver4 + MathHelper.PiOver2, Color.Blue);

            _playerControl = new TankControllerHuman(this, _tank);

            Camera = new TrackingCamera(this, _tank);

            _fadeBox = new FadeBox(this);

        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            _pausePending = true;

            base.OnDeactivated(sender, args);
        }

        private void HandleOffHeightMap(ref int row, ref int col)
        {
            if (row >= _terrain.NUM_ROWS)
                row = _terrain.NUM_ROWS - 1;
            else if (row < 0)
                row = 0;

            if (col >= _terrain.NUM_COLS)
                col = _terrain.NUM_COLS - 1;
            else if (col < 0)
                col = 0;
        }


        Vector3 RowColumn(Vector3 position)
        {
            // calculate X and Z
            int col = (int)((position.X + _terrain.worldWidth) / _terrain.cellWidth);
            int row = (int)((position.Z + _terrain.worldHeight) / _terrain.cellHeight);
            HandleOffHeightMap(ref row, ref col);

            return new Vector3(col, 0.0f, row);
        }


        float Height(int row, int col)
        {
            HandleOffHeightMap(ref row, ref col);
            return _terrain.PositionY(col + row * _terrain.NUM_COLS);
        }


        public float CellHeight(Vector3 position)
        {
            // get top left row and column indicies
            Vector3 cellPosition = RowColumn(position);
            int row = (int)cellPosition.Z;
            int col = (int)cellPosition.X;

            // distance from top left of cell
            float distanceFromLeft, distanceFromTop;
            distanceFromLeft = position.X % _terrain.cellWidth;
            distanceFromTop = position.Z % _terrain.cellHeight;

            // lerp projects height relative to known dimensions
            float topHeight = MathHelper.Lerp(Height(row, col),
                                                  Height(row, col + 1),
                                                  distanceFromLeft);
            float bottomHeight = MathHelper.Lerp(Height(row + 1, col),
                                                  Height(row + 1, col + 1),
                                                  distanceFromLeft);
            return MathHelper.Lerp(topHeight, bottomHeight, distanceFromTop);
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _be = new BasicEffect(GraphicsDevice);

            _vb = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, 2100, BufferUsage.None);
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[2100];


            for (int z = 0; z < 60; z++)
            {
                for (int x = 0; x < 35; x++)
                {
                    vertices[35 * z + x].Position.X = 50 * (x - 17);
                    vertices[35 * z + x].Position.Y = 0;
                    vertices[35 * z + x].Position.Z = 50 * (z - 30);

                    vertices[35 * z + x].TextureCoordinate.X = 1f * x;
                    vertices[35 * z + x].TextureCoordinate.Y = 1f * z;

                    vertices[35 * z + x].Normal.X = 0;
                    vertices[35 * z + x].Normal.Y = 1;
                    vertices[35 * z + x].Normal.Z = 0;
                }
            }
            _vb.SetData<VertexPositionNormalTexture>(vertices);

            _ib = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, 12036, BufferUsage.None);
            short[] indices = new short[12036];
            int i = 0;
            for (int z = 0; z < 60 - 1; z++)
            {
                for (int x = 0; x < 35 - 1; x++)
                {
                    indices[i++] = (short)(35 * z + x);
                    indices[i++] = (short)(35 * z + x + 1);
                    indices[i++] = (short)(35 * (z + 1) + x);

                    indices[i++] = (short)(35 * (z + 1) + x);
                    indices[i++] = (short)(35 * z + x + 1);
                    indices[i++] = (short)(35 * (z + 1) + x + 1);
                }
            }
            _ib.SetData<short>(indices);

            _skybox.DrawOrder = 0;
            Components.Add(_skybox);

            Components.Add(_playerControl);

            Components.Add(_tank);
            Components.Add(_tank2);
            Components.Add(_tank3);

            Components.Add(Camera);


            _fadeBox.Initialize();
            _fadeBox.FadeIn();


            base.Initialize();

            BoundingSphereRenderer = new BoundingSphereRenderer(GraphicsDevice);

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _terrain = Content.Load<Terrain>("Images\\heightMap");

            _desertTexture = Content.Load<Texture2D>("Textures\\Desert");

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _crosshairTexture = Content.Load<Texture2D>(@"textures\crosshair");
            _warningLightTexture = Content.Load<Texture2D>(@"textures\warninglight");

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


        GameTime GetPauseAdjustedGameTime(GameTime gameTime)
        {
            TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
            TimeSpan totalGameTime = gameTime.TotalGameTime;

            if (_paused)
            {
                elapsedGameTime = TimeSpan.FromSeconds(0);
                totalGameTime = _lastPausedTime;
            }

            gameTime = new GameTime(totalGameTime - _totalPausedTime, elapsedGameTime);

            return gameTime;
        }


        void TogglePause(GameTime gameTime)
        {
            if (_paused)
            {
                UnpauseGame(gameTime);
            }
            else
            {
                PauseGame(gameTime);
            }
        }


        void PauseGame(GameTime gameTime)
        {
            _paused = true;
            IsMouseVisible = true;

            _lastPausedTime = gameTime.TotalGameTime;
        }


        void UnpauseGame(GameTime gameTime)
        {
            _paused = false;
            IsMouseVisible = false;

            _totalPausedTime += gameTime.TotalGameTime - _lastPausedTime;
            Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Check for game exit request
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Check for pause request
            if (_pausePending)
            {
                _pausePending = false;
                PauseGame(gameTime);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                if (_pauseKeyReleased)
                {
                    TogglePause(gameTime);
                }

                _pauseKeyReleased = false;
            }
            else
            {
                _pauseKeyReleased = true;
            }

            gameTime = GetPauseAdjustedGameTime(gameTime);


            if (!_paused)
            {
                base.Update(gameTime);

                // See if the player has fired a shot
                //FireShots(gameTime);


                List<Bullet> bulletsToRemove = new List<Bullet>();
                foreach (Bullet bullet in _bullets)
                {
                    bullet.Update(gameTime);


                    if (_tank2.Collides(bullet.Position))
                    {
                        _tank2.ApplyImpact(bullet.Velocity * 0.2f);
                        bullet.IsDead = true;
                    }

                    if (_tank3.Collides(bullet.Position))
                    {
                        _tank3.ApplyImpact(bullet.Velocity * 0.2f);
                        bullet.IsDead = true;
                    }


                    if (bullet.IsDead)
                    {
                        bulletsToRemove.Add(bullet);
                    }
                }


                foreach (Bullet bullet in bulletsToRemove)
                {
                    _bullets.Remove(bullet);
                }


                Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
            }


            _fadeBox.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            gameTime = GetPauseAdjustedGameTime(gameTime);


            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (Bullet bullet in _bullets)
            {
                bullet.Draw(gameTime);
            }

            base.Draw(gameTime);

            _be.Projection = Camera.Projection;
            _be.View = Camera.View;
            _be.Texture = _desertTexture;
            _be.TextureEnabled = true;
            _be.EnableDefaultLighting();
            _be.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
            _be.SpecularColor = new Vector3(0.0f, 0.0f, 0.0f);
            _be.DiffuseColor = new Vector3(0.6f, 0.6f, 0.6f);
            _be.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.Indices = _ib;
            GraphicsDevice.SetVertexBuffer(_vb);
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2100, 0, 4200);


            _spriteBatch.Begin();

            //_spriteBatch.Draw(_crosshairTexture,
            //    new Vector2((Window.ClientBounds.Width / 2)
            //        - (_crosshairTexture.Width / 2),
            //        (Window.ClientBounds.Height / 2)
            //        - (_crosshairTexture.Height / 2)),
            //        Color.White);

            int offset = 0;
            if (((int)gameTime.TotalGameTime.TotalMilliseconds) % 2000 > 1000)
                offset = _warningLightTexture.Height / 2;

            _spriteBatch.Draw(_warningLightTexture,
                new Vector2(0, 0),
                new Rectangle(0, offset, _warningLightTexture.Width, _warningLightTexture.Height / 2),
                Color.White);

            _spriteBatch.End();

            // Set suitable renderstates for drawing a 3D model
            //GraphicsDevice.BlendState = BlendState.AlphaBlend;
            _fadeBox.Draw(gameTime);


            if (_paused)
            {
                // TODO: Render "Paused, hit esc to exit" text
            }

        }


        public void PlayCue(string name)
        {
            if (!_muted)
                _soundBank.PlayCue(name);
        }


        public void PlayCue(Cue cue)
        {
            if (!_muted)
                cue.Play();
        }


        public void AddBullet(Bullet b)
        {
            _bullets.Add(b);
        }
    }
}
