public class ResourceSetter : DebugPanel.Setter
{
	private string filterText_GenerateItem;

	private string filterText_AddRune;

	private string filterText_AddEnchant;

	private string filterText_AddInfluence;

	private string filterText_AddSkill;

	private string filterText_AddSkillWord;

	private string filterText_DoStoryPoint;

	private string filterText_ChangeMap;

	public override string name => "内容调试";

	public ResourceSetter(DebugPanel debugPanel)
		: base(debugPanel)
	{
	}

	public override void OnSwitchGroup()
	{
	}

	private void SetGenerateItem()
	{
		if (DebugPanel.Setter.applicationState?.assetCatalog == null)
		{
			return;
		}
		debugPanel.DespawnAllInPanel(3);
		foreach (ItemDefinition itemDef in DebugPanel.Setter.applicationState.assetCatalog.itemDefCatalog.ValidAssets)
		{
			if (!filterText_GenerateItem.IsNullOrEmpty() && !itemDef.name.Contains(filterText_GenerateItem))
			{
				continue;
			}
			debugPanel.AddInvokeCell(itemDef.name, delegate
			{
				ItemContainer itemContainer = base.gameState?.playerState?.bft?.tableContainer;
				if (itemContainer != null)
				{
					EnqueueAction(new SpawnItemAction(new Item(itemDef), itemContainer, withAnim: true, null, () => 0));
				}
			}, 3);
		}
	}

	private void SetAddRune()
	{
		if (DebugPanel.Setter.applicationState?.assetCatalog == null)
		{
			return;
		}
		debugPanel.DespawnAllInPanel(3);
		foreach (RuneDef runeDef in DebugPanel.Setter.applicationState.assetCatalog.runeDefCatalog.ValidAssets)
		{
			if (!filterText_AddRune.IsNullOrEmpty() && !runeDef.name.Contains(filterText_AddRune))
			{
				continue;
			}
			debugPanel.AddInvokeCell(runeDef.name, delegate
			{
				ItemInfoDisplayBase componentInChildren = base.gameState.UIManager.GetComponentInChildren<ItemInfoDisplayBase>();
				if (!(componentInChildren == null))
				{
					Item item = componentInChildren.item;
					EnqueueAction(new ChangeRuneAction(item, runeDef, 1, null));
				}
			}, 3);
		}
	}

	private void SetAddEnchant()
	{
		if (DebugPanel.Setter.applicationState?.assetCatalog == null)
		{
			return;
		}
		debugPanel.DespawnAllInPanel(3);
		foreach (EnchantDef enchantDef in DebugPanel.Setter.applicationState.assetCatalog.enchantDefCatalog.ValidAssets)
		{
			if (!filterText_AddEnchant.IsNullOrEmpty() && !enchantDef.name.Contains(filterText_AddEnchant))
			{
				continue;
			}
			debugPanel.AddInvokeCell(enchantDef.name, delegate
			{
				ItemInfoDisplayBase componentInChildren = base.gameState.UIManager.GetComponentInChildren<ItemInfoDisplayBase>();
				if (!(componentInChildren == null))
				{
					Item item = componentInChildren.item;
					EnqueueAction(new ChangeEnchantAction(item, null, enchantDef, 1));
				}
			}, 3);
		}
	}

	private void SetAddInfluence()
	{
		if (DebugPanel.Setter.applicationState?.assetCatalog == null)
		{
			return;
		}
		debugPanel.DespawnAllInPanel(3);
		foreach (InfluenceDef influenceDef in DebugPanel.Setter.applicationState.assetCatalog.influenceCatalog.ValidAssets)
		{
			if (filterText_AddInfluence.IsNullOrEmpty() || influenceDef.name.Contains(filterText_AddInfluence))
			{
				debugPanel.AddInvokeCell(influenceDef.name, delegate
				{
					EnqueueAction(new AddInfluenceAction(new Influence(influenceDef), base.gameState.playerState.bft));
				}, 3);
			}
		}
	}

	private void SetAddSkill()
	{
		if (DebugPanel.Setter.applicationState?.assetCatalog == null)
		{
			return;
		}
		debugPanel.DespawnAllInPanel(3);
		foreach (SkillDef skillDef in DebugPanel.Setter.applicationState.assetCatalog.SkillDefCatalog.ValidAssets)
		{
			if (filterText_AddSkill.IsNullOrEmpty() || skillDef.name.Contains(filterText_AddSkill))
			{
				debugPanel.AddInvokeCell(skillDef.name, delegate
				{
					EnqueueAction(new ChangeSkillAction(new GameSkill(new ProfileSkill(skillDef, 0)), ChangeSkillAction.Type.AddOutSlot));
				}, 3);
			}
		}
	}

	private void SetAddSkillWord()
	{
		if (DebugPanel.Setter.applicationState?.assetCatalog == null)
		{
			return;
		}
		debugPanel.DespawnAllInPanel(3);
		foreach (SkillWordDef validAsset in DebugPanel.Setter.applicationState.assetCatalog.skillWordCatalog.ValidAssets)
		{
			if (!filterText_AddSkillWord.IsNullOrEmpty() && !validAsset.name.Contains(filterText_AddSkillWord))
			{
				continue;
			}
			debugPanel.AddInvokeCell(validAsset.name, delegate
			{
				SkillInfoDisplay skillInfoDisplay = base.fortressState?.uiManager.GetComponentInChildren<SkillInfoDisplay>();
				if (skillInfoDisplay == null)
				{
					skillInfoDisplay = base.gameState?.UIManager.GetComponentInChildren<SkillInfoDisplay>();
				}
				if (skillInfoDisplay != null)
				{
					skillInfoDisplay.SetDirty();
				}
			}, 3);
		}
	}

	private void SetDoStoryPoint()
	{
		if (DebugPanel.Setter.applicationState?.assetCatalog == null)
		{
			return;
		}
		debugPanel.DespawnAllInPanel(3);
		foreach (StoryPoint storyPoint in DebugPanel.Setter.applicationState.assetCatalog.storyPointCatalog.ValidAssets)
		{
			if (filterText_DoStoryPoint.IsNullOrEmpty() || storyPoint.name.Contains(filterText_DoStoryPoint))
			{
				debugPanel.AddInvokeCell(storyPoint.name, delegate
				{
					storyPoint.Execute(DebugPanel.Setter.applicationState);
				}, 3);
			}
		}
	}

	private void SetChangeMap()
	{
		if (DebugPanel.Setter.applicationState?.assetCatalog == null)
		{
			return;
		}
		debugPanel.DespawnAllInPanel(3);
		foreach (DraftMapInfoData validAsset in DebugPanel.Setter.applicationState.assetCatalog.draftMapCatalog.ValidAssets)
		{
			if (filterText_ChangeMap.IsNullOrEmpty() || validAsset.mapName.Contains(filterText_ChangeMap))
			{
				DraftMapRef mapRef = new DraftMapRef(validAsset);
				debugPanel.AddInvokeCell(validAsset.mapName, delegate
				{
					EnqueueAction(new EnterNewStageAction(1, toWelcome: false, showLoading: false, base.gameState.dungeonMapManager.currentLevel, mapRef));
				}, 3);
			}
		}
	}
}
