using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace SpeedCanyon
{
    class SpinningEnemy : BasicModel
    {
        Matrix rotation = Matrix.Identity;

        // Rotation and movement variables
        float yawAngle = 0;
        float pitchAngle = 0;
        float rollAngle = 0;
        Vector3 direction;

        public SpinningEnemy(Model m, Vector3 Position,
            Vector3 Direction, float yaw, float pitch, float roll)
            : base(m)
        {
            _world = Matrix.CreateTranslation(Position);
            yawAngle = yaw;
            pitchAngle = pitch;
            rollAngle = roll;
            direction = Direction;
        }

        public override void Update()
        {
            // Rotate model
            rotation *= Matrix.CreateFromYawPitchRoll(yawAngle,
                pitchAngle, rollAngle);

            // Move model
            _world *= Matrix.CreateTranslation(direction);
        }

        public override Matrix GetWorld()
        {
            return rotation * _world;
        }
    }
}