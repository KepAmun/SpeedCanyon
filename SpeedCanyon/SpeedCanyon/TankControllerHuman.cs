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
        public TankControllerHuman(Game1 game)
            : base(game)
        {

        }

        protected override void SetCommands()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            MoveDirection moveDirection = MoveDirection.None;

            if (keyboardState.IsKeyDown(Keys.S))
            {
                moveDirection--;
            }

            if (keyboardState.IsKeyDown(Keys.W))
            {
                moveDirection++;
            }

            Move = moveDirection;



            TurnDirection turnDirection = TurnDirection.None;
            if (keyboardState.IsKeyDown(Keys.A))
            {
                turnDirection--;
            }

            if (keyboardState.IsKeyDown(Keys.D))
            {
                turnDirection++;
            }

            TurnWheels = turnDirection;



            float dx = mouseState.X - (Game.Window.ClientBounds.Width / 2);

            TargetTurretAngle = MathHelper.WrapAngle(TargetTurretAngle + dx * 0.02f);



            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                FireCannon = true;
            }
            else
            {
                FireCannon = false;
            }

        }
    }
}
