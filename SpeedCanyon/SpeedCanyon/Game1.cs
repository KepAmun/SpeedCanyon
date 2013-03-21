using System;
using System.Collections.Generic;
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
        public BoundingSphereRenderer BoundingSphereRenderer { get; private set; }

        Tank _tank;
        Tank _tank2;
        Tank _tank3;

        TankControllerHuman _playerControl;

        IndexBuffer _ib;
        VertexBuffer _vb;
        BasicEffect _be;

        VertexPositionColorTexture[] _groundVertices = new VertexPositionColorTexture[4];
        Texture2D _grassTexture;


        Model _baseModel;
        Model _fanModel;
        Matrix[] _fanMatrix;
        Matrix[] _baseMatrix;
        const int WINDMILL_BASE = 0;
        const int WINDMILL_FAN = 1;
        private float _fanRotation = 0.0f;
        bool _rotatingClockwise = false;

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

        // Shot variables
        float _shotSpeed = 10;
        int _shotDelay = 300;
        int _shotCountdown = 0;

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
            //_pausePending = true;

            base.OnDeactivated(sender, args);
        }

        private void InitializeRoutes()
        {
            // length of world quadrant
            const float END = +BOUNDARY; // Error page 346. 
            // -BOUNDARY should be +BOUNDARY

            // 1st Bezier curve control points (1st route)
            _bezierA[0] = new Vector3(END + 5.0f, 0.4f, 5.0f);           // start
            _bezierA[1] = new Vector3(END + 5.0f, 2.4f, 3.0f * END);     // ctrl 1
            _bezierA[2] = new Vector3(-END - 5.0f, 4.4f, 3.0f * END);    // ctrl 2
            _bezierA[3] = new Vector3(-END - 5.0f, 5.4f, 5.0f);          // end

            // 1st new Bezier between old Bezier curves (2nd route)
            _newbezierA[0] = new Vector3(-END - 5.0f, 5.4f, 5.0f);          // start
            _newbezierA[1] = new Vector3(-END + 20.0f, 2.4f, 3.0f);    // ctrl 1
            _newbezierA[2] = new Vector3(-END + 20.0f, 4.4f, -3.0f);   // ctrl 2
            _newbezierA[3] = new Vector3(-END - 5.0f, 5.4f, -5.0f);         // end

            // 2nd Bezier curve control points (3rd route)
            _bezierB[0] = new Vector3(-END - 5.0f, 5.4f, -5.0f);         // start
            _bezierB[1] = new Vector3(-END - 5.0f, 4.4f, -3.0f * END);   // ctrl 1
            _bezierB[2] = new Vector3(END + 5.0f, 2.4f, -3.0f * END);    // ctrl 2
            _bezierB[3] = new Vector3(END + 5.0f, 0.4f, -5.0f);          // end

            // 2nd new Bezier between old Bezier curves (4th route)
            _newbezierB[0] = new Vector3(END + 5.0f, 0.4f, -5.0f);         // start
            _newbezierB[1] = new Vector3(END - 20.0f, 4.4f, -3.0f);   // ctrl 1
            _newbezierB[2] = new Vector3(END - 20.0f, 2.4f, 3.0f);    // ctrl 2
            _newbezierB[3] = new Vector3(END + 5.0f, 0.4f, 5.0f);          // end
        }

        private void InitializeTimeLine()
        {
            _keyFrameTime[0] = 4.8f; // time to complete route 1
            _keyFrameTime[1] = 2.8f; // time to complete route 2
            _keyFrameTime[2] = 4.8f; // time to complete route 3
            _keyFrameTime[3] = 2.8f; // time to complete route 4
        }

        private int KeyFrameNumber()
        {
            float timeLapsed = 0.0f;

            // retrieve current leg of trip
            for (int i = 0; i < NUM_KEYFRAMES; i++)
            {
                if (timeLapsed > _tripTime)
                    return i - 1;
                else
                    timeLapsed += _keyFrameTime[i];
            }
            return 3;               // special case for last route
        }

        private Vector3 GetPositionOnCurve(Vector3[] bezier, float fraction)
        {
            // returns absolute position on curve based on relative
            // position on curve (relative position ranges from 0% to 100%)
            return bezier[0] * (1.0f - fraction) * (1.0f - fraction) * (1.0f - fraction) +
                    bezier[1] * 3.0f * fraction * (1.0f - fraction) * (1.0f - fraction) +
                    bezier[2] * 3.0f * fraction * fraction * (1.0f - fraction) +
                    bezier[3] * fraction * fraction * fraction;
        }

        private Vector3 GetPositionOnLine(Vector3[] line, float fraction)
        {
            // returns absolute position on line based on relative position
            // on curve (relative position ranges from 0% to 100%)
            Vector3 lineAtOrigin = line[1] - line[0];
            return line[0] + fraction * lineAtOrigin;
        }

        private void UpdateKeyframeAnimation(GameTime gameTime)
        {
            // update total trip time, use modulus to prevent variable overflow
            _tripTime += (gameTime.ElapsedGameTime.Milliseconds / 1000.0f);
            _tripTime = _tripTime % TOTAL_TRIP_TIME;

            // get the current route number from a total of four routes
            int routeNum = KeyFrameNumber();

            // sum times for preceding keyframes
            float keyFrameStartTime = 0.0f;

            for (int i = 0; i < routeNum; i++)
                keyFrameStartTime += _keyFrameTime[i];

            // calculate time spent during current route
            float timeBetweenKeys = _tripTime - keyFrameStartTime;

            // calculate percentage of current route completed
            float fraction = timeBetweenKeys / _keyFrameTime[routeNum];

            // get current X, Y, Z of object being animated
            // find point on line or curve by passing in % completed
            switch (routeNum)
            {
                case 0: // first curve
                    _currentPosition = GetPositionOnCurve(_bezierA, fraction);
                    break;
                case 1: // first line
                    _currentPosition = GetPositionOnCurve(_newbezierA, fraction);
                    break;
                case 2: // 2nd curve
                    _currentPosition = GetPositionOnCurve(_bezierB, fraction);
                    break;
                case 3: // 2nd line
                    _currentPosition = GetPositionOnCurve(_newbezierB, fraction);
                    break;
            }
            // get rotation angle about Y based on change in X and Z speed
            Vector3 speed = _currentPosition - _previousPosition;
            _previousPosition = _currentPosition;
            _Yrotation = (float)Math.Atan2((float)speed.X,
                                                 (float)speed.Z);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            InitializeRoutes();
            InitializeTimeLine();


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

                    vertices[35 * z + x].TextureCoordinate.X = 10f * x;
                    vertices[35 * z + x].TextureCoordinate.Y = 10f * z;

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
            _grassTexture = Content.Load<Texture2D>("Textures\\grass");

            _baseModel = Content.Load<Model>("Models\\base");
            _baseMatrix = new Matrix[_baseModel.Bones.Count];
            _baseModel.CopyAbsoluteBoneTransformsTo(_baseMatrix);

            _fanModel = Content.Load<Model>("Models\\fan");
            _fanMatrix = new Matrix[_fanModel.Bones.Count];
            _fanModel.CopyAbsoluteBoneTransformsTo(_fanMatrix);

            // load jet
            _jetModel = Content.Load<Model>("Models\\cf18");
            _jetMatrix = new Matrix[_jetModel.Bones.Count];
            _jetModel.CopyAbsoluteBoneTransformsTo(_jetMatrix);


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

                    BoundingSphere bs = _baseModel.Meshes[1].BoundingSphere.Transform(Matrix.CreateScale(5) * Matrix.CreateTranslation(0.0f, 0.52f, -4.0f));

                    if (bs.Contains(bullet.Position) == ContainmentType.Contains)
                    {
                        _rotatingClockwise = !_rotatingClockwise;
                        bullet.IsDead = true;
                    }


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


                // Update jet position
                UpdateKeyframeAnimation(gameTime);


                Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
            }


            _fadeBox.Update(gameTime);
        }


        void DrawWindmill(Model model, int modelNum, GameTime gameTime)
        {
            // CODE CHANGE: Removed culling instructions to avoid unwanted clipping 
            //              model when converting from MilkShape .fbx to Blender .fbx.

            //graphics.GraphicsDevice.RenderState.CullMode // don't draw backface
            //         = CullMode.CullClockwiseFace;       // when many vertices

            foreach (ModelMesh mesh in model.Meshes)
            {
                // 1: declare matrices
                Matrix world, scale, rotationZ, translation;

                // 2: initialize matrices
                scale = Matrix.CreateScale(5.0f, 5.0f, 5.0f);
                translation = Matrix.CreateTranslation(0.0f, 0.52f, -4.0f);
                rotationZ = Matrix.CreateRotationY(0.0f);

                if (modelNum == WINDMILL_FAN)
                {
                    translation = Matrix.CreateTranslation(0.0f, 0.7f, -4.0f);
                    // calculate time between frames for system independent speed
                    _fanRotation += (float)gameTime.ElapsedGameTime.TotalSeconds * 10;

                    // prevent var overflow - store remainder
                    _fanRotation = _fanRotation % (2.0f * (float)Math.PI);

                    float rot = _fanRotation;
                    if (_rotatingClockwise)
                        rot = -rot;

                    rotationZ = Matrix.CreateRotationY(rot);
                }

                // 3: build cumulative world matrix using I.S.R.O.T. sequence
                // identity, scale, rotate, orbit(translate&rotate), translate
                world = scale * rotationZ * translation;

                // 4: set shader parameters
                foreach (BasicEffect effect in mesh.Effects)
                {
                    if (modelNum == WINDMILL_BASE)
                        effect.World = _baseMatrix[mesh.ParentBone.Index] * world;
                    if (modelNum == WINDMILL_FAN)
                        effect.World = _fanMatrix[mesh.ParentBone.Index] * world;

                    effect.View = Camera.View;
                    effect.Projection = Camera.Projection;
                    effect.EnableDefaultLighting();
                }
                // 5: draw object
                mesh.Draw();

                //if (modelNum == WINDMILL_BASE)
                //    BoundingSphereRenderer.Render(mesh.BoundingSphere, Matrix.Identity * world, Camera.View, Camera.Projection);
            }
        }


        private void DrawCF18(Model model)
        {
            // 1: declare matrices
            Matrix scale, translate, rotateX, rotateY, world;

            // 2: initialize matrices
            translate = Matrix.CreateTranslation(_currentPosition);
            scale = Matrix.CreateScale(0.1f, 0.1f, 0.1f);
            rotateX = Matrix.CreateRotationX(0.0f);
            rotateY = Matrix.CreateRotationY(_Yrotation);

            // 3: build cumulative world matrix using I.S.R.O.T. sequence
            // identity, scale, rotate, orbit(translate & rotate), translate
            world = scale * rotateX * rotateY * translate;

            // set shader parameters
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = _jetMatrix[mesh.ParentBone.Index] * world;
                    effect.View = Camera.View;
                    effect.Projection = Camera.Projection;
                    effect.EnableDefaultLighting();
                    effect.SpecularColor = new Vector3(0.0f, 0.0f, 0.0f);

                    if (KeyFrameNumber() == 1)
                    {
                        effect.SpecularColor = new Vector3(1.0f, 0.0f, 0.0f);
                    }
                    else if (KeyFrameNumber() == 2)
                    {
                        effect.SpecularColor = new Vector3(0.0f, 1.0f, 0.0f);
                    }
                    else if (KeyFrameNumber() == 3)
                    {
                        effect.SpecularColor = new Vector3(0.0f, 0.0f, 1.0f);
                    }
                }
                mesh.Draw();
            }
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

            DrawCF18(_jetModel);

            foreach (Bullet bullet in _bullets)
            {
                bullet.Draw(gameTime);
            }

            base.Draw(gameTime);

            DrawWindmill(_baseModel, WINDMILL_BASE, gameTime);
            DrawWindmill(_fanModel, WINDMILL_FAN, gameTime);

            _be.Projection = Camera.Projection;
            _be.View = Camera.View;
            _be.Texture = _grassTexture;
            _be.TextureEnabled = true;
            _be.EnableDefaultLighting();
            _be.AmbientLightColor = new Vector3(0.8f, 0.8f, 0.6f);
            _be.SpecularColor = new Vector3(0.0f, 0.0f, 0.0f);
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


        protected void FireShots(GameTime gameTime)
        {
            if (_shotCountdown <= 0)
            {
                // Did player press space bar or left mouse button?
                if (Keyboard.GetState().IsKeyDown(Keys.Space) ||
                    Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    Vector3 bulletVelocity = Camera.Target - Camera.Position;
                    bulletVelocity.Normalize();
                    bulletVelocity *= 10.0f;

                    Bullet newBullet = new Bullet(this, Camera.Position, bulletVelocity);
                    newBullet.Initialize();
                    _bullets.Add(newBullet);

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
