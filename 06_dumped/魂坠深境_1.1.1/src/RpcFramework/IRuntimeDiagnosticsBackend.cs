using System.Collections.Generic;

namespace RpcFramework;

public interface IRuntimeDiagnosticsBackend
{
	IReadOnlyList<RuntimeConsoleLogEntry> GetConsoleLog(int sinceSequence, int maxEntries, string? minLevel, string? messageContains = null, string? messageExcludeContains = null);

	void ClearConsoleLog();

	RuntimeScreenshotResult CaptureScreenshot(RuntimeScreenshotRequest request);
}
