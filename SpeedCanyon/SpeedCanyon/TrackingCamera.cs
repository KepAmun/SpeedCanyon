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
    public class TrackingCamera : Camera
    {
        Tank _target;

        float _distance;
        const float MAX_DISTANCE = 20;
        const float MIN_DISTANCE = 3;
        const float MAX_LOOK_AHEAD = 10;

        int _lastScrollWheelValue;


        public TrackingCamera(Game game, Tank target)
            : base(game)
        {

            _target = target;

            _pitch = MathHelper.ToRadians(45);
            _yaw = 0;
            _distance = 10;
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
            //KeyboardState keyboardState = Keyboard.GetState();

            //float dx = mouseState.X - ScreenCenter.X;
            float dy = mouseState.Y - ScreenCenter.Y;

            // Yaw rotation
            //float yawDelta = dx * 0.2f * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //_yaw = MathHelper.WrapAngle(_yaw + yawDelta);

            _yaw = _target.LookYaw;


            // Pitch rotation
            float pitchDelta = dy * 0.2f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            float newPitch = MathHelper.Clamp(_pitch + pitchDelta, -_maxPitch, _maxPitch);

            float newDistance = MathHelper.Clamp(_distance - ((mouseState.ScrollWheelValue - _lastScrollWheelValue) * 0.2f * (float)gameTime.ElapsedGameTime.TotalSeconds), MIN_DISTANCE, MAX_DISTANCE);

            _lastScrollWheelValue = mouseState.ScrollWheelValue;

            float cosYaw = (float)Math.Cos(_target.FacingAngle - _yaw);
            float sinYaw = (float)Math.Sin(_target.FacingAngle - _yaw);

            Vector3 direction = new Vector3(
                (float)Math.Cos(newPitch) * cosYaw,
                -(float)Math.Sin(newPitch),
                (float)Math.Cos(newPitch) * sinYaw);

            Vector3 offset = newDistance * direction;

            Position = _target.FocalPoint - offset;

            if (Position.Y < 0.1) // Don't use the new pitch if it would move the the camera below ground
            {
                direction = new Vector3(
                    (float)Math.Cos(_pitch) * cosYaw,
                    -(float)Math.Sin(_pitch),
                    (float)Math.Cos(_pitch) * sinYaw);

                offset = _distance * direction;

                Position = _target.FocalPoint - offset;

            }
            else
            {
                _pitch = newPitch;
                _distance = newDistance;
            }


            Target = Position + direction;

            base.Update(gameTime);
        }
    }
}
