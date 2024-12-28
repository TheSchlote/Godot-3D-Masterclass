using Godot;
using System;

public partial class SpringArm : SpringArm3D
{
    [Export] public float RotationSpeed { get; set; } = 5.0f;
    [Export] public float MinXRotation { get; set; } = -Mathf.Pi / 3;
    [Export] public float MaxXRotation { get; set; } = Mathf.Pi / 3;

    public void Look(Vector2 direction)
    {
        GD.Print($"Looking: {direction}");

        // Apply vertical rotation (X-axis)
        RotationDegrees += new Vector3(
            direction.Y * RotationSpeed * (float)GetProcessDeltaTime(),
            0,
            0
        );

        // Clamp the vertical rotation to stay within the min/max limits
        RotationDegrees = new Vector3(
            Mathf.Clamp(RotationDegrees.X, Mathf.RadToDeg(MinXRotation), Mathf.RadToDeg(MaxXRotation)),
            RotationDegrees.Y,
            RotationDegrees.Z
        );

        // Apply horizontal rotation (Y-axis)
        RotationDegrees += new Vector3(
            0,
            direction.X * RotationSpeed * (float)GetProcessDeltaTime(),
            0
        );
    }
}
