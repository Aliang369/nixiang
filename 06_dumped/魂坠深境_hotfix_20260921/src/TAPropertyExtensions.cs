using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.Accounts.Sdk;
using CoreKit.Config;
using UnityEngine;

public static class TAPropertyExtensions
{
	public static Dictionary<string, object> AppendLocationCommonProperty(this Dictionary<string, object> properties, PathLocation location)
	{
		return properties.Append("location_type", location.type.ToString()).Append("location_progress", location.localProgress).Append("location_variant_type", location.variantType.ToString())
			.Append("location_modifier_list", new List<string>())
			.Append("is_hide_location", location.hideLoc);
	}

	public static Dictionary<string, object> AppendGameplayCommonProperty(this Dictionary<string, object> properties, GameStateInfo gameState)
	{
		PathLocation currentLocation = gameState.dungeonMapManager.currentLocation;
		return Append(value: (gameState.dungeonMapManager.currentDungeonMap?.sourceDraftMap)?.CTAssetId, properties: properties.Append("difficulty", gameState.difficultyState.contractDifficulty.ToString()).Append("character", gameState.playerState.CharacterDef.FullID).Append("game_guid", gameState.GameGUID)
			.Append("current_character", gameState.playerState.character.def.FullID)
			.Append("current_character_class", gameState.playerState.character.def.attachClass.FullID)
			.Append("character_mastery", gameState.playerState.MasteryInThisGame?.FullID)
			.Append("global_progress", gameState.dungeonMapManager.currentGlobalProgress)
			.Append("stage", gameState.contractState.stage)
			.Append("game_time", new TimeSpan(gameState.gameTimer.GetTime()).TotalSeconds)
			.Append("health", gameState.playerState.bft.health)
			.Append("max_health", gameState.playerState.bft.maxHealth)
			.Append("coin", gameState.playerState.coinCount)
			.Append("crystal", gameState.playerState.crystalCount)
			.Append("game_contract_id", gameState.contractState.selectedContract.contractDef.FullID), key: "draftmap_id").Append("draftmap_pivotIndex", currentLocation?.draftMapIndex ?? (-1)).Append("willpower", gameState.playerState.RemainWillPower).Append("max_willpower", gameState.playerState.MaxWillPower)
			.Append("player_current_deck", TAFormatter.FormatItemDetailList(gameState.playerState.bft.deckContainer.items))
			.Append("player_current_bless", gameState.playerState.bft.influences.Select((Influence i) => i.def.FullID).ToList())
			.Append("current_location_type", gameState.dungeonMapManager?.currentLocation?.type.ToString())
			.Append("current_forget_content_id_list", (from i in gameState.playerState.character.characterClass.GetForgetShrineItems()
				select i.FullID).ToList())
			.AppendApplicationCommonProperty(gameState.applicationState);
	}

	public static Dictionary<string, object> AppendApplicationCommonProperty(this Dictionary<string, object> properties, ApplicationStateInfo applicationState)
	{
		if (applicationState == null)
		{
			return properties;
		}
		return properties.Append("sum_play_time", new TimeSpan(applicationState.playTimeRecordManager.GetPlayTime()).TotalSeconds).Append("sum_finished_game_count", applicationState.recordManager.FinishedGameCount).Append("common_current_credit_currency", ScriptableObjectSingleton<FortressDataManager>.instance.creditCurrencyMatter.GetCount(applicationState))
			.Append("common_current_common_currency", ScriptableObjectSingleton<FortressDataManager>.instance.commonCurrencyMatter.GetCount(applicationState))
			.Append("common_current_chracter_count", applicationState.characterManager.GetAllUnlockedCharacterStates().Count())
			.Append("common_current_season_level", (applicationState.seasonManager.currentSeason?.seasonLeveLine?.currentLevelIndex).GetValueOrDefault())
			.Append("common_current_player_level", applicationState.globalUnlockManager.globalLevelLine.currentLevelIndex);
	}

	public static Dictionary<string, object> AppendGlobalCommonProperty(this Dictionary<string, object> properties)
	{
		return Append(value: ((AccountStateManager.Instance?.State)?.User)?.Id.ToString(), properties: properties.Append("utc_time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("version_name", Application.version).Append("channel_name", SdkConfigManager.gameConfig.Distro.Name)
			.Append("unique_id", TAAdapter.GetDeviceId()), key: "user_id").Append("current_login_status", GetCurrentLoginStatus());
	}

	private static string GetCurrentLoginStatus()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		SdkState sdkState = AccountStateManager.Instance?.State;
		User user = sdkState?.User;
		string result = null;
		if (sdkState != null && user != null)
		{
			result = (((int)sdkState.CurrentLoginType == 11 && string.IsNullOrEmpty(user.Phone) && string.IsNullOrEmpty(user.Email)) ? "游客" : "账号绑定");
		}
		return result;
	}

	public static Dictionary<string, object> Append(this Dictionary<string, object> properties, string key, string value)
	{
		if (value != null)
		{
			properties.Add(key, value);
		}
		return properties;
	}

	public static Dictionary<string, object> Append(this Dictionary<string, object> properties, string key, bool value)
	{
		properties.Add(key, value);
		return properties;
	}

	public static Dictionary<string, object> Append(this Dictionary<string, object> properties, string key, int value)
	{
		properties.Add(key, value);
		return properties;
	}

	public static Dictionary<string, object> Append(this Dictionary<string, object> properties, string key, double value)
	{
		properties.Add(key, value);
		return properties;
	}

	public static Dictionary<string, object> Append(this Dictionary<string, object> properties, string key, List<string> value)
	{
		if (value != null)
		{
			properties.Add(key, value);
		}
		return properties;
	}

	public static Dictionary<string, object> Append(this Dictionary<string, object> properties, string key, Dictionary<string, object> value)
	{
		if (value != null)
		{
			properties.Add(key, value);
		}
		return properties;
	}

	public static Dictionary<string, object> Append(this Dictionary<string, object> properties, string key, List<Dictionary<string, object>> value)
	{
		if (value != null)
		{
			properties.Add(key, value);
		}
		return properties;
	}
}
