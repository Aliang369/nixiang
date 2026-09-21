using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.GameConfig;

public class DifficultyInfoCell : CellUI<DifficultyInfo>, IDisplayModulesController, IWheelScrollRectSnappingAnchor
{
	[SerializeField]
	public TextMeshProUGUI nameText;

	[SerializeField]
	public RectTransform unlockMask;

	[SerializeField]
	public Image iconImage;

	[SerializeField]
	public Image outline;

	[SerializeField]
	public Color selectedColor;

	[SerializeField]
	public Color unselectedColor;

	[NonSerialized]
	public DifficultySelectionPanel difficultySelectionPanel;

	[NonSerialized]
	private InputModule inputModule;

	public DifficultyInfo cellDifficulty => base.setupData;

	public DifficultyInfo finalDifficulty => cellDifficulty;

	public RectTransform snappingAnchor => iconImage.rectTransform;

	protected override void Init()
	{
		difficultySelectionPanel = parent as DifficultySelectionPanel;
		inputModule = GetModule<InputModule>();
		if (inputModule != null)
		{
			InputModule obj = inputModule;
			obj.onPointerClick = (Action<InputModuleBase>)Delegate.Combine(obj.onPointerClick, new Action<InputModuleBase>(OnPointerClick));
		}
	}

	protected override void Dispose()
	{
		if (inputModule != null)
		{
			InputModule obj = inputModule;
			obj.onPointerClick = (Action<InputModuleBase>)Delegate.Remove(obj.onPointerClick, new Action<InputModuleBase>(OnPointerClick));
		}
	}

	protected override void OnSetup(DifficultyInfo t)
	{
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (finalDifficulty == null)
		{
			return;
		}
		outline.gameObject.SetActive(value: false);
		iconImage.gameObject.SetActive(value: false);
		nameText.transform.parent.gameObject.SetActive(value: false);
		unlockMask.gameObject.SetActive(value: false);
		if (difficultySelectionPanel.IsDifficultyConfirmable(cellDifficulty.difficultyLevel))
		{
			iconImage.gameObject.SetActive(value: true);
			nameText.transform.parent.gameObject.SetActive(value: true);
			nameText.text = finalDifficulty.difficultyName;
			iconImage.sprite = finalDifficulty.icon;
			if (difficultySelectionPanel.current == this)
			{
				outline.gameObject.SetActive(value: true);
				iconImage.color = selectedColor;
				nameText.color = selectedColor;
			}
			else
			{
				iconImage.color = unselectedColor;
				nameText.color = unselectedColor;
			}
		}
		else
		{
			unlockMask.gameObject.SetActive(value: true);
		}
	}

	public void Select()
	{
		if (!(difficultySelectionPanel.current == this))
		{
			difficultySelectionPanel.TrySelectCell(this);
		}
	}

	private void OnPointerClick(InputModuleBase module)
	{
		PointerEventData pointerEventData = module?.eventData;
		if (pointerEventData != null && pointerEventData.button == PointerEventData.InputButton.Left && !pointerEventData.dragging && !(finalDifficulty == null))
		{
			difficultySelectionPanel.SetCurrentDifficulty(finalDifficulty.difficultyLevel);
		}
	}
}
