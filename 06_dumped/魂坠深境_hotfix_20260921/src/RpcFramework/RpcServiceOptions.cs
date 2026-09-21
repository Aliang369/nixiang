using System;
using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RpcServiceOptions
{
	public Func<IRpcHostState, string, JObject>? BuildEditorStatus { get; set; }

	public Func<RpcFileStore, string, string>? GetStatusPath { get; set; }

	public Func<RpcRequest, string>? ResolveRunningAction { get; set; }

	public Func<RpcRequest, IRpcRequestScope>? BeginRequestScope { get; set; }

	public Action<RpcResult>? BeforeWriteResult { get; set; }

	public Func<RpcResult, JObject>? BuildResultJson { get; set; }

	public string? DefaultTool { get; set; }

	public string? DefaultAction { get; set; }
}
