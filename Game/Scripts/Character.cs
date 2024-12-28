using Godot;
using System;

public partial class Character : CharacterBody3D
{
    public const float JumpVelocity = 4.5f;

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
    private float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    private float _movementSpeed;  // Variable for current speed
    private Vector3 _xzVelocity = Vector3.Zero;
    private Vector3 _direction = Vector3.Zero;
    private AnimationTree _animation;
    private Node3D _rig;
    private AnimationNodeStateMachinePlayback _stateMachine;
    public override void _Ready()
    {
        _animation = GetNode<AnimationTree>("AnimationTree");
        _rig = GetNode<Node3D>("Rig");
        _movementSpeed = WalkingSpeed;
        _stateMachine = (AnimationNodeStateMachinePlayback)_animation.Get("parameters/playback");
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
    public void Jump()
    {
        if (IsOnFloor())
        {
            _stateMachine.Travel("Jump_Start");
        }
    }
    
    public void ApplyJumpVelocity()
    {
        Vector3 velocity = Velocity;
        velocity.Y = JumpVelocity;
        Velocity = velocity;
    }


    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Isolate horizontal (XZ) movement from vertical (Y)
        _xzVelocity = new Vector3(velocity.X, 0, velocity.Z);

        // Apply gravity if not on the floor
        if (!IsOnFloor())
        {
            velocity.Y -= _gravity * (float)delta;
        }

        // Apply movement or deceleration based on input
        if (_direction != Vector3.Zero)
        {
            float targetAngle = Mathf.Atan2(_direction.X, _direction.Z);
            float currentAngle = _rig.Rotation.Y;
            float angleDifference = Mathf.Wrap(targetAngle - currentAngle, -Mathf.Pi, Mathf.Pi);

            _rig.RotateY(
                Mathf.Clamp(RotationSpeed * (float)delta, 0, Mathf.Abs(angleDifference)) * Mathf.Sign(angleDifference)
            );

            // Determine acceleration or turn-around based on direction
            if (_direction.Dot(_xzVelocity.Normalized()) >= 0)
            {
                // Accelerate
                _xzVelocity = _xzVelocity.MoveToward(
                    _direction * _movementSpeed,
                    Acceleration //* (float)delta
                );
            }
            else
            {
                // Turn around (decelerate faster when changing direction)
                _xzVelocity = _xzVelocity.MoveToward(
                    _direction * _movementSpeed,
                    Deceleration //* (float)delta
                );
            }
        }
        else
        {
            // Decelerate to stop if no input is provided
            _xzVelocity = _xzVelocity.MoveToward(
                Vector3.Zero,
                Deceleration * (float)delta
            );
        }

        _animation.Set("parameters/Locomotion/blend_position", _xzVelocity.Length() / RunningSpeed);


        // Apply adjusted XZ velocity back to the character
        velocity.X = _xzVelocity.X;
        velocity.Z = _xzVelocity.Z;

        // Set final velocity and move character
        Velocity = velocity;
        MoveAndSlide();
    }
}
