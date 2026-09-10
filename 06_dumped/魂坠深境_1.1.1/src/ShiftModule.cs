using UnityEngine;

public class ShiftModule : MonoBehaviour
{
	[SerializeField]
	private Vector2 factor;

	private void Update()
	{
		GyroscopeCameraController gyroscopeCameraController = Bootstrap.applicationState?.currentVirtualCamera?.GetComponent<GyroscopeCameraController>();
		if (!(gyroscopeCameraController == null))
		{
			base.transform.localPosition = (gyroscopeCameraController.offset * factor).AsVector3();
		}
	}
}
