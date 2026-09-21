using System;
using UnityEngine;

public class EnemyCrystalShield : MonoBehaviour
{
	[Serializable]
	public class VFXConfig
	{
		[SerializeField]
		public string name;

		[SerializeField]
		public NFXSetup nfxConfig;

		[SerializeField]
		public float time;

		[SerializeField]
		public Transform pivot;
	}

	[SerializeField]
	public VFXConfig[] vfxConfigs;
}
