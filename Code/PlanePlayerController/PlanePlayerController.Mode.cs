using Sandbox.Movement;
using System;
namespace Sandbox;


public sealed partial class PlanePlayerController : Component
{
	public MoveModePlaneWalk Mode { get; private set; }

	void ChooseBestMoveMode()
	{
		// var best = GetComponents<MoveMode>( false ).MaxBy( x => x.Score( this ) );
		var best = Components.Get<MoveModePlaneWalk>();
		if ( Mode == best ) return;

		Mode?.OnModeEnd( best );

		Mode = best;

		if ( Body?.PhysicsBody is { } body )
		{
			body.Sleeping = false;
		}

		Mode?.OnModeBegin();
	}
}
