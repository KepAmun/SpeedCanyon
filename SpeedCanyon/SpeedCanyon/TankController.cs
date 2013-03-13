using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpeedCanyon
{
    public abstract class TankController : GameComponent
    {
        public Tank Tank { get; set; }

        public enum MoveDirection { Back=-1, None=0, Forward=1 };
        public enum TurnDirection { Left=-1, None=0, Right=1 };

        public MoveDirection Move { get; protected set; }
        public TurnDirection TurnWheels { get; protected set; }
        public float TargetTurretYaw { get; protected set; }

        public bool FireCannon { get; protected set; }


        public TankController(Game1 game)
            : base(game)
        {
            Move = MoveDirection.None;
            TurnWheels = TurnDirection.None;
            TargetTurretYaw = 0;
            FireCannon = false;

        }

        public override void Initialize()
        {


            base.Initialize();
        }


        public override void Update(GameTime gameTime)
        {
            SetCommands();
            base.Update(gameTime);
        }


        protected abstract void SetCommands();

    }
}
