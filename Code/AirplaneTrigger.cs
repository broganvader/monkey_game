using Sandbox;

public sealed class AirplaneTrigger : Component, Component.ITriggerListener
{
	private GameObject airplane;
	protected override void OnStart()
	{
		airplane = GameObject.Parent;

		base.OnStart();
	}

	public void OnTriggerEnter( Collider other )
	{
		Log.Info( $"Something exited" );
		var player = other.Components.GetInParentOrSelf<PlanePlayerController>();
		// Log.Info( $"{other.GameObject.Name} left the pickup zone" );
		if ( player == null ) return;
		
		Log.Info( $"Player Entered" );
		MoveModePlaneWalk movemode = player.Components.Get<MoveModePlaneWalk>();
		if ( movemode == null ) return;
		movemode.Airplane = GameObject.Parent;
		player.Tags.Add( "InPlane" );	}

    public void OnTriggerExit( Collider other )
	{
		
		Log.Info( $"Something exited" );
		var player = other.Components.GetInParentOrSelf<PlanePlayerController>();
		// Log.Info( $"{other.GameObject.Name} left the pickup zone" );
		if ( player == null ) return;
		
		Log.Info( $"Player exited" );
		MoveModePlaneWalk movemode = player.Components.Get<MoveModePlaneWalk>();
		if ( movemode == null ) return;
		movemode.Airplane = null;
		player.Tags.Remove( "InPlane" );
	}

}
