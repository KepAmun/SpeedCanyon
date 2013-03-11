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
    public class Bullet : DrawableGameComponent
    {
        public Vector3 Position { get; protected set; }
        public Vector3 Velocity { get; protected set; }
        Model _model;

        Vector3 _startPosition;

        public bool IsDead { get; set; }

        public Bullet(Game game, Vector3 position, Vector3 velocity)
            : base(game)
        {
            Position = position;
            Velocity = velocity;

            _startPosition = Position;

            IsDead = false;
        }


        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }


        protected override void LoadContent()
        {
            _model = Game.Content.Load<Model>("Models\\bullet");

            base.LoadContent();
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Vector3.DistanceSquared(Position, _startPosition) > 10000)
            {
                IsDead = true;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // 1: declare matrices
            Matrix scale, translate, rotation, world;

            // 2: initialize matrices
            translate = Matrix.CreateTranslation(Position);
            scale = Matrix.CreateScale(0.02f, 0.02f, 0.02f);

            Vector3 v = Velocity;
            v.Normalize();
            float yaw = -(float)Math.Atan2(v.Z, v.X);
            float pitch = (float)Math.Asin(v.Y);

            // Bullet model is rotated the wrong way, set pre-rotate to correct it.
            // (Otherwise, pitch and roll are swapped.)
            rotation = Matrix.CreateRotationY(MathHelper.PiOver2);// *Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw - MathHelper.PiOver2);
            rotation *= Matrix.CreateFromYawPitchRoll(yaw - MathHelper.PiOver2, pitch, 0);

            // 3: build cumulative world matrix using I.S.R.O.T. sequence
            // identity, scale, rotate, orbit(translate & rotate), translate
            world = scale * rotation * translate;

            // set shader parameters
            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.View = ((Game1)Game).Camera.View;
                    effect.Projection = ((Game1)Game).Camera.Projection;
                    effect.EnableDefaultLighting();
                    effect.SpecularColor = new Vector3(0.0f, 0.0f, 0.0f);
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }

    }
}
