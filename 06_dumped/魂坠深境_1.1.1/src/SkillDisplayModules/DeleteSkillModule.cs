using System;
using Common.UI;
using UnityEngine;

namespace SkillDisplayModules;

public class DeleteSkillModule : SkillDisplay.DisplayModule
{
	public MatterCell matterCellPrefab;

	public RectTransform matterCellParent;

	public GameObject deleteAbleDarken;

	private DeleteSkillPanel parentPanel;

	private CellUIPrefabPool<MatterCell, MatterInfo> deleteGainMatterCellPool;

	public bool Deleteable => base.profileSkill.IsDeleteAble;

	public override void Init()
	{
		InputModule module = base.display.GetModule<InputModule>();
		module.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(module.onHandleInput, new Action<InputModuleBase>(_OnClicked));
		deleteGainMatterCellPool = base.display.GetPrefabPool<MatterCell, MatterInfo>(matterCellPrefab, matterCellParent);
		parentPanel = base.display.GetUIInParents<DeleteSkillPanel>();
	}

	public override void UpdateDisplay()
	{
		if (base.profileSkill != null)
		{
			deleteGainMatterCellPool.DespawnAll();
			deleteGainMatterCellPool.Spawn(base.profileSkill.GetDeleteSkillGainMatter());
			deleteAbleDarken.SetActive(!Deleteable);
		}
	}

	private void _OnClicked(InputModuleBase input)
	{
		if (parentPanel == null || !input.pointerClicked)
		{
			return;
		}
		if (base.display.transform.IsChildOf(parentPanel.storageSkillCellParent))
		{
			if (!Deleteable)
			{
				if (base.profileSkill.isFavorite)
				{
					UIDisplayModule.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_mark_favorite_skill_cant_delete);
				}
			}
			else
			{
				parentPanel._OnStorageSkillClicked(base.profileSkill);
			}
		}
		else if (base.display.transform.IsChildOf(parentPanel.deleteSkillCellParent))
		{
			parentPanel._OnDeleteSkillClicked(base.profileSkill);
		}
	}
}
