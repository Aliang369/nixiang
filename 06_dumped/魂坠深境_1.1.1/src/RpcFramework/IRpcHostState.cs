namespace RpcFramework;

public interface IRpcHostState
{
	bool IsCompiling { get; }

	bool IsUpdating { get; }

	bool IsPlaying { get; }

	bool IsPlayingOrWillChangePlaymode { get; }

	bool EditorModeEnabled { get; }

	string? ActiveSceneName { get; }

	string? ActiveScenePath { get; }
}
