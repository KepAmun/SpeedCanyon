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
        public Tank Owner { get; protected set; }
        Model _model;

        public bool IsDead { get; set; }

        public Bullet(Game game, Tank owner, Vector3 position, Vector3 velocity)
            : base(game)
        {
            Owner = owner;
            Position = position;
            Velocity = velocity;

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
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;// *0.1f;

            Velocity -= new Vector3(0, timeDelta * 9.81f, 0);

            Position += Velocity * timeDelta;

            if (Position.Y < ((Game1)Game).CellHeight(Position))
            {
                IsDead = true;
                AudioEmitter emitter = new AudioEmitter();
                emitter.Position = Position;
                ((Game1)Game).PlayCue("bulletImpactGround", emitter);
                ((Game1)Game).AddExplosion(Position);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix scale, translate, rotation, world;

            translate = Matrix.CreateTranslation(Position);
            scale = Matrix.CreateScale(0.1f);

            Vector3 v = Velocity;
            v.Normalize();
            float yaw = -(float)Math.Atan2(v.Z, v.X);
            float pitch = (float)Math.Asin(v.Y);

            //rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, 0); // TODO: Figure out why this is not working
            rotation = Matrix.CreateFromYawPitchRoll(yaw, 0, pitch); // and this is

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
                    effect.SpecularColor = new Vector3(1.0f, 1.0f, 0.3f);
                    effect.SpecularPower = 20;
                }

                mesh.Draw();
                //BoundingSphere bs = mesh.BoundingSphere;
                //bs.Center = Vector3.Zero;
                //((Game1)Game).BoundingSphereRenderer.Render(bs, world, ((Game1)Game).Camera.View, ((Game1)Game).Camera.Projection);
            }

            base.Draw(gameTime);
        }

    }
}
