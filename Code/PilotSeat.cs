using Sandbox;

public sealed class PilotSeat : Component, Component.IPressable
{
	GameObject airplane;
	AirplaneBase airplanebase;
	protected override void OnStart()
	{	
		airplane = GameObject.Parent;
		airplanebase = airplane.Components.Get<AirplaneBase>();
		Log.Info( $"airplane: {airplane}" );

	}

	public bool Press( IPressable.Event e) {
		Log.Info( $"pressed" );
		if ( e.Source is PlanePlayerController player ) {
			// player.Mode.Airplane = GameObject.Parent;
			airplanebase.MountSeat(player);
		}


		return true;
	}

}
