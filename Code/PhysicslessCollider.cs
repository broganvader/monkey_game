using Sandbox;

public sealed class PhysicslessCollider : Component, IScenePhysicsEvents
{
	[Property] public GameObject ObjToFollow { get; set; }

	protected override void OnStart()
	{
		Tags.Add("player");
	}
	public void PrePhysicsStep()
	{
		WorldRotation = ObjToFollow.WorldTransform.Rotation;
		WorldPosition = ObjToFollow.WorldTransform.Position;

	}
}
