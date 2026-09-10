using System;

namespace RpcFramework;

public interface IRpcRequestScope : IDisposable
{
	void Complete(RpcResult result);
}
