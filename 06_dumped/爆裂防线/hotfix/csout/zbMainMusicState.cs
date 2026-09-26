public class zbMainMusicState : zbMusicState
{
	public zbMainMusicState(EMusicState eState)
		: base(eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		zbMusicMgr.Instance.Play("audio/music/rm_home" + Rand.Range(1, 4) + ".ogg");
	}
}
