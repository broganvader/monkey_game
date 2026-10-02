using Sandbox;
using Sandbox.Physics;

public sealed class PhysicslessCollider : Component, IScenePhysicsEvents
{
	[Property] public GameObject ObjToFollow { get; set; }

	protected override void OnStart()
	{
		Tags.Add( "player" );
	}
	float PerSecToPerTick = 1f / ProjectSettings.Physics.FixedUpdateFrequency;

	public void PrePhysicsStep()
	{
		// PhysicsSettings phyiscsettings = PhysicsSettings;
		Rigidbody FollowingBody = ObjToFollow.Components.Get<Rigidbody>();
		Rigidbody MyBody = Components.Get<Rigidbody>();
		// MyBody.Velocity = FollowingBody.Velocity;

		WorldRotation = ObjToFollow.WorldTransform.Rotation;
		WorldRotation = WorldRotation.RotateAroundAxis( FollowingBody.AngularVelocity.Normal,  FollowingBody.AngularVelocity.Length );

		WorldPosition = ObjToFollow.WorldTransform.Position + (FollowingBody.Velocity * PerSecToPerTick);

	}
}
