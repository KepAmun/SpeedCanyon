using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpeedCanyon
{
    class ParticleExplosion
    {
        // Particle arrays and vertex buffer
        VertexPositionTexture[] _verts;
        Vector3[] _vertexDirectionArray;
        Color[] _vertexColorArray;
        VertexBuffer _particleVertexBuffer;

        // Position
        Vector3 _position;

        // Life
        int _lifeLeft;

        // Rounds and particle counts
        int _numParticlesPerRound;
        int _maxParticles;
        static Random _rnd = new Random();
        int _roundTime;
        int _timeSinceLastRound = 0;

        // Vertex and graphics info
        GraphicsDevice _graphicsDevice;

        // Settings
        ParticleSettings _particleSettings;

        // Effect
        Effect _particleEffect;

        // Textures
        Texture2D _particleColorsTexture;

        // Array indices
        int _endOfLiveParticlesIndex = 0;
        int _endOfDeadParticlesIndex = 0;

        public bool IsDead
        {
            get { return _endOfDeadParticlesIndex == _maxParticles; }
        }

        public ParticleExplosion(GraphicsDevice graphicsDevice, Vector3 position,
            int lifeLeft, int roundTime, int numParticlesPerRound, int maxParticles,
            Texture2D particleColorsTexture, ParticleSettings particleSettings, Effect particleEffect)
        {
            this._position = position;
            this._lifeLeft = lifeLeft;
            this._numParticlesPerRound = numParticlesPerRound;
            this._maxParticles = maxParticles;
            this._roundTime = roundTime;
            this._graphicsDevice = graphicsDevice;
            this._particleSettings = particleSettings;
            this._particleEffect = particleEffect;
            this._particleColorsTexture = particleColorsTexture;

            InitializeParticleVertices();

        }

        private void InitializeParticleVertices()
        {
            // Instantiate all particle arrays
            _verts = new VertexPositionTexture[_maxParticles * 4];
            _vertexDirectionArray = new Vector3[_maxParticles];
            _vertexColorArray = new Color[_maxParticles];

            // Get color data from colors texture
            Color[] colors = new Color[_particleColorsTexture.Width * _particleColorsTexture.Height];
            _particleColorsTexture.GetData(colors);

            // Loop until max particles
            for (int i = 0; i < _maxParticles; ++i)
            {
                float size = (float)_rnd.NextDouble() * _particleSettings.maxSize;

                // Set position, direction and size of particle
                _verts[i * 4] = new VertexPositionTexture(_position, new Vector2(0, 0));
                _verts[(i * 4) + 1] = new VertexPositionTexture(new Vector3(_position.X, _position.Y + size, _position.Z), new Vector2(0, 1));
                _verts[(i * 4) + 2] = new VertexPositionTexture(new Vector3(_position.X + size, _position.Y, _position.Z), new Vector2(1, 0));
                _verts[(i * 4) + 3] = new VertexPositionTexture(new Vector3(_position.X + size, _position.Y + size, _position.Z), new Vector2(1, 1));

                // Create a random velocity/direction
                Vector3 direction = new Vector3(
                    (float)_rnd.NextDouble() * 2 - 1,
                    (float)_rnd.NextDouble() * 2 - 1,
                    (float)_rnd.NextDouble() * 2 - 1);
                direction.Normalize();

                // Multiply by NextDouble to make sure that
                // all particles move at random speeds
                direction *= (float)_rnd.NextDouble() * 0.2f;

                // Set direction of particle
                _vertexDirectionArray[i] = direction;

                // Set color of particle by getting a random color from the texture
                _vertexColorArray[i] = colors[(_rnd.Next(0, _particleColorsTexture.Height) * _particleColorsTexture.Width) + _rnd.Next(0, _particleColorsTexture.Width)];

            }

            // Instantiate vertex buffer
            _particleVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionTexture), _verts.Length, BufferUsage.None);

        }

        public void Update(GameTime gameTime)
        {
            // Decrement life left until it's gone
            if (_lifeLeft > 0)
                _lifeLeft -= gameTime.ElapsedGameTime.Milliseconds;

            // Time for new round?
            _timeSinceLastRound += gameTime.ElapsedGameTime.Milliseconds;
            if (_timeSinceLastRound > _roundTime)
            {
                // New round - add and remove particles
                _timeSinceLastRound -= _roundTime;

                // Increment end of live particles index each
                // round until end of list is reached
                if (_endOfLiveParticlesIndex < _maxParticles)
                {
                    _endOfLiveParticlesIndex += _numParticlesPerRound;
                    if (_endOfLiveParticlesIndex > _maxParticles)
                        _endOfLiveParticlesIndex = _maxParticles;
                }

                if (_lifeLeft <= 0)
                {
                    // Increment end of dead particles index each
                    // round until end of list is reached
                    if (_endOfDeadParticlesIndex < _maxParticles)
                    {
                        _endOfDeadParticlesIndex += _numParticlesPerRound;
                        if (_endOfDeadParticlesIndex > _maxParticles)
                            _endOfDeadParticlesIndex = _maxParticles;
                    }
                }
            }

            // Update positions of all live particles
            for (int i = _endOfDeadParticlesIndex;
                i < _endOfLiveParticlesIndex; ++i)
            {
                _vertexDirectionArray[i].Y -= 0.01f;

                _verts[i * 4].Position += _vertexDirectionArray[i];
                _verts[(i * 4) + 1].Position += _vertexDirectionArray[i];
                _verts[(i * 4) + 2].Position += _vertexDirectionArray[i];
                _verts[(i * 4) + 3].Position += _vertexDirectionArray[i];

            }
        }

        public void Draw(Camera camera)
        {
            _graphicsDevice.SetVertexBuffer(_particleVertexBuffer);
            RasterizerState prevState = _graphicsDevice.RasterizerState;
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Only draw if there are live particles
            if (_endOfLiveParticlesIndex - _endOfDeadParticlesIndex > 0)
            {
                for (int i = _endOfDeadParticlesIndex; i < _endOfLiveParticlesIndex; ++i)
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

            _graphicsDevice.RasterizerState = prevState;
        }
    }
}
