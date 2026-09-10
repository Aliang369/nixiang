using ContainerOfSoul.Display.Fortress.GameConfig;
using ContainerOfSoul.Display.Fortress.IndexDisplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PVPModeMainPanel : UIBase
{
	public PVPDefenderDetailPanel defenderDetailPanel;

	public GameConfigPanel gameConfigPanel;

	public PVPAllCompInsepctPanel allCompInseptPanel;

	public PVPModeGameRecordPanel pvpModeGameRecordPanel;

	public ReocrdInfoPanel recordInfoPanel;

	public PVPDefenderRankPanel defenderRankPanel;

	public PVPModePurchasePanel pvpModePurchasePanel;

	public MatterCell defenderToGetMatterCell;

	public GameButton startChallengeGameButton;

	public TextMeshProUGUI remainChallengeCountTodayText;

	public GameButton openChallengeGameRecordButton;

	public Image defenderAvatarImage;

	public GameButton createDefenderButton;

	public GameObject nonDefenderHint;

	public RectTransform defenderInfoArea;

	public TextMeshProUGUI defenderNameText;

	public TextMeshProUGUI defenderInfoText;

	public GameButton replaceDefenderButton;

	public GameButton getDefenderMatterButton;

	public GameButton inspectDefenderButton;

	public GameButton inspectAllDefenderCompButton;

	public GameButton inspectDefenderRankButton;

	public GameButton toPurchaseButton;

	public GameObject modeLockedHintMask;

	public TextMeshProUGUI modeLockedRemainTimeText;

	public GameButton helpButton;

	public GameButton backButton;

	public PVPDefender CurrentDefender => UIBase.applicationState.pvpModeManager.currentDefender;
}
