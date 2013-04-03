using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace SpeedCanyon
{
    class TankControllerHuman : TankController
    {
        protected const float _maxPitch = (float)(89.9 * Math.PI / 180);

        public TankControllerHuman(Game1 game, Tank tank)
            : base(game, tank)
        {
        }

        protected override void SetCommands()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            Tank.MoveDirection moveDirection = Tank.MoveDirection.None;

            if (keyboardState.IsKeyDown(Keys.S))
            {
                moveDirection--;
            }

            if (keyboardState.IsKeyDown(Keys.W))
            {
                moveDirection++;
            }

            _tank.Throttle = moveDirection;



            Tank.TurnDirection turnDirection = Tank.TurnDirection.None;
            if (keyboardState.IsKeyDown(Keys.A))
            {
                turnDirection--;
            }

            if (keyboardState.IsKeyDown(Keys.D))
            {
                turnDirection++;
            }

            _tank.Steering = turnDirection;



            float dx = mouseState.X - (Game.Window.ClientBounds.Width / 2);
            float dy = mouseState.Y - (Game.Window.ClientBounds.Height / 2);

            _tank.TargetTurretYaw = MathHelper.WrapAngle(_tank.TargetTurretYaw + dx * 0.002f);

            _tank.TargetTurretPitch = MathHelper.Clamp(_tank.TargetTurretPitch + dy * 0.002f, -_maxPitch, _maxPitch);


            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                _tank.FireCannon = true;
            }
            else
            {
                _tank.FireCannon = false;
            }

        }
    }
}
