using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class AiSessionFeature : IRpcFeatureModule, IRpcRequestHandler
{
	private readonly IAiSessionBackend backend;

	private string? activeSessionId;

	public AiSessionFeature(IAiSessionBackend backend)
	{
		this.backend = backend;
	}

	public void Register(RpcCommandRouter router)
	{
		router.Register("aiSession", this);
	}

	public RpcResult Handle(RpcRequest request, RpcExecutionContext context)
	{
		string text = request.Body.Value<string>("command") ?? request.Action;
		if (string.IsNullOrWhiteSpace(text))
		{
			throw new InvalidOperationException("aiSession request is missing command.");
		}
		if (text == "createSession")
		{
			AiObservation aiObservation = backend.CreateSession(request.Body);
			activeSessionId = aiObservation.SessionId;
			return RpcResult.Success(request, "Created AI session.", aiObservation.ToJObject());
		}
		if (text == "getStatus")
		{
			return RpcResult.Success(request, "AI session status.", backend.GetStatus());
		}
		string text2 = request.Body.Value<string>("sessionId") ?? activeSessionId;
		if (text == "observe")
		{
			if (text2 == null)
			{
				text2 = string.Empty;
			}
			return RpcResult.Success(request, "AI session observation.", backend.Observe(text2).ToJObject());
		}
		if (string.IsNullOrWhiteSpace(text2))
		{
			throw new InvalidOperationException("No active AI session.");
		}
		switch (text)
		{
		case "stepUntilDecision":
			return RpcResult.Success(request, "Stepped AI session.", backend.StepUntilDecision(text2, request.Body).ToJObject());
		case "closeSession":
			backend.CloseSession(text2);
			if (string.Equals(activeSessionId, text2, StringComparison.Ordinal))
			{
				activeSessionId = null;
			}
			return RpcResult.Success(request, "Closed AI session.", new JObject { ["sessionId"] = text2 });
		case "act":
			return HandleAct(request, text2);
		default:
			throw new InvalidOperationException("Unsupported aiSession.command: " + text);
		}
	}

	private RpcResult HandleAct(RpcRequest request, string sessionId)
	{
		int? num = request.Body.Value<int?>("decisionSeq");
		if (!num.HasValue)
		{
			throw new InvalidOperationException("act is missing decisionSeq.");
		}
		string actionId = request.Body.Value<string>("actionId");
		if (string.IsNullOrWhiteSpace(actionId))
		{
			throw new InvalidOperationException("act is missing actionId.");
		}
		AiObservation aiObservation = backend.Observe(sessionId);
		if (aiObservation.DecisionSeq != num.Value)
		{
			throw new InvalidOperationException("Stale decisionSeq.");
		}
		if (!aiObservation.LegalActions.Any((AiLegalAction a) => string.Equals(a.ActionId, actionId, StringComparison.Ordinal)))
		{
			throw new InvalidOperationException("Unknown or illegal actionId: " + actionId);
		}
		AiActResult aiActResult = backend.Act(sessionId, actionId, request.Body);
		return RpcResult.Success(request, "Applied AI session action.", aiActResult.ToJObject());
	}
}
