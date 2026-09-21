using System;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusLocationBonusCell : CellUI<BonusLocationBonus>, IDisplayModulesController
{
	public const float flipTime = 0.1f;

	public Image bonusImage;

	public ItemDisplayBase slotDisplay;

	public Image hideIcon;

	public Image TriggerNotifyImage;

	public TextMeshProUGUI title;

	public TextMeshProUGUI countText;

	public GameObject rareBonusHint;

	public bool showing;

	public Action<BonusLocationBonusCell> onclick;

	public BonusLocationBonus bonus;

	[NonSerialized]
	private RewardUncollectedTag itemUncollectedTag;

	[NonSerialized]
	private InputModule inputModule;

	protected override void Init()
	{
		inputModule = GetModule<InputModule>();
		InputModule obj = inputModule;
		obj.onPointerClick = (Action<InputModuleBase>)Delegate.Combine(obj.onPointerClick, new Action<InputModuleBase>(_OnPointerClick));
		InitUI(slotDisplay);
		itemUncollectedTag = RewardUncollectedTag.AttachTo(slotDisplay, RewardUncollectedTagLayout.BonusLocationCard);
		showing = true;
	}

	protected override void OnSetup(BonusLocationBonus t)
	{
		bonus = t;
		TriggerNotifyImage.gameObject.SetActive(value: false);
		showing = true;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (showing)
		{
			if (bonus.bonus is GetSingleItemBonus getSingleItemBonus)
			{
				slotDisplay.gameObject.SetActive(value: true);
				slotDisplay.Setup(getSingleItemBonus.item);
				itemUncollectedTag?.Setup(getSingleItemBonus.item?.GetDef(), CellUI<BonusLocationBonus>.applicationState);
				bonusImage.gameObject.SetActive(value: false);
				countText.gameObject.SetActive(value: false);
			}
			else
			{
				slotDisplay.gameObject.SetActive(value: false);
				itemUncollectedTag?.Hide();
				bonusImage.gameObject.SetActive(value: true);
				bonusImage.sprite = bonus.bonus.PreviewIcon;
				countText.gameObject.SetActive(value: true);
				countText.text = bonus.bonus.countText;
			}
			if (title != null)
			{
				title.gameObject.SetActive(value: true);
				title.text = bonus.bonus.previewText;
			}
			if (hideIcon != null)
			{
				hideIcon.gameObject.SetActive(value: false);
			}
			if (bonus.rare)
			{
				rareBonusHint.gameObject.SetActive(value: true);
			}
			else
			{
				rareBonusHint.gameObject.SetActive(value: false);
			}
		}
		else
		{
			slotDisplay.gameObject.SetActive(value: false);
			itemUncollectedTag?.Hide();
			bonusImage.gameObject.SetActive(value: false);
			rareBonusHint.gameObject.SetActive(value: false);
			if (title != null)
			{
				title.gameObject.SetActive(value: false);
			}
			countText.gameObject.SetActive(value: false);
			if (hideIcon != null)
			{
				hideIcon.gameObject.SetActive(value: true);
			}
		}
	}

	public void NotifyGet()
	{
		TriggerNotifyImage.gameObject.SetActive(value: true);
	}

	public void HideCell()
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append(base.transform.DOScaleX(0f, 0.1f));
		sequence.AppendCallback(delegate
		{
			showing = false;
			SetDirty();
		});
		sequence.Append(base.transform.DOScaleX(1f, 0.1f));
		sequence.Play();
	}

	public void ShowCell()
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append(base.transform.DOScaleX(0f, 0.1f));
		sequence.AppendCallback(delegate
		{
			showing = true;
			SetDirty();
		});
		sequence.Append(base.transform.DOScaleX(1f, 0.1f));
		sequence.Play();
	}

	private void _OnPointerClick(InputModuleBase _)
	{
		onclick?.Invoke(this);
	}
}
