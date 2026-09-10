using System.Collections.Generic;

public sealed class EvilContaminatedDaggerAfterConsumeAbility : Ability
{
	private const string ProcessedEventKey = "EvilContaminatedDaggerAfterConsumeAbility.Processed";

	public ItemSkinDef evilSkin;

	public EnchantDef comboEnchant;

	public int comboValue = 2;

	public int healthLoss = 6;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AfterConsume;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (eve != null && eve.type == GameEvent.Type.AfterConsume && eve.TargetItem != null)
		{
			return eve.TargetItem.skin == evilSkin;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve) && !eve.nodeTempValues.ContainsKey("EvilContaminatedDaggerAfterConsumeAbility.Processed"))
		{
			eve.nodeTempValues["EvilContaminatedDaggerAfterConsumeAbility.Processed"] = true;
			Item targetItem = eve.TargetItem;
			if (comboEnchant != null)
			{
				eve.gameState.actionManager.EnqueueAction(new ChangeEnchantAction(targetItem, targetItem, comboEnchant, comboValue, popup: true, eve.spellInfo));
			}
			BattleFieldTransform ownerBft = targetItem.OwnerBft;
			if (ownerBft != null)
			{
				eve.gameState.actionManager.EnqueueAction(new ChangeBftHealthAction(ownerBft, -healthLoss, targetItem, null, realHealthOnly: true));
			}
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(healthLoss),
			refEnchantDefs = ((comboEnchant == null) ? null : new NodeDisplayInfo.Element<List<EnchantDef>>(new List<EnchantDef> { comboEnchant })),
			refEnchants = ((comboEnchant == null) ? null : new NodeDisplayInfo.Element<List<Enchant>>(new List<Enchant>
			{
				new Enchant(comboEnchant, comboValue)
			}))
		};
	}
}
