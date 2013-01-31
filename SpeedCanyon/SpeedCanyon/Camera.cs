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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace SpeedCanyon
{

    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        //Camera matrices
        public Matrix _view { get; protected set; }
        public Matrix _projection { get; protected set; }

        // Camera vectors
        public Vector3 _cameraPosition { get; protected set; }
        Vector3 _cameraDirection;
        Vector3 _cameraUp;

        // Speed
        float _speed = 3;

        // Mouse stuff
        MouseState _prevMouseState;

        public Camera(Game game, Vector3 pos, Vector3 target, Vector3 up)
            : base(game)
        {
            // Build camera view matrix
            _cameraPosition = pos;
            _cameraDirection = target - pos;
            _cameraDirection.Normalize();
            _cameraUp = up;
            CreateLookAt();


            _projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                (float)Game.Window.ClientBounds.Width /
                (float)Game.Window.ClientBounds.Height,
                1, 3000);
        }

        public override void Initialize()
        {
            // Set mouse position and do initial get state
            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2,
                Game.Window.ClientBounds.Height / 2);
            _prevMouseState = Mouse.GetState();

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            Vector3 groundCameraDirection = _cameraDirection;
            groundCameraDirection.Y = 0;
            groundCameraDirection.Normalize();

            // Move forward/backward
            if (Keyboard.GetState().IsKeyDown(Keys.W))
                _cameraPosition += groundCameraDirection * _speed;
            if (Keyboard.GetState().IsKeyDown(Keys.S))
                _cameraPosition -= groundCameraDirection * _speed;
            // Move side to side
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                _cameraPosition += Vector3.Cross(_cameraUp, groundCameraDirection) * _speed;
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                _cameraPosition -= Vector3.Cross(_cameraUp, groundCameraDirection) * _speed;

            // Yaw rotation
            _cameraDirection = Vector3.Transform(_cameraDirection,
                Matrix.CreateFromAxisAngle(_cameraUp, (-MathHelper.PiOver4 / 150) *
                (Mouse.GetState().X - _prevMouseState.X)));

            // Roll rotation
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                _cameraUp = Vector3.Transform(_cameraUp,
                    Matrix.CreateFromAxisAngle(_cameraDirection,
                    MathHelper.PiOver4 / 45));
            }
            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                _cameraUp = Vector3.Transform(_cameraUp,
                    Matrix.CreateFromAxisAngle(_cameraDirection,
                    -MathHelper.PiOver4 / 45));
            }

            // Pitch rotation
            _cameraDirection = Vector3.Transform(_cameraDirection,
                Matrix.CreateFromAxisAngle(Vector3.Cross(_cameraUp, _cameraDirection),
                (MathHelper.PiOver4 / 100) *
                (Mouse.GetState().Y - _prevMouseState.Y)));

            //cameraUp = Vector3.Transform(cameraUp,
            //    Matrix.CreateFromAxisAngle(Vector3.Cross(cameraUp, cameraDirection),
            //    (MathHelper.PiOver4 / 100) *
            //    (Mouse.GetState().Y - prevMouseState.Y)));

            // Reset prevMouseState
            _prevMouseState = Mouse.GetState();

            // Recreate the camera view matrix
            CreateLookAt();

            base.Update(gameTime);
        }

        private void CreateLookAt()
        {
            _view = Matrix.CreateLookAt(_cameraPosition,
                _cameraPosition + _cameraDirection, _cameraUp);
        }
    }
}
