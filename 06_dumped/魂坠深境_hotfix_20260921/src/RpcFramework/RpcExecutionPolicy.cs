namespace RpcFramework;

public sealed class RpcExecutionPolicy
{
	public RpcPolicyDecision CanProcess(RpcRequest request, IRpcHostState host)
	{
		if (host.IsCompiling || host.IsUpdating)
		{
			return new RpcPolicyDecision
			{
				Allowed = false,
				Message = "Host is compiling or updating."
			};
		}
		if (host.IsPlayingOrWillChangePlaymode && !CanProcessDuringPlayMode(request, host))
		{
			return new RpcPolicyDecision
			{
				Allowed = false,
				Message = "Host is in Play Mode. Only editorCommand getStatus/getConsoleLog/setEditorMode/exitPlayMode are accepted while playing."
			};
		}
		return RpcPolicyDecision.Allow;
	}

	public bool CanProcessDuringPlayMode(RpcRequest request, IRpcHostState host)
	{
		if (request.Tool != "editorCommand")
		{
			return false;
		}
		return request.Command switch
		{
			"getStatus" => true, 
			"getConsoleLog" => true, 
			"setEditorMode" => true, 
			"exitPlayMode" => host.EditorModeEnabled, 
			_ => false, 
		};
	}
}
