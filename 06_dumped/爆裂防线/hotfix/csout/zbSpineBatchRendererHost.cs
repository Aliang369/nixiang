using System;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public class zbSpineBatchRendererHost : MonoBehaviour
{
	public zbSpineBatchRenderer owner;

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
	}
}
