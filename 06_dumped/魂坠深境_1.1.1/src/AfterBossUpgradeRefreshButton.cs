using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AfterBossUpgradeRefreshButton : MonoBehaviour
{
	[SerializeField]
	private GameButton button;

	[SerializeField]
	private TextMeshProUGUI countLabel;

	[Tooltip("素材花费节点（广告按钮可留空/隐藏）")]
	[SerializeField]
	private GameObject costRoot;

	[SerializeField]
	private Image matterIcon;

	[SerializeField]
	private TextMeshProUGUI priceLabel;

	[NonSerialized]
	private Action onClick;

	public void Init(Action onClickCallback)
	{
		onClick = onClickCallback;
		if (!(button == null))
		{
			button.onClick.RemoveListener(HandleClick);
			button.onClick.AddListener(HandleClick);
		}
	}

	public void SetShown(bool shown)
	{
		if (base.gameObject.activeSelf != shown)
		{
			base.gameObject.SetActive(shown);
		}
	}

	public void BindCurrency(int usedCount, int maxCount, bool interactable, int price, int ownedCount, Sprite icon)
	{
		if (button != null)
		{
			button.interactable = interactable;
		}
		SetCountText(usedCount, maxCount);
		if (costRoot != null)
		{
			costRoot.SetActive(value: true);
		}
		if (matterIcon != null)
		{
			matterIcon.sprite = icon;
		}
		if (priceLabel != null)
		{
			priceLabel.text = $"{price}/{ownedCount}";
		}
	}

	public void BindAd(int usedCount, int maxCount, bool interactable)
	{
		if (button != null)
		{
			button.interactable = interactable;
		}
		SetCountText(usedCount, maxCount);
		if (costRoot != null)
		{
			costRoot.SetActive(value: false);
		}
	}

	private void SetCountText(int usedCount, int maxCount)
	{
		if (countLabel != null)
		{
			countLabel.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_root_bonfire_bless_refresh_count_arg0_arg1.FormatWith(usedCount, maxCount);
		}
	}

	private void HandleClick()
	{
		onClick?.Invoke();
	}
}
