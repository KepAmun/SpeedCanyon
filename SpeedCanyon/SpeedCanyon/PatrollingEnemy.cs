using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpeedCanyon
{
    class PatrollingEnemy : BasicModel
    {
        Vector3[] _patrol;
        Vector3 _destination;
        Vector3 _position;
        //Vector3 _velocity = new Vector3(0, 0, 0);
        Vector3 _direction = new Vector3(0, 0, 0); // (pitch, yaw, roll)
        //float _maxSpeed;

        public PatrollingEnemy(Model m)
            : base(m)
        {
            _patrol = new Vector3[2];
            _patrol[0] = new Vector3(20, 20, 0);
            _patrol[1] = new Vector3(20, 20, -400);

            _position = _patrol[0];
            _destination = _patrol[1];
            _direction.Y = MathHelper.Pi;
        }

        public override void Update()
        {
            if (_position.Z > _destination.Z)
            {
                _position.Z -= 2;
                if (_position.Z <= _destination.Z)
                {
                    _position.Z = _destination.Z;
                    _destination = _patrol[0];
                    _direction.Y = 0;
                }
            }
            else
            {
                _position.Z += 2;
                if (_position.Z >= _destination.Z)
                {
                    _position.Z = _destination.Z;
                    _destination = _patrol[1];
                    _direction.Y = MathHelper.Pi;
                }
            }

        }

        public override Matrix GetWorld()
        {
            Matrix m = _world * 
                Matrix.CreateFromYawPitchRoll(_direction.Y, _direction.X, _direction.Z) * 
                Matrix.CreateTranslation(_position);

            return m;
        }
    }
}
