using UnityEngine;

public class zbCameraHelper
{
	public Camera mainCamera;

	private static zbCameraHelper sInstance;

	public static zbCameraHelper Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbCameraHelper();
			}
			return sInstance;
		}
	}

	public void Init()
	{
		mainCamera = Camera.main;
	}
}
