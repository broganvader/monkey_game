using Sandbox;
using static Sandbox.Component;
using System;
using Sandbox.Movement;

public sealed class AirplaneBase : Component, IScenePhysicsEvents
{
    [Property] public float Thrust { get; set; } = 100000f;
    [Property] public float Airbrake_Thrust { get; set; } = 100000f;
    [Property] public float Pitch_Force { get; set; } = 2000000f;
    [Property] public float Roll_Force { get; set; } = 5000000f;
    [Property] public float Rotational_Drag_Force { get; set; } = 1000f;
    [Property] public float Drag_Force { get; set; } = 5000f;


    // [Property] public float Yaw_Force { get; set; } = 50000f; // only pitch and roll
    [Property] public float max_speed {get; set; } = 3500f;
    [Property] public float min_speed_thresh {get; set; } = 200f;
    [Property] public float max_speed_thresh {get; set; } = 3500f;
    // [Property] public float Grarvity_Scale_Real {get; set; } = 0.2f;
    [Property] public float lift_scale { get; set; } = 2000f;

    [Property] public float Mount_Radius { get; set; } = 256f;
    [Property] public float Mount_Cooldown { get; set; } = 0.5f;


    [Property] public GameObject PilotSeat { get; set; }

    public List<GameObject> Seats { get; set; }


    public PlanePlayerController pilot;
    private float catching_wind; // 0-1, based on max speed

    TimeSince MountCooldownTracker = 0f;

    void update_catching_wind(Rigidbody body){
        float cur_speed = body.Velocity.Length;

        if (cur_speed <= min_speed_thresh){
            catching_wind = 0f;
        }
        else if (cur_speed <= max_speed_thresh){
            catching_wind = (cur_speed - min_speed_thresh) / (max_speed_thresh - min_speed_thresh); 
        }
        else {
            catching_wind = 1f;
        }
    }

    void draw_vector_debug( Vector3 Vector, Color Color )
    {
        Vector3 startPos = WorldPosition;

        // Let's draw a vector representing the forward direction scaled by 100 units
        Vector3 endPos = startPos + Vector;

        // Draw a simple line (defaults to white, visible for 1 frame)
        // DebugOverlay.Line( startPos, endPos );

        // Draw with custom Color, Duration (seconds), and depth test (overlay)
        // Setting overlay: true renders it on top of geometry
        DebugOverlay.Line( startPos, endPos, Color, duration: 0f, overlay: true );
    }
    Rigidbody body;
    // public GameObject SpawnPointChild;
    public GameObject SpawnPointChild;



    protected override void OnStart()
    {
        List<GameObject> allChildren = GameObject.Children;
        SpawnPointChild = allChildren.FirstOrDefault( x => x.Name == "SpawnPoint" );
        Seats = ParseSeats( allChildren );
    

        // SpawnPointLoc = SpawnPointChild.WorldPosition;

        body = Components.Get<Rigidbody>();
        // Tags.Add( "plane" );
        GameObject.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
        base.OnStart();
    }
    

    void PlanePhysics()
    {
        
        update_catching_wind( body );
        Rotation currentRotation = WorldRotation;

        if ( body == null )
            return;

        //apply lift:
        // body.ApplyForce(
        //     currentRotation.Up * lift_scale * catching_wind
        // );
        //get total amoutn of air hitting the plane
        // Vector3 drag_vector = body.Velocity.ProjectOnNormal( currentRotation.Forward.Normal ) - body.Velocity;

        Vector3 rotational_drag_vector = -body.Velocity.Cross( currentRotation.Forward.Normal );


        //get lift
        // Vector3 lift_vector = currentRotation.Up.Dot(drag_vector);
        Vector3 lift_vector = body.Velocity.ProjectOnNormal( currentRotation.Up.Normal );

        //apply lift
        body.ApplyForce(
            lift_vector * lift_scale * -1 * catching_wind
        );

        //apply rotational drag
        body.ApplyTorque(
            rotational_drag_vector * Rotational_Drag_Force
        );

        // draw_vector_debug( rotational_drag_vector, Color.Red );
        // draw_vector_debug( drag_vector, Color.Blue );

        //TODO add drag
    }

    void InputPhysics()
    {
        // Log.Info( $"Physics" );
        Rotation currentRotation = WorldRotation;
        // if ( Input.Down( "Use" ) && MountCooldownTracker >= Mount_Cooldown )
        // {
        //     MountCooldownTracker = 0f;
        //     DismountSeat( pilot );
        // }

        //Forwards and back
        if ( Input.Down( "Jump" ) && body.Velocity.Length <= max_speed )
        {
            Log.Info( $"IM THRUSTTTINGGG:  {body.Velocity.Length}" );
            body.ApplyForce(
                currentRotation.Forward * Thrust
            );
        }
        if ( Input.Down( "Duck" ) )
        {
            body.ApplyForce(
                currentRotation.Backward * Thrust
            );
        }


        //Roll
        if ( Input.Down( "Left" ) )
        {
            body.ApplyTorque(
                currentRotation.Forward * Roll_Force * -1 * catching_wind
            );
        }
        if ( Input.Down( "Right" ) )
        {
            body.ApplyTorque(
                currentRotation.Forward * Roll_Force * catching_wind
            );
        }

        //pitch
        if ( Input.Down( "Forward" ) )
        {
            body.ApplyTorque(
                currentRotation.Right * Pitch_Force * -1 * catching_wind
            );
        }
        if ( Input.Down( "Backward" ) )
        {
            body.ApplyTorque(
                currentRotation.Right * Pitch_Force * catching_wind
            );
        }
    }

    // public void PrePhysicsStep()
    // {
    //     PlanePhysics();
    //     if ( pilot != null ) InputPhysics();
    
    // }

    // public void 

    protected override void OnFixedUpdate()
    {
        // update_catching_wind( body );
        // Rotation currentRotation = WorldRotation;

        // if ( body == null )
        //     return;

        // //apply lift:
        // // body.ApplyForce(
        // //     currentRotation.Up * lift_scale * catching_wind
        // // );
        // //get total amoutn of air hitting the plane
        // // Vector3 drag_vector = body.Velocity.ProjectOnNormal( currentRotation.Forward.Normal ) - body.Velocity;

        // Vector3 rotational_drag_vector = -body.Velocity.Cross( currentRotation.Forward.Normal );


        // //get lift
        // // Vector3 lift_vector = currentRotation.Up.Dot(drag_vector);
        // Vector3 lift_vector = body.Velocity.ProjectOnNormal( currentRotation.Up.Normal );

        // //apply lift
        // body.ApplyForce(
        //     lift_vector * lift_scale * -1 * catching_wind
        // );

        // //apply rotational drag
        // body.ApplyTorque(
        //     rotational_drag_vector * Rotational_Drag_Force * catching_wind
        // );

        // // draw_vector_debug( rotational_drag_vector, Color.Red );
        // // draw_vector_debug( drag_vector, Color.Blue );

        // //TODO add drag

	    // Log.Info( $"Owner is {Network.OwnerId}" );
        PlanePhysics();
        if ( pilot != null ) InputPhysics();


    }

    public void PutInNearSeat( PlanePlayerController player )
    {
        foreach (GameObject Seat in Seats )
        {
           
        }
    }
    
    static List<GameObject> ParseSeats( List<GameObject> children)
    {
        List<GameObject> output = new List<GameObject>();
        foreach ( GameObject child in children )
        {
            if ( child.Tags.Has( "sittable" ) ) output.Add( child );
        }
        return output;
    }

    // [Rpc.Owner]
    // private void ChangeOwnership(PlanePlayerController player)
    // {
    //     // GameObject.Network.SetOwnerTransfer( player );
    // }


    public void MountSeat( PlanePlayerController player )
    {
        Log.Info( $"MOUNTING" );
        // Log.Info($"Mount_Cooldown: {Mount_Cooldown}");
        if ( MountCooldownTracker < Mount_Cooldown ) return;
        if ( player.Tags.Has( "sitting" ) )
        {
            DismountSeat( player );
            return;
        }
        MountCooldownTracker = 0f;

        player.Tags.Add( "sitting" );

        player.Body.Enabled = false;
        player.ColliderObject.Enabled = false;
        player.UseAnimatorControls = false;

        player.Renderer.Set( "sit", 4 );
        // player.GameObject.SetParent( GameObject );
        player.WorldPosition = PilotSeat.WorldPosition;

        pilot = player;
        GameObject.Network.TakeOwnership();

    }

    public void DismountSeat( PlanePlayerController player )
    {
        Log.Info($"DIS MOUNTING");
        if (MountCooldownTracker < Mount_Cooldown )
        {
            Log.Info($"COOLDOWN FAILED DIS MOUNTING");
            return;
        } 
        MountCooldownTracker = 0f;

        player.Tags.Remove("sitting");

        player.Body.Enabled = true;
        player.ColliderObject.Enabled = true;
        player.UseAnimatorControls = true;

        // pilot.LocalPosition = SpawnPointChild.LocalPosition;
        // pilot.LocalRotation = SpawnPointChild.LocalRotation;
        // pilot.GameObject.SetParent( null );
        pilot = null;

        GameObject.Network.DropOwnership();

    }
    
    

//     public bool Press( IPressable.Event e) {
//         //put this into the airplane trigger
//         // if ( e.Source is PlanePlayerController player )
//         // {
//         //     MoveModePlaneWalk movemode = player.Components.Get<MoveModePlaneWalk>();
//         //     if ( player.Tags.Has( "InPlane" ) )
//         //     {
//         //         movemode.Airplane = null;
//         //         player.Tags.Remove( "InPlane" );
//         //         // player.WorldPosition = 
//         //     }
//         //     else
//         //     {
//         //         movemode.Airplane = GameObject;
//         //         player.Tags.Add( "InPlane" );
//         //         // player.LocalPosition = SpawnPointChild.LocalPosition; //SpawnPointChild.WorldPosition; BROKEN. Done in 
//         //     }

//         //     //player.UseInputControls = false;
//         //     // MountCooldownTracker = 0f;
//         //     // MountSeat( player );
//         // }
        
//         return true;
//     }
}