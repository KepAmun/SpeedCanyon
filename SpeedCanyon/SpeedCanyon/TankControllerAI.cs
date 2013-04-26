using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpeedCanyon
{
    class TankControllerAI : TankController
    {
        int _faction;

        enum AiState
        {
            Search, Gather, Return, Attack, Defend,
        }

        AiState _state;


        public TankControllerAI(int faction, Game1 game, Tank tank) :
            base(game, tank)
        {
            _faction = faction;

            _state = AiState.Search;
        }

        Tank.TurnDirection wanderTurn = Tank.TurnDirection.None;

        protected override void SetCommands()
        {
            //Vector3 closestResource= Game.FindResource(_tank.Position, 100);
            Tank closestTank = null;// Game.FindTank(_tank.Position, 100);

            Tank.TurnDirection turnDirection = Tank.TurnDirection.None;
            Tank.MoveDirection moveDirection = Tank.MoveDirection.None;

            switch (_state)
            {
                case AiState.Search:
                    if (closestTank != null)
                    {
                        _state = AiState.Attack;
                    }
                    else
                    {
                        moveDirection = Tank.MoveDirection.Forward;

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

                        turnDirection = wanderTurn;
                    }
                    break;

                case AiState.Gather:
                    break;
                case AiState.Return:
                    break;
                case AiState.Attack:
                    break;
                case AiState.Defend:
                    break;
                default:
                    break;
            }




            _tank.TargetTurretYaw = 0;// MathHelper.WrapAngle(_tank.TargetTurretYaw + dx * 0.002f);

            _tank.TargetTurretPitch = MathHelper.ToRadians(25);// MathHelper.Clamp(_tank.TargetTurretPitch + dy * 0.002f, -_maxPitch, _maxPitch);

            _tank.Throttle = moveDirection;
            _tank.Steering = turnDirection;

            _tank.FireCannon = true;
        }
    }
}
