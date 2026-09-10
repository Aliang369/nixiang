using System.Collections.Generic;

public class PainToLifeAbility : Ability
{
	private const string PainToLifeInfluenceId = "eaba925d4911e0866801b95480644283";

	public const string MaxHealthMigrationKey = "PainToLife.MaxHealth50.V2";

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (eve?.CasterInfluence?.OwnerBft == null || eve.CasterInfluence.def?.id != "eaba925d4911e0866801b95480644283")
		{
			return false;
		}
		BattleFieldTransform ownerBft = eve.CasterInfluence.OwnerBft;
		if (eve.type == GameEvent.Type.AddInfluence)
		{
			return eve.TargetInfluence == eve.CasterInfluence;
		}
		DamageCommand damagecommand = eve.damagecommand;
		if (eve.type == GameEvent.Type.GenerateDamagePierceResolve && eve.resolveInfo != null && eve.damageResolveType == Damage.ResolveType.Pierce && damagecommand?.targetBFT == ownerBft)
		{
			return (damagecommand.source as IBftOwnable)?.OwnerBft == ownerBft;
		}
		return false;
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AddInfluence;
		yield return GameEvent.Type.GenerateDamagePierceResolve;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		BattleFieldTransform battleFieldTransform = eve?.CasterInfluence?.OwnerBft;
		if (eve.type == GameEvent.Type.GenerateDamagePierceResolve)
		{
			eve.resolveInfo.pierce = true;
			return;
		}
		Influence casterInfluence;
		Influence influence = (casterInfluence = eve.CasterInfluence);
		if (casterInfluence.tempValues == null)
		{
			casterInfluence.tempValues = new Dictionary<string, object>();
		}
		int num = (int)battleFieldTransform.maxHealth / 2;
		if (num > 0)
		{
			eve.gameState.actionManager.EnqueueAction(new ChangeBftMaxHealthAction(battleFieldTransform, num, baseAdd: true, hideNfx: true));
		}
		influence.tempValues["PainToLife.MaxHealth50.V2"] = true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
