using UnityEngine;

public class ItemInfoDisplay_Normal : ItemInfoDisplayBase
{
	[SerializeField]
	public MainItemInfoDisplayBase mainItemInfoDisplay;

	protected override void Init()
	{
		InitUI(mainItemInfoDisplay);
		base.Init();
	}

	protected override void OnSetup(Item dailyContract)
	{
		base.OnSetup(dailyContract);
		mainItemInfoDisplay.Setup(dailyContract);
	}

	protected override void OnUnset()
	{
		base.OnUnset();
		mainItemInfoDisplay.Unset();
	}

	public override bool IsReference(MainItemInfoDisplayBase display)
	{
		if (display != null)
		{
			return display != mainItemInfoDisplay;
		}
		return false;
	}

	public override Vector2 GetLastItemInfoDisplayPosition()
	{
		return mainItemInfoDisplay.RectTransform().anchoredPosition;
	}
}
