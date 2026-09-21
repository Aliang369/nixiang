using System.Linq;
using UnityEngine;

public class HiddenParticlePrewarmer : MonoBehaviour
{
	private ParticleSystem[] particles;

	private ParticleSystemRenderer[] renderers;

	private bool[] rendererStates;

	private void Awake()
	{
		particles = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		renderers = GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
		rendererStates = renderers.Select((ParticleSystemRenderer renderer) => ((Renderer)(object)renderer).enabled).ToArray();
	}

	private void OnEnable()
	{
		SetVisible(visible: false);
		ParticleSystem[] array = particles;
		foreach (ParticleSystem obj in array)
		{
			obj.Stop(false, (ParticleSystemStopBehavior)0);
			obj.Play(false);
		}
	}

	public void Reveal()
	{
		SetVisible(visible: true);
	}

	private void OnDisable()
	{
		SetVisible(visible: false);
		ParticleSystem[] array = particles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop(false, (ParticleSystemStopBehavior)0);
		}
	}

	private void SetVisible(bool visible)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			((Renderer)(object)renderers[i]).enabled = visible && rendererStates[i];
		}
	}
}
