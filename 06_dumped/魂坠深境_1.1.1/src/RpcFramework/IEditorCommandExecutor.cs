using System.Collections.Generic;

namespace RpcFramework;

public interface IEditorCommandExecutor
{
	RpcEditorStatus GetStatus(string reason);

	IReadOnlyList<RpcLogEntry> GetConsoleLogs(int maxEntries);

	void RequestScriptCompilation();

	void SetEditorMode(bool enabled);

	void OpenScene(string scenePath, bool saveModifiedScenes, bool force);

	void EnterPlayMode(string? scenePath, bool saveModifiedScenes, bool force);

	void ExitPlayMode();
}
