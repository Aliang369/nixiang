using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RpcFileStore
{
	public string RootPath { get; }

	public string RequestsPath { get; }

	public string RunningPath { get; }

	public string ResultsPath { get; }

	public string LogsPath { get; }

	public string StatusPath { get; }

	public RpcFileStore(string rootPath)
	{
		RootPath = Path.GetFullPath(rootPath);
		RequestsPath = Path.Combine(RootPath, "requests");
		RunningPath = Path.Combine(RootPath, "running");
		ResultsPath = Path.Combine(RootPath, "results");
		LogsPath = Path.Combine(RootPath, "logs");
		StatusPath = Path.Combine(RootPath, "status");
		EnsureDirectories();
	}

	public void EnsureDirectories()
	{
		Directory.CreateDirectory(RequestsPath);
		Directory.CreateDirectory(RunningPath);
		Directory.CreateDirectory(ResultsPath);
		Directory.CreateDirectory(LogsPath);
		Directory.CreateDirectory(StatusPath);
	}

	public string GetRequestPath(string jobId)
	{
		return Path.Combine(RequestsPath, jobId + ".request.json");
	}

	public string GetRunningRequestPath(string jobId)
	{
		return Path.Combine(RunningPath, jobId + ".request.json");
	}

	public string GetRunningStatusPath(string jobId)
	{
		return Path.Combine(RunningPath, jobId + ".running.json");
	}

	public string GetResultPath(string jobId)
	{
		return Path.Combine(ResultsPath, jobId + ".result.json");
	}

	public string GetDuplicateResultPath(string jobId)
	{
		return Path.Combine(ResultsPath, jobId + ".duplicate.result.json");
	}

	public string GetLogPath(string jobId)
	{
		return Path.Combine(LogsPath, jobId + ".log.jsonl");
	}

	public string GetEditorStatusPath()
	{
		return Path.Combine(StatusPath, "editor-status.json");
	}

	public string? GetNextClaimedRequestPath()
	{
		return Directory.GetFiles(RunningPath, "*.request.json").OrderBy(File.GetLastWriteTimeUtc).ThenBy(Path.GetFileName, StringComparer.Ordinal)
			.FirstOrDefault();
	}

	public ClaimRequestResult ClaimNextRequest()
	{
		string text = Directory.GetFiles(RequestsPath, "*.request.json").OrderBy(File.GetLastWriteTimeUtc).ThenBy(Path.GetFileName, StringComparer.Ordinal)
			.FirstOrDefault();
		if (string.IsNullOrEmpty(text))
		{
			return ClaimRequestResult.None;
		}
		string jobId = JobIdFromRequestPath(text);
		if (File.Exists(GetResultPath(jobId)))
		{
			return RejectDuplicateRequest(jobId, text);
		}
		string runningRequestPath = GetRunningRequestPath(jobId);
		DeleteIfExists(runningRequestPath);
		File.Move(text, runningRequestPath);
		WriteRunningStatus(new RpcRunningStatus
		{
			JobId = jobId,
			Action = null,
			Phase = "Received",
			State = "running",
			Message = null,
			StartedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		});
		return ClaimRequestResult.Claimed(runningRequestPath);
	}

	public void WriteRunningStatus(RpcRunningStatus status)
	{
		WriteJsonAtomic(GetRunningStatusPath(status.JobId), status.ToJObject(), overwrite: true);
	}

	public void WriteEditorStatus(RpcEditorStatus status)
	{
		WriteJsonAtomic(GetEditorStatusPath(), status.ToJObject(), overwrite: true);
	}

	public void WriteResult(RpcResult result)
	{
		if (string.IsNullOrWhiteSpace(result.JobId))
		{
			throw new InvalidOperationException("Cannot write an RPC result without jobId.");
		}
		WriteJsonAtomic(GetResultPath(result.JobId), result.ToJObject(), overwrite: false);
	}

	public void WriteDuplicateResult(RpcResult result)
	{
		if (string.IsNullOrWhiteSpace(result.JobId))
		{
			throw new InvalidOperationException("Cannot write an RPC duplicate result without jobId.");
		}
		WriteJsonAtomic(GetDuplicateResultPath(result.JobId), result.ToJObject(), overwrite: true);
	}

	public void WriteJobLog(string jobId, IReadOnlyList<RpcLogEntry> logs)
	{
		string logPath = GetLogPath(jobId);
		string text = logPath + ".tmp";
		using (StreamWriter streamWriter = new StreamWriter(text, append: false))
		{
			foreach (RpcLogEntry log in logs)
			{
				streamWriter.WriteLine(log.ToJObject().ToString(Formatting.None));
			}
		}
		ReplaceFile(text, logPath, overwrite: true);
	}

	public void CleanupRunningFiles(string jobId, string requestPath)
	{
		DeleteIfExists(requestPath);
		DeleteIfExists(GetRunningStatusPath(jobId));
	}

	public string ReadAllText(string path)
	{
		return File.ReadAllText(path);
	}

	public void WriteJsonAtomic(string path, JObject obj, bool overwrite)
	{
		string text = path + ".tmp";
		File.WriteAllText(text, RpcJson.Format(obj));
		ReplaceFile(text, path, overwrite);
	}

	public static string JobIdFromClaimedRequestPath(string requestPath)
	{
		return Path.GetFileName(requestPath).Replace(".request.json", string.Empty, StringComparison.Ordinal);
	}

	public static string JobIdFromRequestPath(string requestPath)
	{
		return Path.GetFileName(requestPath).Replace(".request.json", string.Empty, StringComparison.Ordinal);
	}

	private ClaimRequestResult RejectDuplicateRequest(string jobId, string requestPath)
	{
		RpcResult rpcResult = new RpcResult
		{
			JobId = jobId,
			Ok = false,
			Action = "unknown",
			Phase = "DuplicateJob",
			Source = requestPath,
			Summary = "Rejected duplicate jobId because a result already exists.",
			Error = new RpcError
			{
				Message = "Duplicate jobId `" + jobId + "`. Existing result will not be overwritten."
			}
		};
		WriteDuplicateResult(rpcResult);
		WriteJobLog(jobId, rpcResult.Logs);
		DeleteIfExists(requestPath);
		return ClaimRequestResult.DuplicateRejected(GetDuplicateResultPath(jobId));
	}

	private static void ReplaceFile(string tmp, string path, bool overwrite)
	{
		if (!overwrite && File.Exists(path))
		{
			DeleteIfExists(tmp);
			throw new IOException("Refusing to overwrite existing file: " + path);
		}
		if (overwrite)
		{
			DeleteIfExists(path);
		}
		File.Move(tmp, path);
	}

	private static void DeleteIfExists(string path)
	{
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}
}
