using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpeedCanyon
{
    class TankControllerAI : TankController
    {
        public TankControllerAI(Game1 game, Tank tank) :
            base(game, tank)
        {
        }

        Tank.TurnDirection wanderTurn = Tank.TurnDirection.None;

        protected override void SetCommands()
        {
            _tank.Throttle = Tank.MoveDirection.Forward;
            _tank.Steering = Tank.TurnDirection.None;

            _tank.FireCannon = false;
            _tank.TargetTurretYaw = 0;
            _tank.TargetTurretPitch = MathHelper.ToRadians(15);


            ResourcePickup closestResource = Game.FindResource(_tank.Position, 100);
            Vector3 closestResourceDelta = Vector3.Zero;
            float closestResourceDist = float.MaxValue;

            if (closestResource != null)
            {
                closestResourceDelta = _tank.Position - closestResource.Position;
                closestResourceDist = closestResourceDelta.LengthSquared();
            }

            Tank closestTank = Game.FindTank(_tank, 100);
            Vector3 closestTankDelta = Vector3.Zero;
            float closestTankDist = float.MaxValue;


            float targetMoveAngle = 0;
            float targetAttackAngle = 0;

            if (closestTank != null)
            {
                closestTankDelta = _tank.Position - closestTank.Position;
                closestTankDist = closestTankDelta.LengthSquared();

                Vector3 futureTargetPosition = closestTank.Position + (0.012f * closestTankDist * closestTank.Velocity);
                Vector3 targetPosDelta = _tank.Position - futureTargetPosition;

                targetAttackAngle = MathHelper.WrapAngle((float)Math.Atan2(targetPosDelta.Z, targetPosDelta.X) - _tank.FacingYaw);

                _tank.FireCannon = true;
                _tank.TargetTurretYaw = MathHelper.WrapAngle(targetAttackAngle + MathHelper.Pi);
                _tank.TargetTurretPitch = MathHelper.ToRadians(12 - 7f * (closestTankDist / 10000));
            }

            if (closestResource != null && closestResourceDist < closestTankDist)
            {
                targetMoveAngle = MathHelper.WrapAngle((float)Math.Atan2(closestResourceDelta.Z, closestResourceDelta.X) - _tank.FacingYaw);
            }
            else if (closestTank != null)
            {
                targetMoveAngle = targetAttackAngle;

                if (_tank.Health < 5 && _tank.Health < closestTank.Health)
                {
                    targetMoveAngle = -targetAttackAngle;
                }
            }
            else
            {
                double r = Game.Rnd.NextDouble();

                switch (wanderTurn)
                {
                    case Tank.TurnDirection.Left:
                        if (r > 0.95)
                        {
                            wanderTurn = Tank.TurnDirection.None;
                        }
                        break;

                    case Tank.TurnDirection.None:
                        if (r < 0.05)
                        {
                            wanderTurn = Tank.TurnDirection.Left;
                        }
                        else if (r > 0.95)
                        {
                            wanderTurn = Tank.TurnDirection.Right;
                        }
                        break;

                    case Tank.TurnDirection.Right:
                        if (r > 0.95)
                        {
                            wanderTurn = Tank.TurnDirection.None;
                        }
                        break;

                    default:
                        break;
                }

                _tank.Steering = wanderTurn;
            }


            if (targetMoveAngle > 0)
            {
                _tank.Steering = Tank.TurnDirection.Left;
            }
            else if (targetMoveAngle < 0)
            {
                _tank.Steering = Tank.TurnDirection.Right;
            }


        }
    }
}
