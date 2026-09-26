using System;
using UnityEngine;

[DefaultExecutionOrder(11000)]
public class zbSpineGpuAnimRendererHost : MonoBehaviour
{
	public zbSpineGpuAnimRenderer owner;

	private void LateUpdate()
	{
		if (owner != null)
		{
			try
			{
				owner.Build();
			}
			catch (Exception exception)
			{
				owner.OnBuildException(exception);
			}
		}
		try
		{
			zbSpineGpuAnimRuntime.TickPendingReconvert();
		}
		catch (Exception ex)
		{
			zbPerfLog.LogError("[zbSpineGpuAnim] 回归排队处理异常：" + ex.Message);
		}
	}
}
