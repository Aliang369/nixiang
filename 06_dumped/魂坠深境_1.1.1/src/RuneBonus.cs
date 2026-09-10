using System;
using System.Collections.Generic;
using UnityEngine;

public class RuneBonus : BattleBonusObject
{
	[SaveField(null, null)]
	public Rune rune;

	public override object infoObject => rune;

	public override Sprite PreviewIcon => rune.def.Icon;

	public override string previewText => string.Format(ScriptableObjectSingleton<TextDataPreset>.instance.term_attach_arg0_rune, rune.def.Name);

	private RuneBonus()
	{
	}

	public RuneBonus(Rune rune)
	{
		this.rune = rune;
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		MintageRunePanel_Select.SetupInfo setupInfo = new MintageRunePanel_Select.SetupInfo(new List<Rune> { rune });
		setupInfo.canBack = (Func<bool>)Delegate.Combine(setupInfo.canBack, (Func<bool>)(() => true));
		setupInfo.onConfirm = (Action<(List<Rune>, Item)>)Delegate.Combine(setupInfo.onConfirm, (Action<(List<Rune>, Item)>)delegate
		{
			ConfirmGotBonus();
		});
		setupInfo.onCancel = (Action)Delegate.Combine(setupInfo.onCancel, new Action(base.ConfirmGotBonus));
		setupInfo.onUnavailable = (Action)Delegate.Combine(setupInfo.onUnavailable, new Action(base.ConfirmBonusUnavailable));
		setupInfo.runeSlotLimit = true;
		gameState.UIManager.mintageRunePanel_Select.Setup(setupInfo);
	}
}
