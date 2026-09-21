using System.Collections;
using UnityEngine;

public class MaskGuideSequence : UIBase
{
	public GuideMask[] GuideMasks => GetComponentsInChildren<GuideMask>(includeInactive: true);

	protected override void Init()
	{
		GuideMask[] guideMasks = GuideMasks;
		foreach (GuideMask child in guideMasks)
		{
			InitUI(child);
		}
	}

	public IEnumerator GuideCor()
	{
		base.gameObject.SetActive(value: true);
		GuideMask[] guideMasks = GuideMasks;
		for (int i = 0; i < guideMasks.Length; i++)
		{
			guideMasks[i].SetActiveAndDirty(active: false);
		}
		GuideMask[] guideMasks2 = GuideMasks;
		foreach (GuideMask guide in guideMasks2)
		{
			guide.StartGuide(this);
			yield return new WaitUntil(() => guide.finished);
		}
		base.gameObject.SetActive(value: false);
	}
}
