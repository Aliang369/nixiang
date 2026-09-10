using System;
using TMPro;
using UnityEngine;

public class ItemNameModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private bool qualityColor;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[NonSerialized]
	private bool upgraded;

	public override void Setup()
	{
		if (ShouldShow())
		{
			nameText.gameObject.SetActive(value: true);
			upgraded = base.slotDisplay.item.upgraded;
			SetNameText();
		}
		else
		{
			nameText.gameObject.SetActive(value: false);
		}
	}

	public override void UpdateDisplay()
	{
		if (base.slotDisplay.item.upgraded != upgraded)
		{
			SetNameText();
		}
	}

	private bool ShouldShow()
	{
		if (base.slotDisplay.item.OwnerBft != null && base.slotDisplay.item.OwnerBft.isEnemy)
		{
			return true;
		}
		if (!base.slotDisplay.item.HasIcon())
		{
			return true;
		}
		return false;
	}

	private void SetNameText()
	{
		nameText.text = base.slotDisplay.item.GetItemName();
		nameText.color = base.slotDisplay.item.GetItemNameColor(qualityColor);
		if (base.slotDisplay.item.IsCoolDowning)
		{
			nameText.color *= Color.grey;
		}
	}

	public void OnExile()
	{
		nameText.gameObject.SetActive(value: false);
	}
}
