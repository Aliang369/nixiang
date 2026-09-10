using UnityEngine;

public class EnviromentCamera : MonoBehaviour
{
	public Camera enviromentCamera;

	private void Start()
	{
		enviromentCamera.transparencySortMode = TransparencySortMode.CustomAxis;
		enviromentCamera.transparencySortAxis = Vector3.forward;
	}
}
