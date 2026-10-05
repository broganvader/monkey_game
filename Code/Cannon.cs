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
	
	// GameObject parent_object = Parent;
    
	protected override void OnUpdate()
	{	
		Rotation currentRotation = WorldRotation;
		Rigidbody body = Components.GetInAncestorsOrSelf<Rigidbody>();
        // Log.Info( $"Cannon_Operator: {cannon_operator}" );

        if (cannon_operator == null){
            return;
        }

		if (Input.Down( "Attack1") && last_shot > Cooldown ){
            Log.Info( $"Cannon_Operator: {cannon_operator}" );
			GameObject instance = projectile.Clone(spawner.WorldPosition);
			var projectile_body = instance.GetComponent<Rigidbody>();
			if( projectile_body.IsValid() ){
				projectile_body.ApplyForce(WorldRotation.Forward * Projectile_Force);
			}
            last_shot = 0;

            Log.Info( $"body: {body}" );
            Log.Info( $"spawner: {spawner}" );
			body.ApplyImpulseAt(spawner.WorldPosition, WorldRotation.Backward * Recoil_Force);

		}

	}

	public bool Press( IPressable.Event e) {
        if ( e.Source is PlanePlayerController player ) {
            if ( cannon_operator != null ) {   // exit operator seat
                player.Tags.Remove("sitting");

                player.Body.Enabled = true;
                player.ColliderObject.Enabled = true;
                player.UseAnimatorControls = true;

                cannon_operator.GameObject.SetParent( null );
                cannon_operator = null;
                return true;
            }
            player.Tags.Add("sitting");

            //player.UseInputControls = false;
            player.Body.Enabled = false;
            player.ColliderObject.Enabled = false;
            player.UseAnimatorControls = false;

            player.Renderer.Set("sit",4);
            player.GameObject.SetParent( GameObject );
            // player.LocalPosition = Vector3.Up * 5;

            cannon_operator = player;
        }
        return true;
    }
}