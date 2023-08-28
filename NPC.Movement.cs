using Sandbox;

namespace XeNPC2;

public partial class NPC : AnimatedEntity
{
	MoveHelper Mover;
	public Vector3 WishDirection;


	public float GroundBounce = 0;
	public float WallBounce = 0;
	public float MaxStandableAngle = 50;
	public float StepSize = 18;
	public float Gravity = 800;

	public void TryMove()
	{
		Velocity -= new Vector3( 0, 0, (Gravity * Scale) * 0.5f ) * Time.Delta;
		StepMove();
	}

	public virtual void StepMove()
	{
		Mover = new MoveHelper( Position, Velocity );
		var bbox = CollisionBounds;
		Mover.Trace = Mover.Trace.Size( bbox ).Ignore( this );
		Mover.MaxStandableAngle = MaxStandableAngle;

		Mover.TryMoveWithStep( Time.Delta, StepSize * Scale );

		Position = Mover.Position;
		Velocity = Mover.Velocity;
	}

	public virtual float MovementSpeed => 200;
	public virtual void ProcessNavigationDirection( Vector3 wishDirection )
	{
		WishDirection = wishDirection;
		Velocity = Velocity.AddClamped( WishDirection * MovementSpeed, MovementSpeed );
		Rotation = Rotation.Lerp( Rotation, Rotation.LookAt( WishDirection, Vector3.Up ), Time.Delta * 3 );
	}
}
