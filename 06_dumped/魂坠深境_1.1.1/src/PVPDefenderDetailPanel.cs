using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PVPDefenderDetailPanel : UIBase
{
	public Image defenderAvatarImage;

	public TextMeshProUGUI defenderAvatarEnemyNameText;

	public RectTransform inspectDefenderArea;

	public TextMeshProUGUI defenderNameText;

	public TextMeshProUGUI defenderInfoText;

	public RectTransform editDefenderArea;

	public TMP_InputField defenderNameInput;

	public TextMeshProUGUI unableConfirmEditDefenderReasonText;

	public GameButton confirmEditDefenderButton;

	public GameButton editDeckPresetButton;

	public PVPDefenderPropertyDisplay defenderPropertyDisplay;

	public PVPDefenderRuleTextDisplay defenderRuleTextDisplay;

	public GameButton testBattleButton;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemDisplayPool;

	[NonSerialized]
	private CellUIPrefabPool<InfluenceDisplay, Influence> influenceDisplayPool;

	[NonSerialized]
	public PVPDefender currentDefender;

	[NonSerialized]
	public bool editMode;

	public bool WithDefenderAvatar => currentDefender?.enemyAvatarRef != null;

	protected override void Init()
	{
	}

	public void Setup((PVPDefender defender, bool editMode) data)
	{
	}
}
