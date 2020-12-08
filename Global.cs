using Godot;
using System;
using System.Diagnostics;

public class Global : Node
{
    // ----------------------------- Variable declarations -----------------------------

    // --------- Counter ---------
    public Label counter; // Points counter in game
    public int points;
    public int ReadyCatch = 0;


    // --------- Timer ---------
    public bool firstTimerBall = true;
    public bool doRun = true;
    public bool timerRotated = false;
    public float timerRotation = Mathf.Pi;
    public bool TimeEnded = false;
    public int TimerBalls = 0;


    // --------- Game states ---------
    public string GameState;
    public string StateIdle = "Idle";
    public string StateReady = "Ready";
    public string StateStart = "Starting";
    public string StateTimeOver = "TimeOver";
    public string StateGameOver = "Ended";


    // --------- Collectibles ---------
    public float directionMin = 1f + Mathf.Pi;
    public float directionMax = 2.2f + Mathf.Pi;
    public float speedMin = 4f;
    public float speedMax = 6f;
    public Timer CharacterTimer;


    // --------- Levels ---------
    Sprite LabelStar1;
    Sprite LabelStar2;
    Sprite LabelStar3;
    Sprite LabelTag;
    AudioStreamPlayer NextLevelSound;


    // --------- General ---------
    public Control IdleInfo;
    public AudioStreamPlayer BackgroundMusicIdle;
    public AudioStreamPlayer BackgroundMusicGame;



    // ----------------------------- _Ready function -----------------------------
    public override void _Ready()
    {
        // --------- Define the variables ---------
        counter = GetNode<Label>("/root/Spatial/counter/label-tag/Label");
        CharacterTimer = GetNode<Timer>("/root/Spatial/CharacterTimer");
        BackgroundMusicIdle = GetNode<AudioStreamPlayer>("/root/Spatial/BackgroundMusicIdle");
        BackgroundMusicGame = GetNode<AudioStreamPlayer>("/root/Spatial/BackgroundMusicGame");

        LabelTag = GetNode<Sprite>("/root/Spatial/counter/label-tag");
        LabelStar1 = GetNode<Sprite>("/root/Spatial/counter/levels/WhiteStars1");
        LabelStar2 = GetNode<Sprite>("/root/Spatial/counter/levels/GoldStars");
        LabelStar3 = GetNode<Sprite>("/root/Spatial/counter/levels/WhiteStars3");
        NextLevelSound = GetNode<AudioStreamPlayer>("/root/Spatial/counter/NextLevelSound");

        IdleInfo = GetNode<Control>("/root/Spatial/IdleInfo");

        // --------- Set variables ---------
        GameState = StateIdle;
        IdleInfo.Visible = true;

        // --------- Run the correct background music ---------
        BackgroundMusicIdle.Play();
        BackgroundMusicGame.Stop();

        // --------- Set the level to level 0 ---------
        Level0();
    }


    // ----------------------------- Counter function -----------------------------
    public void Counter()
    {
        points++; // add one to points
        counter.Text = points.ToString(); // update the text on the label
        Vector2 offset = new Vector2(0, -40); // Offset for the label movement
        LabelTag.Translate(offset); // Move the label towards the next level



        // --------- Define the number of points needed for different level ---------
        if (points == 2)
        {
            Level1();
        }
        if (points == 12)
        {
            Level2();
        }
        if (points == 23)
        {
            Level3();
        }
    }


    // ----------------------------- Reset game function -----------------------------
    public void ResetGameValues()
    {
        // --------- Reset Counter ---------
        ReadyCatch = 0;
        points = 0;
        counter.Text = points.ToString();

        // --------- Reset Timer ---------
        firstTimerBall = true; // Make the timer working again
        doRun = true;
        timerRotated = false;

        // --------- Reset Level ---------
        LabelStar1.Visible = false;
        LabelStar2.Visible = false;
        LabelStar3.Visible = false;
        Vector2 pos = new Vector2(86.092f, 1044.191f);
        LabelTag.Position = pos;
        Level0();

        // --------- General Reset ---------
        GameState = StateIdle;
        IdleInfo.Visible = true;
        BackgroundMusicIdle.Play();
        BackgroundMusicGame.Stop();
    }



    // ----------------------------- Level functions -----------------------------
    public void Level0()
    {
        speedMin = 4f;
        speedMax = 4.2f;
        CharacterTimer.WaitTime = 1.5f;
    }

    public void Level1()
    {
        speedMin = 8f;
        speedMax = 8.3f;
        CharacterTimer.WaitTime = 0.7f;

        LabelStar1.Visible = true;
        NextLevelSound.Play();
    }
    public void Level2()
    {
        speedMin = 12f;
        speedMax = 12.2f;
        CharacterTimer.WaitTime = 0.5f;

        LabelStar2.Visible = true;
        NextLevelSound.Play();
    }
    public void Level3()
    {
        speedMin = 18f;
        speedMax = 18f;
        CharacterTimer.WaitTime = 0.2f;

        LabelStar3.Visible = true;
        NextLevelSound.Play();
    }
}
