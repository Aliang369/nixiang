using System;
using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RpcEditorStatus
{
	public string Reason { get; set; } = string.Empty;

	public DateTimeOffset UpdatedAt { get; set; }

	public bool ServiceEnabled { get; set; }

	public bool EditorModeEnabled { get; set; }

	public bool WorkerProcessing { get; set; }

	public bool IsCompiling { get; set; }

	public bool IsUpdating { get; set; }

	public bool IsPlaying { get; set; }

	public bool IsPlayingOrWillChangePlaymode { get; set; }

	public bool IsBusy { get; set; }

	public string? ActiveSceneName { get; set; }

	public string? ActiveScenePath { get; set; }

	public static RpcEditorStatus FromHost(IRpcHostState host, string reason, bool workerProcessing = false)
	{
		return new RpcEditorStatus
		{
			Reason = reason,
			UpdatedAt = DateTimeOffset.UtcNow,
			ServiceEnabled = true,
			EditorModeEnabled = host.EditorModeEnabled,
			WorkerProcessing = workerProcessing,
			IsCompiling = host.IsCompiling,
			IsUpdating = host.IsUpdating,
			IsPlaying = host.IsPlaying,
			IsPlayingOrWillChangePlaymode = host.IsPlayingOrWillChangePlaymode,
			IsBusy = (host.IsCompiling || host.IsUpdating || host.IsPlayingOrWillChangePlaymode),
			ActiveSceneName = host.ActiveSceneName,
			ActiveScenePath = host.ActiveScenePath
		};
	}

	public JObject ToJObject()
	{
		return new JObject
		{
			["reason"] = Reason,
			["updatedAt"] = UpdatedAt.ToString("o"),
			["serviceEnabled"] = ServiceEnabled,
			["editorModeEnabled"] = EditorModeEnabled,
			["workerProcessing"] = WorkerProcessing,
			["isCompiling"] = IsCompiling,
			["isUpdating"] = IsUpdating,
			["isPlaying"] = IsPlaying,
			["isPlayingOrWillChangePlaymode"] = IsPlayingOrWillChangePlaymode,
			["isBusy"] = IsBusy,
			["activeSceneName"] = ActiveSceneName,
			["activeScenePath"] = ActiveScenePath
		};
	}
}
