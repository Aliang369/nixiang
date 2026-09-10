using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RpcService
{
	private readonly RpcFileStore fileStore;

	private readonly RpcCommandRouter router;

	private readonly IRpcHostState host;

	private readonly RpcExecutionPolicy policy;

	private readonly RpcServiceOptions options;

	public RpcService(RpcFileStore fileStore, RpcCommandRouter router, IRpcHostState host, RpcExecutionPolicy? policy = null, RpcServiceOptions? options = null)
	{
		this.fileStore = fileStore;
		this.router = router;
		this.host = host;
		this.policy = policy ?? new RpcExecutionPolicy();
		this.options = options ?? new RpcServiceOptions();
	}

	public RpcPollOutcome PollOnce()
	{
		fileStore.EnsureDirectories();
		WriteEditorStatus("poll");
		string text = fileStore.GetNextClaimedRequestPath();
		if (string.IsNullOrEmpty(text))
		{
			ClaimRequestResult claimRequestResult = fileStore.ClaimNextRequest();
			if (claimRequestResult.State == ClaimRequestState.None)
			{
				return RpcPollOutcome.NoRequest();
			}
			if (claimRequestResult.State == ClaimRequestState.DuplicateRejected)
			{
				return RpcPollOutcome.DuplicateRejected(claimRequestResult.Path);
			}
			text = claimRequestResult.Path;
		}
		return ProcessRequest(text);
	}

	public RpcPollOutcome ProcessNext()
	{
		return PollOnce();
	}

	private RpcPollOutcome ProcessRequest(string requestPath)
	{
		string jobId = RpcFileStore.JobIdFromClaimedRequestPath(requestPath);
		string action = "unknown";
		string phase = "Received";
		DateTimeOffset startedAt = ReadRunningStartedAt(jobId) ?? DateTimeOffset.UtcNow;
		IRpcRequestScope rpcRequestScope = null;
		RpcResult rpcResult;
		try
		{
			RpcRequest rpcRequest = RpcRequest.FromJson(fileStore.ReadAllText(requestPath), requestPath, jobId, options.DefaultTool, options.DefaultAction);
			rpcRequest.JobId = jobId;
			startedAt = ReadRunningStartedAt(jobId) ?? startedAt;
			action = options.ResolveRunningAction?.Invoke(rpcRequest) ?? rpcRequest.Action;
			phase = "ValidateDsl";
			JObject jObject = new JObject
			{
				["tool"] = rpcRequest.Tool,
				["source"] = rpcRequest.Source
			};
			if (!string.IsNullOrWhiteSpace(rpcRequest.Command))
			{
				jObject["command"] = rpcRequest.Command;
			}
			WriteRunning(jobId, action, phase, "running", "tool=" + rpcRequest.Tool, "validateDsl", jObject, startedAt);
			RpcPolicyDecision rpcPolicyDecision = policy.CanProcess(rpcRequest, host);
			if (!rpcPolicyDecision.Allowed)
			{
				WriteRunning(jobId, action, rpcPolicyDecision.Phase, rpcPolicyDecision.State, rpcPolicyDecision.Message, rpcPolicyDecision.Phase, jObject, startedAt);
				return RpcPollOutcome.Busy(requestPath);
			}
			RpcExecutionContext context = new RpcExecutionContext(delegate(string nextPhase, string? state, string? message, string? currentStep, JObject? details)
			{
				phase = nextPhase;
				WriteRunning(jobId, action, nextPhase, state, message, currentStep, details, startedAt);
			});
			rpcRequestScope = options.BeginRequestScope?.Invoke(rpcRequest);
			rpcResult = router.Dispatch(rpcRequest, context);
		}
		catch (Exception exception)
		{
			rpcResult = RpcResult.Failure(jobId, requestPath, action, phase, exception);
		}
		RpcResult rpcResult2 = rpcResult;
		if (rpcResult2.JobId == null)
		{
			string text = (rpcResult2.JobId = jobId);
		}
		rpcResult2 = rpcResult;
		if (rpcResult2.Action == null)
		{
			string text = (rpcResult2.Action = action);
		}
		rpcResult2 = rpcResult;
		if (rpcResult2.Phase == null)
		{
			string text = (rpcResult2.Phase = phase);
		}
		rpcResult2 = rpcResult;
		if (rpcResult2.Source == null)
		{
			string text = (rpcResult2.Source = requestPath);
		}
		try
		{
			rpcRequestScope?.Complete(rpcResult);
			options.BeforeWriteResult?.Invoke(rpcResult);
			fileStore.WriteJobLog(rpcResult.JobId, rpcResult.Logs);
			WriteResult(rpcResult);
			fileStore.CleanupRunningFiles(jobId, requestPath);
			return RpcPollOutcome.Completed(rpcResult);
		}
		catch (IOException exception2)
		{
			RpcResult rpcResult3 = RpcResult.Failure(jobId, requestPath, action, "DuplicateResult", exception2);
			rpcResult3.Summary = "Rejected duplicate jobId because a result already exists.";
			fileStore.WriteDuplicateResult(rpcResult3);
			fileStore.CleanupRunningFiles(jobId, requestPath);
			return RpcPollOutcome.DuplicateRejected(fileStore.GetDuplicateResultPath(jobId));
		}
		finally
		{
			rpcRequestScope?.Dispose();
		}
	}

	private void WriteEditorStatus(string reason)
	{
		JObject obj = options.BuildEditorStatus?.Invoke(host, reason) ?? RpcEditorStatus.FromHost(host, reason).ToJObject();
		string path = options.GetStatusPath?.Invoke(fileStore, reason) ?? fileStore.GetEditorStatusPath();
		fileStore.WriteJsonAtomic(path, obj, overwrite: true);
	}

	private void WriteRunning(string jobId, string? action, string phase, string? state, string? message, string? currentStep, JObject? details, DateTimeOffset startedAt)
	{
		fileStore.WriteRunningStatus(new RpcRunningStatus
		{
			JobId = (string.IsNullOrWhiteSpace(jobId) ? "unknown" : jobId),
			Action = action,
			Phase = phase,
			CurrentStep = currentStep,
			State = state,
			Message = message,
			StartedAt = startedAt,
			UpdatedAt = DateTimeOffset.UtcNow,
			Details = details
		});
	}

	private DateTimeOffset? ReadRunningStartedAt(string jobId)
	{
		if (string.IsNullOrWhiteSpace(jobId))
		{
			return null;
		}
		string runningStatusPath = fileStore.GetRunningStatusPath(jobId);
		if (!File.Exists(runningStatusPath))
		{
			return null;
		}
		try
		{
			if (DateTimeOffset.TryParse(JObject.Parse(fileStore.ReadAllText(runningStatusPath)).Value<string>("startedAt"), out var result))
			{
				return result;
			}
		}
		catch
		{
			return null;
		}
		return null;
	}

	private void WriteResult(RpcResult result)
	{
		if (options.BuildResultJson == null)
		{
			fileStore.WriteResult(result);
			return;
		}
		JObject obj = options.BuildResultJson(result);
		string resultPath = fileStore.GetResultPath(result.JobId);
		fileStore.WriteJsonAtomic(resultPath, obj, overwrite: false);
	}
}
