using Sandbox;

namespace XeNPC2;

public partial class NPC : AnimatedEntity
{
	MoveHelper Mover;
	public Vector3 WishDirection;


	public float GroundBounce = 0;
	public float WallBounce = 0;
	public float MaxStandableAngle = 50;

	public void TryMove()
	{

		var bbox = CollisionBounds;

		Mover = new MoveHelper( Position, Velocity );
		Mover.GroundBounce = GroundBounce;
		Mover.WallBounce = WallBounce;
		Mover.MaxStandableAngle = MaxStandableAngle;
		Mover.Trace = Mover.Trace.Ignore( this ).Size( bbox );
		if ( !Velocity.IsNearlyZero( 0.001f ) )
		{
			Mover.TryUnstuck();
			Mover.TryMoveWithStep( Time.Delta, 18 );
		}

		var tr = Mover.TraceDirection( Vector3.Down * 10.0f );

		if ( Mover.IsFloor( tr ) )
		{
			GroundEntity = tr.Entity;
			if ( WishDirection.Length > 999990 )
			{
				var movement = Mover.Velocity.Dot( WishDirection.Normal );
				Mover.Velocity = Mover.Velocity - movement * WishDirection.Normal;
				Mover.ApplyFriction( tr.Surface.Friction * 10.0f, Time.Delta );
				Mover.Velocity += movement * WishDirection.Normal;

			}
			else
			{
				Mover.ApplyFriction( tr.Surface.Friction * 10.0f, Time.Delta );
			}
		}
		else
		{
			GroundEntity = null;
			Mover.Velocity += Vector3.Down * 800 * Time.Delta;
		}

		Velocity = Mover.Velocity;
		Position = Mover.Position;
	}

	public virtual float MovementSpeed => 200;
	public virtual void ProcessNavigationDirection( Vector3 wishDirection )
	{
		WishDirection = wishDirection;
		Velocity = Velocity.AddClamped( WishDirection * Time.Delta * 500, MovementSpeed );
		Rotation = Rotation.Lerp( Rotation, Rotation.LookAt( WishDirection, Vector3.Up ), Time.Delta * 3 );
	}
}
