public class ExitCrystalFreezeAction : ExecuteAction
{
	public BattleFieldTransform bft;

	public ExitCrystalFreezeAction(BattleFieldTransform bft)
	{
		this.bft = bft;
	}

	protected override void Update()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (bft.crystalFreeze)
		{
			bft.crystalFreeze = false;
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_foe_crystal_deactivated);
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerExitCrystalFreeze(bft);
			});
		}
	}
}
