using UnityEngine;

public class zbWorldScaler
{
	public static readonly float designWidth = 720f;

	public static readonly float designHeight = 1480f;

	public static readonly float ppu = 100f;

	public static float cameraOrthographicSize = 5f;

	public static void AdaptCamera(Camera mainCamera)
	{
		if (!((Object)(object)mainCamera == (Object)null) && mainCamera.orthographic)
		{
			float num = designWidth / ppu;
			float num2 = designHeight / ppu;
			float num3 = (float)Screen.width / (float)Screen.height;
			float num4 = num / num2;
			if (num3 >= num4)
			{
				mainCamera.orthographicSize = num2 / 2f;
			}
			else
			{
				float num5 = num / 2f;
				mainCamera.orthographicSize = num5 / num3;
			}
			cameraOrthographicSize = mainCamera.orthographicSize;
		}
	}
}
