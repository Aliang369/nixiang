public abstract class GameJournalRecord
{
	[SaveField(null, null)]
	public long time;

	public virtual string NetworkSaveArg0 { get; set; }

	public virtual string NetworkSaveArg1 { get; set; }

	public abstract bool IsVisibleToPlayer(ApplicationStateInfo applicationState);

	public abstract string GenerateDescription(ApplicationStateInfo applicationState);

	public virtual void OnTrack(InGameRecorderManager manager)
	{
		time = manager.gameState.gameTimer.GetTime();
	}

	public string GenerateRecordLine(ApplicationStateInfo applicationState)
	{
		return TextDisplayHelper.GetDetailTimeSpanText(time) + " " + GenerateDescription(applicationState);
	}
}
