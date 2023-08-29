using Sandbox;
using System.Drawing;

namespace XeNPC2;

public partial class NPC : AnimatedEntity
{
	MoveHelper Mover;
	public Vector3 WishVelocity;
	public float WishSpeed;

	[Net] public float Acceleration { get; set; } = 10.0f;
	[Net] public float MoveFriction { get; set; } = 1.0f;
	[Net] public float GroundFriction { get; set; } = 4.0f;
	[Net] public float StopSpeed { get; set; } = 100.0f;
	[Net] public float GroundAngle { get; set; } = 46.0f;

	public float GroundBounce = 0;
	public float WallBounce = 0; 
	public float StepSize = 18;
	public float Gravity = 800;

	public void TryMove()
	{ 
		if ( GroundEntity != null )
		{
			ApplyFriction( GroundFriction * SurfaceFriction );
		}

		Velocity -= new Vector3( 0, 0, (Gravity * Scale) * 0.5f ) * Time.Delta;

		if (GroundEntity != null ) Velocity = Velocity.WithZ( 0 );
		Accelerate( WishVelocity.Normal, WishVelocity.Length, 0, Acceleration );
		if (GroundEntity != null ) Velocity = Velocity.WithZ( 0 );

		StepMove(); 
		CategorizePosition();

		bool Debug = false;
		if ( Debug )
		{
			DebugOverlay.Box( Position + TraceOffset, CollisionBounds.Mins, CollisionBounds.Maxs, Color.Red );
			DebugOverlay.Box( Position, CollisionBounds.Mins, CollisionBounds.Maxs, Color.Blue );

			var pos = Position;
			if ( Game.IsServer ) pos = Position + (Vector3.Up * 30);

			DebugOverlay.Text( $"        Position: {Position}", pos + (Vector3.Up * 0) );
			DebugOverlay.Text( $"        Velocity: {Velocity}", pos + (Vector3.Up * 5) );
			DebugOverlay.Text( $"    BaseVelocity: {BaseVelocity}", pos + (Vector3.Up * 10));
			DebugOverlay.Text( $"    GroundEntity: {GroundEntity} [{GroundEntity?.Velocity}]", pos + (Vector3.Up * 15) );
			DebugOverlay.Text( $" SurfaceFriction: {SurfaceFriction}", pos + (Vector3.Up * 20) );
			DebugOverlay.Text( $"    WishVelocity: {WishVelocity}", pos + (Vector3.Up * 25) );
			DebugOverlay.Text( $"    WishSpeed: {WishSpeed}", pos + (Vector3.Up * 30) );
			DebugOverlay.Text( $"    Speed: {Velocity.Length}", pos + (Vector3.Up * 35) );
		}
		WishVelocity = Vector3.Zero;
	}

	public virtual void StepMove()
	{
		Mover = new MoveHelper( Position, Velocity );
		var bbox = CollisionBounds;
		Mover.Trace = Mover.Trace.Size( bbox ).Ignore( this );
		Mover.MaxStandableAngle = GroundAngle;

		Mover.TryMoveWithStep( Time.Delta, StepSize * Scale );

		Position = Mover.Position;
		Velocity = Mover.Velocity;
	}
	public void CategorizePosition()
	{
		SurfaceFriction = 1.0f;
		var point = Position - Vector3.Up * (2 * Scale);
		var vBumpOrigin = Position;
		var pm = TraceBBox( vBumpOrigin, point, 4.0f );

		bool bMoveToEndPos = false;

		if ( GroundEntity != null ) // and not underwater
		{
			bMoveToEndPos = true;
			point.z -= StepSize * Scale;
		}

		if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
		{
			ClearGroundEntity();
			bMoveToEndPos = false;

			if ( Velocity.z > 0 )
				SurfaceFriction = 0.25f;
		}
		else
		{
			UpdateGroundEntity( pm );
		}
		if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
		{
			Position = pm.EndPosition;
		}
	}

	public Vector3 GroundNormal { get; set; }
	/// <summary>
	/// We have a new ground entity
	/// </summary>
	public virtual void UpdateGroundEntity( TraceResult tr )
	{
		GroundNormal = tr.Normal;

		// VALVE HACKHACK: Scale this to fudge the relationship between vphysics friction values and player friction values.
		// A value of 0.8f feels pretty normal for vphysics, whereas 1.0f is normal for players.
		// This scaling trivially makes them equivalent.  REVISIT if this affects low friction surfaces too much.
		SurfaceFriction = tr.Surface.Friction * 1.25f;
		if ( SurfaceFriction > 1 ) SurfaceFriction = 1;

		//if ( tr.Entity == GroundEntity ) return;

		Vector3 oldGroundVelocity = default;
		if ( GroundEntity != null ) oldGroundVelocity = GroundEntity.Velocity;

		bool wasOffGround = GroundEntity == null;

		GroundEntity = tr.Entity;

		if ( GroundEntity != null )
		{
			BaseVelocity = GroundEntity.Velocity;
		}
	}

	/// <summary>
	/// We're no longer on the ground, remove it
	/// </summary>
	public virtual void ClearGroundEntity()
	{
		if ( GroundEntity == null ) return;

		GroundEntity = null;
		GroundNormal = Vector3.Up;
		SurfaceFriction = 1.0f;
	}

	/// <summary>
	/// Any bbox traces we do will be offset by this amount. 
	/// </summary>
	public Vector3 TraceOffset;

	/// <summary>
	/// Traces the current bbox and returns the result.
	/// liftFeet will move the start position up by this amount, while keeping the top of the bbox at the same
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, CollisionBounds.Mins, CollisionBounds.Maxs, liftFeet );
	}

	/// <summary>
	/// Traces the bbox and returns the trace result.
	/// LiftFeet will move the start position up by this amount, while keeping the top of the bbox at the same 
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			liftFeet *= Scale;
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets", "player" )
					.Ignore( this )
					.Run();

		tr.EndPosition -= TraceOffset;
		return tr;
	}

	protected float SurfaceFriction;
	/// <summary>
	/// Add our wish direction and speed onto our velocity
	/// </summary>
	public virtual void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
	{
		// This gets overridden because some games (CSPort) want to allow dead (observer) players
		// to be able to move around.
		// if ( !CanAccelerate() )
		//     return; 
		speedLimit *= Scale;
		acceleration /= Scale;
		if ( speedLimit > 0 && wishspeed > speedLimit )
			wishspeed = speedLimit;

		// See if we are changing direction a bit
		var currentspeed = Velocity.Dot( wishdir );

		// Reduce wishspeed by the amount of veer.
		var addspeed = wishspeed - currentspeed;

		// If not going to add any speed, done.
		if ( addspeed <= 0 )
			return;

		// Determine amount of acceleration.
		var accelspeed = (acceleration * Scale) * Time.Delta * wishspeed * SurfaceFriction;

		// Cap at addspeed
		if ( accelspeed > addspeed )
			accelspeed = addspeed;

		Velocity += wishdir * accelspeed;
	}

	/// <summary>
	/// Remove ground friction from velocity
	/// </summary>
	public virtual void ApplyFriction( float frictionAmount = 1.0f )
	{
		// If we are in water jump cycle, don't apply friction
		//if ( player->m_flWaterJumpTime )
		//   return; 
		// Not on ground - no friction   
		// Calculate speed 
		var speed = Velocity.Length;
		if ( speed < 0.1f ) return;

		// Bleed off some speed, but if we have less than the bleed
		//  threshold, bleed the threshold amount.
		float control = (speed < StopSpeed * Scale) ? (StopSpeed * Scale) : speed;

		// Add the amount to the drop amount.
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity
		float newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;

		if ( newspeed != speed )
		{
			newspeed /= speed;
			Velocity *= newspeed;
		}

		// mv->m_outWishVel -= (1.f-newspeed) * mv->m_vecVelocity;
	}

	public virtual float MovementSpeed => 200;
	public virtual void ProcessNavigationDirection( Vector3 wishDirection )
	{
		WishVelocity = wishDirection * (WishSpeed * wishDirection.Length); 
		Rotation = Rotation.Lerp( Rotation, Rotation.LookAt( WishVelocity, Vector3.Up ), Time.Delta * 3 );
	}
}
