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
		WorldRotation = ObjToFollow.WorldTransform.Rotation.RotateAroundAxis( FollowingBody.AngularVelocity.Normal,  FollowingBody.AngularVelocity.Length );
		WorldPosition = ObjToFollow.WorldTransform.Position + (FollowingBody.Velocity * PerSecToPerTick);
	}

	public Transform UpdateTransform(Transform Input )
	{
		return new Transform(Input.Position + (FollowingBody.Velocity * PerSecToPerTick), Input.Rotation.RotateAroundAxis( FollowingBody.AngularVelocity,  FollowingBody.AngularVelocity.Length), 1);
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
		UpdatePosition();
	}

// 	public override void OnFixedUpdate()
// 	{
// 		// UpdatePosition();
// 	}
}
