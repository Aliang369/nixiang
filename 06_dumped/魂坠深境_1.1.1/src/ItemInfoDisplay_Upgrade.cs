using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ItemInfoDisplay_Upgrade : ItemInfoDisplayBase
{
	[SerializeField]
	private MainItemInfoDisplayBase beforeUpgradeItemDisplay;

	[SerializeField]
	private MainItemInfoDisplayBase afterUpgradeItemDisplay;

	[SerializeField]
	private RectTransform beforeUpgradePivot;

	[SerializeField]
	private RectTransform afterUpgradePivot;

	[SerializeField]
	private RectTransform middlePivot;

	[SerializeField]
	private UpgradePanel panel;

	[NonSerialized]
	private Item after;

	[NonSerialized]
	private Item before;

	[NonSerialized]
	private Sequence sequence;

	[NonSerialized]
	public bool upgrading;

	[SerializeField]
	private GameObject costTextPanel;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private TextMeshProUGUI upgradeButtonText;

	[SerializeField]
	private GameButton upgradeButton;

	private bool IsSelectSameSkinToUpgrade
	{
		get
		{
			if (panel.isSkinUpgradeMode && before?.skin != null && after?.skin != null)
			{
				return before.skin == after.skin;
			}
			return false;
		}
	}

	private bool SkinNotUnlocked
	{
		get
		{
			if (panel.isSkinUpgradeMode && after?.skin != null)
			{
				return !after.skin.IsUnlocked(CellUI<Item>.applicationState);
			}
			return false;
		}
	}

	protected override void Init()
	{
		InitUI(beforeUpgradeItemDisplay);
		InitUI(afterUpgradeItemDisplay);
		base.Init();
		upgradeButton.onClick.AddListener(_OnUpgradeButtonClicked);
		upgrading = false;
	}

	protected override void OnSetup(Item dailyContract)
	{
		base.OnSetup(dailyContract);
		if (before != null)
		{
			beforeUpgradeItemDisplay.setAsUpgradeDisplay = false;
			beforeUpgradeItemDisplay.Setup(before);
			beforeUpgradeItemDisplay.gameObject.SetActive(value: true);
		}
		else
		{
			beforeUpgradeItemDisplay.Unset();
			beforeUpgradeItemDisplay.gameObject.SetActive(value: false);
		}
		if (after != null)
		{
			afterUpgradeItemDisplay.setAsUpgradeDisplay = true;
			afterUpgradeItemDisplay.Setup(after);
			afterUpgradeItemDisplay.gameObject.SetActive(value: true);
		}
		else
		{
			afterUpgradeItemDisplay.Unset();
			afterUpgradeItemDisplay.gameObject.SetActive(value: false);
		}
		if (before != null)
		{
			upgradeTextGenerator.before = beforeUpgradeItemDisplay.GetModule<SpellTextModule>();
		}
		if (after != null)
		{
			upgradeTextGenerator.after = afterUpgradeItemDisplay.GetModule<SpellTextModule>();
		}
	}

	protected override void OnUnset()
	{
		base.OnUnset();
		beforeUpgradeItemDisplay.Unset();
		afterUpgradeItemDisplay.Unset();
	}

	public void SetupItems(Item before, Item after)
	{
		if (upgrading)
		{
			sequence?.Kill();
		}
		upgrading = false;
		GetModule<ItemInfoReferenceModule>().HideAllSideReference();
		this.before = before;
		this.after = after;
		Unset();
		if (before != null && after == null)
		{
			beforeUpgradeItemDisplay.transform.position = middlePivot.position;
			Setup(before);
		}
		else if (before != null && after != null)
		{
			beforeUpgradeItemDisplay.transform.position = beforeUpgradePivot.position;
			afterUpgradeItemDisplay.transform.position = afterUpgradePivot.position;
			Setup(before);
		}
		SetupCostText();
		SetupButton();
	}

	public void OnUpgrade()
	{
		if (!upgrading)
		{
			sequence?.Kill();
			upgrading = true;
			panel.UpgradeItem();
			RectTransform beforeRect = beforeUpgradeItemDisplay.transform as RectTransform;
			RectTransform afterRect = afterUpgradeItemDisplay.transform as RectTransform;
			CanvasGroup afterCanvasGroup = afterUpgradeItemDisplay.GetComponent<CanvasGroup>();
			Vector3 position = middlePivot.position;
			UpgradeRoomEventPanel.Config upgradeRoomDisplayConfig = CellUI<Item>.applicationState.vfxDataPreset.upgradeRoomDisplayConfig;
			GetModule<ItemInfoReferenceModule>().HideAllSideReference();
			sequence = DOTween.Sequence();
			sequence.timeScale = CellUI<Item>.applicationState.settingManager.GetUIScale();
			sequence.Append(beforeRect.DOMove(beforeUpgradePivot.position, 0f));
			sequence.Join(afterRect.DOMove(afterUpgradePivot.position, 0f));
			sequence.AppendCallback(delegate
			{
				costTextPanel.gameObject.SetActive(value: false);
				upgradeButton.gameObject.SetActive(value: false);
			});
			sequence.Append(beforeRect.DOMove(position, upgradeRoomDisplayConfig.upgradeTime1));
			sequence.Join(afterRect.DOMove(position, upgradeRoomDisplayConfig.upgradeTime1));
			sequence.AppendCallback(delegate
			{
				beforeUpgradeItemDisplay.Setup(before);
				panel.PlayUpgradeVFX();
			});
			sequence.Append(afterRect.DOScale(upgradeRoomDisplayConfig.upgradeScale, upgradeRoomDisplayConfig.upgradeTime2));
			sequence.Join(DOTweenModuleUI.DOFade(afterCanvasGroup, 0f, upgradeRoomDisplayConfig.upgradeTime2));
			sequence.AppendCallback(delegate
			{
				SetupItems(before, null);
				GetModule<ItemInfoReferenceModule>().ShowAllSideReference();
			});
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				afterRect.localScale = beforeRect.localScale;
				costTextPanel.gameObject.SetActive(value: true);
				afterCanvasGroup.alpha = 1f;
				sequence = null;
				upgrading = false;
				CellUI<Item>.gameState.StartCoroutine(OnAnimEnd);
			});
			sequence.Play();
		}
		IEnumerator OnAnimEnd()
		{
			yield return new WaitUntil(() => panel.IsVFXCompleted());
			panel.AfterUpgrade();
		}
	}

	public override bool IsReference(MainItemInfoDisplayBase display)
	{
		if (display != null && display != beforeUpgradeItemDisplay)
		{
			return display != afterUpgradeItemDisplay;
		}
		return false;
	}

	public override Vector2 GetLastItemInfoDisplayPosition()
	{
		if (after != null)
		{
			return afterUpgradeItemDisplay.RectTransform().anchoredPosition;
		}
		return beforeUpgradeItemDisplay.RectTransform().anchoredPosition;
	}

	private void _OnUpgradeButtonClicked()
	{
		if (panel.CanConfirmUpgrade())
		{
			OnUpgrade();
			panel.SetDirty();
		}
	}

	private void SetupCostText()
	{
		if (after == null)
		{
			if (!before.Upgradeable)
			{
				costText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_upgraded;
			}
			else if (panel.isSkinUpgradeMode && CellUI<Item>.applicationState.itemSkinManager.GetAllUnlockedSkinsOfItem(before.def).All((ItemSkinDef s) => s == before.skin))
			{
				costText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_without_skin_to_set;
			}
			else
			{
				costText.text = "";
			}
		}
		else if (IsSelectSameSkinToUpgrade)
		{
			costText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_item_set_to_skin_arg0_already.FormatWith(after.skin.attachSkinSet.SkinSetNameWithColor);
		}
		else if (SkinNotUnlocked)
		{
			costText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_skin_arg0_not_unlock_yet.FormatWith(after.skin.attachSkinSet.SkinSetNameWithColor);
		}
		else
		{
			if (before == null || after == null)
			{
				return;
			}
			int num = panel.GetUpgradeCrystalPrice();
			int num2 = panel.GetUpgradeHealthPrice();
			int num3 = panel.GetSetSkinCoinPrice();
			if (CellUI<Item>.gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.ItemUpgradeFreeTimes) > 0)
			{
				num = 0;
				num2 = 0;
				num3 = 0;
			}
			costText.text = "";
			upgradeButtonText.text = "";
			if (num2 > 0)
			{
				if (costText.text.Length > 0)
				{
					costText.text += Environment.NewLine;
				}
				if (upgradeButtonText.text.Length > 0)
				{
					upgradeButtonText.text += "  ";
				}
				costText.text += ScriptableObjectSingleton<TextDataPreset>.instance.term_upgrade_with_health_arg0.FormatWith(num2);
				TextMeshProUGUI textMeshProUGUI = upgradeButtonText;
				textMeshProUGUI.text = textMeshProUGUI.text + ScriptableObjectSingleton<GameDataManager>.instance.healthInTextIcon.ToTMPSpriteText() + "-" + num2.ToString().ColorText(Color.red);
			}
			if (num3 > 0)
			{
				if (costText.text.Length > 0)
				{
					costText.text += Environment.NewLine;
				}
				if (upgradeButtonText.text.Length > 0)
				{
					upgradeButtonText.text += "  ";
				}
				costText.text += ScriptableObjectSingleton<TextDataPreset>.instance.term_set_skin_with_coin_arg0_to_skin_arg1.FormatWith(num3, after.skin.attachSkinSet.SkinSetNameWithColor);
				upgradeButtonText.text += $"{ScriptableObjectSingleton<GameDataManager>.instance.coinTextIcon.ToTMPSpriteText()}-{num3}";
			}
			if (num > 0 || costText.text == "")
			{
				costText.text += ScriptableObjectSingleton<TextDataPreset>.instance.term_upgrade_with_crystal_cost_arg0.FormatWith(num);
				upgradeButtonText.text += $"{ScriptableObjectSingleton<GameDataManager>.instance.crystalInTextIcon.ToTMPSpriteText()}-{num}";
			}
		}
	}

	private void SetupButton()
	{
		if (before == null || after == null || IsSelectSameSkinToUpgrade || SkinNotUnlocked)
		{
			upgradeButton.gameObject.SetActive(value: false);
		}
		else if (panel.CanConfirmUpgrade())
		{
			upgradeButton.gameObject.SetActive(value: true);
			upgradeButton.interactable = true;
		}
		else
		{
			upgradeButton.gameObject.SetActive(value: true);
			upgradeButton.interactable = false;
		}
	}
}
