using System;
using Common.UI;
using UnityEngine;

public class FinishHintPanel : UIBase
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
	private FinishHint prefab;

	[NonSerialized]
	private CellUIPrefabPool<FinishHint, FinishHint.Info> pool;

	protected override void Init()
	{
		base.gameObject.SetActive(value: true);
		GetPrefabPool(ref pool, prefab);
	}

	public FinishHint HintText(string s, float timeScale = 1f)
	{
		return pool.Spawn(new FinishHint.Info
		{
			s = s,
			timeScale = timeScale
		});
	}

	public void Despawn(FinishHint quickHint)
	{
		pool.Despawn(quickHint);
	}
}
