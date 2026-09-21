using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContainerOfSoul.TargetRequirement;

public class SelectSkillsCount : CastPrepare
{
	[SerializeField]
	private ValueGenerator countGenerator;

	[SerializeField]
	private bool isSingleSelect;

	[SerializeField]
	[LocalizeText("SelectSkillsCount施放准备描述")]
	public string panelHint;

	[SerializeField]
	private SkillSelector pool;

	[SerializeField]
	private PlayerSelectItemsEffect.SelectPanelType selectPanelType;

	[SerializeField]
	private ValueCompareCheckNode.CompareType type;

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { countGenerator, pool };
	}

	public override void TriggerPrepare(GameEvent eve, Action onConfirm, Action onCancel = null)
	{
		ElementSelectPanel<GameSkill>.SelectInfo info = SelectPanelHelper.SelectSkillCount(eve, pool, countGenerator, type, onConfirm, onCancel, CastPrepare.GetCasterOwner(eve), () => true, panelHint);
		eve.gameState.UIManager.skillSelectPanel.Setup(info);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			str = new NodeDisplayInfo.Element<string>(panelHint)
		};
	}
}
