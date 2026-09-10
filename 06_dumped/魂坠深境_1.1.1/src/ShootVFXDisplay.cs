using System.Collections;
using UnityEngine;

public class ShootVFXDisplay : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem main;

	[SerializeField]
	private ParticleSystem head;

	[SerializeField]
	private TrailRenderer trail;

	public void Stop()
	{
		StartCoroutine(DelayStop());
	}

	public IEnumerator DelayStop()
	{
		main.Stop();
		head.Stop();
		yield return new WaitForSeconds(trail.time);
		Object.Destroy(base.gameObject);
	}
}
