using Godot;
using System;

public partial class Character : CharacterBody3D
{
    public const float JumpVelocity = 4.5f;

    [Export] 
    public float WalkingSpeed { get; set; } = 1.0f;

    [Export] 
    public float Acceleration { get; set; } = 2.0f;

    [Export] 
    public float Deceleration { get; set; } = 4.0f;

    private Vector3 _xzVelocity = Vector3.Zero;
    private Vector3 _direction = Vector3.Zero;
    private float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    private AnimationTree _animation;
    public override void _Ready()
    {
        // Fetch the AnimationTree node
        _animation = GetNode<AnimationTree>("AnimationTree");
    }

    // Method to set movement direction
    public void Move(Vector3 direction)
    {
        _direction = direction;
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

        // Handle jumping
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        // Apply movement or deceleration
        if (_direction != Vector3.Zero)
        {
            _xzVelocity = _xzVelocity.MoveToward(
                _direction * WalkingSpeed, 
                Acceleration * (float)delta
            );
        }
        else
        {
            _xzVelocity = _xzVelocity.MoveToward(
                Vector3.Zero, 
                Deceleration * (float)delta
            );
        }

        _animation.Set("parameters/Locomotion/blend_position", _xzVelocity.Length() / WalkingSpeed);


        // Apply adjusted XZ velocity back to the character
        velocity.X = _xzVelocity.X;
        velocity.Z = _xzVelocity.Z;

        // Set final velocity and move character
        Velocity = velocity;
        MoveAndSlide();
    }
}
