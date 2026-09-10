using System;
using Common.UI;
using TMPro;
using UnityEngine.UI;

public class PVPDefenderAvatarSelectCell : CellUI<EnemyInfoData>, IDisplayModulesController
{
	public Image enemyAvatarIcon;

	public TextMeshProUGUI enemyAvatarNameText;

	public PVPDefenderPropertyDisplay defenderPropertyDisplay;

	[NonSerialized]
	private PVPSelectDefenderAvatarPanel parentSelectDefenderAvatarPanel;

	protected override void Init()
	{
		InitUI(defenderPropertyDisplay);
		parentSelectDefenderAvatarPanel = GetUIInParents<PVPSelectDefenderAvatarPanel>();
	}

	protected override void UpdateDisplay()
	{
		if (base.setupData != null)
		{
			enemyAvatarIcon.sprite = base.setupData.avatar;
			enemyAvatarNameText.text = base.setupData.name;
			defenderPropertyDisplay.Setup(base.setupData.pvpEnemyAvatarConfig.GenerateBaseDefenderProperty());
		}
	}
}
