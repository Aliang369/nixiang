public class TransitReloadRoomAction : ExecuteAction
{
	private readonly EncounterSetupInfo setupInfo;

	private readonly bool withTransitAnim;

	public TransitReloadRoomAction(EncounterSetupInfo info, bool withTransitAnim = true)
	{
		setupInfo = info;
		this.withTransitAnim = withTransitAnim;
	}

	protected override void Update()
	{
		if (withTransitAnim)
		{
			EnqueueAction(new ExitRoomTransitAnimAction());
		}
		EnqueueAction(new SetupEncounterAction(setupInfo));
		if (withTransitAnim)
		{
			EnqueueAction(new EnterRoomTransitAnimAction());
		}
	}
}
