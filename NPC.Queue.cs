

using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XeNPC2;
public partial class NPC
{
	[SkipHotload]
	public Queue<NPCTask> NPCTaskQueue = new Queue<NPCTask>();
	[Event.Hotload]
	void HotloadFix()
	{
		NPCTaskQueue = new Queue<NPCTask>();
	}

	async Task ProcessQueue()
	{
		while ( true )
		{
			if ( NPCTaskQueue != null && NPCTaskQueue.Count != 0 )
			{

				NPCTask CurrentTask = NPCTaskQueue.Dequeue();
				await CurrentTask.HandleTask( this );
				//Log.Info( "Task Finished!" );
			}
			await GameTask.NextPhysicsFrame();
		}
	}
}
public class NPCTask
{
	public delegate void OnEndCallBack();
	public OnEndCallBack OnTaskEnd;
	public Entity Sequence;
	public bool DidFinish = false;
	public virtual async Task HandleTask( NPC owner )
	{
		return;
	}
	public virtual void OnEnd()
	{
		DidFinish = true;
		if (OnTaskEnd != null)
		{
			OnTaskEnd();
		}
		if ( Sequence != null )
		{
			//Sequence.EndSequence();
		}
	}
}
public class MoveToTask : NPCTask
{

	Vector3 Position;
	bool Running;
	public MoveToTask( Vector3 pos, bool run = false, OnEndCallBack end = null )
	{
		Position = pos;
		Running = run;
		OnTaskEnd = end;
	}
	public override async Task HandleTask( NPC owner )
	{
		owner.Navigation.Output.Finished = false;
		owner.Navigation.Target = Position;
		while ( !owner.Navigation.Output.Finished )
		{
			await GameTask.NextPhysicsFrame();
		}
		OnEnd();
		return;
	}
}
public class RotateToTask : NPCTask
{
	Rotation Rotation;
	public RotateToTask( Rotation rot, OnEndCallBack end = null )
	{
		Rotation = rot;
		OnTaskEnd = end;
	}
	public override async Task HandleTask( NPC owner )
	{
		while ( !owner.Rotation.Forward.AlmostEqual(Rotation.Forward, 0.2f) )
		{
			owner.TargetRotation = Rotation;
			await GameTask.NextPhysicsFrame();
		}
		OnEnd();
		return;
	}
}
public class PlayAnimTask : NPCTask
{
	string Animation;
	public PlayAnimTask( string anim, OnEndCallBack end = null )
	{
		Animation = anim;
		OnTaskEnd = end;
	}
	public override async Task HandleTask( NPC owner )
	{
		owner.DirectPlayback.Play( Animation );
		while ( owner.DirectPlayback.Time < owner.DirectPlayback.Duration )
		{
			await GameTask.NextPhysicsFrame();
		}
		owner.DirectPlayback.Cancel();
		OnEnd();
		return;
	}
}
