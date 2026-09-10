using System.Collections.Generic;

namespace RpcFramework;

public static class EditorCommandNames
{
	public const string RequestScriptCompilation = "requestScriptCompilation";

	public const string GetConsoleLog = "getConsoleLog";

	public const string GetStatus = "getStatus";

	public const string SetEditorMode = "setEditorMode";

	public const string OpenScene = "openScene";

	public const string EnterPlayMode = "enterPlayMode";

	public const string ExitPlayMode = "exitPlayMode";

	public static readonly IReadOnlyCollection<string> All = new HashSet<string> { "requestScriptCompilation", "getConsoleLog", "getStatus", "setEditorMode", "openScene", "enterPlayMode", "exitPlayMode" };
}
