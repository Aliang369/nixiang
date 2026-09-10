using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameSkillCellSlot : UIBase, IPointerClickHandler, IEventSystemHandler
{
	public Image icon;

	public TextMeshProUGUI nameText;

	public Image selectHintImage;

	public RectTransform skillInSlotBase;

	public Image durabilityIcon;

	public TextMeshProUGUI durabilityText;

	public RectTransform durabilityRect;

	[NonSerialized]
	private Action clickCallback;

	private SelectSkillActivatePanel parentPanel;

	[NonSerialized]
	public GameSkill skill;

	public void Setup(GameSkill skill)
	{
		this.skill = skill;
		clickCallback = null;
		SetDirty();
	}

	public void Unset()
	{
		skill = null;
		clickCallback = null;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		clickCallback?.Invoke();
	}

	protected override void Init()
	{
		parentPanel = GetUIInParents<SelectSkillActivatePanel>();
	}

	protected override void UpdateDisplay()
	{
		if (skill != null)
		{
			icon.sprite = skill.Icon;
			nameText.text = skill.Name;
			icon.gameObject.SetActive(value: true);
			if (skill.WithDurability)
			{
				if (skill.DurabilityType == SkillDef.SkillPropertyType.CastDurability)
				{
					durabilityIcon.color = Color.white;
				}
				else if (skill.DurabilityType == SkillDef.SkillPropertyType.BattleDurability)
				{
					durabilityIcon.color = Color.yellow;
				}
				durabilityText.text = skill.durability.ToString();
				durabilityRect.gameObject.SetActive(value: true);
			}
			else
			{
				durabilityRect.gameObject.SetActive(value: false);
			}
			skillInSlotBase.gameObject.SetActive(skill.IsInSlotSkill(UIBase.gameState));
		}
		else
		{
			nameText.gameObject.SetActive(value: false);
			durabilityRect.gameObject.SetActive(value: false);
			icon.gameObject.SetActive(value: false);
			skillInSlotBase.gameObject.SetActive(value: true);
		}
		if (parentPanel != null)
		{
			if (parentPanel.usingSkillSlotPanel.selected == this)
			{
				selectHintImage.gameObject.SetActive(value: true);
			}
			else
			{
				selectHintImage.gameObject.SetActive(value: false);
			}
		}
	}

	public void SetClickCallback(Action callback)
	{
		clickCallback = callback;
	}
}
