using UnityEngine;

public class DungeonMapPanel_Preview : DungeonMapPanel
{
	[SerializeField]
	private UIBase[] childrenUI;

	private DungeonMap pendingMap;

	private DungeonMap setupMap;

	protected override void Init()
	{
		base.Init();
		UIBase[] array = childrenUI;
		foreach (UIBase child in array)
		{
			InitUI(child);
		}
	}

	public override void Setup(DungeonMap map)
	{
		if (pendingMap != map)
		{
			Unset();
			dungeonMapDisplay.ClearDraftMap();
			pendingMap = map;
			setupMap = null;
		}
	}

	public override void Unset()
	{
		base.Unset();
		setupMap = null;
	}

	public bool EnsureSetup()
	{
		if (pendingMap == null)
		{
			pendingMap = UIBase.gameState.dungeonMapManager.currentDungeonMap;
		}
		if (pendingMap == null)
		{
			return false;
		}
		if (setupMap == pendingMap)
		{
			return true;
		}
		base.Setup(pendingMap);
		setupMap = pendingMap;
		return true;
	}
}
