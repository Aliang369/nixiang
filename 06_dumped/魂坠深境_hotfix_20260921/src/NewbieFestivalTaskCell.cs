using System;
using System.Linq;
using ContainerOfSoul.Display.Fortress;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewbieFestivalTaskCell : CellUI<RoutineQuest>
{
	[SerializeField]
	private Image background;

	[SerializeField]
	private Image buttonBackground;

	[SerializeField]
	private GameButton button;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private TextMeshProUGUI progressText;

	[SerializeField]
	private TextMeshProUGUI buttonText;

	private static readonly Color claimableBackgroundColor = new Color32(253, 244, 223, byte.MaxValue);

	private static readonly Color claimableButtonColor = new Color32(244, 224, 175, byte.MaxValue);

	private static readonly Color finishedBackgroundColor = new Color32(242, 242, 242, byte.MaxValue);

	private static readonly Color finishedButtonColor = new Color32(216, 216, 216, byte.MaxValue);

	private RoutineQuest quest => base.setupData;

	private NewbieFestivalManager newbieFestivalManager => CellUI<RoutineQuest>.applicationState.newbieFestivalManager;

	protected override void Init()
	{
		button.onClick.AddListener(_OnButtonClicked);
	}

	protected override void OnSetup(RoutineQuest t)
	{
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (quest != null)
		{
			descriptionText.text = quest.GetDescription(CellUI<RoutineQuest>.applicationState);
			int num = Mathf.Clamp(quest.progress / quest.def.maxProgress, quest.gotRewardLayer, quest.def.maxLayer);
			progressText.text = $"{num}/{quest.def.maxLayer}";
			button.interactable = false;
			buttonBackground.color = Color.white;
			background.color = Color.white;
			buttonText.text = string.Empty;
			if (quest.AllLayerFinished)
			{
				background.color = finishedBackgroundColor;
				buttonBackground.color = finishedButtonColor;
				buttonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_finished;
			}
			else if (quest.GetRewardAble)
			{
				background.color = claimableBackgroundColor;
				buttonBackground.color = claimableButtonColor;
				button.interactable = true;
				buttonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_newbie_festival_claim;
			}
			else if (quest.JumpPanelType != RoutineQuestDef.RoutineQuestNode.JumpPanelType.None)
			{
				button.interactable = true;
				buttonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_newbie_festival_go;
			}
			else
			{
				buttonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_not_finished;
			}
		}
	}

	private void _OnButtonClicked()
	{
		if (quest == null)
		{
			return;
		}
		if (quest.GetRewardAble)
		{
			newbieFestivalManager.ClaimTaskReward(quest, delegate(GlobalRewardPackage package)
			{
				CellUI<RoutineQuest>.fortressState.StartCoroutine(() => CellUI<RoutineQuest>.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(package));
				manager.SetDirty();
			});
		}
		else if (!quest.AllLayerFinished && quest.JumpPanelType != RoutineQuestDef.RoutineQuestNode.JumpPanelType.None)
		{
			JumpToPanel();
		}
	}

	private void JumpToPanel()
	{
		GetUIInParents<NewbieFestivalPanel>()?.HideUI();
		switch (quest.def.node.JumpToPanelType)
		{
		case RoutineQuestDef.RoutineQuestNode.JumpPanelType.None:
			break;
		case RoutineQuestDef.RoutineQuestNode.JumpPanelType.StartGame:
			CellUI<RoutineQuest>.fortressState.uiManager.ShowUI(CellUI<RoutineQuest>.fortressState.uiManager.baseGameUIPanel);
			break;
		case RoutineQuestDef.RoutineQuestNode.JumpPanelType.SkillUpgrade:
			CellUI<RoutineQuest>.fortressState.uiManager.ShowUI(CellUI<RoutineQuest>.fortressState.uiManager.skill_GemPanel);
			break;
		case RoutineQuestDef.RoutineQuestNode.JumpPanelType.StarMap:
			CellUI<RoutineQuest>.fortressState.uiManager.mainUIPanel.mainUISwitchGroup.mainPanelLayerSetups.FirstOrDefault((FortressMainUIPanel.MainPanelLayerSetup l) => l.layer == FortressMainUIPanel.MainPanelLayer.StargazingRoom)?.button.onClick?.Invoke();
			break;
		case RoutineQuestDef.RoutineQuestNode.JumpPanelType.GachaDraw:
			CellUI<RoutineQuest>.fortressState.uiManager.gachaPanel.ShowDefaultEntryPool();
			break;
		case RoutineQuestDef.RoutineQuestNode.JumpPanelType.CombineGem:
			CellUI<RoutineQuest>.fortressState.uiManager.skill_GemPanel.gemCombinePanel.ShowUI();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
