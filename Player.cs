using Godot;
using System;

public partial class Player : Node
{
    private Vector2 _moveDirection = Vector2.Zero;
    public override void _Process(double delta)
    {
        _moveDirection = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBackward");
        GD.Print(_moveDirection);
    }
}
