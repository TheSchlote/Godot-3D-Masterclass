using Godot;
using System;

public partial class Character : CharacterBody3D
{
    [ExportCategory("Locomotion")]
    [Export]
    public float WalkingSpeed { get; set; } = 1.0f;
    [Export]
    public float RunningSpeed { get; set; } = 2.0f;

    [Export]
    public float Acceleration { get; set; } = 2.0f;

    [Export]
    public float Deceleration { get; set; } = 4.0f;
    [Export]
    public float RotationSpeed { get; set; } = Mathf.Pi * 2.0f;

    private float _movementSpeed;  // Variable for current speed
    private Vector3 _xzVelocity = Vector3.Zero;
    private Vector3 _direction = Vector3.Zero;
    private AnimationTree _animation;
    private Node3D _rig;
    private AnimationNodeStateMachinePlayback _stateMachine;

    [ExportCategory("Jumping")]
    [Export]
    public float MinJumpHeight { get; set; } = 1.5f;
    [Export]
    public float MaxJumpHeight { get; set; } = 2.5f;
    [Export]
    public float Mass { get; set; } = 1.0f;
    [Export]
    public float AirControl { get; set; } = 0.5f;
    [Export]
    public float AirBrakes { get; set; } = 0.5f;

    private float _minJumpVelocity;
    private float _maxJumpVelocity;
    private Timer _jumpHold;
    private float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    public override void _Ready()
    {
        _animation = GetNode<AnimationTree>("AnimationTree");
        _rig = GetNode<Node3D>("Rig");
        _movementSpeed = WalkingSpeed;
        _stateMachine = (AnimationNodeStateMachinePlayback)_animation.Get("parameters/playback");
        // Calculate Jump Velocities based on height
        _minJumpVelocity = Mathf.Sqrt(2 * MinJumpHeight * _gravity * Mass);
        _maxJumpVelocity = Mathf.Sqrt(2 * MaxJumpHeight * _gravity * Mass);

        // Get the Timer node for jump hold
        _jumpHold = GetNode<Timer>("Jump Hold");
    }

    // Method to set movement direction
    public void Move(Vector3 direction)
    {
        _direction = direction;
    }

    // Switch to walking speed
    public void Walk()
    {
        _movementSpeed = WalkingSpeed;
    }

    // Switch to running speed
    public void Run()
    {
        _movementSpeed = RunningSpeed;
    }
    public void StartJump()
    {
        if (IsOnFloor())
        {
            _stateMachine.Travel("Jump_Start");
            _jumpHold.Start();
            _jumpHold.Paused = false;
        }
    }

    public void CompleteJump()
    {
        _jumpHold.Paused = true;
    }

    public void ApplyJumpVelocity()
    {
        _jumpHold.Paused = true;

        // Calculate jump velocity based on hold time
        Vector3 velocity = Velocity;
        float holdRatio = (float)Mathf.Min(1 - _jumpHold.TimeLeft / 0.3f, 1);
        velocity.Y = _minJumpVelocity + (_maxJumpVelocity - _minJumpVelocity) * holdRatio;
        Velocity = velocity;
    }


    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        if (_direction != Vector3.Zero)
        {
            float targetAngle = Mathf.Atan2(_direction.X, _direction.Z);
            float currentAngle = _rig.Rotation.Y;
            float angleDifference = Mathf.Wrap(targetAngle - currentAngle, -Mathf.Pi, Mathf.Pi);

            _rig.RotateY(
                Mathf.Clamp(RotationSpeed * (float)delta, 0, Mathf.Abs(angleDifference)) * Mathf.Sign(angleDifference)
            );
        }
        // Isolate horizontal (XZ) movement from vertical (Y)
        _xzVelocity = new Vector3(velocity.X, 0, velocity.Z);

        // Apply gravity if not on the floor
        if (!IsOnFloor())
        {
            velocity.Y -= _gravity * Mass * (float)delta;
        }

        if (IsOnFloor())
        {
            GroundPhysics((float)delta);
        }
        else
        {
            AirPhysics((float)delta);
        }

        _animation.Set("parameters/Locomotion/blend_position", _xzVelocity.Length() / RunningSpeed);


        // Apply adjusted XZ velocity back to the character
        velocity.X = _xzVelocity.X;
        velocity.Z = _xzVelocity.Z;

        // Set final velocity and move character
        Velocity = velocity;
        MoveAndSlide();
    }
    private void GroundPhysics(float delta)
    {
        // Apply movement input to the XZ velocity
        if (_direction != Vector3.Zero)
        {
            if (_direction.Dot(Velocity.Normalized()) >= 0)
            {
                // Accelerate in the direction of movement
                _xzVelocity = _xzVelocity.MoveToward(
                    _direction * _movementSpeed,
                    Acceleration * delta
                );
            }
            else
            {
                // Decelerate if changing direction
                _xzVelocity = _xzVelocity.MoveToward(
                    _direction * _movementSpeed,
                    Deceleration * delta
                );
            }
        }
        else
        {
            // Gradually slow down when no input is provided
            _xzVelocity = _xzVelocity.MoveToward(
                Vector3.Zero,
                Deceleration * delta
            );
        }

        // Update blend position in animation based on movement speed
        _animation.Set("parameters/Locomotion/blend_position", _xzVelocity.Length() / RunningSpeed);
    }
    private void AirPhysics(float delta)
    {
        Vector3 velocity = Velocity;

        // Apply gravity while airborne
        velocity.Y -= _gravity * Mass * delta;

        // Apply air control for directional movement
        if (_direction != Vector3.Zero)
        {
            _xzVelocity = _xzVelocity.MoveToward(
                _direction * _movementSpeed,
                AirControl * delta
            );
        }
        else
        {
            // Decelerate with air brakes if no directional input
            _xzVelocity = _xzVelocity.MoveToward(
                Vector3.Zero,
                AirBrakes * delta
            );
        }

        Velocity = velocity;
    }
}
