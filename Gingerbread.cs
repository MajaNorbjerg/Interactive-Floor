using Godot;
using System;

public class Gingerbread : Spatial
{
     public void CharacterAnimationDefeated(string DefeatedAnimation) // When the defeated animation is over
    {
        this.GetParent().QueueFree(); // Remove the character from the scene
    }
}
