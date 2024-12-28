using Godot;
using System;

public partial class Player : Node
{
    [Export]
    public Character Character { get; set; }
    [Export]
    public Camera3D Camera { get; set; }
    private Vector2 _inputDirection = Vector2.Zero;
    private Vector3 _moveDirection = Vector3.Zero;

    public override void _Process(double delta)
    {
        // Get input direction
        _inputDirection = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBackward");

        // Calculate movement direction relative to the camera
        _moveDirection = (Camera.Basis.X * _inputDirection.X + Camera.Basis.Z * _inputDirection.Y).Normalized();

        // Debugging movement direction
        GD.Print($"Input: {_inputDirection}, Move Direction: {_moveDirection}");

        // Move character directly (no delta multiplication)
        Character.Move(_moveDirection);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Run"))
        {
            Character.Run();
        }
        else if (@event.IsActionReleased("Run"))
        {
            Character.Walk();
        }
        else if (@event.IsActionPressed("Jump"))
        {
            Character.Jump();
        }   
    }
}
