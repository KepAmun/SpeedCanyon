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

    public class Camera : GameComponent
    {
        float _yaw = 0;
        float _maxPitch = 0.99f * MathHelper.PiOver2;
        float _pitch = 0;

        //Camera matrices
        public Matrix View { get; protected set; }
        public Matrix Projection { get; protected set; }

        // Camera vectors
        public Vector3 Position { get; protected set; }
        public Vector3 Target { get; protected set; }
        public Vector3 Up { get; protected set; }

        Point _screenCenter;


        public Camera(Game game, Vector3 pos, Vector3 target, Vector3 up)
            : base(game)
        {
            // Build camera view matrix
            Position = pos;
            Target = target;
            Up = up;


            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                (float)Game.Window.ClientBounds.Width /
                (float)Game.Window.ClientBounds.Height,
                0.1f, 10000);
        }


        public override void Initialize()
        {
            // Set mouse position and do initial get state
            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2,
                Game.Window.ClientBounds.Height / 2);

            MouseState mouseState = Mouse.GetState();
            _screenCenter = new Point(mouseState.X, mouseState.Y);


            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            float dx = mouseState.X - _screenCenter.X;
            float dy = mouseState.Y - _screenCenter.Y;

            // Yaw rotation
            float yawDelta = dx * 0.2f * (float)gameTime.ElapsedGameTime.TotalSeconds;

            _yaw = MathHelper.WrapAngle(_yaw + yawDelta);


            // Pitch rotation
            float pitchDelta = dy * 0.2f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _pitch += pitchDelta;

            if (_pitch > _maxPitch)
            {
                _pitch = _maxPitch;
            }
            else if (_pitch < -_maxPitch)
            {
                _pitch = -_maxPitch;
            }

            _pitch = MathHelper.WrapAngle(_pitch);


            Vector3 direction = new Vector3(
                (float)(Math.Cos(_pitch) * Math.Cos(_yaw)),
                (float)(-Math.Sin(_pitch)),
                (float)(Math.Cos(_pitch) * Math.Sin(_yaw)));

            Vector3 moveDirection = Vector3.Zero;
            bool movingForward = keyboardState.IsKeyDown(Keys.W) || 
                                 keyboardState.IsKeyDown(Keys.Up);
            bool movingBackward = keyboardState.IsKeyDown(Keys.S) || 
                                  keyboardState.IsKeyDown(Keys.Down);
            bool movingLeft = keyboardState.IsKeyDown(Keys.A) || 
                              keyboardState.IsKeyDown(Keys.Left);
            bool movingRight = keyboardState.IsKeyDown(Keys.D) || 
                               keyboardState.IsKeyDown(Keys.Right);

            if (movingForward || movingBackward || movingLeft || movingRight)
            {
                Vector3 fbMovement = Vector3.Zero;
                Vector3 sMovement = Vector3.Zero;

                if (movingForward || movingBackward)
                {
                    fbMovement = direction;
                    if (movingBackward)
                    {
                        fbMovement = -fbMovement;
                    }
                }

                if (movingLeft || movingRight)
                {
                    sMovement = Vector3.Cross(direction, Up);

                    if (movingLeft)
                        sMovement = -sMovement;
                }

                moveDirection = fbMovement + sMovement;
                moveDirection.Y = 0;
                moveDirection.Normalize();
                moveDirection *= 5.0f * (float)gameTime.ElapsedGameTime.TotalSeconds;

            }



            Position += moveDirection;

            Target = Position + direction;

            Mouse.SetPosition(_screenCenter.X, _screenCenter.Y);

            View = Matrix.CreateLookAt(Position, Target, Up);

            base.Update(gameTime);
        }
    }
}