using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectorArea : UIBase, ISelectablePanel
{
	[SerializeField]
	private CharacterSelectorDisplay displayPrefab;

	[SerializeField]
	private ScrollRect scrollRect;

	[NonSerialized]
	private CellUIPrefabPool<CharacterSelectorDisplay, PlayerCharacterState> displayPool;

	[NonSerialized]
	private List<PlayerCharacterState> characters;

	[NonSerialized]
	public Action<PlayerCharacterState> onSelect;

	public PlayerCharacterState selectedCharacter;

	protected override void Init()
	{
		GetPrefabPool(ref displayPool, displayPrefab);
	}

	public void Refresh(PlayerCharacterState target, bool unlockedOnly, bool targetOnly = false)
	{
		if (targetOnly && target != null)
		{
			characters = new List<PlayerCharacterState> { target };
		}
		else
		{
			characters = UIBase.applicationState.characterManager.characterStates?.Where((PlayerCharacterState c) => !unlockedOnly || !c.locked).OrderBy((PlayerCharacterState c) => LimitedCharacterDeploymentService.GetSelectorSortRank(UIBase.applicationState, c)).ThenBy((PlayerCharacterState c) => c.def.sortDisplayOrderIndex <= 0)
				.ThenBy((PlayerCharacterState c) => c.def.sortDisplayOrderIndex)
				.ThenBy((PlayerCharacterState c) => -c.def.SOindex)
				.ToList() ?? new List<PlayerCharacterState>();
		}
		displayPool.DespawnAll();
		displayPool.Sync(characters);
		selectedCharacter = (characters.Contains(target) ? target : characters.FirstOrDefault());
		SnapSelectedCharacterToCenter();
	}

	public bool IsSelected(PlayerCharacterState character)
	{
		return selectedCharacter == character;
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		CharacterSelectorDisplay characterSelectorDisplay = module.parent as CharacterSelectorDisplay;
		if (!(characterSelectorDisplay == null))
		{
			selectedCharacter = characterSelectorDisplay.character;
			onSelect?.Invoke(selectedCharacter);
			parent.SetDirty();
		}
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		CharacterSelectorDisplay characterSelectorDisplay = module.parent as CharacterSelectorDisplay;
		if (characterSelectorDisplay != null)
		{
			return selectedCharacter == characterSelectorDisplay.character;
		}
		return false;
	}

	bool ISelectablePanel.CanSelect(SelectableModuleBase module)
	{
		CharacterSelectorDisplay characterSelectorDisplay = module.parent as CharacterSelectorDisplay;
		if (characterSelectorDisplay != null)
		{
			return characterSelectorDisplay.character != null;
		}
		return false;
	}

	private void SnapSelectedCharacterToCenter()
	{
		if (scrollRect == null || !scrollRect.vertical || selectedCharacter == null)
		{
			return;
		}
		try
		{
			LayoutRebuilder.MarkLayoutForRebuild(base.rectTransform);
			Canvas.ForceUpdateCanvases();
			RectTransform rectTransform = (displayPool?.GetActiveElement(selectedCharacter))?.RectTransform();
			RectTransform content = scrollRect.content;
			RectTransform rectTransform2 = ((scrollRect.viewport != null) ? scrollRect.viewport : scrollRect.RectTransform());
			if (!(rectTransform == null) && !(content == null) && !(rectTransform2 == null))
			{
				float num = content.rect.height - rectTransform2.rect.height;
				if (num <= 0f)
				{
					scrollRect.verticalNormalizedPosition = 1f;
					return;
				}
				Vector3 position = rectTransform.TransformPoint(rectTransform.rect.center);
				float num2 = 0f - content.InverseTransformPoint(position).y - rectTransform2.rect.height * 0.5f;
				float verticalNormalizedPosition = Mathf.Clamp01(1f - num2 / num);
				scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
			}
		}
		catch (Exception)
		{
			TraceLog.Error($"SnapSelectedCharacterToCenter() - {scrollRect} {scrollRect.vertical} {selectedCharacter}");
			throw;
		}
	}
}
