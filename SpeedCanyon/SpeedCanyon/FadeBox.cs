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
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class FadeBox : DrawableGameComponent
    {
        public enum FadeState { Idle, FadingIn, FadingOut }
        FadeState _fadeState = FadeState.Idle;

        public FadeState State { get { return _fadeState; } }

        float _opacity = 1.0f;
        float _fadeDuration = 1000.0f; // In milliseconds

        VertexPositionColor[] _vertices;
        BasicEffect _effect;


        public FadeBox(Game game)
            : base(game)
        {
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _vertices = new VertexPositionColor[4];


            _vertices[0].Position = new Vector3(-1.0f, -1.0f, 0);
            _vertices[1].Position = new Vector3(-1.0f, 1.0f, 0);
            _vertices[2].Position = new Vector3(1.0f, -1.0f, 0);
            _vertices[3].Position = new Vector3(1.0f, 1.0f, 0);
            //_vertices[0].Position = new Vector3(-0.9f, -0.9f, 0);
            //_vertices[1].Position = new Vector3(-0.9f, 0.9f, 0);
            //_vertices[2].Position = new Vector3(0.9f, -0.9f, 0);
            //_vertices[3].Position = new Vector3(0.9f, 0.9f, 0);

            Color c = new Color(0.0f, 0.0f, 0.0f, _opacity);
            _vertices[0].Color = c;
            _vertices[1].Color = c;
            _vertices[2].Color = c;
            _vertices[3].Color = c;

            _effect = new BasicEffect(GraphicsDevice);
            _effect.VertexColorEnabled = true;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (_fadeState != FadeState.Idle)
            {
                float opacityDelta = (float)gameTime.ElapsedGameTime.TotalMilliseconds / _fadeDuration;

                switch (_fadeState)
                {
                    case FadeState.FadingIn:
                        _opacity -= opacityDelta;

                        if (_opacity <= 0)
                        {
                            _opacity = 0;
                            _fadeState = FadeState.Idle;
                        }
                        break;

                    case FadeState.FadingOut:
                        _opacity += opacityDelta;

                        if (_opacity >= 1)
                        {
                            _opacity = 1;
                            _fadeState = FadeState.Idle;
                        }
                        break;

                    case FadeState.Idle:
                    default:
                        break;
                }

                Color c = new Color(0.0f, 0.0f, 0.0f, _opacity);
                _vertices[0].Color = c;
                _vertices[1].Color = c;
                _vertices[2].Color = c;
                _vertices[3].Color = c;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            _effect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawUserPrimitives(
                PrimitiveType.TriangleStrip,
                _vertices,
                0,
                2,
                VertexPositionColor.VertexDeclaration);
        }

        public void FadeOut(float duration = 1000.0f)
        {
            _fadeDuration = duration;
            _fadeState = FadeState.FadingOut;
        }

        public void FadeIn(float duration = 1000.0f)
        {
            _fadeDuration = duration;
            _fadeState = FadeState.FadingIn;
        }

    }
}
