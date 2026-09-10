using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GameOverRequest
{
	public const string ExpCollectorInfluenceId = "#INF#000406#";

	public const string ClassMatterCollectorInfluenceId = "#INF#000405#";

	public const string GemCollectorInfluenceId = "#INF#000404#";

	private static readonly HashSet<string> SettlementModifierInfluenceIds = new HashSet<string>(new string[3] { "#INF#000406#", "#INF#000405#", "#INF#000404#" }, StringComparer.Ordinal);

	[JsonProperty("record", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GameRecord Record { get; set; }

	[JsonProperty("statisticsInfo", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public PlayerStatisticsInfo StatisticsInfo { get; set; }

	[JsonProperty("settlementModifiers", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GameOverSettlementModifiers SettlementModifiers { get; set; }

	public static GameOverSettlementModifiers CreateSettlementModifiers(InGameGlobalVariableStateInfo globalVariableStateInfo, IEnumerable<InfluenceRecord> recordInfluences)
	{
		int expMultiplier = globalVariableStateInfo?.GetVariableValue(InGameGlobalVariableStateInfo.GameOverExpMultiplier) ?? 0;
		int classMatterMultiplier = globalVariableStateInfo?.GetVariableValue(InGameGlobalVariableStateInfo.ClassMatterRewardMultiplier) ?? 0;
		bool normalEnemyGemBonusLikeElite = globalVariableStateInfo?.GetVariableFlag(InGameGlobalVariableStateInfo.NormalEnemyGemBonusLikeElite) ?? false;
		return CreateSettlementModifiers(expMultiplier, classMatterMultiplier, normalEnemyGemBonusLikeElite, recordInfluences);
	}

	public static GameOverSettlementModifiers CreateSettlementModifiers(int expMultiplier, int classMatterMultiplier, bool normalEnemyGemBonusLikeElite, IEnumerable<InfluenceRecord> recordInfluences)
	{
		int num = NormalizeSettlementMultiplier(expMultiplier);
		int num2 = NormalizeSettlementMultiplier(classMatterMultiplier);
		if (num == 1 && num2 == 1 && !normalEnemyGemBonusLikeElite)
		{
			return null;
		}
		return new GameOverSettlementModifiers
		{
			ExpMultiplier = num,
			ClassMatterMultiplier = num2,
			NormalEnemyGemBonusLikeElite = normalEnemyGemBonusLikeElite,
			SourceInfluenceIds = GetSettlementModifierSourceInfluenceIds(recordInfluences)
		};
	}

	private static int NormalizeSettlementMultiplier(int multiplier)
	{
		if (multiplier <= 1)
		{
			return 1;
		}
		return multiplier;
	}

	private static List<string> GetSettlementModifierSourceInfluenceIds(IEnumerable<InfluenceRecord> recordInfluences)
	{
		if (recordInfluences == null)
		{
			return new List<string>();
		}
		return (from influence in recordInfluences
			where influence != null && SettlementModifierInfluenceIds.Contains(influence.Id)
			select influence.Id).Distinct().ToList();
	}

	public GameOverRequest(global::GameRecord record, ApplicationStateInfo applicationState)
	{
		Record = record.ToServer(new NetworkConvertContext(applicationState));
		StatisticsInfo = applicationState.gameStatisticsManager.ToServer();
		SettlementModifiers = CreateSettlementModifiers(applicationState.runningGameState?.globalVariableStateInfo, Record?.Influences);
	}
}
