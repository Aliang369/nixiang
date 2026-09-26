public class zbLoginMusicState : zbMusicState
{
	public zbLoginMusicState(EMusicState eState)
		: base(eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		zbMusicMgr.Instance.Play("audio/music/rm_fight1.ogg");
	}
}
