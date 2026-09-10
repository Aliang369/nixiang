using System.Collections.Generic;
using UnityEngine;

public class PathConnectionLine
{
	[SaveField(null, null)]
	public PathLocation from;

	[SaveField(null, null)]
	public PathLocation to;

	[SaveField(null, null)]
	public List<Vector3> controlPoints;

	public bool ShouldShow(DraftMap draftMap, ApplicationStateInfo applicationState)
	{
		if (from == null || to == null)
		{
			return false;
		}
		if (!from.ShouldShow(draftMap, applicationState) || !to.ShouldShow(draftMap, applicationState))
		{
			return false;
		}
		return true;
	}
}
