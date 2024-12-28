using Godot;
using System;

public partial class Player : Node
{
    [Export]
    public Character Character { get; set; }
    [Export]
    public SpringArm SpringArm { get; set; }
    private Vector2 _inputDirection = Vector2.Zero;
    private Vector3 _moveDirection = Vector3.Zero;

    public override void _Process(double delta)
    {
        SpringArm.Look(Input.GetVector("LookLeft", "LookRight", "LookUp", "LookDown"));

        // Get input direction
        _inputDirection = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBackward");

        // Calculate movement direction relative to the camera
        _moveDirection = (SpringArm.Basis.X * _inputDirection.X + SpringArm.Basis.Z * _inputDirection.Y).Normalized();

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
        if (@event.IsActionPressed("Jump"))
        {
            Character.StartJump();
        }
        else if (@event.IsActionReleased("Jump"))
        {
            Character.CompleteJump();
        }
    }
}
