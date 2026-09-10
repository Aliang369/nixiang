using System;
using UnityEngine;

public class GetCrystalUpgrade : CrystalUpgrade
{
	private readonly int count;

	public override Sprite Icon => ScriptableObjectSingleton<GameDataManager>.instance.crystalBonusIcon;

	public override string Title => ScriptableObjectSingleton<TextDataPreset>.instance.term_mercy_get_crystal;

	protected override int BasePrice => 0;

	public GetCrystalUpgrade(int count)
	{
		this.count = count;
	}

	public override QualityLevel GetQuality()
	{
		return QualityLevel.Common;
	}

	public override string GetDescription(GameStateInfo gameState, out TextDataGenerator generator)
	{
		generator = null;
		string term_mercy_get_crystal_arg0_now = ScriptableObjectSingleton<TextDataPreset>.instance.term_mercy_get_crystal_arg0_now;
		object[] array = new object[1];
		int num = count;
		array[0] = num.ToString().ColorText(ScriptableObjectSingleton<VFXDataPreset>.instance.crystalColor);
		return term_mercy_get_crystal_arg0_now.FormatWith(array);
	}

	public override void Trigger(GameStateInfo gameState, Action successFallback)
	{
		gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			crystal = count
		}));
		successFallback?.Invoke();
	}
}
