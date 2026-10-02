using Sandbox;

public sealed class Cannonball : Component
{
	TimeSince shotout = 0;

	
	
	protected override void OnUpdate()
	{
		if (shotout >= 20f){
			GameObject.Destroy ();
		}
	}
}
