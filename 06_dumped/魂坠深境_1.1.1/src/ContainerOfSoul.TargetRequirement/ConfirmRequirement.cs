using System;
using System.Collections.Generic;

namespace ContainerOfSoul.TargetRequirement;

public class ConfirmRequirement : CastPrepare
{
	[LocalizeText("ConfirmRequirement施放准备描述")]
	public string hintText;

	public override void TriggerPrepare(GameEvent eve, Action onConfirm, Action onCancel = null)
	{
		if (IsEnemyCastPrepare(eve))
		{
			onConfirm?.Invoke();
			return;
		}
		string text = hintText;
		if (eve.GetEveObj(GameEvent.EveObjType.Caster) is IEffectObjectDisplay obj)
		{
			text = TextDataPreset.FormatEffectObjectDescription(obj, text, eve, out var _);
		}
		eve.gameState.UIManager.itemSpellConfirmPanel.Setup(onConfirm, onCancel, text);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			str = new NodeDisplayInfo.Element<string>(hintText)
		};
	}
}
