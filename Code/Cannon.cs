using Sandbox;

public sealed class Cannon : Component, Component.IPressable
{
    [Property] public float Recoil_Force { get; set; } = 500000f;
	[Property] public float Projectile_Force { get; set; } = 500000f;
	[Property] public float Cooldown { get; set; } = 0.5f;


	[Property] GameObject projectile {get;set;}
	[Property] GameObject spawner {get;set;}
	
	TimeSince last_shot = 0;
    private PlanePlayerController cannon_operator;
    Rigidbody body;
    protected override void OnStart()
    {
        body = Components.GetInAncestorsOrSelf<Rigidbody>();
    }

    // GameObject parent_object = Parent;

    protected override void OnUpdate()
    {
        Rotation currentRotation = WorldRotation;

        // Log.Info( $"Cannon_Operator: {cannon_operator}" );

        if ( cannon_operator == null || cannon_operator.IsProxy )
        {
            // Log.Info( $"Heh, not happening: {cannon_operator}, your proxystatus is {cannon_operator.IsProxy}" );
            return;
        }

        if ( Input.Down( "Attack1" ) && last_shot > Cooldown )
        {
            ShootCannon();

        }

    }

    [Rpc.Broadcast]
    private void PlaySound()
    {
        Sound.Play( "explosion1" );
    }

    [Rpc.Host]
    private void Recoil()
    {
        body.ApplyImpulseAt( spawner.WorldPosition, WorldRotation.Backward * Recoil_Force );

    }

    private void ShootCannon()
    {
        GameObject instance = projectile.Clone( spawner.WorldPosition );
        instance.NetworkSpawn();
        var projectile_body = instance.GetComponent<Rigidbody>();
        if ( projectile_body.IsValid() )
        {
            projectile_body.ApplyForce( WorldRotation.Forward * Projectile_Force );
        }
        last_shot = 0;
        Recoil();
        Sound.Play( "explosion1" );

    }
    public bool Press( IPressable.Event e )
    {
        if ( e.Source is PlanePlayerController player )
        {
            if ( cannon_operator != null && player.IsProxy ) return false; // Dont let other players kick cannoneer out
            if ( cannon_operator != null )
            {   // exit operator seat
                player.Tags.Remove( "sitting" );

                player.Body.Enabled = true;
                player.ColliderObject.Enabled = true;
                player.UseAnimatorControls = true;

                // cannon_operator.GameObject.SetParent( null );
                cannon_operator = null;
                return true;
            }
            player.Tags.Add( "sitting" );

            //player.UseInputControls = false;
            player.Body.Enabled = false;
            player.ColliderObject.Enabled = false;
            player.UseAnimatorControls = false;

            player.Renderer.Set( "sit", 4 );
            // player.GameObject.SetParent( GameObject );
            // player.LocalPosition = Vector3.Up * 5;

            cannon_operator = player;
        }
        return true;
    }
}