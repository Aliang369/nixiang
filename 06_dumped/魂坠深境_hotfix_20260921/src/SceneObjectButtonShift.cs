using UnityEngine;

public class SceneObjectButtonShift : MonoBehaviour
{
	[SerializeField]
	private string path;

	private void Update()
	{
		if (!(Bootstrap.applicationState?.currentVirtualCamera?.bindedCamera.GetComponent<Camera>() == null))
		{
			Transform transform = (Bootstrap.applicationState?.fortressState?.fortressRoomManager?.room)?.transform.Find(path);
			if (!(transform == null))
			{
				Vector2 vector = default(Vector2);
				RectTransformUtility.ScreenPointToLocalPointInRectangle(base.transform.parent as RectTransform, (Vector2)Bootstrap.applicationState.currentVirtualCamera.bindedCamera.GetComponent<Camera>().WorldToScreenPoint(transform.position), (Camera)null, ref vector);
				base.transform.localPosition = vector;
			}
		}
	}
}
