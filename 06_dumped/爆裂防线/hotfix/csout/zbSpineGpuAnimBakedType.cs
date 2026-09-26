using System.Collections.Generic;
using UnityEngine;

public class zbSpineGpuAnimBakedType
{
	public struct ClipInfo
	{
		public int rowStart;

		public int frameCount;

		public float duration;
	}

	public const string kSetupPoseClipName = "<setup>";

	public string spinePath;

	public Texture2D animTexture;

	public Texture2D colorTexture;

	public Mesh unionMesh;

	public readonly Dictionary<string, ClipInfo> clips = new Dictionary<string, ClipInfo>();

	public int unionVertexCount;

	public int totalFrames;

	public int unionAttachmentCount;

	public float fps;

	public double bakeMs;

	public bool hasAdditiveSubmesh;

	public int normalTriangleCount;

	public int additiveTriangleCount;

	public long TextureBytes => (((Object)(object)animTexture != (Object)null) ? ((long)((Texture)animTexture).width * (long)((Texture)animTexture).height * 8) : 0) + (((Object)(object)colorTexture != (Object)null) ? ((long)((Texture)colorTexture).width * (long)((Texture)colorTexture).height * 4) : 0);

	public void Release()
	{
		if ((Object)(object)animTexture != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)animTexture);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)animTexture);
			}
			animTexture = null;
		}
		if ((Object)(object)colorTexture != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)colorTexture);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)colorTexture);
			}
			colorTexture = null;
		}
		if ((Object)(object)unionMesh != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)unionMesh);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)unionMesh);
			}
			unionMesh = null;
		}
	}
}
