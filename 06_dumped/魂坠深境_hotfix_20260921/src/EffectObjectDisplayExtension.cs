using System.Collections.Generic;

public static class EffectObjectDisplayExtension
{
	public static List<EffectNodeInfo> GetDisplayEffectNodeInfos(this IEffectObjectDisplay display)
	{
		List<EffectNodeInfo> list = new List<EffectNodeInfo>();
		int num = 0;
		foreach (IEffectNode descendantNode in display.GetDescendantNodes())
		{
			EffectNodeInfo item = new EffectNodeInfo(num++, descendantNode, display);
			list.Add(item);
		}
		return list;
	}

	public static EffectNodeInfo TryGetNodeDisplayInfoOfId(this IEffectObjectDisplay display, string idStr)
	{
		int result = 0;
		if (!int.TryParse(idStr, out result))
		{
			return null;
		}
		EffectNodeInfo effectNodeInfo = display.TryGetNodeDisplayInfoOfId(result);
		if (effectNodeInfo == null)
		{
			TraceLog.Error($"[EffectObjectDisplayExtension] TryGetNodeDisplayInfoOfId() - id not found [{idStr}] [{display}]");
			return null;
		}
		return effectNodeInfo;
	}

	public static EffectNodeInfo TryGetNodeDisplayInfoOfId(this IEffectObjectDisplay display, int id)
	{
		List<EffectNodeInfo> displayEffectNodeInfos = display.GetDisplayEffectNodeInfos();
		if (displayEffectNodeInfos == null || displayEffectNodeInfos.Count == 0)
		{
			return null;
		}
		if (id < 0 || id >= displayEffectNodeInfos.Count)
		{
			return null;
		}
		return displayEffectNodeInfos[id];
	}
}
