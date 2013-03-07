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
            : base(game, target.Position, target.Position, Vector3.Up)
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

            _yaw = _target.LookAngle;


            // Pitch rotation
            float pitchDelta = dy * 0.2f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _pitch += pitchDelta;

            if (_pitch > _maxPitch)
            {
                _pitch = _maxPitch;
            }
            else if (_pitch < 0.1f)
            {
                _pitch = 0.1f;
            }

            _pitch = MathHelper.WrapAngle(_pitch);

            int dd = mouseState.ScrollWheelValue - _lastScrollWheelValue;
            _distance -= dd * 0.2f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _distance = MathHelper.Clamp(_distance, MIN_DISTANCE, MAX_DISTANCE);

            float distRatio = (_distance - MIN_DISTANCE) / (MAX_DISTANCE - MIN_DISTANCE);
            float lookAheadDist = MAX_LOOK_AHEAD - MAX_LOOK_AHEAD * distRatio;

            _lastScrollWheelValue = mouseState.ScrollWheelValue;

            Vector3 offset = new Vector3(
                -_distance * (float)Math.Cos(_target.FacingAngle - _yaw) * (float)Math.Cos(_pitch),
                _distance * (float)Math.Sin(_pitch),
                -_distance * (float)Math.Sin(_target.FacingAngle - _yaw) * (float)Math.Cos(_pitch));

            Position = _target.Position + offset;

            Target = _target.Position + new Vector3(//0, lookAheadDist, 0);
                lookAheadDist * (float)Math.Cos(_target.FacingAngle - _yaw),
                0,
                lookAheadDist * (float)Math.Sin(_target.FacingAngle - _yaw));

            base.Update(gameTime);
        }
    }
}
