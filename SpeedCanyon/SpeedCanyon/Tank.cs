using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpeedCanyon
{
    /// <summary>
    /// Helper class for drawing a tank model with animated wheels and turret.
    /// </summary>
    public class Tank : DrawableGameComponent
    {
        #region Fields

        public new Game1 Game { get { return (Game1)base.Game; } }

        // The XNA framework Model object that we are going to display.
        Model _tankModel;
        Color _color;

        TankController _controller;

        public Vector3 Position { get; protected set; }
        public Vector3 Velocity { get; protected set; }
        public float FacingAngle { get; protected set; }
        public float LookYaw { get; protected set; }

        float _steeringDirection = 0.0f;
        float _maxSteeringDirection = MathHelper.PiOver4;


        // Shortcut references to the bones that we are going to animate.
        // We could just look these up inside the Draw method, but it is more
        // efficient to do the lookups while loading and cache the results.
        ModelBone _leftBackWheelBone;
        ModelBone _rightBackWheelBone;
        ModelBone _leftFrontWheelBone;
        ModelBone _rightFrontWheelBone;
        ModelBone _leftSteerBone;
        ModelBone _rightSteerBone;
        ModelBone _turretBone;
        ModelBone _cannonBone;
        ModelBone _hatchBone;


        // Store the original transform matrix for each animating bone.
        Matrix _leftBackWheelTransform;
        Matrix _rightBackWheelTransform;
        Matrix _leftFrontWheelTransform;
        Matrix _rightFrontWheelTransform;
        Matrix _leftSteerTransform;
        Matrix _rightSteerTransform;
        Matrix _turretTransform;
        Matrix _cannonTransform;
        Matrix _hatchTransform;


        // Array holding all the bone transform matrices for the entire model.
        // We could just allocate this locally inside the Draw method, but it
        // is more efficient to reuse a single array, as this avoids creating
        // unnecessary garbage.
        Matrix[] _boneTransforms;


        // Current animation positions.
        float _wheelRotationValue;
        float _steerRotationValue;
        float _turretRotationValue;
        float _cannonRotationValue;
        float _hatchRotationValue;


        #endregion

        #region Properties


        /// <summary>
        /// Gets or sets the wheel rotation amount.
        /// </summary>
        public float WheelRotation
        {
            get { return _wheelRotationValue; }
            set { _wheelRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the steering rotation amount.
        /// </summary>
        public float SteerRotation
        {
            get { return _steerRotationValue; }
            set { _steerRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the turret rotation amount.
        /// </summary>
        public float TurretRotation
        {
            get { return _turretRotationValue; }
            set { _turretRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the cannon rotation amount.
        /// </summary>
        public float CannonRotation
        {
            get { return _cannonRotationValue; }
            set { _cannonRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the entry hatch rotation amount.
        /// </summary>
        public float HatchRotation
        {
            get { return _hatchRotationValue; }
            set { _hatchRotationValue = value; }
        }


        #endregion


        public Tank(Game1 game, TankController controller)
            : this(game, controller, new Vector3(0,0,0), 0, Color.Black)
        {
        }

        public Tank(Game1 game, TankController controller, Vector3 position, float facingAngle, Color color)
            : base(game)
        {
            _controller = controller;

            _color = color;
            Position = position;
            FacingAngle = facingAngle;

            if (_controller != null)
                _controller.Tank = this;
        }


        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Load the tank model from the ContentManager.
            _tankModel = Game.Content.Load<Model>("Models\\tank");

            // Look up shortcut references to the bones we are going to animate.
            _leftBackWheelBone = _tankModel.Bones["l_back_wheel_geo"];
            _rightBackWheelBone = _tankModel.Bones["r_back_wheel_geo"];
            _leftFrontWheelBone = _tankModel.Bones["l_front_wheel_geo"];
            _rightFrontWheelBone = _tankModel.Bones["r_front_wheel_geo"];
            _leftSteerBone = _tankModel.Bones["l_steer_geo"];
            _rightSteerBone = _tankModel.Bones["r_steer_geo"];
            _turretBone = _tankModel.Bones["turret_geo"];
            _cannonBone = _tankModel.Bones["canon_geo"];
            _hatchBone = _tankModel.Bones["hatch_geo"];

            // Store the original transform matrix for each animating bone.
            _leftBackWheelTransform = _leftBackWheelBone.Transform;
            _rightBackWheelTransform = _rightBackWheelBone.Transform;
            _leftFrontWheelTransform = _leftFrontWheelBone.Transform;
            _rightFrontWheelTransform = _rightFrontWheelBone.Transform;
            _leftSteerTransform = _leftSteerBone.Transform;
            _rightSteerTransform = _rightSteerBone.Transform;
            _turretTransform = _turretBone.Transform;
            _cannonTransform = _cannonBone.Transform;
            _hatchTransform = _hatchBone.Transform;

            // Allocate the transform matrix array.
            _boneTransforms = new Matrix[_tankModel.Bones.Count];

            LookYaw = 0;

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            Vector3 positionChange = Vector3.Zero;

            if (_controller != null)
            {
                _controller.Update(gameTime);

                LookYaw = -_controller.TargetTurretAngle;

                switch (_controller.TurnWheels)
                {
                    case TankController.TurnDirection.Left:
                        //_steeringDirection = MathHelper.Clamp(_steeringDirection + 0.05f, -_maxSteeringDirection, _maxSteeringDirection);
                        FacingAngle -= 0.05f;
                        break;
                    case TankController.TurnDirection.None:
                        //if (_steeringDirection > 0)
                        //    _steeringDirection = MathHelper.Clamp(_steeringDirection + 0.05f, -_maxSteeringDirection, _maxSteeringDirection);
                        //else if (_steeringDirection < 0)
                        //    _steeringDirection = MathHelper.Clamp(_steeringDirection - 0.05f, -_maxSteeringDirection, _maxSteeringDirection);

                        break;
                    case TankController.TurnDirection.Right:
                        //_steeringDirection = MathHelper.Clamp(_steeringDirection - 0.05f, -_maxSteeringDirection, _maxSteeringDirection);
                        FacingAngle += 0.05f;
                        break;
                    default:
                        break;
                }


                //_steerRotationValue = _steeringDirection;
                FacingAngle = MathHelper.WrapAngle(FacingAngle);


                if (_controller.Move != TankController.MoveDirection.None)
                {
                    positionChange.X = (float)Math.Cos(FacingAngle);
                    positionChange.Z = (float)Math.Sin(FacingAngle);

                    if (_controller.Move == TankController.MoveDirection.Back)
                    {
                        positionChange = -positionChange;
                    }
                }
            }


            positionChange *= 10 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //FacingAngle -= _steeringDirection * positionDelta.Length();

            //if (positionDelta.LengthSquared() != 0)
            //{
            //    positionDelta.Normalize();
            //    FacingAngle = (float)Math.Atan2(positionDelta.Y, positionDelta.X);
            //}

            Position += positionChange;

            float desiredTurretAngleChange = MathHelper.WrapAngle(_turretRotationValue - LookYaw);

            float turretAngleChange = 0;

            if (Math.Abs(desiredTurretAngleChange) > double.Epsilon)
            {
                if (Math.Abs(desiredTurretAngleChange) > 2 * (float)gameTime.ElapsedGameTime.TotalSeconds)
                {
                    turretAngleChange = 2 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                if (desiredTurretAngleChange > 0)
                {
                    turretAngleChange = -turretAngleChange;
                }
            }

            _turretRotationValue = MathHelper.WrapAngle(_turretRotationValue + turretAngleChange);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Set the world matrix as the root transform of the model.
            Matrix tankWorld = Matrix.CreateScale(0.005f) * Matrix.CreateRotationY(-FacingAngle + MathHelper.PiOver2) * Matrix.CreateTranslation(Position);
            _tankModel.Root.Transform = tankWorld;

            // Calculate matrices based on the current animation position.
            Matrix wheelRotation = Matrix.CreateRotationX(_wheelRotationValue);
            Matrix steerRotation = Matrix.CreateRotationY(_steerRotationValue);
            Matrix turretRotation = Matrix.CreateRotationY(_turretRotationValue);
            Matrix cannonRotation = Matrix.CreateRotationX(_cannonRotationValue);
            Matrix hatchRotation = Matrix.CreateRotationX(_hatchRotationValue);

            // Apply matrices to the relevant bones.
            _leftBackWheelBone.Transform = wheelRotation * _leftBackWheelTransform;
            _rightBackWheelBone.Transform = wheelRotation * _rightBackWheelTransform;
            _leftFrontWheelBone.Transform = wheelRotation * _leftFrontWheelTransform;
            _rightFrontWheelBone.Transform = wheelRotation * _rightFrontWheelTransform;
            _leftSteerBone.Transform = steerRotation * _leftSteerTransform;
            _rightSteerBone.Transform = steerRotation * _rightSteerTransform;
            _turretBone.Transform = turretRotation * _turretTransform;
            _cannonBone.Transform = cannonRotation * _cannonTransform;
            _hatchBone.Transform = hatchRotation * _hatchTransform;

            // Look up combined bone matrices for the entire model.
            _tankModel.CopyAbsoluteBoneTransformsTo(_boneTransforms);

            // Draw the model.
            foreach (ModelMesh mesh in _tankModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = _boneTransforms[mesh.ParentBone.Index];
                    effect.View = Game.Camera.View;
                    effect.Projection = Game.Camera.Projection;

                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = _color.ToVector3();
                }

                mesh.Draw();
            }


            base.Draw(gameTime);
        }
    }
}
