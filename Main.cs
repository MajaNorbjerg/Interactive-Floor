using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Main : Spatial
{
    Global global;


    // ------------------------ Changing variables ------------------------
    float distanceToGround = 0.2f; // Meters 3.2f

    Vector2 syncPointTL = new Vector2(-8.3f, 8.3f);//(-8.3f, 8.3f)
    Vector2 syncPointBR = new Vector2(8.3f, -8.4f);//(0.3f, -0.4f)


    // --------------------------------------------------------------------


    private int width = 424; //424
    private int height = 240; //240


    RigidBody detectBody;
    MeshDataTool meshDataTool;
    PlaneMesh planeMesh;
    ArrayMesh arrayPlane;
    CollisionShape DetectionCollisionShape;
    MeshInstance meshInstance;
    ConvexPolygonShape convexPolygonShape;
    Vector3 pointZero = new Vector3(0, 0, 0);
    double distanceBetweenPoints = 0.2;



    List<List<Vector3>> detectedPoints = new List<List<Vector3>>();
    List<List<Vector3>> sortedPoints = new List<List<Vector3>>();

    Spatial CharacterContainer;

    [Export]
    public PackedScene Character;
    private Random _random = new Random();
    private float RandRange(float min, float max)
    {
        return (float)_random.NextDouble() * (max - min) + min;
    }


    int number = 2;
    uint shapeowenercount;

    float[] Points;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        global = GetNode<Global>("/root/Global");
        OnStartTimerTimeout();

        CharacterContainer = GetNode<Spatial>("/root/Spatial/CharacterContainer");

        SurfaceTool surfaceTool = new SurfaceTool();
        planeMesh = new PlaneMesh();
        planeMesh.Material = (Material)ResourceLoader.Load("mesh_material.tres");
        planeMesh.SubdivideWidth = width - 2; //Subtract two bc there's already two vertices before we start subdividing
        planeMesh.SubdivideDepth = height - 2;
        planeMesh.Size = new Vector2(42.4f, 24f); //42.47f (42.4f, 24f)

        surfaceTool.CreateFrom(planeMesh, 0);

        arrayPlane = surfaceTool.Commit();


        meshInstance = new MeshInstance();
        meshInstance.Mesh = arrayPlane;


        NewDetectedBody();
        AddChild(meshInstance);
        AddChild(detectBody);


        UpdateShapes();

    }

    void NewDetectedBody()
    {
        detectBody = new RigidBody();
        detectBody.ContactMonitor = true;
        detectBody.ContactsReported = 1;
        detectBody.GravityScale = 0; // Remove gravity
        detectBody.AxisLockLinearX = true;
        detectBody.AxisLockLinearY = true;
        detectBody.AxisLockLinearZ = true;
        detectBody.AxisLockAngularX = true;
        detectBody.AxisLockAngularY = true;
        detectBody.AxisLockAngularZ = true;


    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        base._Process(delta);
    }

    public async void UpdateShapes()
    {

        // await System.Threading.Thread.SpinWait(1000);
        await System.Threading.Tasks.Task.Delay(100);
        try
        {
            using (var frames = pipe.WaitForFrames())
            using (var depth = frames.DepthFrame)
            using (var points = pc.Process(depth).As<Points>())
            {
                // CopyVertices is extensible, any of these will do:
                float[] vertices;
                vertices = new float[points.Count * 3];
                points.CopyVertices(vertices);

                Points = vertices;

            }

            // Remove all children from the detectbody
            foreach (Node child in detectBody.GetChildren())
            {
                child.QueueFree();
            }

            if (detectBody.GetShapeOwners().Count > 0)
            {
                GD.Print(detectBody.GetShapeOwners().Count);
                for (int l = 0; l < detectBody.GetShapeOwners().Count; l++)
                {
                    detectBody.ShapeOwnerClearShapes((uint)l);
                }
            }
        }
        catch
        {

        }
        detectedPoints.Clear();
        sortedPoints.Clear();


        Vector3 currentGroundPoint = new Vector3();

        // allPoints = new Vector3[101760];

        for (int i = 0; i < meshDataTool.GetVertexCount(); i++) // 101760
        {

            Vector3 vertex = meshDataTool.GetVertex(i);


            int indexOfX = i * 3;
            int indexOfY = i * 3 + 1;
            int indexOfZ = i * 3 + 2;
            float x = Points[indexOfX];
            float y = Points[indexOfY];
            float z = Points[indexOfZ];

            bool shapeFoundForPoint = false;
            // GD.Print(x);


            if (z > distanceToGround || z == 0.0f || z < 0 || x < syncPointTL.x || y > syncPointTL.y || x > syncPointBR.x || y < syncPointBR.y)
            {
                z = 0;

            }
            else
            {
                z = (distanceToGround - z) * 2;
                if (vertex.y > 0)
                {

                    currentGroundPoint = vertex;
                    currentGroundPoint.y = 0;
                    //------------------------------------------------- For each point test --------------------------------------------------
                    if (detectedPoints.Count == 0)
                    {
                        // make a vector on the ground for the detection shape to get a height
                        List<Vector3> shapePoints = new List<Vector3> { currentGroundPoint, vertex };
                        detectedPoints.Add(shapePoints); // Add the vectors to a list with shapes

                    }
                    else
                    {
                        for (int j = 0; j < detectedPoints.Count; j++) // Run thrugh all the shapes
                        {
                            int lengthOfShape = detectedPoints[j].Count; // number of points in shape
                            // find the shape with a last added start point...
                            if (detectedPoints[j][lengthOfShape - 2].DistanceTo(currentGroundPoint) < 1) //4 distanceBetweenPoints
                            {

                                detectedPoints[j].Add(currentGroundPoint);
                                detectedPoints[j].Add(vertex);
                                shapeFoundForPoint = true;
                                break;
                            }
                        }
                        if (shapeFoundForPoint == false)
                        {
                            //... create a new shape and add the points
                            List<Vector3> shapePoints = new List<Vector3> { currentGroundPoint, vertex };
                            detectedPoints.Add(shapePoints);

                        }
                    }
                    shapeFoundForPoint = false;

                    //------------------------------------------------------------------------------------------------------------------------
                }
            }
            vertex.y = z;

            meshDataTool.SetVertex(i, vertex); // Draw shape
        }


        Vector3 prevPoint = new Vector3(10, 10, 10);
        Vector3 prevprevPoint = new Vector3(10, 10, 10);


        // ---------------------------- Add points to shapes --------------------------
        for (int i = 0; i < detectedPoints.Count; i++)
        {



            // Point (000) removal
            detectedPoints[i].RemoveAll(item => item == pointZero);

            for (int j = 0; j < detectedPoints[i].Count; j++)
            {
                if (detectedPoints[i][j].z != prevprevPoint.z)
                {
                    prevprevPoint = prevPoint;
                    prevPoint = detectedPoints[i][j];
                }
                else if (detectedPoints[i][j].z == prevprevPoint.z)
                {
                    prevprevPoint = prevPoint;
                    prevPoint = detectedPoints[i][j];

                    detectedPoints[i].Remove(prevprevPoint);
                }
            }


            int pointSets = 100000;
            var sortOut = detectedPoints[i].Where((x, n) => n % pointSets == 0 || n - 1 % pointSets == 0);


            Vector3[] convertedArr = sortOut.ToArray();

            // -------------------------- Must be turned on ----------------------
            convexPolygonShape = new ConvexPolygonShape();

            convexPolygonShape.Points = convertedArr;

            var areaShape = new Area();
            areaShape.AddToGroup("cameraDetections");
            DetectionCollisionShape = new CollisionShape();
            DetectionCollisionShape.Shape = convexPolygonShape;
            DetectionCollisionShape.Shape.Margin = 1;
            areaShape.AddChild(DetectionCollisionShape);
            detectBody.AddChild(areaShape);

            // ----------------------------------------------------------------

        }


        detectedPoints.Clear();
        sortedPoints.Clear();


        // ---------------------------- Remove surface --------------------------
        for (int j = 0; j < arrayPlane.GetSurfaceCount(); j++)
        {
            arrayPlane.SurfaceRemove(j); // Removes the drawing
        }

        meshDataTool.CommitToSurface(arrayPlane); // Resets the surface


        UpdateShapes(); // Rerun function
    }

    // ---------------------------- Start characterTimer function ----------------------------
    public void OnStartTimerTimeout()
    {
        GetNode<Timer>("CharacterTimer").Start();
    }

    // ---------------------------- Start characterTimer function ----------------------------
    public void GameOverTimerTimeout()
    {
        global.ResetGameValues(); // Run function from global class to reset game values
        GetNode<Control>("/root/Spatial/GameOver").Visible = false; // Hide the game over layover

        OnStartTimerTimeout(); // Start characterTimer again
    }



    // ---------------------------- Spawn a collectible ----------------------------
    public void CharacterTimerTimeout()
    {
        string origin = "CharacterPathRight/CharacterSpawnLocation";  // Default origin

        // List of possible origins
        List<String> origins = new List<String> { "CharacterPathButton", "CharacterPathLeft", "CharacterPathTop", "CharacterPathRight" };

        int index = _random.Next(origins.Count); // Choose random index from list
        var RandomOrigin = origins[index]; // Set variable with the origin name
        origin = $"{origins[index]}/CharacterSpawnLocation"; // The new origin is set with string interpolation

        if (RandomOrigin == "CharacterPathButton") // If the random origin is Button
        {
            // The direction span is set in radians...
            //... to make sure they run into the scene
            global.directionMin = 1f;
            global.directionMax = 2.2f;
        }
        else if (RandomOrigin == "CharacterPathLeft")
        {
            global.directionMin = 2.9f + Mathf.Pi;
            global.directionMax = 3.3f + Mathf.Pi;
        }
        else if (RandomOrigin == "CharacterPathTop")
        {
            global.directionMin = 1f + Mathf.Pi;
            global.directionMax = 2.2f + Mathf.Pi;
        }
        else if (RandomOrigin == "CharacterPathRight")
        {
            global.directionMin = 2.9f;
            global.directionMax = 3.3f;
        }


        // Create a collectible instance and add it to the scene.
        var characterInstance = (RigidBody)Character.Instance();
        CharacterContainer.AddChild(characterInstance);

        characterInstance.GetNode<AnimationPlayer>("GingerbreadMan/AnimationPlayer").Play(global.RunningAnimation);


        var characterSpawnLocation = GetNode<PathFollow>(origin); // Choose a random location on Path2D.

        // characterSpawnLocation.RotationMode = PathFollow.RotationModeEnum.Y;
        characterSpawnLocation.Offset = _random.Next(); // Set random offset

        // Position the collectible based on the random position
        Vector3 pos = new Vector3(characterSpawnLocation.Translation.x, 0, characterSpawnLocation.Translation.z);
        characterInstance.Translation = pos;


        // Set the collectibles direction to something inbetween min and max
        float direction = RandRange(global.directionMin, global.directionMax);
        Vector3 rotation = new Vector3(0, direction, 0); // Rotate on the y-axis
        characterInstance.Rotation = rotation;


        // Set the velocity. The speed is based on the level.
        characterInstance.LinearVelocity = new Vector3(RandRange(global.speedMin, global.speedMax), 0, 0).Rotated(rotation, direction);
    }


    // ---------------------------- Use mouse as a detection ----------------------------
    public override void _Input(InputEvent @event)
    {

        Plane DropPlane = new Plane(new Vector3(0, 1, 0), 0);
        Camera MainCamera = GetNode<Camera>("/root/Spatial/Camera");
        var areaMouse = GetNode<Area>("/root/Spatial/Killzone/Area");

        areaMouse.AddToGroup("cameraDetections"); // Add to the cameraDetected group
        if (@event is InputEventMouseMotion eventMouseMotion) // If the mouse is moveing
        {
            // Calculate the position on the plane according to the mouse
            Vector3? _pos3D = DropPlane.IntersectRay(
                MainCamera.ProjectRayOrigin(eventMouseMotion.Position),
                MainCamera.ProjectRayNormal(eventMouseMotion.Position)
            );

            if (_pos3D != null) // If there´s a position on the plane matching the mouse position
            {
                Spatial KillZone = GetNode<Spatial>("/root/Spatial/Killzone");
                Vector3 pos3D = (Vector3)_pos3D;
                KillZone.Translation = new Vector3(pos3D.x, pos3D.y, pos3D.z); // Move the area according to the mouse
            }
        }
    }


}
