using Godot;
using System;

public class timerBall : RigidBody2D
{
    // ----------------------------- Variable declarations -----------------------------
    Global global;
    Node2D Hourglass;
    RigidBody2D Ball;
    Control GameOver;
    Spatial CharacterContainer;


    // ----------------------------- Called when the node enters the scene tree for the first time -----------------------------
    public override void _Ready()
    {
        // --------- Define the variables ---------
        global = GetNode<Global>("/root/Global");
        Hourglass = GetNode<Node2D>("/root/Spatial/timerCounter/Hourglass");
        Ball = GetNode<RigidBody2D>("/root/Spatial/timerCounter/Hourglass/Timerballs/RigidBody2D");
        GameOver = GetNode<Control>("/root/Spatial/GameOver");
        CharacterContainer = GetNode<Spatial>("/root/Spatial/CharacterContainer");
    }

    // ----------------------------- Ball SleepingStateChanged -----------------------------
    public void SleepingStateChanged() // Run when the balls sleeping state changes
    {
        // If the balls laies still after running through the hourglass...
        if (Ball.Sleeping == true && global.firstTimerBall == true && global.GameState == global.StateStart && global.timerRotated == true)
        {
            global.GameState = global.StateTimeOver; // ... then game state is TimeOver
            GetNode<AudioStreamPlayer>("/root/Spatial/TimeOver").Play(); // Play audio
            GetNode<Timer>("/root/Spatial/CharacterTimer").Stop(); // Stop characters from spawning
            global.firstTimerBall = false; // Make sure the function only runs for one of the manny balls
        }
    }

    // ----------------------------- _PhysicsProsess function -----------------------------
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        // --------- Hourglass rotation ---------
        // If the game is started the hourglass starts rotating
        if (global.GameState == global.StateStart && Hourglass.Rotation < global.timerRotation && global.timerRotated == false)
        {
            float rad = Mathf.Pi / 180;
            Hourglass.Rotate(rad * delta / 2); // rotate the hourglass a bit frame by frame
        }
        // If the rotation is bigger than pi radians...
        else if (global.GameState == global.StateStart && Hourglass.Rotation > global.timerRotation && global.timerRotated == false)
        {
            global.timerRotated = true; //then stop the rotation
            // And add pi radians to the rotation variable, so the hourglass can rotate further next time a game starts
            global.timerRotation = global.timerRotation + Mathf.Pi;
        }


        // --------- Let the last collectibles go away before the game ends ---------
        if (CharacterContainer.GetChildCount() == 0 && global.GameState == global.StateTimeOver)
        {
            GD.Print("no more collectibles"); // Print to Godot
            global.GameState = global.StateGameOver; // change gamestate

            // Change background music
            GetNode<AudioStreamPlayer>("/root/Spatial/BackgroundMusicGame").Stop();
            GetNode<AudioStreamPlayer>("/root/Spatial/BackgroundMusicGameOver").Play();
        }

        // --------- Show game over screen ---------
        if (global.GameState == global.StateGameOver && global.doRun == true)
        {
            GameOver.Visible = true; // Show the game over screen

            GetNode<Timer>("/root/Spatial/GameOver/GameOverTimer").Start(); // Start timer for hiding the screen again
            // Replace the value on the game over screen with the number of points
            GetNode<Label>("/root/Spatial/GameOver/ColorRect/VBoxContainer/Points").Text = global.points.ToString();
            global.doRun = false; // Make sure it only run once
        }
    }
}
