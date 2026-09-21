using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RpcResult
{
	public string? JobId { get; set; }

	public bool Ok { get; set; }

	public string? Action { get; set; }

	public string? Phase { get; set; }

	public string? WorkflowStage { get; set; }

	public string? RecommendedNextStep { get; set; }

	public string? AssetPath { get; set; }

	public string? Source { get; set; }

	public string? Summary { get; set; }

	public RpcError? Error { get; set; }

	public JObject? Data { get; set; }

	public JObject? Validation { get; set; }

	public JObject? SemanticDiff { get; set; }

	public JObject? ChangedFiles { get; set; }

	public JArray? SerializationSideEffects { get; set; }

	public List<RpcLogEntry> Logs { get; } = new List<RpcLogEntry>();

	public static RpcResult Success(RpcRequest request, string summary, JObject? data = null)
	{
		return new RpcResult
		{
			JobId = request.JobId,
			Ok = true,
			Action = ((request.Tool == "editorCommand") ? "command" : (request.Command ?? request.Action)),
			Phase = "Completed",
			Source = request.Source,
			Summary = summary,
			Data = data
		};
	}

	public static RpcResult Failure(string jobId, string source, string action, string phase, Exception exception)
	{
		return new RpcResult
		{
			JobId = jobId,
			Ok = false,
			Action = action,
			Phase = phase,
			Source = source,
			Error = new RpcError
			{
				Message = exception.Message,
				StackTrace = exception.ToString()
			}
		};
	}

	public JObject ToJObject()
	{
		IEnumerable<JObject> content = from log in Logs
			where string.Equals(log.Type, "Warning", StringComparison.OrdinalIgnoreCase)
			select log.ToJObject();
		JObject jObject = new JObject();
		foreach (IGrouping<string, RpcLogEntry> item in from log in Logs
			group log by (!string.IsNullOrWhiteSpace(log.Type)) ? log.Type : "Unknown")
		{
			jObject[item.Key] = item.Count();
		}
		return new JObject
		{
			["jobId"] = JobId,
			["ok"] = Ok,
			["action"] = Action,
			["phase"] = Phase,
			["workflowStage"] = WorkflowStage,
			["recommendedNextStep"] = RecommendedNextStep,
			["assetPath"] = AssetPath,
			["source"] = Source,
			["summary"] = Summary,
			["data"] = Data,
			["validation"] = Validation,
			["semanticDiff"] = SemanticDiff,
			["changedFiles"] = ChangedFiles,
			["serializationSideEffects"] = SerializationSideEffects,
			["error"] = Error?.ToJObject(),
			["warnings"] = new JArray(content),
			["logCounts"] = jObject,
			["logs"] = new JArray(Logs.Select((RpcLogEntry log) => log.ToJObject()))
		};
	}
}
