using Godot;
using System;

public class character : RigidBody
{

    public override void _Ready()
    {
    }


    public void CharacterExitScreen() // Signal
    {
        QueueFree(); // Remove the character from the scene
    }


}
