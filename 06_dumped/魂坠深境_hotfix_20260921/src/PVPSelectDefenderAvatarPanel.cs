using System;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PVPSelectDefenderAvatarPanel : UIBase, ISelectablePanel
{
	public PVPDefenderAvatarSelectCell cellPrefab;

	public Transform cellParent;

	public GameObject waitSelectEnemyHint;

	public GameObject currentSelectAvatarInfoArea;

	public TextMeshProUGUI selectingAvatarNameText;

	public Image selectingAvatarImage;

	public PVPDefenderPropertyDisplay selectingPropertyDisplay;

	public PVPDefenderRuleTextDisplay selectingRuleTextDisplay;

	public GameButton confirmSelectAvatarButton;

	public GameButton cancelButton;

	[NonSerialized]
	private PVPDefenderDetailPanel parentDefenderDetailPanel;

	[NonSerialized]
	private CellUIPrefabPool<PVPDefenderAvatarSelectCell, EnemyInfoData> avatarCellPool;

	[NonSerialized]
	private PVPDefenderAvatarSelectCell selectingCell;

	private EnemyInfoData SelectedAvatarData => selectingCell?.setupData;

	protected override void Init()
	{
		InitUI(selectingPropertyDisplay);
		InitUI(selectingRuleTextDisplay);
		avatarCellPool = GetPrefabPool<PVPDefenderAvatarSelectCell, EnemyInfoData>(cellPrefab, cellParent);
		parentDefenderDetailPanel = GetUIInParents<PVPDefenderDetailPanel>();
		confirmSelectAvatarButton.onClick.AddListener(delegate
		{
			parentDefenderDetailPanel.currentDefender.enemyAvatarRef = new EnemyAssetRef(SelectedAvatarData);
			parentDefenderDetailPanel.SetDirty();
			base.gameObject.SetActive(value: false);
		});
		cancelButton.onClick.AddListener(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	protected override void UpdateDisplay()
	{
		avatarCellPool.Sync(UIBase.applicationState.pvpModeManager.unlockedPVPEnemyAvatars.ToList());
		if (SelectedAvatarData != null)
		{
			currentSelectAvatarInfoArea.SetActive(value: true);
			waitSelectEnemyHint.gameObject.SetActive(value: false);
			confirmSelectAvatarButton.gameObject.SetActive(value: true);
			selectingAvatarNameText.text = SelectedAvatarData.name;
			selectingAvatarImage.sprite = SelectedAvatarData.avatar;
			selectingPropertyDisplay.Setup(SelectedAvatarData.pvpEnemyAvatarConfig.GenerateBaseDefenderProperty());
			selectingRuleTextDisplay.Setup(SelectedAvatarData.pvpEnemyAvatarConfig);
		}
		else
		{
			currentSelectAvatarInfoArea.SetActive(value: false);
			waitSelectEnemyHint.gameObject.SetActive(value: true);
			confirmSelectAvatarButton.gameObject.SetActive(value: false);
		}
	}

	public void Select(SelectableModuleBase module)
	{
		PVPDefenderAvatarSelectCell component = module.GetComponent<PVPDefenderAvatarSelectCell>();
		if (component != null)
		{
			selectingCell = component;
			SetDirty();
		}
	}

	public void Unselect(SelectableModuleBase module)
	{
		if (selectingCell == module.GetComponent<PVPDefenderAvatarSelectCell>())
		{
			selectingCell = null;
			SetDirty();
		}
	}

	public bool IsSelected(SelectableModuleBase module)
	{
		return selectingCell == module.GetComponent<PVPDefenderAvatarSelectCell>();
	}

	public bool CanSelect(SelectableModuleBase module)
	{
		PVPDefenderAvatarSelectCell component = module.GetComponent<PVPDefenderAvatarSelectCell>();
		if (component != null)
		{
			return component.setupData != null;
		}
		return false;
	}
}
