using UnityEngine;

public class VFXUtils
{
	public static Vector3 OverlayCanvasPositionToCameraPosition(Vector3 position, Camera camera)
	{
		position.x = camera.transform.position.x + (position.x - (float)(Screen.width / 2)) / (float)Screen.width * camera.orthographicSize;
		position.y = camera.transform.position.y + (position.y - (float)(Screen.height / 2)) / (float)Screen.height * camera.orthographicSize;
		return position;
	}
}
