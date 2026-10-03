using Sandbox;

public sealed class WeaponLocation : Component, Component.IPressable
{

	[Property] GameObject weapon {get;set;}

	protected override void OnUpdate()
	{

	}

	public bool Press( IPressable.Event e) {
		Log.Info( $"pressed" );

        if ( e.Source is PlanePlayerController player ) {
			Vector3 position = Vector3.Zero;//LocalPosition;
			Rotation rotation = Rotation.Identity;//LocalRotation;
			Vector3 scale = WorldScale;
			GameObject curParent = GameObject.Parent;


			Log.Info( $"position: {position}" );
			GameObject weapon_instance = weapon.Clone(GameObject, position,  rotation, scale);
			Component model_renderer = Components.Get<ModelRenderer>();
			Component model_collider = Components.Get<ModelCollider>();


			model_renderer.Enabled = false;
			model_collider.Enabled = false;


		}
		return true;
	}
}
