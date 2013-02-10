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

        // Mouse stuff
        MouseState _prevMouseState;

        // Max yaw/pitch variables
        float _totalYaw = MathHelper.PiOver4 / 2;
        float _currentYaw = 0;
        float _totalPitch = MathHelper.PiOver4 / 2;
        float _currentPitch = 0;

        public Vector3 GetCameraDirection
        {
            get { return _cameraDirection; }
        }

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
            // Yaw rotation
            float yawAngle = (-MathHelper.PiOver4 / 150) *
                    (Mouse.GetState().X - _prevMouseState.X);

            if (Math.Abs(_currentYaw + yawAngle) < _totalYaw)
            {
                _cameraDirection = Vector3.Transform(_cameraDirection,
                    Matrix.CreateFromAxisAngle(_cameraUp, yawAngle));
                _currentYaw += yawAngle;
            }

            // Pitch rotation
            float pitchAngle = (MathHelper.PiOver4 / 150) *
                (Mouse.GetState().Y - _prevMouseState.Y);

            if (Math.Abs(_currentPitch + pitchAngle) < _totalPitch)
            {
                _cameraDirection = Vector3.Transform(_cameraDirection,
                    Matrix.CreateFromAxisAngle(
                        Vector3.Cross(_cameraUp, _cameraDirection),
                    pitchAngle));

                _currentPitch += pitchAngle;
            }

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