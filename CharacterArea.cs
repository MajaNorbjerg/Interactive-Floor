using Godot;
using System;


public class CharacterArea : Godot.Area
{

    // ----------------------------- Variable declarations -----------------------------
    bool cought = false;
    CollisionShape collisionShape;
    CSGBox box;
    RigidBody ball;
    RigidBody boxer;
    TextEdit counter;
    Global global;
    bool firstEntry = true;


    public override void _Ready() // Called when the node enters the scene tree for the first time.
    {
        global = GetNode<Global>("/root/Global");
        collisionShape = GetNode<CollisionShape>("CollisionShape");
        box = GetNode<CSGBox>("CSGBox");
    }

    public void OnAreaEntered(Area area) // Run on area entered signal
    {
        Spatial collisionArea = (Spatial)area;
        if (collisionArea.IsInGroup("cameraDetections")) // If the area is in the group cameraDetections
        // ... and not another character
        {
            if (firstEntry == true) // To make sure it can´t be triggered more than once
            {
                firstEntry = false;
                cought = true; // decklare it caught for the counter
                this.GetParent<RigidBody>().LinearVelocity = new Vector3(0, 0, 0); // Make the character stop going forward

                var gingerbread = this.GetParent().GetNode<Spatial>("GingerbreadMan");
                var theAnimationPlayer = gingerbread.GetNode<AnimationPlayer>("AnimationPlayer");
                gingerbread.GetNode<Particles>("Particles").Emitting = true; // Start particles

                theAnimationPlayer.PlaybackSpeed = 4.5f; // Set animation speed
                theAnimationPlayer.Play("LookUpAnimation"); // Set animation
                theAnimationPlayer.GetAnimation("LookUpAnimation").Loop = false; // Stop the animation looping
                gingerbread.GetNode<AudioStreamPlayer>("AudioStreamPlayer").Play(); // Play the caught audio
            }
        }
    }



    public void OnRemoval() // Run on removed signal
    {
        if (cought) // If the collectible was caught
        {
            // If the game is ongoing
            if (global.GameState == global.StateStart || global.GameState == global.StateTimeOver)
            {
                global.Counter(); // Run the counter function from the global class
            }

            if (global.ReadyCatch < 3 && global.GameState == global.StateIdle) // If Idlestate and number of cought gingermen is below 3
            {
                global.ReadyCatch++; // Add one to readycatch
            }
            if (global.ReadyCatch == 2 && global.GameState == global.StateIdle) // If readycatch is 2, then start the game
            {
                global.ReadyCatch = 0; // Reset readyCatch

                global.GameState = global.StateStart; // Set game state
                global.IdleInfo.Visible = false; // Hide the screen info
                global.firstTimerBall = true; // Activate hourglass
                global.BackgroundMusicIdle.Stop(); // Stop music
                global.BackgroundMusicGame.Play(); // Start new music
            }
        }
    }
}
