using Sandbox;
using XeNPC2.Nav;
namespace XeNPC2;

public partial class NPC : AnimatedEntity
{
	public Rotation TargetRotation;
	public NavSteer Navigation = new();

	public virtual bool UsesNavigation => true;


	[ConVar.Replicated]
	public static bool nav_drawpath { get; set; }

	public virtual void TryNavigate()
	{
		if ( Navigation != null && UsesNavigation )
		{
			using var _b = Profile.Scope( "Steer" );
			Navigation.Tick( Position );

			if ( !Navigation.Output.Finished )
			{
				ProcessNavigationDirection( Navigation.Output.Direction.Normal );
			}

			if ( nav_drawpath )
			{
				Navigation.DebugDrawPath();
			} 
			Rotation = Rotation.Lerp( Rotation, TargetRotation, Time.Delta * 2 );
		}
	}
}
