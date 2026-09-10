using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoPanel : UIBase
{
	[SerializeField]
	private TextMeshProUGUI playerNameText;

	[SerializeField]
	private TextMeshProUGUI dreamLevelText;

	[SerializeField]
	private TextMeshProUGUI levelBadgeText;

	[SerializeField]
	private Slider expBar;

	[SerializeField]
	private Image playerAvatarImage;

	private Sprite defaultPlayerAvatar;

	protected override void Init()
	{
		defaultPlayerAvatar = playerAvatarImage.sprite;
		UIBase.applicationState.eventManager.changeExpResultExecuteEvent.AddListener(OnChangeExp);
		SetDirty();
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.changeExpResultExecuteEvent.RemoveListener(OnChangeExp);
	}

	protected override void UpdateDisplay()
	{
		LevelLine globalLevelLine = UIBase.applicationState.globalUnlockManager.globalLevelLine;
		int currentDisplayLevel = globalLevelLine.CurrentDisplayLevel;
		playerNameText.text = UIBase.applicationState.playerInfoManager.DisplayName;
		dreamLevelText.text = $"Lv.{currentDisplayLevel}";
		levelBadgeText.text = currentDisplayLevel.ToString();
		playerAvatarImage.sprite = UIBase.applicationState.newGameConfigManager.currentNewGameConfig.currentCharacter?.headIcon ?? defaultPlayerAvatar;
		expBar.value = (globalLevelLine.IsMaxLevel ? 1f : ((float)globalLevelLine.currentExp / (float)Mathf.Max(1, globalLevelLine.CurrentLevelExpCost)));
	}

	private void OnChangeExp(LevelLine.ChangeExpResult result)
	{
		if (result != null && result.levelLineDef?.type == LevelLineDef.LevelLineType.Global)
		{
			SetDirty();
		}
	}
}
