using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class EnemyIndexBookCell : CellUI<EnemyInfoData>
{
	[SerializeField]
	private Image enemyIcon;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private GameButton cellButton;

	[SerializeField]
	private Image undiscoveredDark;

	[SerializeField]
	private IndexBookContentMask indexBookContentMask;

	[NonSerialized]
	public EnemyInfoData enemy;

	[NonSerialized]
	private EnemyIndexPanel enemyPanel;

	[NonSerialized]
	private bool selected;

	[NonSerialized]
	private Sequence sequence;

	public bool IsDiscovered => CellUI<EnemyInfoData>.fortressState.applicationState.discoverManager.IsDiscovered(enemy);

	protected override void Init()
	{
		enemyPanel = parent as EnemyIndexPanel;
		cellButton.onClick.AddListener(_OnButtonClick);
		InitUI(indexBookContentMask);
	}

	protected override void OnSetup(EnemyInfoData dailyContract)
	{
		enemy = dailyContract;
		indexBookContentMask.Setup(enemy);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		nameText.text = (IsDiscovered ? enemy.name : ScriptableObjectSingleton<TextDataPreset>.instance.term_undiscovered);
		enemyIcon.sprite = ((enemy.thumbnail != null) ? enemy.thumbnail : ScriptableObjectSingleton<GameDataManager>.instance.enemyAvatarPlaceHolderIcon);
		if (IsDiscovered)
		{
			enemyIcon.color = Color.white;
			undiscoveredDark.gameObject.SetActive(value: false);
		}
		else
		{
			enemyIcon.color = ColorHelper.IndexUnDiscoverColor;
			undiscoveredDark.gameObject.SetActive(value: true);
		}
	}

	private void _OnButtonClick()
	{
		indexBookContentMask.MarkTargetViewed();
		if (IsDiscovered)
		{
			enemyPanel.Select(this);
		}
	}
}
