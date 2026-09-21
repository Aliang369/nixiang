using UnityEngine;

public class VFXsRootFollowCamera : MonoBehaviour
{
	public VirtualCamera cam;

	public float zOffset;

	private void Update()
	{
		if (!(cam.bindedCamera == null))
		{
			Vector3 position = base.transform.position;
			position.z = cam.bindedCamera.transform.position.z + zOffset;
			base.transform.position = position;
		}
	}
}
