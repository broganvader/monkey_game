using Sandbox;
using static Sandbox.Gizmo;
using static Sandbox.ModelPhysics;
using static Sandbox.SubtitleTrack;
using static Sandbox.VertexLayout;
using System.Numerics;
using System;
using Sandbox.Movement;

public sealed class MoveModePlaneWalk : PlaneMoveModeWalk
{
	[Property] public GameObject Airplane { get; set; }
	[Property] float GravityScale { get; set; } = 1.0f;


	public Transform PlaneTransform;
	Transform PrevPlaneTransform;
	public Rigidbody AirplaneBody;
	public bool WasAirplaneNull;
	private bool WasSitting;
	Transform spawnpoint;
	protected override void OnStart()
	{
		if ( Airplane != null )
		{
			WasAirplaneNull = false;
			AirplaneBody = Airplane.GetComponent<Rigidbody>();
			// Controller.BodyCollisionTags.Add("player");
			PlaneTransform = Airplane.WorldTransform;
			PrevPlaneTransform = PlaneTransform;
		}
		else
		{
			AirplaneBody = null;
			PlaneTransform = global::Transform.Zero;
			PrevPlaneTransform = PlaneTransform;
		}
		Tags.Add( "player" );
		Controller = Components.GetOrCreate<PlanePlayerController>();

		Controller.RotationSpeed = 1.0f;

		base.OnStart();


	}

	// taken straight from controller internal code
	internal Vector3 WithVertical( Vector3 value, float vertical )
	{
		var up = PlaneTransform.Rotation.Up;
		return value - up * value.Dot( up ) + up * vertical;
	}

	public override void AddVelocity()
	{
		var body = Controller.Body;
		var wish = Controller.WishVelocity;
		if ( wish.IsNearZeroLength ) return;

		var groundFriction = 0.25f + Controller.GroundFriction * 10;
		var groundVelocity = Controller.GroundVelocity;

		var vertical = body.Velocity.Dot( PlaneTransform.Rotation.Up );

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


	public override void ModifyCamera( ref CameraView view )
	{
		// Vector3 Offset = PrevPlaneTransform.Position - PlaneTransform.Position;
		if ( IsProxy ) return;
		if ( Airplane != null )
		{
			// Log.Info( "AIRPLANE NOT NULL" );
			view.Rotation = view.Rotation * Rotation.Difference( Airplane.LocalRotation, Airplane.WorldRotation );
		}
		// view.Position += Offset;
		base.ModifyCamera( ref view );
	}

	public override Transform CalculateEyeTransform()
	{
		var transform = new Transform();
		transform.Position = Controller.WorldPosition + PlaneTransform.Rotation.Up * (Controller.CurrentHeight - Controller.EyeDistanceFromTop);
		transform.Rotation = PlaneTransform.Rotation * Controller.EyeAngles.ToRotation();
		return transform;
	}


	public override void UpdateRigidBody( Rigidbody body )
	{
		body.Gravity = false;
		bool wantsbrakes = Controller.IsOnGround && Controller.WishVelocity.Length < 1 && Controller.GroundVelocity.Length < 1;
		body.LinearDamping = wantsbrakes ? (10.0f * Controller.BrakePower) : Controller.AirFriction;

		body.AngularDamping = 1f;
	}

	bool needsTP = false;
	float PerSecToPerTick = 1f / ProjectSettings.Physics.FixedUpdateFrequency;
	Vector3 LastPlaneVelocity = Vector3.Zero;
	Vector3 LastPlaneAngVelocity = Vector3.Zero;
	Rotation LastPlaneRotDiff = Rotation.Identity;
	Transform PlayerLocalTransform = global::Transform.Zero;

	private Transform PlayerTransformPrePhys = global::Transform.Zero;

	public override void PrePhysicsStep()
	{
		if ( IsProxy ) return;
		if ( Airplane != null )
		{
			if ( WasAirplaneNull )
			{
				// Log.Info( "Airplane Start" );
				// AirplaneBase airplanelogic = Airplane.GetComponent<AirplaneBase>();
				// GameObject spawnpointchild = airplanelogic.SpawnPointChild;

				// spawnpoint = spawnpointchild.WorldTransform;
				spawnpoint = Airplane.GetComponent<AirplaneBase>().SpawnPointChild.WorldTransform;

				WasAirplaneNull = false;
				needsTP = true;
			}
			if ( AirplaneBody == null )
			{
				AirplaneBody = Airplane.GetComponent<Rigidbody>();
			}

			PlaneTransform = Airplane.WorldTransform;
			Controller.UpDirection = PlaneTransform.Rotation.Up;


			// Controller.BodyCollider.ColliderFlags = ColliderFlags.IgnoreMass;
			// if ( !GameObject.Tags.Has("sitting" ) )
			// {
				// if ( WasSitting )
				// {
				// 	WasSitting = false;
				// 	spawnpoint = Airplane.GetComponent<AirplaneBase>().SpawnPointChild.WorldTransform;
				// 	needsTP = true;
				// }

			PlayerLocalTransform = PrevPlaneTransform.ToLocal( WorldTransform );


			Vector3 PlaneVelocity = AirplaneBody.Velocity * PerSecToPerTick;
			Vector3 PlaneAcceleration = PlaneVelocity - LastPlaneVelocity;// add this to player too, so they dnt slide back when plane accelerates

			Rotation PlaneRotDiff = Rotation.Difference( PrevPlaneTransform.Rotation, PlaneTransform.Rotation );

			WorldTransform = Airplane.WorldTransform.ToWorld( PlayerLocalTransform );

			//Fix lagging behind by 1 tick by planes. 
			WorldPosition += PlaneAcceleration;
			//Fix weird rotation being based around next tick's position. 
			WorldPosition = WorldPosition.RotateAround( WorldPosition + PlaneVelocity, PlaneRotDiff);

			// Draw center of rotation. 
			// Gizmo.Draw.Color = Colors.Red;
			// Gizmo.Draw.LineSphere( WorldPosition + PlaneVelocity, 15 );

			LastPlaneVelocity = PlaneVelocity;
			PrevPlaneTransform = PlaneTransform;
			// }
			// else	//player is sitting
			// {
			// 	WasSitting = true;
			// }
		}
		else
		{
			PlaneTransform = global::Transform.Zero;
			Controller.UpDirection = Vector3.Up;
			WasAirplaneNull = true;
		}
		if ( needsTP )
		{
			Controller.Body.Velocity = Vector3.Zero;
			Vector3 PlaneVelocity = AirplaneBody.Velocity * PerSecToPerTick;
			spawnpoint.Position += PlaneVelocity; //account for tick delay
			WorldTransform = spawnpoint;
			needsTP = false;
		}
		// if ( !Controller.IsOnGround )
		// {
			Controller.Body.ApplyForce( -1 * Controller.UpDirection * 112000 * GravityScale );
		// }
		base.PrePhysicsStep();


	}
	public override void PostPhysicsStep()
	{
		if ( Airplane != null )
		{
			if ( !GameObject.Tags.Has( "sitting" ) )
			{
				// WorldTransform = Airplane.WorldTransform.ToWorld( PlayerLocalTransform );

				// Controller.Body.WorldTransform = Airplane.WorldTransform.ToWorld( PlayerTransformPrePhys );
			}
		}
	}

	public override bool IsStandableSurface( in SceneTraceResult result )
	{
		// if ( Vector3.GetAngle( Controller.UpDirection, result.Normal ) > GroundAngle )
		// 	return false;

		//dont care lolge

		return true;
	}
	
	protected override void OnUpdate()
	{
		if(Airplane != null) PlaneTransform = Airplane.WorldTransform;
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

		Vector3 direction = eyes * input;

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



		// Rotation RotDiff = Rotation.Difference( Airplane.WorldRotation, Airplane.LocalRotation );
		eyes = eyes.Angles() with { pitch = 0 };
		eyes = PlaneTransform.Rotation * eyes;
		// 


		return UpdateMoveBaseAltered( eyes, input );
	}

	protected override void OnRotateRenderBody( SkinnedModelRenderer renderer )
	{
		// base.OnRotateRenderBody(renderer);
		if ( IsProxy ) return;

		// if ( Scene.Is2D )
		// 	return;

		Vector3 up = Controller.UpDirection;
		// Vector3 forward = Airplane.WorldRotation.Forward;


		// Angles eyeAngles = Controller.EyeTransform.Rotation.Angles(); // 
		Rotation eyeRotation = Controller.EyeTransform.Rotation;
		
		//remove up component
		Vector3 eyeForward = eyeRotation.Forward;
		// Vector3 Up = Airplane.WorldRotation.Up;

		Vector3 flatEye = eyeForward - up * eyeForward.Dot(up);

		// float flatEyeDegrees = flatEye.Angle( forward );

		Rotation targetAngle = Rotation.LookAt( flatEye.Normal, up );



		// Rotation targetAngle = eyeForward - // Rotation.FromYaw( eyeAngles.yaw );// * Airplane.WorldRotation;

		// Rotation targetAngle = Controller.EyeTransform.Rotation.Angles();
		
		Vector3 velocity = Controller.WishVelocity.WithZ( 0 );

		float rotateDifference = renderer.WorldRotation.Distance( targetAngle );
		// Rotation oldRotation = renderer.WorldRotation;

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
