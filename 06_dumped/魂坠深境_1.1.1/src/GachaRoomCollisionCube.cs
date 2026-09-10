using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class GachaRoomCollisionCube : MonoBehaviour
{
	private Dictionary<GameObject, int> remainCollideCount = new Dictionary<GameObject, int>();

	public void ResetCollision(List<ParticleSystem> particleSystems)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		remainCollideCount.Clear();
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			if (!((Object)(object)particleSystem == null) && !(((Component)(object)particleSystem).gameObject == null))
			{
				Dictionary<GameObject, int> dictionary = remainCollideCount;
				GameObject key = ((Component)(object)particleSystem).gameObject;
				EmissionModule emission = particleSystem.emission;
				Burst burst = ((EmissionModule)(ref emission)).GetBurst(0);
				dictionary[key] = ((Burst)(ref burst)).cycleCount;
			}
		}
	}

	private void OnParticleCollision(GameObject other)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (remainCollideCount.ContainsKey(other) && remainCollideCount[other] > 0)
		{
			remainCollideCount[other]--;
			RuntimeManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_gacha_draw_crystal, default(Vector3));
		}
	}
}
