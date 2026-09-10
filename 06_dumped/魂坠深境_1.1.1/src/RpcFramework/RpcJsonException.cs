using System;

namespace RpcFramework;

public sealed class RpcJsonException : Exception
{
	public RpcJsonException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
