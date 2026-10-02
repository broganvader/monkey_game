using Sandbox;

public sealed class MoveModePlaneWalk : Sandbox.Movement.MoveModeWalk
{	
	[Property] GameObject Airplane { get; set; }

	Rotation PreviousAirplaneRotaiton;
	Vector3 PreviousAirplanePosition;

	Transform PreviousAirplaneTransform;


	protected override void OnStart()
	{
		PreviousAirplaneTransform = Airplane.WorldTransform;
		base.OnStart();
	}
	protected override void OnUpdate()
	{
		// Rotation CurrentAirplaneRotation = Airplane.WorldRotation;
		// Vector3 CurrentAirplanePosition = Airplane.WorldPosition;
		// Rotation RotationDiff = CurrentAirplaneRotation - PreviousAirplaneRotaiton;
		// Vector3 PositionDiff = CurrentAirplanePosition - PreviousAirplanePosition;
		// WorldRotation += RotationDiff;
		// WorldPosition += PositionDiff;

		// PreviousAirplaneRotaiton = CurrentAirplaneRotation;
		// PreviousAirplanePosition = CurrentAirplanePosition;


		// if ( GameObject.Parent == Airplane )
		// {
		// 	PreviousAirplaneTransform = Airplane.WorldTransform;
		// 	base.OnUpdate();
		// 	return;
		// }

		// Vector3 playerLocal = PreviousAirplaneTransform.PointToLocal( WorldPosition );
		// WorldPosition = Airplane.WorldTransform.PointToWorld( playerLocal );
		// PreviousAirplaneTransform = Airplane.WorldTransform;


		CameraView myView;
		myView.Rotation = Rotation.Difference( Airplane.WorldRotation, Airplane.LocalRotation );

		base.OnUpdate();
	}


	public override void ModifyCamera( ref CameraView view )
	{
		view.Rotation = view.Rotation * Rotation.Difference( Airplane.LocalRotation, Airplane.WorldRotation );
		base.ModifyCamera( ref view );
	}
	
	public override Transform CalculateEyeTransform()
	{
		var transform = new Transform();
		transform.Position = Controller.WorldPosition + Airplane.WorldRotation.Up * (Controller.CurrentHeight - Controller.EyeDistanceFromTop);
		transform.Rotation = Airplane.WorldRotation * Controller.EyeAngles.ToRotation();
		return transform;
	}


	// public override void PrePhysicsStep()
	// {
	// 	Controller.UpDirection = Airplane.WorldRotation.Up;
	// 	base.PrePhysicsStep();
	// }


	public override Vector3 UpdateMove( Rotation eyes, Vector3 input )
	{
		// var myupdatemove = base.base.UpdateMove(eyes, input);
		// return myupdatemove;
		Rotation RotDiff = Rotation.Difference(Airplane.WorldRotation , Airplane.LocalRotation );
		eyes = Airplane.WorldRotation * eyes;

		return base.UpdateMove( eyes, input );
	}
}
