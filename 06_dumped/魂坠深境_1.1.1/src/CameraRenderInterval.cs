using UnityEngine;

public class CameraRenderInterval : MonoBehaviour
{
	public int interval;

	public int timer;

	private Camera camera;

	private void Awake()
	{
		camera = GetComponent<Camera>();
	}

	private void LateUpdate()
	{
		if (interval > 0)
		{
			if (camera.enabled)
			{
				camera.enabled = false;
			}
			if (timer < interval)
			{
				timer++;
				return;
			}
			camera.Render();
			timer = 0;
		}
		else if (!camera.enabled)
		{
			camera.enabled = true;
		}
	}
}
