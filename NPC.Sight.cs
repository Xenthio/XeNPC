using Sandbox;
using System;
using System.Linq;

namespace XeNPC2;

public partial class NPC : AnimatedEntity
{
	public virtual float SightDistance => 500.0f;
	public virtual float SightFieldOfView => 0.5f;
	public virtual float SightTimeBetween => 0.1f;

	/// <summary>
	/// green = visible
	/// red = not visible but still in viewcone
	/// yellow = cached trace, neither entities have moved so assume it's the same
	/// </summary>
	[ConVar.Replicated] public static bool npc_debug_los { get; set; } = false;
	[ConVar.Replicated] public static bool npc_disable_visability { get; set; } = false;
	[ConVar.Replicated] public static bool npc_cache_vis { get; set; } = false;
	[ConVar.Replicated] public static bool npc_scale_vis { get; set; } = false;


	TimeSince TimeSinceLastTrySeen;
	public virtual void TrySee()
	{
		if ( TimeSinceLastTrySeen < SightTimeBetween ) return;
		TimeSinceLastTrySeen = 0;
		Vector3 delta = new Vector3( SightDistance, SightDistance, SightDistance );
		var allents = Entity.FindInBox( new BBox( Position - delta, Position + delta ) )
				.OfType<ICombat>()
				.OrderBy( o => ((o as Entity).Position.Distance( Position )) );

		var count = 100;

		if ( npc_scale_vis ) count = (0.5f / Time.Delta).CeilToInt(); // if we want we can scale the amount to go through depending on fps. 
		foreach ( Entity ent in allents.Take( count ) ) // iterate through the amount specfied
		{
			if ( ent == this ) continue;
			if ( ent.Position.Distance( Position ) > SightDistance ) continue; // Ignore everything further away than entDist
			if ( !InViewCone( ent ) ) continue; // Ignore anything not in our view cone.
			if ( !EntityShouldBeSeen( ent ) ) continue; // Ignore anything that doesn't want to be seen. 
			var relationship = GetRelationship( ent );
			if ( relationship == Relationship.Ignore ) continue; // Ignore anything that we don't care about.
			if ( ent.Health <= 0 ) continue; // Ignore anything dead.
			bool hasdrawn = false;
			var a = Trace.Ray( AimRay.Position, ent.AimRay.Position )
				.WithoutTags( "monster", "npc", "player" ).Ignore( this );

			TraceResult b;
			b = a.Run();


			if ( b.Fraction != 1 )
			{
				if ( npc_debug_los && !hasdrawn ) DebugOverlay.Line( b.StartPosition, ent.AimRay.Position, Color.Red, SightTimeBetween + Time.Delta, false );
				continue;
			}
			if ( npc_debug_los && !hasdrawn ) DebugOverlay.Line( b.StartPosition, ent.AimRay.Position, Color.Green, SightTimeBetween + Time.Delta, false );
			ProcessEntity( ent, relationship );
		}
	}

	/// <summary>
	/// Called for every NPC/Player we can see
	/// </summary>
	/// <param name="ent">The Entity we have seen</param>
	/// <param name="relationship">Our relationship with said entity, refer to HLCombat.cs (TODO MOVE OUT OF HLS2 SPECIFIC CODE)</param>
	public virtual void ProcessEntity( Entity ent, Relationship relationship )
	{
	}

	public bool EntityShouldBeSeen( Entity ent )
	{
		return true;
	}

	[ConVar.Replicated] public static bool npc_draw_cone { get; set; } = false;
	/// <summary>
	/// Check if an Entity is in our view cone, This does NOT check if they're behind walls.
	/// </summary>
	/// <param name="ent">The Entity to check</param>
	/// <returns></returns>
	public bool InViewCone( Entity ent )
	{
		Vector2 vec2LOS;
		float flDot;


		var e = (ent.WorldSpaceBounds.Center - (AimRay.Position));
		vec2LOS = new Vector2( e.x, e.y );
		vec2LOS = vec2LOS.Normal;

		flDot = (float)Vector2.Dot( vec2LOS, new Vector2( Rotation.Forward.x, Rotation.Forward.y ) );

		if ( npc_draw_cone ) DrawViewCone();

		if ( flDot > SightFieldOfView )
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	Color MyDebugColour = Color.Black;
	public void DrawViewCone()
	{
		if ( MyDebugColour == Color.Black )
		{
			MyDebugColour = Color.Random;
		}
		Vector3[] rots = {
				new Vector3( 0, 0, 1 ),
				new Vector3( 0, 1, 0 ),
			};
		Vector3 LastConePos1 = (((AimRay.Position) + Rotation.RotateAroundAxis( rots.Last(), (MathF.Acos( SightFieldOfView ) * (180 / MathF.PI)) ).Backward * 1000));
		Vector3 LastConePos2 = (((AimRay.Position) + Rotation.RotateAroundAxis( rots.Last(), (MathF.Acos( -SightFieldOfView ) * (180 / MathF.PI)) ).Backward * -1000));
		foreach ( Vector3 rot in rots )
		{
			Vector3 Pos1 = (((AimRay.Position) + Rotation.RotateAroundAxis( rot, (MathF.Acos( SightFieldOfView ) * (180 / MathF.PI)) ).Backward * -1000));
			Vector3 Pos2 = (((AimRay.Position) + Rotation.RotateAroundAxis( rot * -1, (MathF.Acos( SightFieldOfView ) * (180 / MathF.PI)) ).Backward * -1000));
			DebugOverlay.Line( (AimRay.Position), Pos1, MyDebugColour, SightTimeBetween + Time.Delta );
			DebugOverlay.Line( (AimRay.Position), Pos2, MyDebugColour, SightTimeBetween + Time.Delta );

			DebugOverlay.Line( Pos1, LastConePos1, MyDebugColour, SightTimeBetween + Time.Delta );
			DebugOverlay.Line( Pos2, LastConePos2, MyDebugColour, SightTimeBetween + Time.Delta );
			DebugOverlay.Line( Pos1, LastConePos2, MyDebugColour, SightTimeBetween + Time.Delta );
			DebugOverlay.Line( Pos2, LastConePos1, MyDebugColour, SightTimeBetween + Time.Delta );
			LastConePos1 = Pos1;
			LastConePos2 = Pos2;
		}
	}

}
