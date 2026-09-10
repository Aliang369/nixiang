using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class AiObservation
{
	public string? SessionId { get; set; }

	public string? Phase { get; set; }

	public int DecisionSeq { get; set; }

	public bool IsGameOver { get; set; }

	public JObject? State { get; set; }

	public AiWaitingInfo? Waiting { get; set; }

	public List<AiAutomaticEffect> AutomaticRewards { get; } = new List<AiAutomaticEffect>();

	public List<AiChoiceGroup> ChoiceGroups { get; } = new List<AiChoiceGroup>();

	public List<AiLegalAction> LegalActions { get; } = new List<AiLegalAction>();

	public List<AiAuxiliaryAction> AuxiliaryActions { get; } = new List<AiAuxiliaryAction>();

	public JObject ToJObject()
	{
		return new JObject
		{
			["sessionId"] = SessionId,
			["phase"] = Phase,
			["decisionSeq"] = DecisionSeq,
			["isGameOver"] = IsGameOver,
			["state"] = State,
			["waiting"] = ((Waiting == null) ? null : Waiting.ToJObject()),
			["automaticRewards"] = new JArray(AutomaticRewards.Select((AiAutomaticEffect a) => a.ToJObject())),
			["choiceGroups"] = new JArray(ChoiceGroups.Select((AiChoiceGroup g) => g.ToJObject())),
			["legalActions"] = new JArray(LegalActions.Select((AiLegalAction a) => a.ToJObject())),
			["auxiliaryActions"] = new JArray(AuxiliaryActions.Select((AiAuxiliaryAction a) => a.ToJObject()))
		};
	}
}
