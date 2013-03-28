using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace SpeedCanyon
{

    public abstract class Camera : GameComponent
    {
        protected float _yaw = -(float)Math.PI / 2;
        protected const float _maxPitch = (float)(89.9 * Math.PI / 180);
        protected float _pitch = 0;

        //Camera matrices
        public Matrix View { get; protected set; }
        public Matrix Projection { get; protected set; }

        // Camera vectors
        public Vector3 Position { get; protected set; }
        public Vector3 Target { get; protected set; }
        public Vector3 Up { get; protected set; }

        protected Point ScreenCenter { get; private set; }


        public Camera(Game game)
            : this(game, Vector3.Zero, Vector3.Zero, Vector3.Up)
        {
        }

        public Camera(Game game, Vector3 pos, Vector3 target, Vector3 up)
            : base(game)
        {
            Position = pos;
            Target = target;
            Up = up;

        }


        public override void Initialize()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                (float)Game.Window.ClientBounds.Width /
                (float)Game.Window.ClientBounds.Height,
                0.1f, 300);

            ScreenCenter = new Point(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);

            // Set mouse position and do initial get state
            Mouse.SetPosition(ScreenCenter.X, ScreenCenter.Y);


            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            View = Matrix.CreateLookAt(Position, Target, Up);

            base.Update(gameTime);
        }
    }
}