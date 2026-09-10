using System;
using System.Collections.Generic;

[Serializable]
public class WorldEventDisplayInfo
{
	public string title;

	public string description;

	public List<WorldEventChoice> choices = new List<WorldEventChoice>();

	public Action<WorldEventDisplayInfo> displayInfoUpdate;

	public bool showLeaveButton;

	public bool needRefresh;

	public void AddChoice(WorldEventChoice choice)
	{
		choices.Add(choice);
		needRefresh = true;
	}

	public void RemoveChoice(WorldEventChoice choice)
	{
		choices.Remove(choice);
		needRefresh = true;
	}

	public void ClearChoice()
	{
		choices.Clear();
		needRefresh = true;
	}
}
