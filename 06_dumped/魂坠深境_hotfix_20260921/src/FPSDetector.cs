using UnityEngine;

public class FPSDetector
{
	private float communicativeTime;

	private int index;

	private bool valid;

	public void StartSample()
	{
		index = 0;
		communicativeTime = 0f;
		valid = true;
	}

	public float EndSample()
	{
		valid = false;
		return (float)index / communicativeTime;
	}

	public void FrameUpdate()
	{
		if (valid)
		{
			communicativeTime += Time.deltaTime;
			index++;
		}
	}
}
