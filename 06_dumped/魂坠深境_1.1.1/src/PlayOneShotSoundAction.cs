using FMODUnity;

public class PlayOneShotSoundAction : ExecuteAction
{
	private readonly EventReference eventKey;

	public PlayOneShotSoundAction(EventReference eventKey)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		this.eventKey = eventKey;
	}

	protected override void Update()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Bootstrap.instance.audioManager.PlayOneShot(eventKey);
		FinishAction();
	}
}
