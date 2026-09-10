using System;
using Common.UI;
using UnityEngine;

public class QuickHintPanel : UIBase
{
	[Serializable]
	public class Config
	{
		[SerializeField]
		public float smallStayTime;

		[SerializeField]
		public float smallFadeTime;
	}

	[SerializeField]
	private QuickHint prefab;

	[NonSerialized]
	private CellUIPrefabPool<QuickHint, QuickHint.Info> pool;

	protected override void Init()
	{
		base.gameObject.SetActive(value: true);
		GetPrefabPool(ref pool, prefab);
	}

	public QuickHint HintText(string s, float timeScale = 1f)
	{
		return pool.Spawn(new QuickHint.Info
		{
			s = s,
			timeScale = timeScale
		});
	}

	public void Despawn(QuickHint quickHint)
	{
		pool.Despawn(quickHint);
	}
}
