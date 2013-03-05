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
    public class FreeCamera : Camera
    {
        public FreeCamera(Game game, Vector3 pos, Vector3 target, Vector3 up)
            : base(game, pos, target, up)
        {
            // TODO: Construct any child components here
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

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            float dx = mouseState.X - ScreenCenter.X;
            float dy = mouseState.Y - ScreenCenter.Y;

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


            base.Update(gameTime);
        }
    }
}
