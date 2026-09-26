using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Spine;
using Spine.Unity;
using UnityEngine;

public static class zbSpineGpuAnimBaker
{
	private class UnionEntry
	{
		public int slotIndex;

		public Attachment attachment;

		public int vertexCount;

		public int startCol;

		public int entryIndex;

		public bool isAdditive;

		public float[] uvs;

		public Color32 color;

		public bool colorValid;
	}

	private class FrameSampler
	{
		private readonly SkeletonData m_data;

		private readonly List<UnionEntry> m_entries;

		private readonly Dictionary<(int slotIndex, Attachment attachment), UnionEntry> m_entryByKey;

		private readonly int m_unionVertexCount;

		private readonly SkeletonRendererInstruction m_instruction;

		private readonly MeshGenerator m_meshGenerator;

		private Skeleton m_skeleton;

		private AnimationState m_state;

		public FrameSampler(SkeletonData data, List<UnionEntry> entries, Dictionary<(int slotIndex, Attachment attachment), UnionEntry> entryByKey, int unionVertexCount)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			m_instruction = new SkeletonRendererInstruction();
			m_meshGenerator = new MeshGenerator();
			base._002Ector();
			m_data = data;
			m_entries = entries;
			m_entryByKey = entryByKey;
			m_unionVertexCount = unionVertexCount;
			m_meshGenerator.settings = new Settings
			{
				pmaVertexColors = true,
				zSpacing = 0f,
				useClipping = true,
				tintBlack = false,
				calculateTangents = false,
				addNormals = false
			};
		}

		public zbSpineGpuAnimBakedType.ClipInfo SampleClip(Animation animation, float fps, int preRollLoops, List<Color> rowColors, List<Color> rowEntryColors, out string failReason)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Expected O, but got Unknown
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Expected O, but got Unknown
			failReason = null;
			float dt = 1f / fps;
			int num = Mathf.Max(1, Mathf.CeilToInt(animation.Duration * fps));
			int num2 = num * Math.Max(0, preRollLoops);
			m_skeleton = new Skeleton(m_data);
			AnimationStateData val = new AnimationStateData(m_data)
			{
				DefaultMix = 0f
			};
			m_state = new AnimationState(val);
			TrackEntry val2 = m_state.SetAnimation(0, animation, true);
			int rowStart = rowColors.Count / m_unionVertexCount;
			Step(0f);
			for (int i = 0; i < num2; i++)
			{
				Step(dt);
			}
			val2.TrackTime = 0f;
			Step(0f);
			for (int j = 0; j < num; j++)
			{
				if (!RecordRow(rowColors, rowEntryColors, out failReason))
				{
					return default(zbSpineGpuAnimBakedType.ClipInfo);
				}
				Step(dt);
			}
			val2.Loop = false;
			val2.TrackTime = animation.Duration;
			Step(0f);
			if (!RecordRow(rowColors, rowEntryColors, out failReason))
			{
				return default(zbSpineGpuAnimBakedType.ClipInfo);
			}
			return new zbSpineGpuAnimBakedType.ClipInfo
			{
				rowStart = rowStart,
				frameCount = num + 1,
				duration = animation.Duration
			};
			void Step(float num3)
			{
				m_state.Update(num3);
				m_skeleton.Update(num3);
				m_state.Apply(m_skeleton);
				m_skeleton.UpdateWorldTransform((Physics)2);
			}
		}

		public zbSpineGpuAnimBakedType.ClipInfo SampleSetupPose(List<Color> rowColors, List<Color> rowEntryColors, out string failReason)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected O, but got Unknown
			failReason = null;
			m_skeleton = new Skeleton(m_data);
			m_skeleton.SetToSetupPose();
			m_skeleton.Update(0f);
			m_skeleton.UpdateWorldTransform((Physics)2);
			int rowStart = rowColors.Count / m_unionVertexCount;
			if (!RecordRow(rowColors, rowEntryColors, out failReason))
			{
				return default(zbSpineGpuAnimBakedType.ClipInfo);
			}
			return new zbSpineGpuAnimBakedType.ClipInfo
			{
				rowStart = rowStart,
				frameCount = 1,
				duration = 0f
			};
		}

		private bool RecordRow(List<Color> rowColors, List<Color> rowEntryColors, out string failReason)
		{
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0310: Unknown result type (might be due to invalid IL or missing references)
			//IL_0312: Unknown result type (might be due to invalid IL or missing references)
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0221: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_023a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Unknown result type (might be due to invalid IL or missing references)
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0272: Unknown result type (might be due to invalid IL or missing references)
			//IL_0277: Unknown result type (might be due to invalid IL or missing references)
			//IL_024a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0251: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
			failReason = null;
			Skeleton skeleton = m_skeleton;
			MeshGenerator.GenerateSingleSubmeshInstruction(m_instruction, skeleton, (Material)null);
			if (m_instruction.hasActiveClipping)
			{
				failReason = "clipping 裁剪附件激活";
				return false;
			}
			ExposedList<Slot> drawOrder = skeleton.DrawOrder;
			Slot[] items = drawOrder.Items;
			Slot[] items2 = skeleton.Slots.Items;
			int count = drawOrder.Count;
			for (int i = 0; i < count; i++)
			{
				if (items[i] != items2[i])
				{
					failReason = "DrawOrder 层级动画";
					return false;
				}
			}
			m_meshGenerator.Begin();
			m_meshGenerator.BuildMeshWithArrays(m_instruction, true);
			MeshGeneratorBuffers buffers = m_meshGenerator.Buffers;
			int count2 = rowColors.Count;
			for (int j = 0; j < m_unionVertexCount; j++)
			{
				rowColors.Add(Color.clear);
			}
			int count3 = rowEntryColors.Count;
			for (int k = 0; k < m_entries.Count; k++)
			{
				rowEntryColors.Add(Color.white);
			}
			Attachment[] items3 = m_instruction.attachments.Items;
			int num = 0;
			int vertexCount = buffers.vertexCount;
			for (int l = 0; l < count; l++)
			{
				Attachment val = items3[l];
				if (val == null)
				{
					continue;
				}
				int num2;
				if (val is RegionAttachment)
				{
					num2 = 4;
				}
				else
				{
					MeshAttachment val2 = (MeshAttachment)(object)((val is MeshAttachment) ? val : null);
					if (val2 == null)
					{
						if (!(val is ClippingAttachment))
						{
							failReason = "未知附件类型 " + ((object)val).GetType().Name;
							return false;
						}
						continue;
					}
					num2 = ((VertexAttachment)val2).WorldVerticesLength >> 1;
				}
				if (!m_entryByKey.TryGetValue((l, val), out var value))
				{
					failReason = "皮肤并集外附件（代码 SetAttachment 动态设置）";
					return false;
				}
				Vector3[] vertexBuffer = buffers.vertexBuffer;
				Vector2[] uvBuffer = buffers.uvBuffer;
				Color32[] colorBuffer = buffers.colorBuffer;
				Color32 val3 = default(Color32);
				for (int m = 0; m < num2; m++)
				{
					int num3 = num + m;
					Vector3 val4 = vertexBuffer[num3];
					rowColors[count2 + value.startCol + m] = new Color(val4.x, val4.y, 1f, 0f);
					if (m == 0)
					{
						val3 = colorBuffer[num3];
					}
					else
					{
						Color32 val5 = colorBuffer[num3];
						if (val5.r != val3.r || val5.g != val3.g || val5.b != val3.b || val5.a != val3.a)
						{
							failReason = "同帧 entry 内逐顶点颜色不一致（理论不可达——spine 颜色为 attachment 级）";
							return false;
						}
					}
					if (!value.colorValid)
					{
						value.color = colorBuffer[num3];
						value.uvs = GetEntryUvs(value, num3, uvBuffer);
						value.colorValid = true;
						continue;
					}
					Vector2 val6 = uvBuffer[num3];
					float num4 = value.uvs[m << 1];
					float num5 = value.uvs[(m << 1) + 1];
					if (Mathf.Abs(val6.x - num4) > 1E-06f || Mathf.Abs(val6.y - num5) > 1E-06f)
					{
						failReason = "附件 UV 逐帧变化（理论不可达）";
						return false;
					}
				}
				rowEntryColors[count3 + value.entryIndex] = Color32.op_Implicit(val3);
				num += num2;
				if (num > vertexCount)
				{
					failReason = "顶点游标错位";
					return false;
				}
			}
			if (num != vertexCount)
			{
				failReason = "顶点游标与缓冲总数不一致";
				return false;
			}
			return true;
		}

		private static float[] GetEntryUvs(UnionEntry entry, int srcIndex, Vector2[] uvBuffer)
		{
			float[] array = new float[entry.vertexCount << 1];
			for (int i = 0; i < entry.vertexCount; i++)
			{
				array[i << 1] = uvBuffer[srcIndex + i].x;
				array[(i << 1) + 1] = uvBuffer[srcIndex + i].y;
			}
			return array;
		}
	}

	private static readonly int[] kRegionTriangles = new int[6] { 0, 2, 1, 2, 3, 1 };

	public static zbSpineGpuAnimBakedType Bake(string spinePath, SkeletonDataAsset sda, float fps, int preRollLoops, out string excludeReason)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Invalid comparison between Unknown and I4
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		excludeReason = null;
		Stopwatch stopwatch = Stopwatch.StartNew();
		if ((Object)(object)sda == (Object)null || sda.atlasAssets == null || sda.atlasAssets.Length != 1)
		{
			excludeReason = "骨骼资产无效或非单图集";
			return null;
		}
		SkeletonData skeletonData = sda.GetSkeletonData(false);
		if (skeletonData == null)
		{
			excludeReason = "骨骼数据未解析";
			return null;
		}
		Skeleton val = new Skeleton(skeletonData);
		Skin val2 = ((val.Skin != null) ? val.Skin : skeletonData.DefaultSkin);
		if (val2 == null)
		{
			excludeReason = "无皮肤数据";
			return null;
		}
		List<UnionEntry> list = new List<UnionEntry>();
		Dictionary<(int, Attachment), UnionEntry> dictionary = new Dictionary<(int, Attachment), UnionEntry>();
		List<SkinEntry> list2 = new List<SkinEntry>();
		int num = 0;
		int count = skeletonData.Slots.Count;
		bool flag = false;
		if (sda.blendModeMaterials != null && sda.blendModeMaterials.RequiresBlendModeMaterials)
		{
			flag = sda.blendModeMaterials.applyAdditiveMaterial && sda.blendModeMaterials.additiveMaterials != null && sda.blendModeMaterials.additiveMaterials.Count > 0;
		}
		for (int i = 0; i < count; i++)
		{
			bool isAdditive = flag && (int)skeletonData.Slots.Items[i].BlendMode == 1;
			list2.Clear();
			val2.GetAttachments(i, list2);
			for (int j = 0; j < list2.Count; j++)
			{
				SkinEntry val3 = list2[j];
				Attachment attachment = ((SkinEntry)(ref val3)).Attachment;
				int attachmentVertexCount = GetAttachmentVertexCount(attachment);
				if (attachmentVertexCount > 0)
				{
					UnionEntry unionEntry = new UnionEntry
					{
						slotIndex = i,
						attachment = attachment,
						vertexCount = attachmentVertexCount,
						startCol = num,
						entryIndex = list.Count,
						isAdditive = isAdditive
					};
					list.Add(unionEntry);
					dictionary.Add((i, attachment), unionEntry);
					num += attachmentVertexCount;
				}
			}
		}
		if (num == 0)
		{
			excludeReason = "并集顶点数为 0";
			return null;
		}
		zbSpineGpuAnimBakedType zbSpineGpuAnimBakedType2 = new zbSpineGpuAnimBakedType
		{
			spinePath = spinePath,
			unionVertexCount = num,
			unionAttachmentCount = list.Count,
			fps = fps
		};
		List<Color> list3 = new List<Color>();
		List<Color> rowEntryColors = new List<Color>();
		FrameSampler frameSampler = new FrameSampler(skeletonData, list, dictionary, num);
		Enumerator<Animation> enumerator = skeletonData.Animations.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Animation current = enumerator.Current;
				zbSpineGpuAnimBakedType.ClipInfo value = BakeClip(frameSampler, current, fps, preRollLoops, list3, rowEntryColors, out var failReason);
				if (value.frameCount <= 0)
				{
					excludeReason = "clip[" + current.Name + "] 烘焙失败：" + failReason;
					return null;
				}
				zbSpineGpuAnimBakedType2.clips[current.Name] = value;
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		zbSpineGpuAnimBakedType.ClipInfo value2 = frameSampler.SampleSetupPose(list3, rowEntryColors, out var failReason2);
		if (value2.frameCount <= 0)
		{
			excludeReason = "<setup> 采样失败：" + failReason2;
			return null;
		}
		zbSpineGpuAnimBakedType2.clips["<setup>"] = value2;
		zbSpineGpuAnimBakedType2.totalFrames = list3.Count / num;
		zbSpineGpuAnimBakedType2.animTexture = CreateAnimTexture(num, zbSpineGpuAnimBakedType2.totalFrames, list3);
		if ((Object)(object)zbSpineGpuAnimBakedType2.animTexture == (Object)null)
		{
			excludeReason = "动画纹理创建失败";
			return null;
		}
		zbSpineGpuAnimBakedType2.colorTexture = CreateEntryColorTexture(list.Count, zbSpineGpuAnimBakedType2.totalFrames, rowEntryColors);
		if ((Object)(object)zbSpineGpuAnimBakedType2.colorTexture == (Object)null)
		{
			excludeReason = "颜色纹理创建失败";
			zbSpineGpuAnimBakedType2.Release();
			return null;
		}
		zbSpineGpuAnimBakedType2.unionMesh = BuildUnionMesh(spinePath, list, num, zbSpineGpuAnimBakedType2.totalFrames, zbSpineGpuAnimBakedType2);
		if ((Object)(object)zbSpineGpuAnimBakedType2.unionMesh == (Object)null)
		{
			excludeReason = "并集网格构建失败";
			zbSpineGpuAnimBakedType2.Release();
			return null;
		}
		stopwatch.Stop();
		zbSpineGpuAnimBakedType2.bakeMs = stopwatch.Elapsed.TotalMilliseconds;
		return zbSpineGpuAnimBakedType2;
	}

	private static zbSpineGpuAnimBakedType.ClipInfo BakeClip(FrameSampler sampler, Animation animation, float fps, int preRollLoops, List<Color> rowColors, List<Color> rowEntryColors, out string failReason)
	{
		return sampler.SampleClip(animation, fps, preRollLoops, rowColors, rowEntryColors, out failReason);
	}

	private static int GetAttachmentVertexCount(Attachment attachment)
	{
		if (attachment is RegionAttachment)
		{
			return 4;
		}
		MeshAttachment val = (MeshAttachment)(object)((attachment is MeshAttachment) ? attachment : null);
		if (val != null)
		{
			return ((VertexAttachment)val).WorldVerticesLength >> 1;
		}
		return 0;
	}

	private static Texture2D CreateAnimTexture(int width, int height, List<Color> rowColors)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		if (height <= 0)
		{
			return null;
		}
		Color[] pixels = rowColors.ToArray();
		rowColors.Clear();
		Texture2D val = new Texture2D(width, height, (TextureFormat)17, false, true)
		{
			name = "zbSpineGpuAnimTex",
			filterMode = (FilterMode)0,
			wrapMode = (TextureWrapMode)1
		};
		try
		{
			val.SetPixels(pixels);
			val.Apply(false, false);
			return val;
		}
		catch (Exception)
		{
			Object.Destroy((Object)(object)val);
			return null;
		}
	}

	private static Texture2D CreateEntryColorTexture(int entryCount, int frameCount, List<Color> rowEntryColors)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		if (entryCount <= 0 || frameCount <= 0)
		{
			return null;
		}
		Color[] pixels = rowEntryColors.ToArray();
		rowEntryColors.Clear();
		Texture2D val = new Texture2D(entryCount, frameCount, (TextureFormat)5, false, true)
		{
			name = "zbSpineGpuAnimColorTex",
			filterMode = (FilterMode)0,
			wrapMode = (TextureWrapMode)1
		};
		try
		{
			val.SetPixels(pixels);
			val.Apply(false, false);
			return val;
		}
		catch (Exception)
		{
			Object.Destroy((Object)(object)val);
			return null;
		}
	}

	private static Mesh BuildUnionMesh(string spinePath, List<UnionEntry> entries, int unionVertexCount, int totalFrames, zbSpineGpuAnimBakedType baked)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Expected O, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] collection = (Vector3[])(object)new Vector3[unionVertexCount];
		Vector2[] array = (Vector2[])(object)new Vector2[unionVertexCount];
		Vector3[] array2 = (Vector3[])(object)new Vector3[unionVertexCount];
		Vector2[] array3 = (Vector2[])(object)new Vector2[unionVertexCount];
		Color32[] array4 = (Color32[])(object)new Color32[unionVertexCount];
		List<int> list = new List<int>(unionVertexCount * 3);
		List<int> list2 = new List<int>(unionVertexCount);
		for (int i = 0; i < unionVertexCount; i++)
		{
			array4[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
		foreach (UnionEntry entry in entries)
		{
			float[] array5 = entry.uvs;
			if (array5 == null)
			{
				Attachment attachment = entry.attachment;
				RegionAttachment val = (RegionAttachment)(object)((attachment is RegionAttachment) ? attachment : null);
				if (val != null)
				{
					float[] uVs = val.UVs;
					array5 = new float[entry.vertexCount << 1];
					array5[0] = uVs[2];
					array5[1] = uVs[3];
					array5[2] = uVs[0];
					array5[3] = uVs[1];
					for (int j = 2; j < entry.vertexCount; j++)
					{
						array5[j << 1] = uVs[j << 1];
						array5[(j << 1) + 1] = uVs[(j << 1) + 1];
					}
				}
				else
				{
					array5 = ((MeshAttachment)entry.attachment).UVs;
				}
			}
			for (int k = 0; k < entry.vertexCount; k++)
			{
				array[entry.startCol + k] = new Vector2(array5[k << 1], array5[(k << 1) + 1]);
				array2[entry.startCol + k] = new Vector3((float)(entry.startCol + k), (float)unionVertexCount, (float)totalFrames);
				array3[entry.startCol + k] = new Vector2((float)entry.entryIndex, (float)entries.Count);
				if (entry.colorValid)
				{
					array4[entry.startCol + k] = entry.color;
				}
			}
			int[] array6 = ((entry.attachment is RegionAttachment) ? kRegionTriangles : ((MeshAttachment)entry.attachment).Triangles);
			List<int> list3 = (entry.isAdditive ? list2 : list);
			for (int l = 0; l < array6.Length; l++)
			{
				list3.Add(array6[l] + entry.startCol);
			}
		}
		Mesh val2 = new Mesh
		{
			name = "zbSpineGpuAnimMesh_" + Path.GetFileNameWithoutExtension(spinePath)
		};
		val2.SetVertices(new List<Vector3>(collection));
		val2.SetUVs(0, new List<Vector2>(array));
		val2.SetUVs(1, new List<Vector3>(array2));
		val2.SetUVs(2, new List<Vector2>(array3));
		val2.SetColors(new List<Color32>(array4));
		if (list2.Count > 0)
		{
			val2.subMeshCount = 2;
			val2.SetTriangles(list, 0);
			val2.SetTriangles(list2, 1);
		}
		else
		{
			val2.SetTriangles(list, 0);
		}
		baked.hasAdditiveSubmesh = list2.Count > 0;
		baked.normalTriangleCount = list.Count / 3;
		baked.additiveTriangleCount = list2.Count / 3;
		val2.bounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(200f, 200f, 200f));
		val2.UploadMeshData(false);
		return val2;
	}
}
