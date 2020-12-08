using Godot;
using System;

public class TimerballsSpawnpoints : Node2D
{
    PackedScene ball = (PackedScene)GD.Load("res://Sprites/timerBall.tscn");
    Random r = new Random();
    Camera camera;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SpawnTimerBalls();
    }

    public void SpawnTimerBalls()
    {
        for (int i = 0; i < 300; i++)
        {
            Spawn();
        }
    }


    public void Spawn()
    {
        var ballInstance = (RigidBody2D)ball.Instance();
        Vector2 pos = new Vector2();
        pos.x = r.Next(435, 495); //435,495
        pos.y = r.Next(395, 396); //275,276
        ballInstance.Position = pos;
        this.AddChild(ballInstance);
    }
}
