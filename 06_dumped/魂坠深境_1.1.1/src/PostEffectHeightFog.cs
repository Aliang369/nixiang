using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class PostEffectHeightFog : MonoBehaviour
{
	public enum ResolutionScale
	{
		Full = 1,
		Half = 2,
		Quarter = 4,
		Eighth = 8
	}

	public float bottom;

	public Color colorBottom = Color.white;

	public Color colorTop = Color.white;

	[Space]
	public float density = 1f;

	public float densityPow = 1f;

	[Header("Noise")]
	public Vector3 direction = Vector3.forward;

	[Space]
	public Light DirectionalLight;

	public float pow = 2f;

	public ResolutionScale resolutionScale = ResolutionScale.Full;

	public float scale = 5f;

	public Shader shader;

	public float speed = 0.05f;

	public float top = 5f;

	private const string _FogButtom = "_FogBottom";

	private const string _FogColorBottom = "_FogColorBottom";

	private const string _FogColorTop = "_FogColorTop";

	private const string _FogDensity = "_FogDensity";

	private const string _FogTop = "_FogTop";

	private const string _FrustumCornersRay = "_FrustumCornersRay";

	private Camera effectCamera;

	private Material effectMaterial;

	private Camera EffectCamera
	{
		get
		{
			if (effectCamera == null)
			{
				effectCamera = GetComponent<Camera>();
			}
			return effectCamera;
		}
	}

	private Material EffectMaterial
	{
		get
		{
			if (effectMaterial == null)
			{
				effectMaterial = new Material(shader);
			}
			return effectMaterial;
		}
	}

	private Resolution GetResolution()
	{
		return new Resolution
		{
			width = EffectCamera.pixelWidth / (int)resolutionScale,
			height = EffectCamera.pixelHeight / (int)resolutionScale
		};
	}

	private void OnDisable()
	{
		EffectCamera.depthTextureMode &= ~DepthTextureMode.Depth;
	}

	private void OnEnable()
	{
		EffectCamera.depthTextureMode |= DepthTextureMode.Depth;
	}

	private void OnPreRender()
	{
		if (EffectCamera.orthographic)
		{
			EffectMaterial.EnableKeyword("PROJ_METHOD_ORTHOGRAPHIC");
			EffectMaterial.DisableKeyword("PROJ_METHOD_PERSPECTIVE");
			float orthographicSize = EffectCamera.orthographicSize;
			float num = EffectCamera.aspect * orthographicSize;
			Vector3 vector = EffectCamera.transform.up * orthographicSize;
			Vector3 vector2 = EffectCamera.transform.right * num;
			Vector3 vector3 = -vector - vector2;
			Vector3 vector4 = vector - vector2;
			Vector3 vector5 = vector + vector2;
			Vector3 vector6 = -vector + vector2;
			Matrix4x4 identity = Matrix4x4.identity;
			identity.SetRow(0, vector3);
			identity.SetRow(1, vector4);
			identity.SetRow(2, vector5);
			identity.SetRow(3, vector6);
			EffectMaterial.SetVector("_CameraWorldDir", base.transform.forward);
			EffectMaterial.SetMatrix("_FrustumCornersRay", identity);
		}
		else
		{
			EffectMaterial.DisableKeyword("PROJ_METHOD_ORTHOGRAPHIC");
			EffectMaterial.EnableKeyword("PROJ_METHOD_PERSPECTIVE");
			Matrix4x4 identity2 = Matrix4x4.identity;
			float fieldOfView = EffectCamera.fieldOfView;
			float nearClipPlane = EffectCamera.nearClipPlane;
			float aspect = EffectCamera.aspect;
			float num2 = nearClipPlane * Mathf.Tan(fieldOfView * 0.5f * ((float)Math.PI / 180f));
			Vector3 vector7 = num2 * base.transform.up;
			Vector3 vector8 = num2 * base.transform.right * aspect;
			Vector3 vector9 = nearClipPlane * base.transform.forward;
			Vector3 vector10 = vector9 + vector8 + vector7;
			vector10 /= nearClipPlane;
			Vector3 vector11 = vector9 - vector8 + vector7;
			vector11 /= nearClipPlane;
			Vector3 vector12 = vector9 + vector8 - vector7;
			vector12 /= nearClipPlane;
			Vector3 vector13 = vector9 - vector8 - vector7;
			vector13 /= nearClipPlane;
			identity2.SetRow(0, vector13);
			identity2.SetRow(1, vector11);
			identity2.SetRow(2, vector10);
			identity2.SetRow(3, vector12);
			EffectMaterial.SetMatrix("_FrustumCornersRay", identity2);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (EffectMaterial != null && source != null)
		{
			EffectMaterial.SetFloat("_FogDensity", density);
			EffectMaterial.SetFloat("_DensityPow", densityPow);
			EffectMaterial.SetColor("_FogColorBottom", colorBottom);
			EffectMaterial.SetColor("_FogColorTop", colorTop);
			EffectMaterial.SetFloat("_FogBottom", bottom);
			EffectMaterial.SetFloat("_FogTop", top);
			EffectMaterial.SetVector("_FogDirection", direction);
			EffectMaterial.SetFloat("_FogSpeed", speed);
			EffectMaterial.SetFloat("_FogScale", scale);
			EffectMaterial.SetFloat("_FogPow", pow);
			Resolution resolution = GetResolution();
			RenderTexture temporary = RenderTexture.GetTemporary(resolution.width, resolution.height, 0, source.format);
			Graphics.Blit(source, temporary, EffectMaterial, 0);
			EffectMaterial.SetTexture("_FogTex", temporary);
			Graphics.Blit(source, destination, EffectMaterial, 1);
			RenderTexture.ReleaseTemporary(temporary);
		}
		else
		{
			Graphics.Blit(source, destination);
		}
	}
}
