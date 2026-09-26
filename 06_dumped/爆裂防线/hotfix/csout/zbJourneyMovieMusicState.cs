public class zbJourneyMovieMusicState(EMusicState eState) : zbMusicState(eState)
{
	private float m_fScaleFactor = 0.5f;

	public override void OnEnter()
	{
		base.OnEnter();
		if (zbSelf.Instance.journeyOneHandler.IsActive())
		{
			zbMusicMgr.Instance.Play("audio/music/rm_fight1.ogg", m_fScaleFactor);
		}
		else if (zbSelf.Instance.journeyTwoHandler.IsActive())
		{
			zbMusicMgr.Instance.Play("audio/music/rm_fight2.ogg", m_fScaleFactor);
		}
		else
		{
			zbMusicMgr.Instance.Play("audio/music/rm_fight4.ogg", m_fScaleFactor);
		}
	}
}
