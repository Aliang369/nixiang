using System;
using System.Collections.Generic;

namespace ContainerOfSoul.TargetRequirement;

public abstract class CastPrepare : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public abstract List<IEffectNode> GetChildren();

	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo();
	}

	public abstract void TriggerPrepare(GameEvent eve, Action onConfirm, Action onCancel = null);

	protected static BattleFieldTransform GetCasterOwner(GameEvent eve)
	{
		return eve.CasterOwner;
	}

	protected bool IsEnemyCastPrepare(GameEvent eve)
	{
		return GetCasterOwner(eve)?.isEnemy ?? false;
	}
}
