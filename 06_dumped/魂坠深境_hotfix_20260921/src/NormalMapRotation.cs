using System;
using System.Collections.Generic;
using System.IO;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class NormalMapRotation : MonoBehaviour
{
	public const float PI = 3.1415925f;

	private const string info = "1. 配置好Texture(原始Normal图)和SkeletonDataAsset\n2. 点击Setup生成新图\n3. 点击Save将图片保存到本地\n4. 需要点击Unity外部再点Unity才会刷新资源管理器\n\n注意：如果有多张图片，需要先配置SelectedName。\n先点击一次Setup，图片的名称会显示在TextureName列表中\n将选择图片的名字复制到SelectedName中再点击一次Setup即可正确生成\n";

	[SerializeField]
	public Image originTextureDisplay;

	[SerializeField]
	public Image targetTextureDisplay;

	[SerializeField]
	[Range(0f, 6.283185f)]
	public float radian;

	[SerializeField]
	[Range(0f, 360f)]
	public float angle;

	[SerializeField]
	public bool useRadian;

	[SerializeField]
	private Texture2D texture;

	[SerializeField]
	private SkeletonDataAsset skeletonDataAsset;

	[NonSerialized]
	public Texture2D newTexture;

	[NonSerialized]
	private string selectedName;

	[NonSerialized]
	private List<string> textureNames;

	public float finalRadian
	{
		get
		{
			if (!useRadian)
			{
				return angle / 180f * 3.1415925f;
			}
			return radian;
		}
	}

	private void Rotate(Texture2D src, Texture2D target, float radian)
	{
		List<string> list = textureNames;
		if (list != null && list.Count == 1)
		{
			selectedName = textureNames[0];
		}
		List<AtlasRegion> list2 = new List<AtlasRegion>();
		AtlasAssetBase[] atlasAssets = skeletonDataAsset.atlasAssets;
		for (int i = 0; i < atlasAssets.Length; i++)
		{
			foreach (AtlasRegion atla in atlasAssets[i].GetAtlas(false))
			{
				if (atla.degrees == 90 && atla.page.name == selectedName)
				{
					list2.AddUnique(atla);
				}
			}
		}
		int width = target.width;
		int height = target.height;
		Color[] pixels = src.GetPixels();
		for (int j = 0; j < width; j++)
		{
			for (int k = 0; k < height; k++)
			{
				float u = (float)j / (float)width;
				float v = (float)k / (float)height;
				if (UVInRegions(u, v, list2))
				{
					pixels[j + k * height] = NormalExtends.Rotate(pixels[j + k * height], radian);
				}
			}
		}
		target.SetPixels(pixels);
		target.Apply();
	}

	private void SaveTexture(Texture2D newTexture, string path)
	{
		byte[] bytes = ImageConversion.EncodeToPNG(newTexture);
		path = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + "/" + path;
		File.WriteAllBytes(path, bytes);
		Debug.Log(path);
	}

	private void SetupSkeletonData()
	{
		textureNames = new List<string>();
		AtlasAssetBase[] atlasAssets = skeletonDataAsset.atlasAssets;
		for (int i = 0; i < atlasAssets.Length; i++)
		{
			foreach (AtlasRegion atla in atlasAssets[i].GetAtlas(false))
			{
				textureNames.AddUnique(atla.page.name);
			}
		}
	}

	private bool UVInRegion(float u, float v, AtlasRegion region)
	{
		if (u < region.u || u > region.u2)
		{
			return false;
		}
		if (v < region.v2 || v > region.v)
		{
			return false;
		}
		return true;
	}

	private bool UVInRegions(float u, float v, List<AtlasRegion> regions)
	{
		foreach (AtlasRegion region in regions)
		{
			if (UVInRegion(u, v, region))
			{
				return true;
			}
		}
		return false;
	}

	public void Rotate()
	{
		if (!(texture == null))
		{
			newTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, mipChain: false);
			Rotate(texture, newTexture, finalRadian);
			targetTextureDisplay.sprite = Sprite.Create(newTexture, new Rect(0f, 0f, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f));
		}
	}
}
