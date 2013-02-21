using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpeedCanyon
{
    class Skybox : DrawableGameComponent
    {
        Texture2D[] _textures;

        VertexBuffer _vertexBuffer;
        string _assetNameBase;
        Matrix _scaleTransform;

        BasicEffect _effect;

        new Game1 Game { get { return (Game1)base.Game; } }

        public Skybox(Game1 game, string assetNameBase, float scale = 5000)
            : base(game)
        {
            _assetNameBase = assetNameBase;
            _scaleTransform = Matrix.CreateScale(scale);

        }

        public override void Initialize()
        {
            base.Initialize();

            _effect = new BasicEffect(GraphicsDevice);
            _effect.World = _scaleTransform;
            _effect.TextureEnabled = true;

            _vertexBuffer = new VertexBuffer(Game.GraphicsDevice, VertexPositionTexture.VertexDeclaration, 24, BufferUsage.None);

            VertexPositionTexture[] vertices = new VertexPositionTexture[24];

            // Box Coordinates
            Vector3 nwt = new Vector3(-1, 1, 1); // nwt = North West Top
            Vector3 nwb = new Vector3(-1, -1, 1);
            Vector3 net = new Vector3(1, 1, 1);
            Vector3 neb = new Vector3(1, -1, 1);

            Vector3 swt = new Vector3(-1, 1, -1);
            Vector3 swb = new Vector3(-1, -1, -1);
            Vector3 set = new Vector3(1, 1, -1);
            Vector3 seb = new Vector3(1, -1, -1); // seb = South East Bottom

            // Texture Coordinates
            Vector2 tr = new Vector2(0, 0); // tr = Top Right
            Vector2 tl = new Vector2(1, 0);
            Vector2 br = new Vector2(0, 1);
            Vector2 bl = new Vector2(1, 1); // bl = Bottom Left

            // North Face
            vertices[0].Position = nwt; vertices[0].TextureCoordinate = tr;
            vertices[1].Position = net; vertices[1].TextureCoordinate = tl;
            vertices[2].Position = nwb; vertices[2].TextureCoordinate = br;
            vertices[3].Position = neb; vertices[3].TextureCoordinate = bl;

            // South Face                                                 
            vertices[4].Position = set; vertices[4].TextureCoordinate = tr;
            vertices[5].Position = swt; vertices[5].TextureCoordinate = tl;
            vertices[6].Position = seb; vertices[6].TextureCoordinate = br;
            vertices[7].Position = swb; vertices[7].TextureCoordinate = bl;

            // East Face                                                  
            vertices[8].Position = net; vertices[8].TextureCoordinate = tr;
            vertices[9].Position = set; vertices[9].TextureCoordinate = tl;
            vertices[10].Position = neb; vertices[10].TextureCoordinate = br;
            vertices[11].Position = seb; vertices[11].TextureCoordinate = bl;

            // West Face
            vertices[12].Position = swt; vertices[12].TextureCoordinate = tr;
            vertices[13].Position = nwt; vertices[13].TextureCoordinate = tl;
            vertices[14].Position = swb; vertices[14].TextureCoordinate = br;
            vertices[15].Position = nwb; vertices[15].TextureCoordinate = bl;

            // Top Face
            vertices[16].Position = swt; vertices[16].TextureCoordinate = tr;
            vertices[17].Position = set; vertices[17].TextureCoordinate = tl;
            vertices[18].Position = nwt; vertices[18].TextureCoordinate = br;
            vertices[19].Position = net; vertices[19].TextureCoordinate = bl;

            // Bottom Face
            vertices[20].Position = nwb; vertices[20].TextureCoordinate = tr;
            vertices[21].Position = neb; vertices[21].TextureCoordinate = tl;
            vertices[22].Position = swb; vertices[22].TextureCoordinate = br;
            vertices[23].Position = seb; vertices[23].TextureCoordinate = bl;

            _vertexBuffer.SetData<VertexPositionTexture>(vertices);

        }

        protected override void LoadContent()
        {
            _textures = new Texture2D[6];
            _textures[0] = Game.Content.Load<Texture2D>(_assetNameBase + "_north");
            _textures[1] = Game.Content.Load<Texture2D>(_assetNameBase + "_south");
            _textures[2] = Game.Content.Load<Texture2D>(_assetNameBase + "_east");
            _textures[3] = Game.Content.Load<Texture2D>(_assetNameBase + "_west");
            _textures[4] = Game.Content.Load<Texture2D>(_assetNameBase + "_top");
            _textures[5] = Game.Content.Load<Texture2D>(_assetNameBase + "_bottom");

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            for (int i = 0; i < 6; i++)
            {
                _textures[i].Dispose();
            }

            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            _effect.Projection = Game.Camera.Projection;
            _effect.View = Game.Camera.View;

            for (int i = 0; i < 6; i++)
            {
                _effect.Texture = _textures[i];

                _effect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 4 * i, 2);
            }

            base.Draw(gameTime);
        }

    }
}
