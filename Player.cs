using Godot;
using System;

public partial class Player : Node
{
    [Export]
    public CharacterBody3D Character { get; set; }
    [Export]
    public Camera3D Camera { get; set; }
    private Vector2 _inputDirection = Vector2.Zero;
    private Vector3 _moveDirection = Vector3.Zero;
    public override void _Process(double delta)
    {
        // Get input direction
        _inputDirection = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBackward");

        // Calculate movement direction based on camera orientation
        _moveDirection = (Camera.Basis.X * new Vector3(1, 0, 1).Normalized()) * _inputDirection.X;
        _moveDirection += (Camera.Basis.Z * new Vector3(1, 0, 1).Normalized()) * _inputDirection.Y;

        // Move character
        Character.MoveAndCollide(_moveDirection * (float)delta);
    }
}
