using System;
using UnityEngine;

public class DungeonMapPanel_Map : DungeonMapPanel
{
	[SerializeField]
	private UIBase[] childrenUI;

	[SerializeField]
	private RectTransform playerHealth;

	[SerializeField]
	public DeckButton deckButton;

	[SerializeField]
	public GameLootRewardsDisplayArea lootRewardsDisplayArea;

	[SerializeField]
	private DraftMapProgressAxisDisplay draftMapProgressAxisDisplay;

	[NonSerialized]
	public ConvertablePivot playerHealthPivot;

	protected override void Init()
	{
		base.Init();
		InitUI(lootRewardsDisplayArea);
		CreateDraftMapProgressAxisIfNeeded();
		InitUI(draftMapProgressAxisDisplay);
		playerHealthPivot = new ConvertablePivot(playerHealth);
		UIBase[] array = childrenUI;
		foreach (UIBase child in array)
		{
			InitUI(child);
		}
	}

	public override void Setup(DungeonMap map)
	{
		base.Setup(map);
		draftMapProgressAxisDisplay?.Setup();
	}

	private void CreateDraftMapProgressAxisIfNeeded()
	{
		if (!(draftMapProgressAxisDisplay != null))
		{
			GameObject gameObject = new GameObject("DraftMapProgressAxis", typeof(RectTransform));
			RectTransform obj = gameObject.transform as RectTransform;
			obj.SetParent(base.transform, worldPositionStays: false);
			obj.anchorMin = new Vector2(1f, 0.5f);
			obj.anchorMax = new Vector2(1f, 0.5f);
			obj.pivot = new Vector2(1f, 0.5f);
			obj.anchoredPosition = new Vector2(-24f, 0f);
			obj.sizeDelta = new Vector2(360f, 720f);
			obj.SetAsLastSibling();
			draftMapProgressAxisDisplay = gameObject.AddComponent<DraftMapProgressAxisDisplay>();
		}
	}
}
