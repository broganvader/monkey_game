using Sandbox;
using Sandbox.Physics;

public sealed class PhysicslessCollider : Component, IScenePhysicsEvents
{
	[Property] public GameObject ObjToFollow { get; set; }
	private Rigidbody FollowingBody;
	protected override void OnStart()
	{
		FollowingBody = ObjToFollow.Components.Get<Rigidbody>();
		Tags.Add( "player" );
	}
	float PerSecToPerTick = 1f / ProjectSettings.Physics.FixedUpdateFrequency;

	private void UpdatePosition()
	{
		WorldRotation = ObjToFollow.WorldTransform.Rotation; //RotateAroundAxis( FollowingBody.AngularVelocity.Normal,  FollowingBody.AngularVelocity.Length * 90f * PerSecToPerTick);
		WorldPosition = ObjToFollow.WorldTransform.Position;// + (FollowingBody.Velocity * PerSecToPerTick);
	}

	Transform PrevObjTransform = global::Transform.Zero;

	private void UpdatePositionPredict()
	{


		Rotation Delta = Rotation.Difference( PrevObjTransform.Rotation, ObjToFollow.WorldTransform.Rotation );

		WorldRotation = ObjToFollow.WorldTransform.Rotation;// * Delta;//.RotateAroundAxis( FollowingBody.AngularVelocity.Normal, FollowingBody.AngularVelocity.Length * PerSecToPerTick );
		WorldPosition = ObjToFollow.WorldTransform.Position + (FollowingBody.Velocity * PerSecToPerTick);
	}

	public void PostPhysicsStep()
	{
		// PhysicsSettings phyiscsettings = PhysicsSettings;
		// Rigidbody FollowingBody = ObjToFollow.Components.Get<Rigidbody>();
		// Rigidbody MyBody = Components.Get<Rigidbody>();
		// MyBody.Velocity = FollowingBody.Velocity;

		// WorldRotation = ObjToFollow.WorldTransform.Rotation;
		// WorldRotation = ObjToFollow.WorldTransform.Rotation.RotateAroundAxis( FollowingBody.AngularVelocity.Normal,  FollowingBody.AngularVelocity.Length );
		// WorldRotation = ObjToFollow.WorldTransform.Rotation;
		// WorldPosition = ObjToFollow.WorldTransform.Position;//+ (FollowingBody.Velocity * PerSecToPerTick);
		// UpdatePosition();

	}
	public void PrePhysicsStep()
	{
		UpdatePositionPredict();
	}

// 	public override void OnFixedUpdate()
// 	{
// 		// UpdatePosition();
// 	}
}
