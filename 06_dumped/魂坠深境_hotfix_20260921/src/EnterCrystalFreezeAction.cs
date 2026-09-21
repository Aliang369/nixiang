using System.Linq;

public class EnterCrystalFreezeAction : ExecuteAction
{
	public BattleFieldTransform bft;

	public EnterCrystalFreezeAction(BattleFieldTransform bft)
	{
		this.bft = bft;
	}

	protected override void Update()
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		if (bft.crystalFreeze)
		{
			return;
		}
		foreach (Item item in from it in bft.tableContainer.GetAllItems()
			where !it.blackboard.StayInTable
			select it)
		{
			EnqueueAction(new ConsumeItemAction(item, ConsumeItemAction.ConsumeType.Deathblow));
		}
		foreach (Item phase2Item in bft.characterAI.phase2Items)
		{
			EnqueueAction(new SpawnItemAction(phase2Item, bft.drawPileItemContainer, withAnim: true));
		}
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerEnemyEnterPhase2AfterConsumeItem(bft);
		});
		bft.crystalFreeze = true;
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_foe_crystal_activated);
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerEnterCrystalFreeze(bft);
		});
	}
}
