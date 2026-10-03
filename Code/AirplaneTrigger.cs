using Sandbox;

public sealed class AirplaneTrigger : Component, Component.ITriggerListener
{

	public void OnTriggerEnter( Collider other )
	{
		Log.Info( $"{other.GameObject.Name} entered the pickup zone" );
	}

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
