using UnityEngine;

[ExecuteInEditMode]
public class CameraRTInspector : MonoBehaviour
{
	[SerializeField]
	private RenderTexture _renderTexture;

	public RenderTexture renderTexture
	{
		get
		{
			return _renderTexture;
		}
		set
		{
			_renderTexture = value;
			((Component)this).GetComponent<Camera>().targetTexture = value;
		}
	}

	private void OnValidate()
	{
		if ((Object)(object)renderTexture != (Object)null)
		{
			((Component)this).GetComponent<Camera>().targetTexture = renderTexture;
		}
	}
}
