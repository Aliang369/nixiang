using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUpgradeSwitchTag : SwitchTag
{
	[SerializeField]
	private Image background;

	[SerializeField]
	private Graphic label;

	[SerializeField]
	private Color selectedBackgroundColor = Color.white;

	[SerializeField]
	private Color unselectedBackgroundColor = Color.white;

	[SerializeField]
	private Color selectedTextColor = Color.white;

	[SerializeField]
	private Color unselectedTextColor = Color.white;

	protected override bool ControlButtonInteractable => false;

	public override void SetText(string text)
	{
		if (label == null)
		{
			label = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		}
		if (label is TextMeshProUGUI textMeshProUGUI)
		{
			textMeshProUGUI.text = text;
		}
	}

	protected override void Init()
	{
		base.Init();
		if (background == null)
		{
			background = GetComponent<Image>();
		}
		if (label == null)
		{
			label = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		}
		OnUnselect();
	}

	protected override void OnSelect()
	{
		if (background != null)
		{
			background.color = selectedBackgroundColor;
		}
		if (label != null)
		{
			label.color = selectedTextColor;
		}
	}

	protected override void OnUnselect()
	{
		if (background != null)
		{
			background.color = unselectedBackgroundColor;
		}
		if (label != null)
		{
			label.color = unselectedTextColor;
		}
	}
}
