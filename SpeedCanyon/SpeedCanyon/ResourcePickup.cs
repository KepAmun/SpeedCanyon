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
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ResourcePickup : DrawableGameComponent, IGameObject
    {
        Model _model;
        Texture2D _texture;

        Vector3 _position;

        public bool IsDead { get; set; }

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }


        public new Game1 Game { get { return (Game1)base.Game; } }


        public ResourcePickup(Game game, Vector3 position)
            : base(game)
        {
            _position = position;

            _position.Y = Game.CellHeight(_position);

            IsDead = false;
        }


        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }


        protected override void LoadContent()
        {
            _model = Game.Content.Load<Model>("Models\\barrel");
            _texture = Game.Content.Load<Texture2D>("Models\\textures\\barrel_3_diffuse");

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                    effect.World = Matrix.CreateScale(0.3f) * Matrix.CreateTranslation(_position);
                    effect.View = Game.Camera.View;
                    effect.Projection = Game.Camera.Projection;
                    effect.TextureEnabled = true;
                    effect.Texture = _texture;

                    effect.LightingEnabled = true;
                    effect.AmbientLightColor = Color.DarkGray.ToVector3();
                    effect.DiffuseColor = Color.DarkGray.ToVector3();
                    effect.SpecularColor = Color.White.ToVector3(); 
                    effect.SpecularPower = 100;
                    effect.DirectionalLight0.Direction = Game.LightDirection;
                    effect.DirectionalLight0.SpecularColor = Color.White.ToVector3();
                }

                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}
