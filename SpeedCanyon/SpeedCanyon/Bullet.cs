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
        Vector3 _position;
        Vector3 _velocity;
        Model _model;

        Vector3 _startPosition;

        public bool IsDead { get; private set; }

        public Bullet(Game game, Vector3 position, Vector3 velocity)
            : base(game)
        {
            _position = position;
            _velocity = velocity;

            _startPosition = _position;

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
            _model = Game.Content.Load<Model>("Models\\ammo");

            base.LoadContent();
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            _position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Vector3.DistanceSquared(_position, _startPosition) > 1000)
            {
                IsDead = true;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // 1: declare matrices
            Matrix scale, translate, rotateX, rotateY, world;

            // 2: initialize matrices
            translate = Matrix.CreateTranslation(_position);
            scale = Matrix.CreateScale(0.02f, 0.02f, 0.02f);
            rotateX = Matrix.CreateRotationX(0.0f);
            rotateY = Matrix.CreateRotationY(0.0f);

            // 3: build cumulative world matrix using I.S.R.O.T. sequence
            // identity, scale, rotate, orbit(translate & rotate), translate
            world = scale * rotateX * rotateY * translate;

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
