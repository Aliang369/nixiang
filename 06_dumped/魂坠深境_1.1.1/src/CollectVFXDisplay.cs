using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectVFXDisplay : MonoBehaviour
{
	public class VFXParticlePhase2Data
	{
		public Vector3 startPosition;
	}

	[SerializeField]
	private ParticleSystem particleSystem;

	[SerializeField]
	private AnimationCurve speedCurve;

	[SerializeField]
	private float rotatopmSpeed;

	[SerializeField]
	public float spreadTime;

	[SerializeField]
	public float collectTime;

	[NonSerialized]
	private Action onFinish;

	[NonSerialized]
	private List<VFXParticlePhase2Data> particleDatas;

	[NonSerialized]
	private Particle[] particles;

	[NonSerialized]
	private bool started;

	[NonSerialized]
	private Vector3 targetPosition;

	[NonSerialized]
	private float timeSinceStart;

	private void Update()
	{
		timeSinceStart += Time.deltaTime;
		if (!started)
		{
			started = true;
			StartCoroutine(StartPhase2());
		}
	}

	public void Setup(Vector3 startPosition, Vector3 endPosition, Action onFinish)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		MainModule main = particleSystem.main;
		particles = (Particle[])(object)new Particle[((MainModule)(ref main)).maxParticles];
		targetPosition = endPosition;
		base.transform.position = startPosition;
		this.onFinish = onFinish;
		timeSinceStart = 0f;
		started = false;
	}

	private IEnumerator StartPhase2()
	{
		MainModule main = particleSystem.main;
		yield return new WaitForSeconds(spreadTime);
		if (particleSystem.GetParticles(particles) == 0)
		{
			onFinish?.Invoke();
			yield break;
		}
		if ((int)((MainModule)(ref main)).simulationSpace == 0)
		{
			targetPosition = base.transform.InverseTransformPoint(targetPosition);
		}
		else if ((int)((MainModule)(ref main)).simulationSpace == 2)
		{
			targetPosition = ((MainModule)(ref main)).customSimulationSpace.InverseTransformPoint(targetPosition);
		}
		particleDatas = new List<VFXParticlePhase2Data>();
		for (int i = 0; i < particles.Length; i++)
		{
			((Particle)(ref particles[i])).angularVelocity3D = Vector3.one;
			((Particle)(ref particles[i])).angularVelocity = 0f;
			((Particle)(ref particles[i])).velocity = Vector3.zero;
			particleDatas.Add(new VFXParticlePhase2Data());
			particleDatas[i].startPosition = ((Particle)(ref particles[i])).position;
		}
		particleSystem.SetParticles(particles);
		((MainModule)(ref main)).gravityModifierMultiplier = 0f;
		float num;
		do
		{
			if (particleSystem.GetParticles(particles) == 0)
			{
				onFinish?.Invoke();
				yield break;
			}
			for (int j = 0; j < particles.Length; j++)
			{
				((Particle)(ref particles[j])).position = Vector3.Lerp(particleDatas[j].startPosition, targetPosition, speedCurve.Evaluate(timeSinceStart / collectTime));
				ref Particle reference = ref particles[j];
				((Particle)(ref reference)).rotation = ((Particle)(ref reference)).rotation + rotatopmSpeed * Time.deltaTime;
			}
			particleSystem.SetParticles(particles);
			yield return null;
			num = timeSinceStart / collectTime;
		}
		while (num <= 1f);
		for (int k = 0; k < particles.Length; k++)
		{
			((Particle)(ref particles[k])).remainingLifetime = 0f;
		}
		particleSystem.SetParticles(particles);
		onFinish?.Invoke();
	}
}
