using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class StartGameRequest
{
	[JsonProperty("contractId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ContractId { get; set; }

	[JsonProperty("challengeId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ChallengeId { get; set; }

	[JsonProperty("characterId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string CharacterId { get; set; }

	[JsonProperty("difficulty", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DifficultyLevel Difficulty { get; set; }

	[JsonProperty("characterUsageMode", Required = Required.Default)]
	public int CharacterUsageMode { get; set; }

	[JsonProperty("characterMasteryId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string CharacterMasteryId { get; set; }

	public StartGameRequest(NewGameConfigData newGameConfigData, ApplicationStateInfo applicationStateInfo)
	{
		ContractId = newGameConfigData.contract.contractDef.FullID;
		CharacterId = newGameConfigData.currentCharacter?.FullID;
		Difficulty = newGameConfigData.contract.difficultyLevel.ToServer();
		CharacterUsageMode = (int)newGameConfigData.characterUsageMode;
		CharacterMasteryId = newGameConfigData.GetMastery(applicationStateInfo)?.FullID;
		DailyChallengeRunContext dailyChallengeContext = newGameConfigData.dailyChallengeContext;
		if (dailyChallengeContext != null && dailyChallengeContext.isDailyChallenge)
		{
			DailyChallengeRequestMapper.ApplyStartBinding(this, newGameConfigData.dailyChallengeContext, newGameConfigData.contract.difficultyLevel.ToServer());
		}
	}
}
