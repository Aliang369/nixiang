using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using UnityEngine;

public class CharacterManager : GlobalGameManager
{
	[SaveField(null, null)]
	public List<CharacterClassState> characterClassStates;

	[SaveField(null, null)]
	public List<PlayerCharacterState> characterStates;

	public PlayerCharacterState currentCharacter
	{
		get
		{
			return base.applicationState.newGameConfigManager.currentNewGameConfig.currentCharacter.state;
		}
		set
		{
			base.applicationState.newGameConfigManager.currentNewGameConfig.SetCharacter(value.def);
		}
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		applicationState.assetCatalog.characterClassDefCatalog.ValidAssets.BuildToList(ref characterClassStates, (CharacterClassDef ccd, CharacterClassState ccs) => ccs.classDef == ccd, (CharacterClassDef ccd) => new CharacterClassState(ccd));
		applicationState.assetCatalog.characterDefCatalog.ValidAssets.BuildToList(ref characterStates, (PlayerCharacterDef cd, PlayerCharacterState cs) => cs.def == cd, (PlayerCharacterDef cd) => new PlayerCharacterState(cd));
	}

	public List<IconObjectFilterPanel.IconFilterData> GetAllCharacterClassIconFilterDatas(bool includeGeneral, string classFormatStr = null)
	{
		List<IconObjectFilterPanel.IconFilterData> list = new List<IconObjectFilterPanel.IconFilterData>();
		foreach (CharacterClassDef item in base.applicationState.assetCatalog.characterClassDefCatalog.ValidAssets.OrderBy((CharacterClassDef c) => c.sortDisplayOrderIndex))
		{
			list.Add(new IconObjectFilterPanel.IconFilterData(title: (classFormatStr == null) ? item.characterName : classFormatStr.FormatWith(item.characterName), target: item, icon: item.professionIcon));
		}
		if (includeGeneral)
		{
			list.Add(new IconObjectFilterPanel.IconFilterData(null, ScriptableObjectSingleton<GameDataManager>.instance.generalClassIcon, ScriptableObjectSingleton<TextDataPreset>.instance.term_all_character_class_general_skills));
		}
		return list;
	}

	public List<PlayerCharacterState> GetClassAllCharacters(CharacterClassDef characterClass)
	{
		return characterStates.FindAll((PlayerCharacterState c) => c.def.attachClass == characterClass);
	}

	public string GetClassAllCharactersString(CharacterClassDef characterClass)
	{
		List<PlayerCharacterState> classAllCharacters = GetClassAllCharacters(characterClass);
		string text = "";
		foreach (PlayerCharacterState item in classAllCharacters)
		{
			text = text + item.def.characterName + " ";
		}
		return text;
	}

	public PlayerCharacterState GetCharacterState(PlayerCharacterDef characterDef)
	{
		return characterStates.Find((PlayerCharacterState c) => c.def == characterDef);
	}

	public PlayerCharacterState GetOrCreateCharacterState(PlayerCharacterDef characterDef)
	{
		if (characterDef == null)
		{
			return null;
		}
		if (characterStates == null)
		{
			characterStates = new List<PlayerCharacterState>();
		}
		PlayerCharacterState playerCharacterState = characterStates.Find((PlayerCharacterState c) => c.def == characterDef);
		if (playerCharacterState != null)
		{
			return playerCharacterState;
		}
		playerCharacterState = new PlayerCharacterState(characterDef);
		characterStates.Add(playerCharacterState);
		Debug.LogWarning("[CharacterManager] Missing runtime character state for " + characterDef.characterName + ", created fallback state.");
		return playerCharacterState;
	}

	public CharacterClassState GetCharacterClassState(CharacterClassDef characterClass)
	{
		CharacterClassState characterClassState = characterClassStates.Find((CharacterClassState c) => c.classDef == characterClass);
		if (characterClassState != null)
		{
			return characterClassState;
		}
		characterClassState = new CharacterClassState(characterClass);
		characterClassStates.Add(characterClassState);
		return characterClassState;
	}

	public CharacterClassState GetCharacterClassState(CharacterClassType characterClass)
	{
		CharacterClassState characterClassState = characterClassStates.Find((CharacterClassState c) => c.classDef.mClass == characterClass);
		if (characterClassState != null)
		{
			return characterClassState;
		}
		characterClassState = new CharacterClassState(base.applicationState.assetCatalog.characterClassDefCatalog.ValidAssets.Find((CharacterClassDef c) => c.mClass == characterClass));
		characterClassStates.Add(characterClassState);
		return characterClassState;
	}

	public CharacterClassDef GetCharacterClassDef(CharacterClassType characterClass)
	{
		return base.applicationState.assetCatalog.characterClassDefCatalog.ValidAssets.Find((CharacterClassDef c) => c.mClass == characterClass);
	}

	public IEnumerable<PlayerCharacterState> GetAllUnlockedCharacterStates()
	{
		return characterStates.FindAll((PlayerCharacterState c) => !c.locked);
	}

	public bool IsNew(PlayerCharacterDef character)
	{
		return characterStates.Find((PlayerCharacterState c) => c.def == character) == null;
	}

	public void UnlockCharacterByObsessionDebris(PlayerCharacterState playerCharacterState, Action onComplete)
	{
		if (playerCharacterState.CheckCanUnlockCharacterByObsessionDebris() && GlobalConfigManager.instance.enableNetwork)
		{
			UnlockCharacterRequest unlockCharacterRequest = new UnlockCharacterRequest();
			unlockCharacterRequest.CharacterId = playerCharacterState.def.FullID;
			new GeneralGameAsyncOperationRequestOnly<UnlockCharacterRequest>(unlockCharacterRequest, base.applicationState.ChillyRoomSDK.Character.UnlockCharacterAsync).OnComplete(OnComplete).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_unlocking_character);
		}
		void OnComplete()
		{
			TraceLog.TryDebug(() => "[ChillyRoomServiceExtends] UnlockCharacterByObsessionDebris.OnComplete()");
			GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo
			{
				matter = new MatterInfo(playerCharacterState.def.obsession, 1)
			};
			globalRewardInfo.giveAppendInfo = new GlobalRewardGiveAppendInfo
			{
				isNewGive = true,
				isObsessionDebris = true,
				convertObsessionToUnlockCharacter = true
			};
			globalRewardInfo.Give(base.applicationState);
			base.fortressState.uiManager.characterGetDisplay.Setup(globalRewardInfo);
			base.applicationState.matterManager.ConsumeMatter(playerCharacterState.obsessionDebrisInfo.matterDef, playerCharacterState.obsessionDebrisUnlockRequiredCount, "解锁角色");
			onComplete?.Invoke();
		}
	}
}
