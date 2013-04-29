using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace SpeedCanyon
{
    /// <summary>
    /// Helper class for drawing a tank model with animated wheels and turret.
    /// </summary>
    public class Tank : DrawableGameComponent, IGameObject
    {
        #region Fields

        public new Game1 Game { get { return (Game1)base.Game; } }

        Model _tankModel;
        Color _color;

        public int MaxHealth { get; set; }

        int _health;
        public int Health
        {
            get { return _health; }
            set { _health = Math.Min(value, MaxHealth); }
        }

        public bool IsDead { get { return Health <= 0; } }


        Vector3 _position;
        public Vector3 Position
        {
            get
            {
                return _position;
            }

            protected set
            {
                _position = value;
            }
        }

        public Vector3 FocalPoint { get; protected set; }

        Vector3 _velocity;
        public Vector3 Velocity
        {
            get
            {
                return _velocity;
            }

            protected set
            {
                _velocity = value;
            }
        }

        public float FacingYaw { get; protected set; }
        public float LookYaw { get; protected set; }

        float _steeringDirection = 0.0f;
        float _maxSteeringDirection = MathHelper.PiOver4;

        public enum MoveDirection { Back = -1, None = 0, Forward = 1 };
        public enum TurnDirection { Left = -1, None = 0, Right = 1 };

        public MoveDirection Throttle { get; set; }
        public TurnDirection Steering { get; set; }
        public float TargetTurretYaw { get; set; }
        public float TargetTurretPitch { get; set; }

        bool _onGround;

        public bool FireCannon { get; set; }
        TimeSpan _lastShot = TimeSpan.Zero;
        TimeSpan _fireDelay = TimeSpan.FromSeconds(0.4f);

        public Cue EngineNoise { get; set; }
        public AudioEmitter AudioEmitter { get; private set; }
        public AudioListener AudioListener { get; private set; }


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

        public int Faction { get; private set; }


        public Tank(Game1 game, Vector3 position, float facingAngle, int faction, Color color)
            : base(game)
        {
            Faction = faction;

            _color = color;
            Position = position;
            Velocity = Vector3.Zero;
            FacingYaw = facingAngle;
            LookYaw = 0;

            Throttle = MoveDirection.None;
            Steering = TurnDirection.None;
            TargetTurretYaw = 0;
            FireCannon = false;

            _onGround = true;

            AudioEmitter = new AudioEmitter();
            AudioListener = new AudioListener();

            MaxHealth = 10;
            Health = MaxHealth;
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


            base.LoadContent();
        }


        public override void Update(GameTime gameTime)
        {
            Vector3 impulse = Vector3.Zero;

            LookYaw = -TargetTurretYaw;

            float turnSpeed = 2.2f * (float)gameTime.ElapsedGameTime.TotalSeconds;

            switch (Steering)
            {
                case TurnDirection.Left:
                    _steeringDirection = MathHelper.Clamp(_steeringDirection + turnSpeed, -_maxSteeringDirection, _maxSteeringDirection);
                    break;

                case TurnDirection.None:

                    if (_steeringDirection > 0)
                        _steeringDirection = MathHelper.Clamp(_steeringDirection - turnSpeed, 0, _maxSteeringDirection);
                    else if (_steeringDirection < 0)
                        _steeringDirection = MathHelper.Clamp(_steeringDirection + turnSpeed, -_maxSteeringDirection, 0);

                    break;

                case TurnDirection.Right:
                    _steeringDirection = MathHelper.Clamp(_steeringDirection - turnSpeed, -_maxSteeringDirection, _maxSteeringDirection);
                    break;

                default:
                    break;
            }


            _steerRotationValue = _steeringDirection;

            if (_onGround)
            {
                if (Throttle != MoveDirection.None)
                {
                    float steeringChange = (int)Throttle * -3 * _steeringDirection * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    FacingYaw = MathHelper.WrapAngle(FacingYaw + steeringChange);

                    impulse.X = (float)Math.Cos(FacingYaw);
                    impulse.Z = (float)Math.Sin(FacingYaw);

                    if (Throttle == MoveDirection.Back)
                    {
                        impulse = -impulse;
                    }
                }

                float enginePower = 1.5f;

                impulse *= enginePower * (float)gameTime.ElapsedGameTime.TotalSeconds;

                Velocity += impulse;
                Velocity *= 0.92f;
            }



            _velocity.Y -= 9.81f * 0.2f * (float)gameTime.ElapsedGameTime.TotalSeconds;

            float prevY = Position.Y;
            Position += Velocity;// *(float)gameTime.ElapsedGameTime.TotalSeconds;

            _onGround = false;

            if (!IsDead)
            {
                float cellHeight = Game.CellHeight(Position);
                if (_position.Y <= cellHeight)
                {
                    //if (!_onGround && (cellHeight - _position.Y > 0.3f))
                    //    Game.PlayCue("metalcrash");

                    _position.Y = cellHeight;

                    _onGround = true;
                }
            }

            //_velocity.Y = Position.Y - prevY;

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


            _cannonRotationValue = MathHelper.Clamp(TargetTurretPitch - MathHelper.ToRadians(10), -MathHelper.PiOver2, MathHelper.PiOver2);

            Vector3 flatVelocity = Velocity;
            flatVelocity.Y = 0;

            _wheelRotationValue +=
                (float)Math.Cos(FacingYaw - Math.Atan2(flatVelocity.Z, flatVelocity.X)) *
                flatVelocity.Length() *
                15 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Set the world matrix as the root transform of the model.
            Matrix tankRotation = Matrix.CreateRotationY(-FacingYaw + MathHelper.PiOver2);
            //tankRotation.Up = Game.ProjectedUp(Position, Velocity, 3);
            //tankRotation.Right = Vector3.Normalize(Vector3.Cross(tankRotation.Forward, tankRotation.Up));

            //tankRotation.Forward = Vector3.Normalize(Vector3.Cross(tankRotation.Up, tankRotation.Right));

            Matrix tankWorld = Matrix.CreateScale(0.005f) * tankRotation * Matrix.CreateTranslation(Position);
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

            // Audio
            AudioEmitter.Position = Position;
            AudioEmitter.Velocity = Velocity;

            AudioListener.Position = Position;
            AudioListener.Velocity = Velocity;
            AudioListener.Forward = new Vector3((float)Math.Cos(-LookYaw), 0, (float)Math.Sin(-LookYaw));

            EngineNoise.SetVariable("EngineSpeed", Math.Min(flatVelocity.Length(), 0.3f));

            // Camera Point
            FocalPoint = new Vector3(0, 0.6f, 0) +
                Vector3.Transform(_tankModel.Meshes["turret_geo"].BoundingSphere.Center, _boneTransforms[_tankModel.Meshes["turret_geo"].ParentBone.Index]);

            // Cannon Fire
            if (FireCannon && gameTime.TotalGameTime > _lastShot + _fireDelay)
            {
                float cannonYaw = -TurretRotation + FacingYaw;
                float cannonPitch = _cannonRotationValue + MathHelper.PiOver2;
                Vector3 bulletPosition = Vector3.Transform(new Vector3(0, 0, 100),
                    _boneTransforms[_tankModel.Meshes["canon_geo"].ParentBone.Index]);

                Bullet b = new Bullet(Game, this, bulletPosition, Velocity + 90 * new Vector3(
                    (float)Math.Sin(cannonPitch) * (float)Math.Cos(cannonYaw),
                    (float)Math.Cos(cannonPitch),
                    (float)Math.Sin(cannonPitch) * (float)Math.Sin(cannonYaw)));

                b.Initialize();
                Game.AddBullet(b);
                Game.PlayCue("tankfire", AudioEmitter);

                _lastShot = gameTime.TotalGameTime;
            }

            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            // Testing initial bullet position
            //Vector3 bulletPosition = Vector3.Transform(new Vector3(0, 0, 100), 
            //    _boneTransforms[_tankModel.Meshes["canon_geo"].ParentBone.Index]);

            //BoundingSphere bbs = new BoundingSphere(bulletPosition, 0.1f);
            //Game.BoundingSphereRenderer.Render(bbs, Matrix.Identity, Game.Camera.View, Game.Camera.Projection);



            //int i = 0;
            // Draw the model.
            foreach (ModelMesh mesh in _tankModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = _boneTransforms[mesh.ParentBone.Index];
                    effect.View = Game.Camera.View;
                    effect.Projection = Game.Camera.Projection;

                    effect.LightingEnabled = true;
                    effect.AmbientLightColor = Vector3.Lerp(_color.ToVector3(), Color.BurlyWood.ToVector3(), 0.3f);
                    effect.DiffuseColor = Color.BurlyWood.ToVector3();
                    effect.SpecularColor = Color.BurlyWood.ToVector3();
                    effect.SpecularPower = 100;
                    effect.DirectionalLight0.Direction = Game.LightDirection;
                    effect.DirectionalLight0.SpecularColor = Color.White.ToVector3();
                }

                //if (i == 0 || i == 1 || i == 4 || i == 5 || i == 11 )
                //{
                //    BoundingSphere bs = mesh.BoundingSphere;
                //    Game.BoundingSphereRenderer.Render(bs, _boneTransforms[mesh.ParentBone.Index], Game.Camera.View, Game.Camera.Projection);
                //}

                //i++;

                mesh.Draw();
            }


            base.Draw(gameTime);
        }


        public void ApplyImpact(Vector3 vector, int damage = 0)
        {
            _health -= damage;

            if (_health <= 0)
            {
                vector *= 2;
                vector.Y = 30;
            }

            Velocity += vector / 20;

        }


        public bool Collides(Vector3 point)
        {
            bool result = false;


            int[] collisionMeshIndices = { 0, 1, 4, 5, 10, 11 };

            for (int i = 0; i < 6; i++)
            {
                ModelMesh mesh = _tankModel.Meshes[collisionMeshIndices[i]];

                BoundingSphere bs = mesh.BoundingSphere;

                bs = bs.Transform(_boneTransforms[mesh.ParentBone.Index]);

                if (bs.Contains(point) != ContainmentType.Disjoint)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }


        public bool Collides(Tank otherTank)
        {
            bool result = false;


            int[] collisionMeshIndices = { 0, 1, 4, 5, 10, 11 };

            for (int i = 0; i < 6; i++)
            {
                ModelMesh mesh = _tankModel.Meshes[collisionMeshIndices[i]];

                BoundingSphere boundingSphere = mesh.BoundingSphere;

                boundingSphere = boundingSphere.Transform(_boneTransforms[mesh.ParentBone.Index]);


                for (int c = 0; c < 6; c++)
                {
                    ModelMesh otherMesh = otherTank._tankModel.Meshes[collisionMeshIndices[c]];

                    BoundingSphere otherBoundingSphere = otherMesh.BoundingSphere;

                    otherBoundingSphere = otherBoundingSphere.Transform(otherTank._boneTransforms[otherMesh.ParentBone.Index]);

                    if (boundingSphere.Contains(otherBoundingSphere) != ContainmentType.Disjoint)
                    {
                        result = true;
                        break;
                    }
                }

                if (result == true)
                    break;
            }

            return result;
        }


        public void Respawn(Vector3 respawnLocation)
        {
            Position = respawnLocation;
            Velocity = Vector3.Zero;
            _health = MaxHealth;
        }
    }
}
