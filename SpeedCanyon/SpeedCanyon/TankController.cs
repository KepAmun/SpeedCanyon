using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpeedCanyon
{
    public abstract class TankController : GameComponent
    {
        public new Game1 Game { get { return (Game1)base.Game; } }

        protected Tank _tank;


        public TankController(Game1 game, Tank tank)
            : base(game)
        {
            _tank = tank;
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
