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

        IndexBuffer _ib;
        VertexBuffer _vb;
        BasicEffect _be;

        VertexPositionColorTexture[]
            groundVertices = new VertexPositionColorTexture[4];
        private Texture2D grassTexture;

        Model baseModel; Model fanModel;
        Matrix[] fanMatrix; Matrix[] baseMatrix;
        const int WINDMILL_BASE = 0;
        const int WINDMILL_FAN = 1;
        private float fanRotation = 0.0f;

        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;


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
        public bool Paused { get; private set; }
        bool _escReleased = true;

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
            _be = new BasicEffect(GraphicsDevice);
            _be.VertexColorEnabled = true;

            _vb = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, 2100, BufferUsage.None);
            VertexPositionColor[] vertices = new VertexPositionColor[2100];


            for (int z = 0; z < 60; z++)
            {
                for (int x = 0; x < 35; x++)
                {
                    vertices[35 * z + x].Position = new Vector3(0.5f * (x-17), 0, 0.5f * (z-30));
                    Color c = new Color();

                    if (x % 2 == 0)
                        c.B = 127;
                    else
                        c.B = 63;

                    if (z % 2 == 0)
                        c.G = 127;
                    else
                        c.G = 63;

                    vertices[35 * z + x].Color = c;
                }
            }
            _vb.SetData<VertexPositionColor>(vertices);

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

            // Initialize Camera
            Camera = new Camera(this,
                new Vector3(0, 1, 0),
                Vector3.Zero,
                Vector3.Up);
            Components.Add(Camera);


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
            grassTexture = Content.Load<Texture2D>("Textures\\grass");

            baseModel = Content.Load<Model>("Models\\base");
            baseMatrix = new Matrix[baseModel.Bones.Count];
            baseModel.CopyAbsoluteBoneTransformsTo(baseMatrix);

            fanModel = Content.Load<Model>("Models\\fan");
            fanMatrix = new Matrix[fanModel.Bones.Count];
            fanModel.CopyAbsoluteBoneTransformsTo(fanMatrix);


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

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
                if (_escReleased)
                {
                    Paused = !Paused;
                    _escReleased = false;
                }
            }
            else
            {
                _escReleased = true;
            }


            // See if the player has fired a shot
            FireShots(gameTime);


            base.Update(gameTime);


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
                    fanRotation += (float)gameTime.ElapsedGameTime.TotalSeconds * 10;

                    // prevent var overflow - store remainder
                    fanRotation = fanRotation % (2.0f * (float)Math.PI);
                    rotationZ = Matrix.CreateRotationY(-fanRotation);
                }

                // 3: build cumulative world matrix using I.S.R.O.T. sequence
                // identity, scale, rotate, orbit(translate&rotate), translate
                world = scale * rotationZ * translation;

                // 4: set shader parameters
                foreach (BasicEffect effect in mesh.Effects)
                {
                    if (modelNum == WINDMILL_BASE)
                        effect.World = baseMatrix[mesh.ParentBone.Index] * world;
                    if (modelNum == WINDMILL_FAN)
                        effect.World = fanMatrix[mesh.ParentBone.Index] * world;

                    effect.View = Camera.View;
                    effect.Projection = Camera.Projection;
                    effect.EnableDefaultLighting();
                }
                // 5: draw object
                mesh.Draw();
            }
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);

            DrawWindmill(baseModel, WINDMILL_BASE, gameTime);
            DrawWindmill(fanModel, WINDMILL_FAN, gameTime);

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            _be.Projection = Camera.Projection;
            _be.View = Camera.View;
            _be.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.Indices = _ib;
            GraphicsDevice.SetVertexBuffer(_vb);
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2100, 0, 4200);

            // Set suitable renderstates for drawing a 3D model
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;


            _spriteBatch.Begin();

            _spriteBatch.Draw(_crosshairTexture,
                new Vector2((Window.ClientBounds.Width / 2)
                    - (_crosshairTexture.Width / 2),
                    (Window.ClientBounds.Height / 2)
                    - (_crosshairTexture.Height / 2)),
                    Color.White);

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
            if (!_muted)
                _soundBank.PlayCue(name);
        }

        public void PlayCue(Cue cue)
        {
            if (!_muted)
                cue.Play();
        }
    }
}
