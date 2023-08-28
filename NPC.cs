using Sandbox;
using System.Linq;

namespace XeNPC2;

public partial class NPC : AnimatedEntity, ICombat
{
	public override void Spawn()
	{

		GameTask.RunInThreadAsync( ProcessQueue );
		base.Spawn();
	}

	[Event.Tick.Server]
	internal void NPCServerTick()
	{
		if ( LifeState == LifeState.Alive )
		{
			Think();
		}
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( LifeState != LifeState.Alive )
			return;

		// Check for headshot damage
		var isHeadshot = info.Hitbox.HasTag( "head" );
		if ( isHeadshot )
		{
			info.Damage *= 2.5f;
		}

		if ( Health > 0 && info.Damage > 0 )
		{
			Health -= info.Damage;

			if ( Health <= 0 )
			{
				Health = 0;
				OnKilled();
			}
		}

		this.ProceduralHitReaction( info, 0.05f );
	}
	public override void OnKilled()
	{
		if ( LifeState == LifeState.Alive )
		{
			//CreateRagdoll( Controller.Velocity, LastDamage.Position, LastDamage.Force,
			//LastDamage.BoneIndex, LastDamage.HasTag( "bullet" ), LastDamage.HasTag( "blast" ) );

			LifeState = LifeState.Dead;
			EnableAllCollisions = false;
			EnableDrawing = false;

			//Controller.Remove();
			//Animator.Remove();
			//Inventory.Remove();

			// Disable all children as well.
			Children.OfType<ModelEntity>()
				.ToList()
				.ForEach( x => x.EnableDrawing = false );

			//AsyncRespawn();
		}
	}
}
