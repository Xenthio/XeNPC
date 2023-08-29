using Sandbox;
using System;
using System.Drawing;

namespace XeNPC2;

public partial class NPC : AnimatedEntity
{ 
	public void TryAnimate()
	{
		WithVelocity( Velocity );
		WithWishVelocity(WishVelocity);
	}

	public float Neck
	{
		get => GetAnimParameterFloat( "neck" );
		set => SetAnimParameter( "neck", value );
	}


	public void WithVelocity( Vector3 Velocity )
	{
		var dir = Velocity;
		var forward = Rotation.Forward.Dot( dir );
		var sideward = Rotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		SetAnimParameter( "move_direction", angle );
		SetAnimParameter( "move_speed", Velocity.Length );
		SetAnimParameter( "move_groundspeed", Velocity.WithZ( 0 ).Length );
		SetAnimParameter( "move_y", sideward );
		SetAnimParameter( "move_x", forward );
		SetAnimParameter( "move_z", Velocity.z );
	}

	public void WithWishVelocity( Vector3 Velocity )
	{
		var dir = Velocity;
		var forward = Rotation.Forward.Dot( dir );
		var sideward = Rotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		SetAnimParameter( "wish_direction", angle );
		SetAnimParameter( "wish_speed", Velocity.Length );
		SetAnimParameter( "wish_groundspeed", Velocity.WithZ( 0 ).Length );
		SetAnimParameter( "wish_y", sideward );
		SetAnimParameter( "wish_x", forward );
		SetAnimParameter( "wish_z", Velocity.z );
	}

}
