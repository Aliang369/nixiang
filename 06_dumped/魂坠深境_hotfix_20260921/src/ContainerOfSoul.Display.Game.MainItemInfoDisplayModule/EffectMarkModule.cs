using System;
using Common.UI;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public class EffectMarkModule : MainItemInfoDisplayBase.DisplayModule
{
	public EffectMarkDisplay effectMarkDisplay;

	public HorizontalLayoutGroup effectMarkDisplayParent;

	[NonSerialized]
	private CellUIPrefabPool<EffectMarkDisplay, EffectMarkAbility.EffectMarkInfo> effectMarkDisplayPool;

	public override void Init()
	{
		effectMarkDisplayPool = base.mainItemInfoDisplay.GetPrefabPool<EffectMarkDisplay, EffectMarkAbility.EffectMarkInfo>(effectMarkDisplay, effectMarkDisplayParent.transform);
	}

	public override void UpdateDisplay()
	{
		if (UIDisplayModule.gameState == null)
		{
			return;
		}
		GameEvent.ResolveInfo resolveInfo = UIDisplayModule.gameState.gameEventManager.TriggerEffectMarkResolve(base.mainItemInfoDisplay.item);
		effectMarkDisplayPool.DespawnAll();
		foreach (EffectMarkAbility.EffectMarkInfo effectMarkInfo in resolveInfo.effectMarkInfos)
		{
			if (effectMarkInfo.showType.HasFlag(EffectMarkAbility.ShowType.Mark))
			{
				effectMarkDisplayPool.Spawn(effectMarkInfo);
			}
		}
	}
}
