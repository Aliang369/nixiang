using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTrialPanel : CellUI<IdolState>
{
	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private GameButton toGachaButton;

	[SerializeField]
	private ScrollRectExtend scrollRectExtend;

	[SerializeField]
	private CellUIPrefabPoolConfig<CharacterTrialCell, PlayerCharacterState> pool;

	private IdolState idolState => base.setupData;

	protected override void Init()
	{
		Init(pool);
		toGachaButton.onClick.AddListener(OnToGachaButtonClick);
	}

	protected override void OnSetup(IdolState setupData)
	{
		pool.Sync(CellUI<IdolState>.applicationState.characterManager.characterStates.Where((PlayerCharacterState c) => c.def.attachClass.mClass == setupData.idolDef.attachClass).ToList());
		LayoutRebuilder.MarkLayoutForRebuild(base.rectTransform);
		Canvas.ForceUpdateCanvases();
		scrollRectExtend.verticalNormalizedPosition = 1f;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		var (num, num2) = CalculateAndSetProgress();
		titleText.text = $"{CellUI<IdolState>.textData.term_resonance}{TextDisplayHelper.SoulText(idolState.idolDef.attachClass)} {num}/{num2}";
	}

	private (int, int) CalculateAndSetProgress()
	{
		int num = 0;
		int num2 = 0;
		foreach (PlayerCharacterState item in CellUI<IdolState>.applicationState.characterManager.characterStates.Where((PlayerCharacterState c) => c.def.attachClass.mClass == base.setupData.idolDef.attachClass).ToList())
		{
			int num3 = base.setupData.CharacterSoulCount(item.def);
			int num4 = ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig.newCharacterReward + item.def.obsessionLevelDefs.Count;
			num += num3;
			num2 += num4;
		}
		return (num, num2);
	}

	private void OnToGachaButtonClick()
	{
		CellUI<IdolState>.fortressState.uiManager.idolBasePanel.HideUI();
		CellUI<IdolState>.fortressState.uiManager.gachaPanel.ShowDefaultEntryPool();
	}
}
