using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace SpeedCanyon
{
    class SpinningEnemy : BasicModel
    {
        Matrix _rotation = Matrix.Identity;

        // Rotation and movement variables
        float _yawAngle = 0;
        float _pitchAngle = 0;
        float _rollAngle = 0;
        Vector3 _direction;

        public SpinningEnemy(Model m, Vector3 Position,
            Vector3 Direction, float yaw, float pitch, float roll)
            : base(m)
        {
            _world = Matrix.CreateTranslation(Position);
            _yawAngle = yaw;
            _pitchAngle = pitch;
            _rollAngle = roll;
            _direction = Direction;
        }

        public override void Update()
        {
            // Rotate model
            _rotation *= Matrix.CreateFromYawPitchRoll(_yawAngle, _pitchAngle, _rollAngle);

            // Move model
            _world *= Matrix.CreateTranslation(_direction);
        }

        public override Matrix GetWorld()
        {
            return _rotation * _world;
        }
    }
}