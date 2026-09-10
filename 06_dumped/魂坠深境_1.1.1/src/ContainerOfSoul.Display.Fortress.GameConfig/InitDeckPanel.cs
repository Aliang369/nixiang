using System;
using Common.UI;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.GameConfig;

public class InitDeckPanel : UIBase
{
	[SerializeField]
	private InitItemCell itemCellPrefab;

	[SerializeField]
	private Transform itemCellParent;

	private GameConfigPanel gameConfigPanel;

	[NonSerialized]
	private CellUIPrefabPool<InitItemCell, ItemDefinition> itemCellPool;

	protected override void Init()
	{
		gameConfigPanel = (GameConfigPanel)parent;
		itemCellPool = GetPrefabPool<InitItemCell, ItemDefinition>(itemCellPrefab, itemCellParent);
		UIBase.applicationState.eventManager.SetNewGameConfigEvent.AddListener(_OnGameConfigUpdate);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.SetNewGameConfigEvent.RemoveListener(_OnGameConfigUpdate);
	}

	public void Setup(CharacterClassState characterClass)
	{
		itemCellPool.DespawnAll();
		foreach (ItemDefinition item in characterClass.classDef.character.initDeck)
		{
			itemCellPool.Spawn(item).gameObject.SetActive(value: true);
		}
	}

	private void _OnGameConfigUpdate(NewGameConfigData config)
	{
		if (config.currentCharacter != null)
		{
			Setup(UIBase.applicationState.characterManager.GetCharacterClassState(config.currentCharacter.attachClass));
		}
	}
}
