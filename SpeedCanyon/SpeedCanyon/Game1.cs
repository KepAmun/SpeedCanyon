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
        const int NUM_COLS = 257;
        const int NUM_ROWS = 257;
        float _terrainScale = 10;
        private const float BOUNDARY = 16.0f;

        public BoundingSphereRenderer BoundingSphereRenderer { get; private set; }

        List<Tank> _tanks = new List<Tank>(3);

        TankController _playerControl;
        TankController _ai1Control;
        TankController _ai2Control;

        IndexBuffer _groundIndexBuffer;
        VertexBuffer _groundVertexBuffer;
        BasicEffect _basicEffect;

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
            (float)(Math.Sin(65 * DtoR) * Math.Cos(-30 * DtoR)),
            -(float)Math.Cos(65 * DtoR),
            (float)(Math.Sin(65 * DtoR) * Math.Sin(-30 * DtoR)));

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
        bool _gameOver = false;
        bool _pausePending = false;
        TimeSpan _lastPausedTime = TimeSpan.FromSeconds(0);
        TimeSpan _totalPausedTime = TimeSpan.FromSeconds(0);


        //Explosion stuff
        List<ParticleExplosion> _explosions = new List<ParticleExplosion>();
        ParticleExplosionSettings _particleExplosionSettings = new ParticleExplosionSettings();
        ParticleSettings _particleSettings = new ParticleSettings();
        Texture2D _explosionTexture;
        Texture2D _explosionColorsTexture;
        Effect _explosionEffect;

        SpriteFont _titleFont;
        SpriteFont _menuFont;
        SpriteFont _menuFontSmall;
        SpriteFont _scoreFont;

        TimeSpan _levelDuration;

        int[] _scores = new int[3];
        Vector3[] _spawnLocations = new Vector3[3];


        List<ResourcePickup> _resources;
        TimeSpan _nextResourceSpawnTime;
        TimeSpan _resourceSpawnDelay;

        KeyboardState prevKeyboardState;


        public Game1()
        {
            //_muted = true;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Rnd = new Random();
            _skybox = new Skybox(this, "Textures/terra");

            // Set preferred resolution
            _graphics.PreferredBackBufferWidth = 960;
            _graphics.PreferredBackBufferHeight = 540;

            _bullets = new List<Bullet>();

            _spawnLocations[0] = new Vector3(-100, 0, 0);
            _spawnLocations[1] = new Vector3(70, 0, 70);
            _spawnLocations[2] = new Vector3(70, 0, -70);

            _tanks.Add(new Tank(this, _spawnLocations[0], 0, 0, Color.Black));
            _tanks.Add(new Tank(this, _spawnLocations[1], -(MathHelper.PiOver4 + MathHelper.PiOver2), 1, Color.Green));
            _tanks.Add(new Tank(this, _spawnLocations[2], MathHelper.PiOver4 + MathHelper.PiOver2, 2, Color.Blue));

            _playerControl = new TankControllerHuman(this, _tanks[0]);
            //_playerControl = new TankControllerAI(0, this, _tanks[0]);
            _ai1Control = new TankControllerAI(this, _tanks[1]);
            _ai2Control = new TankControllerAI(this, _tanks[2]);

            Camera = new TrackingCamera(this, _tanks[0]);

            _fadeBox = new FadeBox(this);

            _levelDuration = TimeSpan.FromMinutes(4);

            _scores[0] = 0;
            _scores[1] = 0;
            _scores[2] = 0;
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


        Vector3 CellNormal(int row, int col)
        {
            HandleOffHeightMap(ref row, ref col);

            return new Vector3(_terrain.NormalX(col + row * _terrain.NUM_COLS),
                               _terrain.NormalY(col + row * _terrain.NUM_COLS),
                               _terrain.NormalZ(col + row * _terrain.NUM_COLS));
        }


        Vector3 Normal(Vector3 position)
        {
            // coordinates for top left of cell
            Vector3 cellPosition = RowColumn(position);
            int row = (int)cellPosition.Z;
            int col = (int)cellPosition.X;
            // distance from top left of cell
            float distanceFromLeft = position.X % _terrain.cellWidth;
            float distanceFromTop = position.Z % _terrain.cellHeight;

            // use lerp to interpolate normal at point within cell
            Vector3 topNormal = Vector3.Lerp(CellNormal(row, col),
                                                   CellNormal(row, col + 1),
                                                   distanceFromLeft);
            Vector3 bottomNormal = Vector3.Lerp(CellNormal(row + 1, col),
                                                   CellNormal(row + 1, col + 1),
                                                   distanceFromLeft);
            Vector3 normal = Vector3.Lerp(topNormal,
                                                   bottomNormal,
                                                   distanceFromTop);
            normal.Normalize(); // convert to unit vector for consistency
            return normal;
        }


        Vector3 NormalWeight(Vector3 position, Vector3 speed,
                             float numCells, float directionScalar)
        {
            float weight = 0.0f;
            float startWeight = 0.0f;
            float totalSteps = (float)numCells;
            Vector3 nextPosition;
            Vector3 cumulativeNormal = Vector3.Zero;

            for (int i = 0; i <= numCells; i++)
            {
                // get position in next cell
                nextPosition = ProjectedXZ(position, speed, directionScalar);
                if (i == 0)
                {
                    // current cell
                    startWeight = CellWeight(position, nextPosition);
                    weight = startWeight / totalSteps;
                }
                else if (i == numCells) // end cell
                    weight = (1.0f - startWeight) / totalSteps;
                else                    // all cells in between
                    weight = 1.0f / totalSteps;

                cumulativeNormal += weight * Normal(position);
                position = nextPosition;
            }
            cumulativeNormal.Normalize();
            return cumulativeNormal;
        }


        public Vector3 ProjectedUp(Vector3 position, Vector3 speed, int numCells)
        {
            Vector3 frontAverage, backAverage, projectedUp;

            // total steps must be 0 or more. 0 steps means no shock absorption.
            if (numCells <= 0)
                return Normal(position);

            // weighted average of normals ahead and behind enable smoother ride.
            else
            {
                frontAverage = NormalWeight(position, speed, numCells, 1.0f);
                backAverage = NormalWeight(position, speed, numCells, -1.0f);
            }
            projectedUp = (frontAverage + backAverage) / 2.0f;
            projectedUp.Normalize();
            return projectedUp;
        }

        public Vector3 ProjectedXZ(Vector3 position, Vector3 speed,
                                  float directionScalar)
        {
            // only consider change in X and Z when projecting position
            // in neighboring cell.
            Vector3 velocity = new Vector3(speed.X, 0.0f, speed.Z);
            velocity = Vector3.Normalize(velocity);
            float changeX = directionScalar * _terrain.cellWidth * velocity.X;
            float changeZ = directionScalar * _terrain.cellHeight * velocity.Z;

            return new Vector3(position.X + changeX, 0.0f, position.Z + changeZ);
        }

        float CellWeight(Vector3 currentPosition, Vector3 nextPosition)
        {
            Vector3 currRowColumn = RowColumn(currentPosition);
            int currRow = (int)currRowColumn.Z;
            int currCol = (int)currRowColumn.X;
            Vector3 nextRowColumn = RowColumn(nextPosition);
            int nextRow = (int)nextRowColumn.Z;
            int nextCol = (int)nextRowColumn.X;

            // find row and column between current cell and neighbor cell
            int rowBorder, colBorder;
            if (currRow < nextRow)
                rowBorder = currRow + 1;
            else
                rowBorder = currRow;

            if (currCol < nextCol)              // next cell at right of current cell
                colBorder = currCol + 1;
            else
                colBorder = currCol;            // next cell at left of current cell
            Vector3 intersect = Vector3.Zero;   // margins between current
            // and next cell

            intersect.X = -BOUNDARY + colBorder * _terrain.cellWidth;
            intersect.Z = -BOUNDARY + rowBorder * _terrain.cellHeight;
            currentPosition.Y
                          = 0.0f;               // not concerned about height
            // find distance between current position and cell border
            Vector3 difference = intersect - currentPosition;
            float lengthToBorder = difference.Length();

            // find distance to projected location in neighboring cell
            difference = nextPosition - currentPosition;
            float lengthToNewCell = difference.Length();
            if (lengthToNewCell == 0)              // prevent divide by zero
                return 0.0f;

            // weighted distance in current cell relative to the entire
            // distance to projected position
            return lengthToBorder / lengthToNewCell;
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
            position /= _terrainScale;

            // get top left row and column indices
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
            return _terrainScale * MathHelper.Lerp(topHeight, bottomHeight, distanceFromTop);
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _terrain = Content.Load<Terrain>("Images\\heightMap");
            InitializeIndices();
            InitializeVertexBuffer();

            _basicEffect = new BasicEffect(GraphicsDevice);


            _skybox.DrawOrder = 0;
            Components.Add(_skybox);

            Components.Add(_playerControl);
            Components.Add(_ai1Control);
            Components.Add(_ai2Control);

            Components.Add(_tanks[0]);
            Components.Add(_tanks[1]);
            Components.Add(_tanks[2]);

            Components.Add(Camera);


            _fadeBox.Initialize();
            _fadeBox.FadeIn();

            _resources = new List<ResourcePickup>();
            _resourceSpawnDelay = TimeSpan.FromSeconds(10);
            _nextResourceSpawnTime = TimeSpan.FromSeconds(0);

            base.Initialize();

            BoundingSphereRenderer = new BoundingSphereRenderer(GraphicsDevice);

        }


        private void InitializeIndices()
        {
            short[] indices; // indices for 1 subset
            indices = new short[2 * NUM_COLS]; // sized for 1 subset
            _groundIndexBuffer = new IndexBuffer(
                GraphicsDevice, // graphics device
                typeof(short),           // data type is short
                indices.Length,          // array size in bytes
                BufferUsage.WriteOnly);  // memory allocation

            // store indices for one subset of vertices
            // see Figure 11-2 for the first subset of indices
            int counter = 0;
            for (int col = 0; col < NUM_COLS; col++)
            {
                indices[counter++] = (short)col;
                indices[counter++] = (short)(col + NUM_COLS);
            }
            _groundIndexBuffer.SetData(indices); // store in index buffer
        }


        private void InitializeVertexBuffer()
        {
            _groundVertexBuffer = new VertexBuffer(
                                GraphicsDevice,            // graphics device
                                typeof(VertexPositionNormalTexture),// vertex type
                                NUM_COLS * NUM_ROWS,                // element count
                                BufferUsage.WriteOnly);             // memory use

            // store vertices temporarily while initializing them
            VertexPositionNormalTexture[] vertex
                = new VertexPositionNormalTexture[NUM_ROWS * NUM_COLS];

            // set grid width and height
            //float colWidth = (float)2 * BOUNDARY / (NUM_COLS - 1);
            //float rowHeight = (float)2 * BOUNDARY / (NUM_ROWS - 1);

            // set position, color, and texture coordinates
            for (int row = 0; row < NUM_ROWS; row++)
            {
                for (int col = 0; col < NUM_COLS; col++)
                {
                    vertex[col + row * NUM_COLS].Position       // position
                              = new Vector3(_terrain.PositionX(col + row * NUM_COLS),
                                            _terrain.PositionY(col + row * NUM_COLS),
                                            _terrain.PositionZ(col + row * NUM_COLS));
                    float U = (float)col / (float)(NUM_COLS - 1); // UV
                    float V = (float)row / (float)(NUM_ROWS - 1);
                    vertex[col + row * NUM_COLS].TextureCoordinate
                            = new Vector2(U, V);
                    vertex[col + row * NUM_COLS].Normal         // normal
                            = new Vector3(_terrain.NormalX(col + row * NUM_COLS),
                                          _terrain.NormalY(col + row * NUM_COLS),
                                          _terrain.NormalZ(col + row * NUM_COLS));
                }
            }

            // commit data to vertex buffer
            _groundVertexBuffer.SetData(vertex);
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _desertTexture = Content.Load<Texture2D>("Textures\\Desert");

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _crosshairTexture = Content.Load<Texture2D>(@"textures\crosshair");
            _warningLightTexture = Content.Load<Texture2D>(@"textures\warninglight");


            // Load explosion textures and effect
            _explosionTexture = Content.Load<Texture2D>(@"Textures\Particle");
            _explosionColorsTexture = Content.Load<Texture2D>(@"Textures\Desert");
            _explosionEffect = Content.Load<Effect>(@"effects\particle");

            // Set effect parameters that don't change per particle
            _explosionEffect.CurrentTechnique = _explosionEffect.Techniques["Technique1"];
            _explosionEffect.Parameters["theTexture"].SetValue(_explosionTexture);



            // Load sounds and play initial sounds
            _audioEngine = new AudioEngine(@"Content\Audio\GameAudio.xgs");
            _waveBank = new WaveBank(_audioEngine, @"Content\Audio\Wave Bank.xwb");
            _soundBank = new SoundBank(_audioEngine, @"Content\Audio\Sound Bank.xsb");

            _tanks[0].EngineNoise = _soundBank.GetCue("truckidling");
            _tanks[1].EngineNoise = _soundBank.GetCue("truckidling");
            _tanks[2].EngineNoise = _soundBank.GetCue("truckidling");

            _tanks[0].EngineNoise.Apply3D(_tanks[0].AudioListener, _tanks[0].AudioEmitter);
            _tanks[1].EngineNoise.Apply3D(_tanks[0].AudioListener, _tanks[1].AudioEmitter);
            _tanks[2].EngineNoise.Apply3D(_tanks[0].AudioListener, _tanks[2].AudioEmitter);

            PlayCue(_tanks[0].EngineNoise);
            PlayCue(_tanks[1].EngineNoise);
            PlayCue(_tanks[2].EngineNoise);

            _audioEngine.GetCategory("Music").SetVolume(0.1f);

            // Play the soundtrack
            _trackCue = _soundBank.GetCue("TheReconMission");
            PlayCue(_trackCue);

            _titleFont = Content.Load<SpriteFont>(@"Fonts/TitleFont");
            _menuFont = Content.Load<SpriteFont>(@"Fonts/MenuFont");
            _menuFontSmall = Content.Load<SpriteFont>(@"Fonts/MenuFontSmall");
            _scoreFont = Content.Load<SpriteFont>(@"Fonts/ScoreFont");
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

        void ToggleMute()
        {
            _muted = !_muted;

            float volume = _muted ? 0 : 1;

            AudioCategory c = _audioEngine.GetCategory("Default");
            
            c.SetVolume(volume);
            c = _audioEngine.GetCategory("Music");
            c.SetVolume(volume/5);
        }


        void PauseGame(GameTime gameTime)
        {
            _paused = true;
            IsMouseVisible = true;

            _lastPausedTime = gameTime.TotalGameTime;
            AudioCategory c = _audioEngine.GetCategory("Default");
            c.Pause();
            c = _audioEngine.GetCategory("Music");
            c.Pause();
        }


        void UnpauseGame(GameTime gameTime)
        {
            _paused = false;
            IsMouseVisible = false;

            _totalPausedTime += gameTime.TotalGameTime - _lastPausedTime;

            Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
            _trackCue.Resume();

            AudioCategory c = _audioEngine.GetCategory("Default");
            c.Resume();
            c = _audioEngine.GetCategory("Music");
            c.Resume();

        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            // Check for game exit request
            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            // Check for pause request
            if (_pausePending)
            {
                _pausePending = false;
                PauseGame(gameTime);
            }

            // Pause key check
            if (!_gameOver && keyboardState.IsKeyDown(Keys.P) && !prevKeyboardState.IsKeyDown(Keys.P))
            {
                TogglePause(gameTime);
            }


            // Mute key check
            if (keyboardState.IsKeyDown(Keys.M) && !prevKeyboardState.IsKeyDown(Keys.M))
            {
                ToggleMute();
            }


            // AI key check
            if (keyboardState.IsKeyDown(Keys.OemTilde) && !prevKeyboardState.IsKeyDown(Keys.OemTilde))
            {
                Components.Remove(_playerControl);
                
                if (_playerControl is TankControllerHuman)
                {
                    _playerControl = new TankControllerAI(this, _tanks[0]);
                }
                else
                {
                    _playerControl = new TankControllerHuman(this, _tanks[0]);
                }

                Components.Add(_playerControl);
            }


            gameTime = GetPauseAdjustedGameTime(gameTime);


            if (!_paused)
            {
                bool[] wasDead = { _tanks[0].IsDead, _tanks[1].IsDead, _tanks[2].IsDead };

                if (_nextResourceSpawnTime < gameTime.TotalGameTime)
                {
                    SpawnResource(gameTime);
                }


                base.Update(gameTime);

                List<ResourcePickup> resourcesToRemove = new List<ResourcePickup>();
                foreach (ResourcePickup resource in _resources)
                {
                    //bullet.Update(gameTime);

                    foreach (Tank tank in _tanks)
                    {
                        if (!resource.IsDead && tank.Collides(resource.Position))
                        {
                            _scores[_tanks.IndexOf(tank)] += 10;

                            PlayCue("metallicclang", tank.AudioEmitter);
                            resource.IsDead = true;
                            tank.Health += 2;

                            break;
                        }
                    }


                    if (resource.IsDead)
                    {
                        resourcesToRemove.Add(resource);
                    }
                }


                foreach (ResourcePickup resource in resourcesToRemove)
                {
                    _resources.Remove(resource);
                    Components.Remove(resource);
                }


                List<Bullet> bulletsToRemove = new List<Bullet>();
                foreach (Bullet bullet in _bullets)
                {
                    bullet.Update(gameTime);

                    foreach (Tank tank in _tanks)
                    {
                        if (!tank.IsDead && !object.ReferenceEquals(bullet.Owner, tank) &&
                            tank.Collides(bullet.Position))
                        {
                            tank.ApplyImpact(bullet.Velocity * 0.2f, 1);
                            _scores[bullet.Owner.Faction]++;
                            bullet.IsDead = true;

                            if (tank.IsDead)
                            {
                                _scores[bullet.Owner.Faction] += 10;
                            }
                            else
                            {
                                PlayCue("metallicclang", tank.AudioEmitter);
                            }


                            break;
                        }
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


                for (int i = 0; i < 2; i++)
                {
                    for (int c = i + 1; c < 3; c++)
                    {
                        if (!_tanks[i].IsDead && _tanks[i].Collides(_tanks[c]))
                        {
                            PlayCue("metalcrash");
                            Vector3 impact = _tanks[i].Position - _tanks[c].Position;
                            _tanks[i].ApplyImpact(impact, 1);
                            _tanks[c].ApplyImpact(-impact, 1);

                            if (_tanks[i].IsDead)
                                _scores[c] += 10;

                            if (_tanks[c].IsDead)
                                _scores[i] += 10;

                        }
                    }
                }

                // Terrain edge collisions
                for (int i = 0; i < 3; i++)
                {
                    if (!_tanks[i].IsDead)
                    {
                        if (_tanks[i].Position.X < -158)
                        {
                            _tanks[i].ApplyImpact(10 * new Vector3(-10 * _tanks[i].Velocity.X, 0, 0));
                            PlayCue("metalcrash", _tanks[i].AudioEmitter);
                        }
                        else if (_tanks[i].Position.X > 158)
                        {
                            _tanks[i].ApplyImpact(10 * new Vector3(-10 * _tanks[i].Velocity.X, 0, 0));
                            PlayCue("metalcrash", _tanks[i].AudioEmitter);
                        }

                        if (_tanks[i].Position.Z < -158)
                        {
                            _tanks[i].ApplyImpact(10 * new Vector3(0, 0, -10 * _tanks[i].Velocity.Z));
                            PlayCue("metalcrash", _tanks[i].AudioEmitter);
                        }
                        else if (_tanks[i].Position.Z > 158)
                        {
                            _tanks[i].ApplyImpact(10 * new Vector3(0, 0, -10 * _tanks[i].Velocity.Z));
                            PlayCue("metalcrash", _tanks[i].AudioEmitter);
                        }
                    }
                    else
                    {
                        if (_tanks[i].Position.Y < -1000)
                        {
                            _tanks[i].Respawn(_spawnLocations[_tanks[i].Faction]);

                            if (i == 0)
                                _fadeBox.FadeIn();
                        }
                    }
                }


                for (int i = 0; i < 3; i++)
                {
                    if (_tanks[i].IsDead && !wasDead[i])
                    {
                        PlayCue("TankExplode", _tanks[i].AudioEmitter);

                        if (i == 0)
                            _fadeBox.FadeOut(4000);
                    }
                }

                UpdateExplosions(gameTime);

                Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
            }


            TimeSpan timeLeft = _levelDuration - gameTime.TotalGameTime;
            if (!_gameOver && timeLeft.TotalSeconds < 1)
            {
                _gameOver = true;
                PauseGame(gameTime);


                if (_scores[0] < _scores[1] && _scores[0] < _scores[2])
                {
                    PlayCue("Applause3");
                }
                else if (_scores[0] < _scores[1] || _scores[0] < _scores[2])
                {
                    PlayCue("Applause2");
                }
                else
                {
                    PlayCue("Applause1");
                }
            }


            _audioEngine.Update();
            _tanks[0].EngineNoise.Apply3D(_tanks[0].AudioListener, _tanks[0].AudioEmitter);
            _tanks[1].EngineNoise.Apply3D(_tanks[0].AudioListener, _tanks[1].AudioEmitter);
            _tanks[2].EngineNoise.Apply3D(_tanks[0].AudioListener, _tanks[2].AudioEmitter);

            _fadeBox.Update(gameTime);


            prevKeyboardState = keyboardState;
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

            base.Draw(gameTime);


            foreach (Bullet bullet in _bullets)
            {
                bullet.Draw(gameTime);
            }


            _basicEffect.Projection = Camera.Projection;
            _basicEffect.View = Camera.View;
            _basicEffect.Texture = _desertTexture;
            _basicEffect.TextureEnabled = true;
            _basicEffect.EnableDefaultLighting();
            _basicEffect.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
            _basicEffect.SpecularColor = new Vector3(0.05f, 0.05f, 0.01f);
            _basicEffect.DiffuseColor = new Vector3(0.6f, 0.6f, 0.6f);
            _basicEffect.CurrentTechnique.Passes[0].Apply();
            _basicEffect.DirectionalLight0.Direction = LightDirection;

            DrawTerrain();



            // Loop through and draw each particle explosion
            foreach (ParticleExplosion pe in _explosions)
            {
                pe.Draw(Camera);
            }



            _fadeBox.Draw(gameTime);


            _spriteBatch.Begin();

            //_spriteBatch.Draw(_crosshairTexture,
            //    new Vector2((Window.ClientBounds.Width / 2)
            //        - (_crosshairTexture.Width / 2),
            //        (Window.ClientBounds.Height / 2)
            //        - (_crosshairTexture.Height / 2)),
            //        Color.White);

            //int offset = 0;
            //if (((int)gameTime.TotalGameTime.TotalMilliseconds) % 2000 > 1000)
            //    offset = _warningLightTexture.Height / 2;

            //_spriteBatch.Draw(_warningLightTexture,
            //    new Vector2(0, 0),
            //    new Rectangle(0, offset, _warningLightTexture.Width, _warningLightTexture.Height / 2),
            //    Color.White);

            TimeSpan timeLeft = _levelDuration - gameTime.TotalGameTime;

            if (_gameOver)
                timeLeft = TimeSpan.FromSeconds(0);


            Color c = Color.Gold;

            _spriteBatch.DrawString(_scoreFont,
                timeLeft.ToString(@"m\:ss"),
                new Vector2(460, 450),
                c);


            if (_paused)
            {
                if (!_gameOver)
                {
                    // TODO: Render "Paused, hit esc to exit" text
                    _spriteBatch.DrawString(_titleFont,
                        "PAUSED",
                        new Vector2(330, 100),
                        c);

                    _spriteBatch.DrawString(_scoreFont,
                         "hit 'P' to resume\n hit Esc to exit",
                         new Vector2(400, 240),
                         c);
                }
                else
                {
                    _spriteBatch.DrawString(_titleFont,
                        "Times Up!",
                        new Vector2(300, 100),
                        c);

                    string rankString = string.Empty;

                    if (_scores[0] < _scores[1] && _scores[0] < _scores[2])
                    {
                        rankString = "3rd";
                    }
                    else if (_scores[0] < _scores[1] || _scores[0] < _scores[2])
                    {
                        rankString = "2nd";
                    }
                    else
                    {
                        rankString = "1st";
                    }


                    _spriteBatch.DrawString(_menuFont,
                        string.Format("{0} place", rankString),
                        new Vector2(415, 180),
                        c);

                    _spriteBatch.DrawString(_scoreFont,
                         "hit Esc to exit",
                         new Vector2(410, 220),
                         c);

                }

            }

            _spriteBatch.DrawString(_scoreFont,
                string.Format("You: {0}",
                _scores[0]),
                new Vector2(430, 500),
                c);

            c = Color.LawnGreen;
            _spriteBatch.DrawString(_scoreFont,
                 string.Format("Green: {0}",
                 _scores[1]),
                 new Vector2(280, 500),
                 c);

            c = Color.Blue;
            _spriteBatch.DrawString(_scoreFont,
                 string.Format("Blue: {0}",
                 _scores[2]),
                 new Vector2(580, 500),
                 c);



            c = Color.DarkRed;
            _spriteBatch.DrawString(_scoreFont,
                string.Format("Health: {0}",
                _tanks[0].Health),
                new Vector2(435, 475),
                c);


            _spriteBatch.End();




        }


        private void DrawTerrain()
        {
            RasterizerState prevRState = GraphicsDevice.RasterizerState;

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            // 1: declare matrices
            Matrix world, translate, rotationX, scale, rotationY;

            // 2: initialize matrices
            scale = Matrix.CreateScale(_terrainScale);
            //rotationY = Matrix.CreateRotationY(0.0f);
            //rotationX = Matrix.CreateRotationX(0.0f);
            //translate = Matrix.CreateTranslation(0.0f, -3.6f, 0.0f);

            _basicEffect.Texture = _desertTexture;// set ground image

            // 3: build cumulative world matrix using I.S.R.O.T. sequence
            // identity, scale, rotate, orbit(translate & rotate), translate
            world = scale;

            // 4: finish setting shader variables
            _basicEffect.World = world;
            _basicEffect.Projection = Camera.Projection;
            _basicEffect.View = Camera.View;

            // 5: draw object
            GraphicsDevice.SetVertexBuffer(_groundVertexBuffer);
            GraphicsDevice.Indices = _groundIndexBuffer;


            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.SetVertexBuffer(_groundVertexBuffer);

                // draw grid one row at a time
                for (int Z = 0; Z < NUM_ROWS - 1; Z++)
                {
                    GraphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleStrip, // primitive type
                                Z * NUM_COLS, // start point in vertex buffer
                                0,                           // vertex buffer offset
                                NUM_COLS * NUM_ROWS, // total verts in vertex buffer
                                0,                           // index buffer offset
                                2 * (NUM_COLS - 1));         // index buffer end
                }
            }

            GraphicsDevice.RasterizerState = prevRState;
        }


        public void PlayCue(string name, AudioEmitter emitter = null)
        {
            if (!_muted)
                PlayCue(_soundBank.GetCue(name), emitter);
        }


        public void PlayCue(Cue cue, AudioEmitter emitter = null)
        {

            if (emitter != null)
            {
                cue.Apply3D(_tanks[0].AudioListener, emitter);
            }

            if (!_muted)
                cue.Play();
        }


        public void AddBullet(Bullet b)
        {
            _bullets.Add(b);
        }

        public void AddExplosion(Vector3 position)
        {
            // Collision! add an explosion.
            _explosions.Add(new ParticleExplosion(GraphicsDevice,
                position,
                Rnd.Next(
                    _particleExplosionSettings.minLife,
                    _particleExplosionSettings.maxLife),
                Rnd.Next(
                    _particleExplosionSettings.minRoundTime,
                    _particleExplosionSettings.maxRoundTime),
                Rnd.Next(
                    _particleExplosionSettings.minParticlesPerRound,
                    _particleExplosionSettings.maxParticlesPerRound),
                Rnd.Next(
                    _particleExplosionSettings.minParticles,
                    _particleExplosionSettings.maxParticles),
                _explosionColorsTexture, _particleSettings,
                _explosionEffect));
        }

        protected void UpdateExplosions(GameTime gameTime)
        {
            // Loop through and update explosions
            for (int i = 0; i < _explosions.Count; ++i)
            {
                _explosions[i].Update(gameTime);
                // If explosion is finished, remove it
                if (_explosions[i].IsDead)
                {
                    _explosions.RemoveAt(i);
                    --i;
                }
            }
        }


        public Tank FindTank(Tank tank, float range)
        {
            return Find<Tank>(_tanks, tank.Position, range, tank);
        }


        public ResourcePickup FindResource(Vector3 position, float range)
        {
            return Find<ResourcePickup>(_resources, position, range);
        }


        public T Find<T>(List<T> collection, Vector3 position, float range, object ignore = null) where T : IGameObject
        {
            T closest = default(T);
            float closestDist = range * range;

            foreach (T gameObject in collection)
            {
                if (!object.ReferenceEquals(gameObject, ignore))
                {
                    float distSq = Vector3.DistanceSquared(gameObject.Position, position);
                    if (distSq < closestDist)
                    {
                        closestDist = distSq;
                        closest = (T)gameObject;
                    }
                }
            }

            return closest;
        }


        void SpawnResource(GameTime gameTime)
        {
            Vector3 position = new Vector3((float)Rnd.NextDouble() * 200 - 100, 0, (float)Rnd.NextDouble() * 200 - 100);

            ResourcePickup resource = new ResourcePickup(this, position);
            Components.Add(resource);
            _resources.Add(resource);

            _nextResourceSpawnTime = gameTime.TotalGameTime + _resourceSpawnDelay;
        }

    }
}
