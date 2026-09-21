public class GatherMultiElementsAction : ExecuteAction
{
	private readonly MultiInfoDisplayPanel.SetupInfo setupInfo;

	public GatherMultiElementsAction(MultiInfoDisplayPanel.SetupInfo setupInfo)
	{
		this.setupInfo = setupInfo;
	}

	protected override void Update()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		foreach (var (rune, target, delta) in setupInfo.runes.OrEmptyIfNull())
		{
			EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_runed));
			EnqueueAction(new ChangeRuneAction(target, rune.def, delta, null));
		}
		foreach (Item item in setupInfo.items.OrEmptyIfNull())
		{
			EnqueueAction(new SpawnItemAction(item, gameState.playerState.DeckContainer));
		}
		foreach (Influence item2 in setupInfo.influences.OrEmptyIfNull())
		{
			EnqueueAction(new AddInfluenceAction(item2, gameState.playerState.bft));
		}
		foreach (GameSkill item3 in setupInfo.skills.OrEmptyIfNull())
		{
			EnqueueAction(new ChangeSkillAction(item3, ChangeSkillAction.Type.AddOutSlot));
		}
		if (setupInfo.crystal != 0)
		{
			gameState.actionManager.EnqueueAction(new ChangeCrystalAction(setupInfo.crystal));
		}
		if (setupInfo.coin != 0)
		{
			gameState.actionManager.EnqueueAction(new ChangeCoinAction(setupInfo.coin));
		}
		EnqueueAction(new ShowMultiInfoAction(setupInfo, halt: true));
	}
}
