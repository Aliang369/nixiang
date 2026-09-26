public class zbDungeonMusicState : zbMusicState
{
	public zbDungeonMusicState(EMusicState eState)
		: base(eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		zbMusicMgr.Instance.Play("audio/music/rm_dungeon" + Rand.Range(1, 4) + ".ogg");
	}
}
