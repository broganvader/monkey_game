using Sandbox;
using static Sandbox.ModelPhysics;
using static Sandbox.VertexLayout;
using System;
using Sandbox.Movement;

public sealed class MoveModePlaneWalk : PlaneMoveModeWalk
{
	[Property] GameObject Airplane { get; set; }
	[Property] float GravityScale { get; set; } = 1.0f;


	Transform PrevPlaneTransform;

	protected override void OnStart()
	{
		// Controller.BodyCollisionTags.Add("player");
		PrevPlaneTransform = Airplane.WorldTransform;
		Tags.Add( "player" );
		// Controller.RotationAngleLimit = 9999f;

		Controller = Components.GetOrCreate<PlanePlayerController>();

		Controller.RotationSpeed = 10f;

		base.OnStart();
	}

	// taken straight from controller internal code
	internal Vector3 WithVertical( Vector3 value, float vertical )
	{
		var up = Airplane.WorldRotation.Up;
		return value - up * value.Dot( up ) + up * vertical;
	}

	public override void AddVelocity()
	{

		Rigidbody AirplaneBody = Airplane.GetComponent<Rigidbody>();


		var body = Controller.Body;
		var wish = Controller.WishVelocity;
		if ( wish.IsNearZeroLength ) return;

		var groundFriction = 0.25f + Controller.GroundFriction * 10;
		var groundVelocity = Controller.GroundVelocity;

		var vertical = body.Velocity.Dot( Airplane.WorldRotation.Up );

		var velocity = body.Velocity - groundVelocity;
		var speed = velocity.Length;

		var maxSpeed = MathF.Max( wish.Length, speed );


		if ( Controller.IsOnGround )
		{
			var amount = 1 * groundFriction;
			velocity = velocity.AddClamped( wish * amount, wish.Length * amount );
		}
		else
		{
			var amount = 0.05f;
			velocity = velocity.AddClamped( wish * amount, wish.Length );
		}

		if ( velocity.Length > maxSpeed )
			velocity = velocity.Normal * maxSpeed;



		velocity += groundVelocity;

		if ( Controller.IsOnGround )
		{
			velocity = WithVertical( velocity, vertical );
		}

		body.Velocity = velocity;
	}


	// protected override void OnUpdate()
	// {

	// 	// Rotation CurrentAirplaneRotation = Airplane.WorldRotation;
	// 	// Vector3 CurrentAirplanePosition = Airplane.WorldPosition;
	// 	// Rotation RotationDiff = CurrentAirplaneRotation - PreviousAirplaneRotaiton;
	// 	// Vector3 PositionDiff = CurrentAirplanePosition - PreviousAirplanePosition;
	// 	// WorldRotation += RotationDiff;
	// 	// WorldPosition += PositionDiff;

	// 	// PreviousAirplaneRotaiton = CurrentAirplaneRotation;
	// 	// PreviousAirplanePosition = CurrentAirplanePosition;


	// 	// if ( GameObject.Parent == Airplane )
	// 	// {
	// 	// 	PreviousAirplaneTransform = Airplane.WorldTransform;
	// 	// 	base.OnUpdate();
	// 	// 	return;
	// 	// }

	// 	// Vector3 playerLocal = PreviousAirplaneTransform.PointToLocal( WorldPosition );
	// 	// WorldPosition = Airplane.WorldTransform.PointToWorld( playerLocal );
	// 	// PreviousAirplaneTransform = Airplane.WorldTransform;

	// 	// Log.Info( $"Ground Object:  {Controller.GroundObject}");
	// 	// Log.Info( $"Ground Velocity: {Controller.GroundVelocity}" );


	// 	//apply gravity towards airplane
	// 	// Controller.Body.ApplyForce( Airplane.WorldRotation.Down * 112000 * GravityScale );



	// 	// base.OnUpdate();
	// }


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


	public override void UpdateRigidBody( Rigidbody body )
	{
		body.Gravity = false;
		bool wantsbrakes = Controller.IsOnGround && Controller.WishVelocity.Length < 1 && Controller.GroundVelocity.Length < 1;
		body.LinearDamping = wantsbrakes ? (10.0f * Controller.BrakePower) : Controller.AirFriction;

		body.AngularDamping = 1f;
	}



	public override void PrePhysicsStep()
	{

		Controller.BodyCollider.ColliderFlags = ColliderFlags.IgnoreMass;
		if ( !GameObject.Parent.Tags.Has( "sittable" ) )
		{
			Transform CurPlaneTransform = Airplane.WorldTransform;
			Transform PlayerLocalTransform = PrevPlaneTransform.ToLocal( WorldTransform );
			WorldTransform = CurPlaneTransform.ToWorld( PlayerLocalTransform );

			// Log.Info( $"Position change:  {PositionChange}" );

			// WorldTransform += TransformChange;

		}
		Controller.Body.ApplyForce( Airplane.WorldRotation.Down * 112000 * GravityScale );
		base.PrePhysicsStep();

		PrevPlaneTransform = Airplane.WorldTransform;

	}
	public override bool IsStandableSurface( in SceneTraceResult result )
	{
		// if ( Vector3.GetAngle( Controller.UpDirection, result.Normal ) > GroundAngle )
		// 	return false;

		//dont care lolge

		return true;
	}



	// public override void PrePhysicsStep()
	// {
	// 	Controller.UpDirection = Airplane.WorldRotation.Up;
	// 	base.PrePhysicsStep();
	// }

	Vector3.SmoothDamped smoothedMovement;
	private Vector3 UpdateMoveBaseAltered( Rotation eyes, Vector3 input )
	{
		// don't normalize, because analog input might want to go slow
		input = input.ClampLength( 1 );

		var direction = eyes * input;

		// Run if we're holding down alt move button
		bool run = Input.Down( Controller.AltMoveButton );

		// if Run is default, flip that logic
		if ( Controller.RunByDefault ) run = !run;

		// if we're running, use run speed, if not use walk speed
		var velocity = run ? Controller.RunSpeed : Controller.WalkSpeed;

		// if we're ducking, always use duck walk speed
		if ( Controller.IsDucking ) velocity = Controller.DuckedSpeed;

		if ( direction.IsNearlyZero( 0.1f ) )
		{
			direction = 0;
		}
		else
		{
			// Retain momentum, once we're moving, we're moving. Don't lerp between directions, only between speeds.
			smoothedMovement.Current = direction.Normal * smoothedMovement.Current.Length;
		}

		//
		// Smooth the wish velocity
		//
		smoothedMovement.Target = direction * velocity;
		smoothedMovement.SmoothTime = smoothedMovement.Target.Length < smoothedMovement.Current.Length ? Controller.DeaccelerationTime : Controller.AccelerationTime;
		smoothedMovement.Update( Time.Delta );

		// If it's near zero, just stop
		if ( smoothedMovement.Current.IsNearlyZero( 0.01f ) )
		{
			smoothedMovement.Current = 0;
		}

		//DebugOverlay.ScreenText( 200, $"{smoothedMovement.Current.Length}" );

		return smoothedMovement.Current;
	}

	public override Vector3 UpdateMove( Rotation eyes, Vector3 input )
	{
		// var myupdatemove = base.base.UpdateMove(eyes, input);
		// return myupdatemove;
		Rotation RotDiff = Rotation.Difference( Airplane.WorldRotation, Airplane.LocalRotation );
		eyes = Airplane.WorldRotation * eyes;


		return UpdateMoveBaseAltered( eyes, input );
	}

	protected override void OnRotateRenderBody( SkinnedModelRenderer renderer )
	{
		// if ( Scene.Is2D )
		// 	return;

		// Angles eyeAngles = Controller.EyeTransform.Rotation.Angles();

		// Rotation targetAngle = Rotation.FromYaw( eyeAngles.yaw ) * Airplane.WorldRotation;

		Rotation targetAngle = Controller.EyeTransform.Rotation.Angles();
		Vector3 velocity = Controller.WishVelocity.WithZ( 0 );

		float rotateDifference = renderer.WorldRotation.Distance( targetAngle );
		Rotation oldRotation = renderer.WorldRotation;

		// We're over the limit - snap it 
		if ( rotateDifference > Controller.RotationAngleLimit )
		{
			var delta = 0.999f - Controller.RotationAngleLimit / rotateDifference;
			var newRotation = Rotation.Lerp( renderer.WorldRotation, targetAngle, delta );

			renderer.WorldRotation = newRotation;
		}

		// Otherwise only rotate while moving
		if ( velocity.Length > 10 )
		{
			// TODO: frame rate dependent

			renderer.WorldRotation = Rotation.Slerp( renderer.WorldRotation, targetAngle,
				Time.Delta * 2.0f * Controller.RotationSpeed * velocity.Length.Remap( 0, 100 ) );
		}

		// AddRotationSpeed( oldRotation, renderer.WorldRotation );
	}



}
