using Sandbox;

namespace XeNPC2;

public partial class NPC : AnimatedEntity
{
	public TagList LikeNPCTags = new();
	public TagList IgnoreNPCTags = new();
	public TagList DislikeNPCTags = new();
	public TagList HateNPCTags = new();
	/// <summary>
	/// Get our relationship with another NPC/Player 
	/// </summary>
	/// <param name="ent"></param>
	/// <returns>The relationship as an enum</returns>
	public Relationship GetRelationship( Entity ent )
	{
		if ( ent.Tags.HasAny( LikeNPCTags ) ) return Relationship.Like;
		if ( ent.Tags.HasAny( IgnoreNPCTags ) ) return Relationship.Like;
		if ( ent.Tags.HasAny( DislikeNPCTags ) ) return Relationship.Dislike;
		if ( ent.Tags.HasAny( HateNPCTags ) ) return Relationship.Hate;
		return Relationship.Neutral;
	}

}

public enum Relationship
{
	Like,
	Ignore,
	Neutral,
	Dislike,
	Hate,
}
