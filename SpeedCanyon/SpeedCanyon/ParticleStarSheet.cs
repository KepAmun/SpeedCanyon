using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpeedCanyon
{
    class ParticleStarSheet
    {
        // Particle arrays and vertex buffer
        VertexPositionTexture[] _verts;
        Color[] _vertexColorArray;
        VertexBuffer _particleVertexBuffer;

        // Behavior variables
        Vector3 _maxPosition;
        int _maxParticles;
        static Random _rnd = new Random();

        // Vertex and graphics info
        GraphicsDevice _graphicsDevice;

        // Settings
        ParticleSettings _particleSettings;

        // Effect
        Effect _particleEffect;

        // Textures
        Texture2D _particleColorsTexture;


        public ParticleStarSheet(GraphicsDevice graphicsDevice,
            Vector3 maxPosition, int maxParticles, Texture2D particleColorsTexture, 
            ParticleSettings particleSettings, Effect particleEffect)
        {
            _maxParticles = maxParticles;
            _graphicsDevice = graphicsDevice;
            _particleSettings = particleSettings;
            _particleEffect = particleEffect;
            _particleColorsTexture = particleColorsTexture;
            _maxPosition = maxPosition;

            InitializeParticleVertices();

        }

        private void InitializeParticleVertices()
        {
            // Instantiate all particle arrays
            _verts = new VertexPositionTexture[_maxParticles * 4];
            _vertexColorArray = new Color[_maxParticles];

            // Get color data from colors texture
            Color[] colors = new Color[_particleColorsTexture.Width * _particleColorsTexture.Height];
            _particleColorsTexture.GetData(colors);

            // Loop until max particles
            for (int i = 0; i < _maxParticles; ++i)
            {
                float size = (float)_rnd.NextDouble() * _particleSettings.maxSize;

                Vector3 position = new Vector3(
                    _rnd.Next(-(int)_maxPosition.X, (int)_maxPosition.X),
                    _rnd.Next(-(int)_maxPosition.Y, (int)_maxPosition.Y),
                    _maxPosition.Z);

                // Set position and size of particle
                _verts[i * 4] = new VertexPositionTexture(position, new Vector2(0, 0));
                _verts[(i * 4) + 1] = new VertexPositionTexture(new Vector3(position.X, position.Y + size, position.Z), new Vector2(0, 1));
                _verts[(i * 4) + 2] = new VertexPositionTexture(new Vector3(position.X + size, position.Y, position.Z), new Vector2(1, 0));
                _verts[(i * 4) + 3] = new VertexPositionTexture(new Vector3(position.X + size, position.Y + size, position.Z), new Vector2(1, 1));

                // Set color of particle by getting a random color from the texture
                _vertexColorArray[i] = colors[(_rnd.Next(0, _particleColorsTexture.Height) * _particleColorsTexture.Width) + _rnd.Next(0, _particleColorsTexture.Width)];

            }

            // Instantiate vertex buffer
            _particleVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionTexture), _verts.Length, BufferUsage.None);

        }


        public void Draw(Camera camera)
        {
            //_graphicsDevice.SetVertexBuffer(_particleVertexBuffer);

            for (int i = 0; i < _maxParticles; ++i)
            {
                _particleEffect.Parameters["WorldViewProjection"].SetValue(
                    camera.View * camera.Projection);
                _particleEffect.Parameters["particleColor"].SetValue(_vertexColorArray[i].ToVector4());

                // Draw particles
                foreach (EffectPass pass in _particleEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    _graphicsDevice.DrawUserPrimitives<VertexPositionTexture>(
                        PrimitiveType.TriangleStrip,
                        _verts, i * 4, 2);

                }
            }
        }
    }
}
