using System;
using System.Collections.Generic;
using UnityEngine;

public class SwitchPanel : UIBase
{
	[SerializeField]
	private List<SwitchTag> switchTags;

	public bool allowUnselect;

	public bool locked;

	[NonSerialized]
	public Action onTagSwitched;

	[NonSerialized]
	public SwitchTag targetShowTag;

	protected override void Init()
	{
		for (int i = 0; i < switchTags.Count; i++)
		{
			SwitchTag switchTag = switchTags[i];
			InitUI(switchTag);
			switchTag.SetPanel(this);
			switchTag.Setup(new SwitchTag.Data
			{
				i = i
			});
		}
	}

	protected override void UpdateDisplay()
	{
		switchTags.ForEach(delegate(SwitchTag tag)
		{
			tag.locked = locked;
		});
	}

	public void SelectFirstOrDefault()
	{
		if (!switchTags.IsNullOrEmpty())
		{
			Select(switchTags[0]);
		}
	}

	public void Select(SwitchTag switchTag)
	{
		if (targetShowTag == switchTag)
		{
			if (allowUnselect)
			{
				targetShowTag = null;
				onTagSwitched?.Invoke();
				SetDirty();
			}
		}
		else
		{
			targetShowTag = switchTag;
			onTagSwitched?.Invoke();
			SetDirty();
		}
	}

	public void Select(int index)
	{
		Select(switchTags[index]);
	}

	public void Unselect()
	{
		if (!(targetShowTag == null))
		{
			targetShowTag = null;
			onTagSwitched?.Invoke();
			SetDirty();
		}
	}

	public void AddTag(SwitchTag tag)
	{
		if (!switchTags.Contains(tag))
		{
			switchTags.Add(tag);
			InitUI(tag);
			tag.SetPanel(this);
		}
	}

	public void ClearTags()
	{
		targetShowTag = null;
		foreach (SwitchTag switchTag in switchTags)
		{
			DisposeUI(switchTag);
		}
		switchTags.Clear();
	}
}
