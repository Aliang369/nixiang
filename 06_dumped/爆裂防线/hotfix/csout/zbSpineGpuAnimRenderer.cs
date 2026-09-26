using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public class zbSpineGpuAnimRenderer
{
	public class MegaBatch
	{
		public class TypeRecord
		{
			public zbSpineGpuAnimBakedType baked;

			public int typeIndex;

			public int vertexOffset;

			public MegaBatch mega;
		}

		public Mesh megaMesh;

		public Texture2DArray animArray;

		public Texture2DArray colorArray;

		public Material material;

		public Material materialAdditive;

		public readonly Dictionary<string, TypeRecord> typeByPath = new Dictionary<string, TypeRecord>();

		public int normalTriangleCount;

		public int additiveTriangleCount;

		public void Release()
		{
			if ((Object)(object)megaMesh != (Object)null)
			{
				if (Application.isPlaying)
				{
					Object.Destroy((Object)(object)megaMesh);
				}
				else
				{
					Object.DestroyImmediate((Object)(object)megaMesh);
				}
				megaMesh = null;
			}
			if ((Object)(object)animArray != (Object)null)
			{
				if (Application.isPlaying)
				{
					Object.Destroy((Object)(object)animArray);
				}
				else
				{
					Object.DestroyImmediate((Object)(object)animArray);
				}
				animArray = null;
			}
			if ((Object)(object)colorArray != (Object)null)
			{
				if (Application.isPlaying)
				{
					Object.Destroy((Object)(object)colorArray);
				}
				else
				{
					Object.DestroyImmediate((Object)(object)colorArray);
				}
				colorArray = null;
			}
			if ((Object)(object)material != (Object)null)
			{
				if (Application.isPlaying)
				{
					Object.Destroy((Object)(object)material);
				}
				else
				{
					Object.DestroyImmediate((Object)(object)material);
				}
				material = null;
			}
			if ((Object)(object)materialAdditive != (Object)null)
			{
				if (Application.isPlaying)
				{
					Object.Destroy((Object)(object)materialAdditive);
				}
				else
				{
					Object.DestroyImmediate((Object)(object)materialAdditive);
				}
				materialAdditive = null;
			}
			typeByPath.Clear();
		}
	}

	private const int kMaxInstancesPerDraw = 64;

	private const int kMonsterLayer = 0;

	private const string kGpuAnimShaderName = "Custom/SpineSkeleton_URP_GPUAnim";

	private const string kCounterInstances = "GPU动画实例数";

	private const string kCounterDraws = "GPU动画draw次数";

	private const string kCounterBuildMicros = "GPU动画聚合us";

	private const string kCounterBuildException = "GPU动画Build异常次数";

	private const string kCounterEffectChunks = "GPU特效chunk数";

	private static readonly int kIdFrameA0 = Shader.PropertyToID("_InstFrameA0");

	private static readonly int kIdFrameA1 = Shader.PropertyToID("_InstFrameA1");

	private static readonly int kIdBlendA = Shader.PropertyToID("_InstBlendA");

	private static readonly int kIdFrameB0 = Shader.PropertyToID("_InstFrameB0");

	private static readonly int kIdFrameB1 = Shader.PropertyToID("_InstFrameB1");

	private static readonly int kIdBlendB = Shader.PropertyToID("_InstBlendB");

	private static readonly int kIdCross = Shader.PropertyToID("_InstCross");

	private static readonly int kIdTint = Shader.PropertyToID("_InstTint");

	private static readonly int kIdMatRow0 = Shader.PropertyToID("_InstMatRow0");

	private static readonly int kIdMatRow1 = Shader.PropertyToID("_InstMatRow1");

	private static readonly int kIdMatRow2 = Shader.PropertyToID("_InstMatRow2");

	private static readonly int kIdTypeIndex = Shader.PropertyToID("_InstTypeIndex");

	private static readonly int kIdAnimTex = Shader.PropertyToID("_AnimTex");

	private static readonly int kIdMainTex = Shader.PropertyToID("_MainTex");

	private static readonly int kIdColorTex = Shader.PropertyToID("_ColorTex");

	private static readonly int kIdAnimTexSize = Shader.PropertyToID("_AnimTexSize");

	private static readonly int kIdDeployVer = Shader.PropertyToID("_DeployVer");

	private static readonly int kIdSrcBlend = Shader.PropertyToID("_SrcBlend");

	private static readonly int kIdDstBlend = Shader.PropertyToID("_DstBlend");

	private static readonly Comparison<int> kOrderComparison = CompareOrder;

	private static float[] s_orderKeys;

	private static readonly Comparison<float> kOldPathZComparison = CompareOldPathZ;

	private readonly GameObject m_root;

	private readonly zbSpineGpuAnimRendererHost m_host;

	private readonly zbCombatMonster m_monsters;

	private readonly zbCombatData m_combatData;

	private Camera m_camera;

	private bool m_valid;

	private readonly List<zbSpineGpuAnimPlayer> m_players;

	private zbSpineGpuAnimPlayer[] m_collectPlayers;

	private Matrix4x4[] m_matrices;

	private float[] m_sortKeys;

	private readonly List<int> m_order;

	private readonly List<float> m_oldPathZ;

	private MaterialPropertyBlock m_mpb;

	private int m_mpbArrayLen;

	private readonly Vector4[] m_instMatRow0;

	private readonly Vector4[] m_instMatRow1;

	private readonly Vector4[] m_instMatRow2;

	private readonly float[] m_instFrameA0;

	private readonly float[] m_instFrameA1;

	private readonly float[] m_instBlendA;

	private readonly float[] m_instFrameB0;

	private readonly float[] m_instFrameB1;

	private readonly float[] m_instBlendB;

	private readonly float[] m_instCross;

	private readonly Vector4[] m_instTint;

	private readonly float[] m_instTypeIndex;

	private static Shader s_gpuShader;

	private static bool s_gpuShaderResolved;

	private static bool s_deployMarkerLogged;

	private const string kGpuAnimAdditiveShaderName = "Custom/SpineSkeleton_URP_GPUAnim_Additive";

	private const string kGpuAnimAdditiveShaderPath = "effect/shader/SpineSkeletonURP_GPUAnimAdditive.shader";

	private static Shader s_gpuAdditiveShader;

	private static bool s_gpuAdditiveShaderResolved;

	private static Material s_templateMaterial;

	private static bool s_templateResolved;

	private const string kTemplateMaterialPath = "effect/material/mt_spine_gpu_anim.mat";

	public int PlayerCount => m_players.Count;

	public int LastInstanceCount { get; private set; }

	public int LastDrawCount { get; private set; }

	public double LastBuildMs { get; private set; }

	public bool isValid => m_valid;

	public static MegaBatch TryBuildMegaBatch(Dictionary<string, zbSpineGpuAnimBakedType> bakedGroup, Material templateMaterial, Material additiveTemplateMaterial)
	{
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected O, but got Unknown
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Expected O, but got Unknown
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Expected O, but got Unknown
		//IL_0526: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		if (bakedGroup == null || bakedGroup.Count == 0 || (Object)(object)templateMaterial == (Object)null)
		{
			return null;
		}
		List<string> list = new List<string>(bakedGroup.Keys);
		list.Sort();
		List<zbSpineGpuAnimBakedType> list2 = new List<zbSpineGpuAnimBakedType>(list.Count);
		foreach (string item in list)
		{
			zbSpineGpuAnimBakedType zbSpineGpuAnimBakedType2 = bakedGroup[item];
			if (zbSpineGpuAnimBakedType2 != null && (Object)(object)zbSpineGpuAnimBakedType2.animTexture != (Object)null && (Object)(object)zbSpineGpuAnimBakedType2.colorTexture != (Object)null && (Object)(object)zbSpineGpuAnimBakedType2.unionMesh != (Object)null)
			{
				list2.Add(zbSpineGpuAnimBakedType2);
			}
		}
		if (list2.Count == 0)
		{
			return null;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (zbSpineGpuAnimBakedType item2 in list2)
		{
			num = Math.Max(num, item2.unionVertexCount);
			num2 = Math.Max(num2, item2.totalFrames);
			num3 = Math.Max(num3, item2.unionAttachmentCount);
		}
		if (num <= 0 || num2 <= 0 || num3 <= 0 || num > 8192 || num2 > 8192)
		{
			zbPerfLog.LogWarning($"[zbSpineGpuAnimRenderer] mega 批次维度异常（{num}x{num2}x{num3}），本场走原渲染");
			return null;
		}
		Texture2DArray val;
		Texture2DArray val2;
		try
		{
			val = new Texture2DArray(num, num2, list2.Count, (TextureFormat)17, false, true)
			{
				name = "zbSpineGpuAnimTexArray",
				filterMode = (FilterMode)0,
				wrapMode = (TextureWrapMode)1
			};
			val2 = new Texture2DArray(num3, num2, list2.Count, (TextureFormat)5, false, true)
			{
				name = "zbSpineGpuAnimColorTexArray",
				filterMode = (FilterMode)0,
				wrapMode = (TextureWrapMode)1
			};
		}
		catch (Exception ex)
		{
			zbPerfLog.LogWarning("[zbSpineGpuAnimRenderer] 纹理数组创建失败（" + ex.Message + "），本场走原渲染");
			return null;
		}
		MegaBatch megaBatch = new MegaBatch
		{
			animArray = val,
			colorArray = val2
		};
		List<Vector3> list3 = new List<Vector3>(list2.Count * 512);
		List<Vector2> list4 = new List<Vector2>(list2.Count * 512);
		List<Vector4> list5 = new List<Vector4>(list2.Count * 512);
		List<Vector2> list6 = new List<Vector2>(list2.Count * 512);
		List<Color32> list7 = new List<Color32>(list2.Count * 512);
		List<int> list8 = new List<int>(list2.Count * 1536);
		List<int> list9 = new List<int>(list2.Count * 256);
		List<Vector2> list10 = new List<Vector2>();
		List<Vector2> list11 = new List<Vector2>();
		for (int i = 0; i < list2.Count; i++)
		{
			zbSpineGpuAnimBakedType zbSpineGpuAnimBakedType3 = list2[i];
			int count = list3.Count;
			if (!CopyTextureRegion(zbSpineGpuAnimBakedType3.animTexture, val, i))
			{
				CpuCopySlice(zbSpineGpuAnimBakedType3.animTexture, val, i);
			}
			if (!CopyTextureRegion(zbSpineGpuAnimBakedType3.colorTexture, val2, i))
			{
				CpuCopySlice(zbSpineGpuAnimBakedType3.colorTexture, val2, i);
			}
			Mesh unionMesh = zbSpineGpuAnimBakedType3.unionMesh;
			list10.Clear();
			unionMesh.GetUVs(0, list10);
			list11.Clear();
			unionMesh.GetUVs(2, list11);
			Color32[] colors = unionMesh.colors32;
			int vertexCount = unionMesh.vertexCount;
			for (int j = 0; j < vertexCount; j++)
			{
				list3.Add(Vector3.zero);
				list4.Add((j < list10.Count) ? list10[j] : Vector2.zero);
				list5.Add(new Vector4((float)j, (float)num, (float)num2, (float)i));
				list6.Add((j < list11.Count) ? new Vector2(list11[j].x, (float)num3) : new Vector2(0f, (float)num3));
				list7.Add((Color32)((j < colors.Length) ? colors[j] : new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)));
			}
			int[] triangles = unionMesh.GetTriangles(0);
			for (int k = 0; k < triangles.Length; k++)
			{
				list8.Add(triangles[k] + count);
			}
			if (zbSpineGpuAnimBakedType3.hasAdditiveSubmesh)
			{
				int[] triangles2 = unionMesh.GetTriangles(1);
				for (int l = 0; l < triangles2.Length; l++)
				{
					list9.Add(triangles2[l] + count);
				}
			}
			megaBatch.typeByPath[zbSpineGpuAnimBakedType3.spinePath] = new MegaBatch.TypeRecord
			{
				baked = zbSpineGpuAnimBakedType3,
				typeIndex = i,
				vertexOffset = count,
				mega = megaBatch
			};
		}
		Mesh val3 = new Mesh
		{
			name = "zbSpineGpuAnimMegaMesh",
			indexFormat = (IndexFormat)1
		};
		val3.SetVertices(list3);
		val3.SetUVs(0, list4);
		val3.SetUVs(1, list5);
		val3.SetUVs(2, list6);
		val3.SetColors(list7);
		if (list9.Count > 0)
		{
			val3.subMeshCount = 2;
			val3.SetTriangles(list8, 0);
			val3.SetTriangles(list9, 1);
		}
		else
		{
			val3.SetTriangles(list8, 0);
		}
		megaBatch.normalTriangleCount = list8.Count / 3;
		megaBatch.additiveTriangleCount = list9.Count / 3;
		val3.bounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(200f, 200f, 200f));
		val3.UploadMeshData(false);
		megaBatch.megaMesh = val3;
		megaBatch.material = CreateMegaGpuMaterial(templateMaterial, val, val2, "");
		if ((Object)(object)megaBatch.material == (Object)null)
		{
			megaBatch.Release();
			return null;
		}
		bool flag = false;
		for (int m = 0; m < list2.Count; m++)
		{
			if (list2[m].hasAdditiveSubmesh && list2[m].additiveTriangleCount > 0)
			{
				flag = true;
				break;
			}
		}
		if (flag && (Object)(object)additiveTemplateMaterial != (Object)null)
		{
			megaBatch.materialAdditive = CreateMegaGpuMaterial(additiveTemplateMaterial, val, val2, "_Additive");
		}
		return megaBatch;
	}

	private static bool CopyTextureRegion(Texture2D src, Texture2DArray dst, int slice)
	{
		try
		{
			Graphics.CopyTexture((Texture)(object)src, 0, 0, 0, 0, ((Texture)src).width, ((Texture)src).height, (Texture)(object)dst, slice, 0, 0, 0);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static void CpuCopySlice(Texture2D src, Texture2DArray dst, int slice)
	{
		Color[] pixels = src.GetPixels();
		Color[] array = (Color[])(object)new Color[((Texture)dst).width * ((Texture)dst).height];
		for (int i = 0; i < ((Texture)src).height; i++)
		{
			Array.Copy(pixels, i * ((Texture)src).width, array, i * ((Texture)dst).width, ((Texture)src).width);
		}
		dst.SetPixels(array, slice);
		dst.Apply(false, false);
	}

	private static Material CreateMegaGpuMaterial(Material templateMaterial, Texture2DArray animArray, Texture2DArray colorArray, string suffix)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		Shader shader = null;
		bool flag = suffix == "_Additive" && ResolveGpuAdditiveShader(out shader);
		Material val;
		Shader shader2;
		if (flag)
		{
			val = new Material(templateMaterial)
			{
				name = "zbSpineGpuAnim_MegaMaterial_" + suffix,
				shader = shader
			};
		}
		else if (ResolveGpuShader(out shader2))
		{
			val = new Material(templateMaterial)
			{
				name = "zbSpineGpuAnim_MegaMaterial_" + suffix,
				shader = shader2
			};
		}
		else
		{
			Material val2 = ResolveTemplateMaterial();
			if ((Object)(object)val2 == (Object)null)
			{
				zbPerfLog.LogWarning("[zbSpineGpuAnimRenderer] GPU 动画 shader 与模板材质均不可用（真机需 mt_spine_gpu_anim.mat 纳入 AB），本场走原渲染");
				return null;
			}
			val = new Material(val2)
			{
				name = "zbSpineGpuAnim_MegaMaterial_" + suffix
			};
			if ((Object)(object)val.shader == (Object)null || ((Object)val.shader).name != "Custom/SpineSkeleton_URP_GPUAnim")
			{
				Shader shader3 = val.shader;
				zbPerfLog.LogWarning("[zbSpineGpuAnimRenderer] 模板材质 effect/material/mt_spine_gpu_anim.mat 未绑定 Custom/SpineSkeleton_URP_GPUAnim（实际=" + ((shader3 != null) ? ((Object)shader3).name : null) + "）——克隆它会导致 GPU 实例全部隐形，本场走原渲染。请在编辑器改绑该 shader 后重打 AB");
				Object.Destroy((Object)(object)val);
				return null;
			}
			val.SetTexture(kIdMainTex, templateMaterial.GetTexture(kIdMainTex));
		}
		val.SetTexture(kIdAnimTex, (Texture)(object)animArray);
		val.SetTexture(kIdColorTex, (Texture)(object)colorArray);
		val.SetVector(kIdAnimTexSize, new Vector4((float)((Texture)animArray).width, (float)((Texture)animArray).height, 0f, 0f));
		if (suffix == "_Additive" && !flag)
		{
			val.SetFloat(kIdSrcBlend, 1f);
			val.SetFloat(kIdDstBlend, 1f);
		}
		return val;
	}

	private static int CompareOrder(int a, int b)
	{
		return s_orderKeys[b].CompareTo(s_orderKeys[a]);
	}

	private static int CompareOldPathZ(float a, float b)
	{
		return b.CompareTo(a);
	}

	private static void LogGpuEnvironmentOnce()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			zbPerfLog.Log("[zbSpineGpuAnimRenderer] GPU环境诊断：机型=" + SystemInfo.deviceModel + $" | 图形API={SystemInfo.graphicsDeviceType} GPU={SystemInfo.graphicsDeviceName} 厂商={SystemInfo.graphicsDeviceVendor}" + $" maxTex={SystemInfo.maxTextureSize}" + $" | 能力位：实例化={SystemInfo.supportsInstancing} RGBAHalf采样={SystemInfo.SupportsTextureFormat((TextureFormat)17)}" + $" ARGBHalfRT={SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)2)}");
		}
		catch (Exception ex)
		{
			zbPerfLog.LogWarning("[zbSpineGpuAnimRenderer] GPU环境诊断异常（忽略）：" + ex.GetType().Name + " " + ex.Message);
		}
	}

	public zbSpineGpuAnimRenderer(Transform parent, Camera camera, zbCombatMonster monsters, zbCombatData combatData, string name)
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		m_valid = true;
		m_players = new List<zbSpineGpuAnimPlayer>(128);
		m_collectPlayers = new zbSpineGpuAnimPlayer[64];
		m_matrices = (Matrix4x4[])(object)new Matrix4x4[64];
		m_sortKeys = new float[64];
		m_order = new List<int>(64);
		m_oldPathZ = new List<float>(32);
		m_mpbArrayLen = -1;
		m_instMatRow0 = (Vector4[])(object)new Vector4[64];
		m_instMatRow1 = (Vector4[])(object)new Vector4[64];
		m_instMatRow2 = (Vector4[])(object)new Vector4[64];
		m_instFrameA0 = new float[64];
		m_instFrameA1 = new float[64];
		m_instBlendA = new float[64];
		m_instFrameB0 = new float[64];
		m_instFrameB1 = new float[64];
		m_instBlendB = new float[64];
		m_instCross = new float[64];
		m_instTint = (Vector4[])(object)new Vector4[64];
		m_instTypeIndex = new float[64];
		base._002Ector();
		m_root = new GameObject(name);
		m_root.transform.SetParent(parent, false);
		m_camera = camera;
		m_monsters = monsters;
		m_combatData = combatData;
		m_host = m_root.AddComponent<zbSpineGpuAnimRendererHost>();
		m_host.owner = this;
	}

	public void SetCamera(Camera camera)
	{
		m_camera = camera;
	}

	public void Register(zbSpineGpuAnimPlayer player)
	{
		if (m_valid && player != null && !m_players.Contains(player))
		{
			m_players.Add(player);
		}
	}

	public void Unregister(zbSpineGpuAnimPlayer player)
	{
		m_players.Remove(player);
	}

	public void Free()
	{
		if (m_valid)
		{
			m_valid = false;
			m_players.Clear();
			Object.Destroy((Object)(object)m_root);
		}
	}

	public void Build()
	{
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Expected O, but got Unknown
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Unknown result type (might be due to invalid IL or missing references)
		//IL_050d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0519: Unknown result type (might be due to invalid IL or missing references)
		//IL_0520: Unknown result type (might be due to invalid IL or missing references)
		//IL_0525: Unknown result type (might be due to invalid IL or missing references)
		//IL_0541: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_054d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0726: Unknown result type (might be due to invalid IL or missing references)
		//IL_075a: Unknown result type (might be due to invalid IL or missing references)
		if (!m_valid)
		{
			return;
		}
		double num = Time.realtimeSinceStartup;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		MegaBatch activeMegaBatch = zbSpineGpuAnimRuntime.ActiveMegaBatch;
		MegaBatch activeEffectMegaBatch = zbSpineGpuAnimRuntime.ActiveEffectMegaBatch;
		bool flag = activeMegaBatch != null && (Object)(object)activeMegaBatch.megaMesh != (Object)null && (Object)(object)activeMegaBatch.material != (Object)null;
		bool flag2 = activeEffectMegaBatch != null && (Object)(object)activeEffectMegaBatch.megaMesh != (Object)null && (Object)(object)activeEffectMegaBatch.material != (Object)null;
		if (!flag && !flag2)
		{
			LastBuildMs = ((double)Time.realtimeSinceStartup - num) * 1000.0;
			LastInstanceCount = 0;
			LastDrawCount = 0;
			zbPerfLog.Set("GPU动画实例数", 0L);
			zbPerfLog.Set("GPU动画draw次数", 0L);
			zbPerfLog.Set("GPU特效chunk数", 0L);
			return;
		}
		if ((Object)(object)m_camera == (Object)null || !((Behaviour)m_camera).isActiveAndEnabled)
		{
			LastBuildMs = ((double)Time.realtimeSinceStartup - num) * 1000.0;
			LastInstanceCount = 0;
			LastDrawCount = 0;
			zbPerfLog.Set("GPU动画实例数", 0L);
			zbPerfLog.Set("GPU动画draw次数", 0L);
			zbPerfLog.Set("GPU特效chunk数", 0L);
			return;
		}
		int num5 = 0;
		for (int i = 0; i < m_players.Count; i++)
		{
			zbSpineGpuAnimPlayer zbSpineGpuAnimPlayer2 = m_players[i];
			if (zbSpineGpuAnimPlayer2.ShouldDrawInstance())
			{
				zbSpineGpuAnimPlayer2.GetRenderInfo(out var matrix, out var _);
				EnsureCapacity(num5 + 1);
				m_collectPlayers[num5] = zbSpineGpuAnimPlayer2;
				m_matrices[num5] = matrix;
				m_sortKeys[num5] = matrix.m23;
				num5++;
			}
		}
		if (num5 > 0)
		{
			m_order.Clear();
			for (int j = 0; j < num5; j++)
			{
				m_order.Add(j);
			}
			s_orderKeys = m_sortKeys;
			m_order.Sort(kOrderComparison);
			TrimBuffers(num5);
			if (m_mpbArrayLen != 64)
			{
				m_mpb = new MaterialPropertyBlock();
				m_mpbArrayLen = 64;
			}
			CollectOldPathZ(m_oldPathZ);
			int num6 = 0;
			int k = 0;
			for (float num7 = m_sortKeys[m_order[0]]; k < m_oldPathZ.Count && m_oldPathZ[k] > num7; k++)
			{
			}
			Bounds val = default(Bounds);
			while (num6 < num5)
			{
				MegaBatch megaBatch = null;
				zbSpineGpuAnimPlayer zbSpineGpuAnimPlayer3 = m_collectPlayers[m_order[num6]];
				if (zbSpineGpuAnimPlayer3.attachedType != null)
				{
					megaBatch = zbSpineGpuAnimPlayer3.attachedType.mega;
				}
				bool flag3 = megaBatch != null && (Object)(object)megaBatch.megaMesh != (Object)null && (Object)(object)megaBatch.material != (Object)null;
				int num8 = Math.Min(num6 + 64, num5);
				float num9 = m_sortKeys[m_order[num6]];
				int l;
				for (l = num6 + 1; l < num8; l++)
				{
					zbSpineGpuAnimPlayer zbSpineGpuAnimPlayer4 = m_collectPlayers[m_order[l]];
					if (((zbSpineGpuAnimPlayer4.attachedType != null) ? zbSpineGpuAnimPlayer4.attachedType.mega : null) != megaBatch)
					{
						break;
					}
					float num10 = m_sortKeys[m_order[l]];
					if (k < m_oldPathZ.Count && m_oldPathZ[k] > num10 && m_oldPathZ[k] < num9)
					{
						break;
					}
				}
				int num11 = l - num6;
				if (megaBatch == activeEffectMegaBatch)
				{
					num4++;
				}
				bool flag4 = flag3 && megaBatch.normalTriangleCount > 0;
				bool flag5 = flag3 && (Object)(object)megaBatch.materialAdditive != (Object)null && megaBatch.additiveTriangleCount > 0;
				float num12 = float.MaxValue;
				float num13 = float.MinValue;
				bool flag6 = false;
				for (int m = 0; m < num11; m++)
				{
					int num14 = m_order[num6 + m];
					zbSpineGpuAnimPlayer zbSpineGpuAnimPlayer5 = m_collectPlayers[num14];
					float num15 = m_sortKeys[num14];
					if (num15 < num12)
					{
						num12 = num15;
					}
					if (num15 > num13)
					{
						num13 = num15;
					}
					if (zbSpineGpuAnimPlayer5.attachedType != null && zbSpineGpuAnimPlayer5.attachedType.baked != null && zbSpineGpuAnimPlayer5.attachedType.baked.hasAdditiveSubmesh && zbSpineGpuAnimPlayer5.attachedType.baked.additiveTriangleCount > 0)
					{
						flag6 = true;
					}
				}
				Vector3 position = ((Component)m_camera).transform.position;
				position.z = (num12 + num13) * 0.5f;
				((Bounds)(ref val))._002Ector(position, new Vector3(500f, 500f, 200f));
				for (int n = 0; n < num11; n++)
				{
					int num16 = m_order[num6 + n];
					zbSpineGpuAnimPlayer zbSpineGpuAnimPlayer6 = m_collectPlayers[num16];
					Matrix4x4 val2 = m_matrices[num16];
					m_instMatRow0[n] = new Vector4(val2.m00, val2.m01, 0f, val2.m03);
					m_instMatRow1[n] = new Vector4(val2.m10, val2.m11, 0f, val2.m13);
					m_instMatRow2[n] = new Vector4(0f, 0f, 1f, val2.m23);
					m_instFrameA0[n] = zbSpineGpuAnimPlayer6.frameA0;
					m_instFrameA1[n] = zbSpineGpuAnimPlayer6.frameA1;
					m_instBlendA[n] = zbSpineGpuAnimPlayer6.blendA;
					m_instFrameB0[n] = zbSpineGpuAnimPlayer6.frameB0;
					m_instFrameB1[n] = zbSpineGpuAnimPlayer6.frameB1;
					m_instBlendB[n] = zbSpineGpuAnimPlayer6.blendB;
					m_instCross[n] = zbSpineGpuAnimPlayer6.crossWeight;
					m_instTint[n] = zbSpineGpuAnimPlayer6.tintVector;
					m_instTypeIndex[n] = ((zbSpineGpuAnimPlayer6.attachedType != null) ? ((float)zbSpineGpuAnimPlayer6.attachedType.typeIndex) : 0f);
				}
				m_mpb.SetVectorArray(kIdMatRow0, m_instMatRow0);
				m_mpb.SetVectorArray(kIdMatRow1, m_instMatRow1);
				m_mpb.SetVectorArray(kIdMatRow2, m_instMatRow2);
				m_mpb.SetFloatArray(kIdFrameA0, m_instFrameA0);
				m_mpb.SetFloatArray(kIdFrameA1, m_instFrameA1);
				m_mpb.SetFloatArray(kIdBlendA, m_instBlendA);
				m_mpb.SetFloatArray(kIdFrameB0, m_instFrameB0);
				m_mpb.SetFloatArray(kIdFrameB1, m_instFrameB1);
				m_mpb.SetFloatArray(kIdBlendB, m_instBlendB);
				m_mpb.SetFloatArray(kIdCross, m_instCross);
				m_mpb.SetVectorArray(kIdTint, m_instTint);
				m_mpb.SetFloatArray(kIdTypeIndex, m_instTypeIndex);
				if (flag4)
				{
					Graphics.DrawMeshInstancedProcedural(megaBatch.megaMesh, 0, megaBatch.material, val, num11, m_mpb, (ShadowCastingMode)0, false, 0, m_camera, (LightProbeUsage)1, (LightProbeProxyVolume)null);
					num3++;
				}
				if (flag5 & flag6)
				{
					Graphics.DrawMeshInstancedProcedural(megaBatch.megaMesh, 1, megaBatch.materialAdditive, val, num11, m_mpb, (ShadowCastingMode)0, false, 0, m_camera, (LightProbeUsage)1, (LightProbeProxyVolume)null);
					num3++;
				}
				num6 = l;
				for (float num17 = m_sortKeys[m_order[l - 1]]; k < m_oldPathZ.Count && m_oldPathZ[k] >= num17; k++)
				{
				}
			}
			num2 = num5;
			if (!s_deployMarkerLogged)
			{
				s_deployMarkerLogged = true;
				MegaBatch megaBatch2 = (flag ? activeMegaBatch : activeEffectMegaBatch);
				Material val3 = activeEffectMegaBatch?.materialAdditive;
				zbPerfLog.Log($"[zbSpineGpuAnimRenderer] 部署标记 v0918c（0917 方案C mega 单批 + chunk 级真实键 + 旧路径边界对齐；0918 CrossFade 权重极性/附件切换窗口修复 + 双 mega 单排序/弹幕特效接入 + 叠加混合属性驱动修复）；shader版本标记={megaBatch2.material.GetFloat(kIdDeployVer):0.0}" + "；叠加shader=" + (((Object)(object)val3 != (Object)null) ? $"{((Object)val3.shader).name}@{val3.GetFloat(kIdDeployVer):0.0}" : "无（本场无叠加型）") + "（923.0=回退固定混合版（黑边根因修复），920.0=叠加固定混合变体，919.0=属性驱动混合（Android Vulkan 已证不可用），918.0=CrossFade 修复版，更旧值=AB 未更新）");
				LogGpuEnvironmentOnce();
			}
		}
		LastBuildMs = ((double)Time.realtimeSinceStartup - num) * 1000.0;
		LastInstanceCount = num2;
		LastDrawCount = num3;
		zbPerfLog.Set("GPU动画实例数", num2);
		zbPerfLog.Set("GPU动画draw次数", num3);
		zbPerfLog.Set("GPU特效chunk数", num4);
		zbPerfLog.Inc("GPU动画聚合us", (long)(LastBuildMs * 1000.0));
	}

	private void CollectOldPathZ(List<float> results)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		results.Clear();
		Dictionary<int, zbMonster> dictionary = ((m_monsters != null) ? m_monsters.monsterList : null);
		if (dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<int, zbMonster> item in dictionary)
		{
			zbMonster value = item.Value;
			if ((Object)(object)value == (Object)null || value.isGoDestroyed || (value.m_spineGpuTakenOver && !value.isUsingSprite))
			{
				continue;
			}
			Transform transform;
			if (value.isUsingSprite)
			{
				transform = ((Component)value).transform;
			}
			else
			{
				if (!((Object)(object)value.m_skeletonRenderer != (Object)null))
				{
					continue;
				}
				transform = ((Component)value.m_skeletonRenderer).transform;
			}
			if (((Component)transform).gameObject.activeInHierarchy)
			{
				results.Add(transform.position.z);
			}
		}
		if (m_combatData != null)
		{
			if (m_combatData.monsterBulletEffectPoolMgr != null)
			{
				m_combatData.monsterBulletEffectPoolMgr.CollectActiveSpineZ(results);
			}
			if (m_combatData.skillEffectPoolMgr != null)
			{
				m_combatData.skillEffectPoolMgr.CollectActiveSpineZ(results);
			}
			if (m_combatData.monsterOnceEffectPoolMgr != null)
			{
				m_combatData.monsterOnceEffectPoolMgr.CollectActiveSpineZ(results);
			}
			if (m_combatData.monsterEffectPoolMgr != null)
			{
				m_combatData.monsterEffectPoolMgr.CollectActiveSpineZ(results);
			}
			if (m_combatData.buffEffectPoolMgr != null)
			{
				m_combatData.buffEffectPoolMgr.CollectActiveSpineZ(results);
			}
			if (m_combatData.skillPoolMgr != null)
			{
				m_combatData.skillPoolMgr.CollectActiveSpineZ(results);
			}
			if (m_combatData.monsterEyeEffectPoolMgr != null)
			{
				m_combatData.monsterEyeEffectPoolMgr.CollectActiveSpineZ(results);
			}
		}
		results.Sort(kOldPathZComparison);
	}

	private void TrimBuffers(int count)
	{
		int num = m_matrices.Length;
		if (count * 4 < num && num > 16)
		{
			int num2;
			for (num2 = 16; num2 < count; num2 *= 2)
			{
			}
			if (num2 < num)
			{
				Array.Resize(ref m_collectPlayers, num2);
				Array.Resize(ref m_matrices, num2);
				Array.Resize(ref m_sortKeys, num2);
			}
		}
	}

	public void OnBuildException(Exception exception)
	{
		zbPerfLog.Inc("GPU动画Build异常次数", 1L);
		string stackTrace = exception.StackTrace;
		int num = stackTrace?.IndexOf("gamescript", StringComparison.Ordinal) ?? (-1);
		string text = ((num >= 0) ? stackTrace.Substring(num, Math.Min(300, stackTrace.Length - num)) : stackTrace);
		zbPerfLog.LogError(string.Format("[zbSpineGpuAnimRenderer] Build 异常：{0}: {1} | 上帧实例={2} draw={3} | {4}", new object[5]
		{
			exception.GetType().Name,
			exception.Message,
			LastInstanceCount,
			LastDrawCount,
			text
		}));
	}

	public string DumpUnits()
	{
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder(1024);
		stringBuilder.AppendLine("[渲染器] 相机=" + (((Object)(object)m_camera != (Object)null) ? ((Object)m_camera).name : "<null>") + " " + $"active={(Object)(object)m_camera != (Object)null && ((Behaviour)m_camera).isActiveAndEnabled} 注册播放器={m_players.Count} " + string.Format("mpbArrayLen={0} 收集数组长={1} 上帧实例={2} draw={3}", new object[4] { m_mpbArrayLen, m_matrices.Length, LastInstanceCount, LastDrawCount }));
		MegaBatch activeMegaBatch = zbSpineGpuAnimRuntime.ActiveMegaBatch;
		if (activeMegaBatch == null)
		{
			stringBuilder.AppendLine("[mega] <null>（本场未构建——怪物全部走原渲染）");
			return stringBuilder.ToString();
		}
		if ((Object)(object)activeMegaBatch.megaMesh == (Object)null)
		{
			stringBuilder.AppendLine("[mega] ★ megaMesh=<null>");
		}
		else
		{
			stringBuilder.AppendLine($"[mega] 顶点={activeMegaBatch.megaMesh.vertexCount} 子网格={activeMegaBatch.megaMesh.subMeshCount} " + $"normalTri={activeMegaBatch.normalTriangleCount} additiveTri={activeMegaBatch.additiveTriangleCount}");
			List<Vector4> list = new List<Vector4>();
			activeMegaBatch.megaMesh.GetUVs(1, list);
			if (list.Count == 0)
			{
				stringBuilder.AppendLine("[mega] ★ megaMesh 无 UV2（shader 采样坐标全 0 —— 顶点塌原点）");
			}
			else
			{
				Vector4 val = list[0];
				Vector4 val2 = list[list.Count - 1];
				stringBuilder.AppendLine(string.Format("[mega] UV2 首尾=({0:0},{1:0},{2:0},{3:0})/({4:0},{5:0},{6:0},{7:0})", new object[8] { val.x, val.y, val.z, val.w, val2.x, val2.y, val2.z, val2.w }) + ((val.y <= 0f || val.z <= 0f) ? "  ★ UV2 宽/高非正值（采样除 0）" : ""));
			}
		}
		string arg = (((Object)(object)activeMegaBatch.animArray != (Object)null) ? string.Format("{0}x{1}x{2} {3}", new object[4]
		{
			((Texture)activeMegaBatch.animArray).width,
			((Texture)activeMegaBatch.animArray).height,
			activeMegaBatch.animArray.depth,
			activeMegaBatch.animArray.format
		}) : "<null>");
		string arg2 = (((Object)(object)activeMegaBatch.colorArray != (Object)null) ? string.Format("{0}x{1}x{2} {3}", new object[4]
		{
			((Texture)activeMegaBatch.colorArray).width,
			((Texture)activeMegaBatch.colorArray).height,
			activeMegaBatch.colorArray.depth,
			activeMegaBatch.colorArray.format
		}) : "<null>");
		stringBuilder.AppendLine($"[mega] animArray={arg} colorArray={arg2} 型数={activeMegaBatch.typeByPath.Count}");
		foreach (KeyValuePair<string, MegaBatch.TypeRecord> item in activeMegaBatch.typeByPath)
		{
			stringBuilder.AppendLine($"[型] {item.Key} typeIndex={item.Value.typeIndex} vertexOffset={item.Value.vertexOffset} " + $"并集顶点={((item.Value.baked != null) ? item.Value.baked.unionVertexCount : 0)} " + $"帧={((item.Value.baked != null) ? item.Value.baked.totalFrames : 0)} " + "additive=" + ((item.Value.baked != null && item.Value.baked.hasAdditiveSubmesh) ? "是" : "否"));
		}
		DumpMaterial(stringBuilder, "普通材质", activeMegaBatch.material, activeMegaBatch.animArray, activeMegaBatch.colorArray);
		if ((Object)(object)activeMegaBatch.materialAdditive != (Object)null)
		{
			DumpMaterial(stringBuilder, "叠加材质", activeMegaBatch.materialAdditive, activeMegaBatch.animArray, activeMegaBatch.colorArray);
		}
		return stringBuilder.ToString();
	}

	private static void DumpMaterial(StringBuilder sb, string label, Material material, Texture2DArray animArray, Texture2DArray colorArray)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)material == (Object)null)
		{
			sb.AppendLine("[" + label + "] <null>");
			return;
		}
		string text = (((Object)(object)material.shader != (Object)null) ? ((Object)material.shader).name : "<null>");
		sb.AppendLine("[" + label + "] shader=" + text + ((text != "Custom/SpineSkeleton_URP_GPUAnim") ? "  ★ 非 Custom/SpineSkeleton_URP_GPUAnim！" : "") + $" enableInstancing={material.enableInstancing}（procedural 路径不依赖此标志，仅供观察）");
		Texture texture = material.GetTexture(kIdMainTex);
		Texture texture2 = material.GetTexture(kIdAnimTex);
		Vector4 vector = material.GetVector(kIdAnimTexSize);
		Texture texture3 = material.GetTexture(kIdColorTex);
		Texture2DArray val = (Texture2DArray)(object)((texture2 is Texture2DArray) ? texture2 : null);
		string text2 = ((val != null) ? $"{((Texture)val).width}x{((Texture)val).height}x{val.depth}" : (((Object)(object)texture2 != (Object)null) ? $"{texture2.width}x{texture2.height}(非数组!)" : "<null>"));
		Texture2DArray val2 = (Texture2DArray)(object)((texture3 is Texture2DArray) ? texture3 : null);
		string text3 = ((val2 != null) ? $"{((Texture)val2).width}x{((Texture)val2).height}x{val2.depth}" : (((Object)(object)texture3 != (Object)null) ? $"{texture3.width}x{texture3.height}(非数组!)" : "<null>"));
		sb.AppendLine("[" + label + "] _MainTex=" + (((Object)(object)texture != (Object)null) ? $"{texture.width}x{texture.height}" : "<null>") + " _AnimTex=" + text2 + (((Object)(object)texture2 != (Object)(object)animArray) ? "  ★ _AnimTex 与纹理数组不是同一张！" : "") + $" _AnimTexSize=({vector.x:0},{vector.y:0}) renderQueue={material.renderQueue}");
		sb.AppendLine($"[{label}] Blend={material.GetFloat(kIdSrcBlend):0}/{material.GetFloat(kIdDstBlend):0}" + ((label == "叠加材质" && (material.GetFloat(kIdSrcBlend) != 1f || material.GetFloat(kIdDstBlend) != 1f)) ? "  ★ 叠加材质混合因子非 One/One（纯加法失效）！" : ""));
		sb.AppendLine("[" + label + "] _ColorTex=" + text3 + (((Object)(object)texture3 != (Object)(object)colorArray) ? "  ★ _ColorTex 与颜色数组不是同一张！" : ""));
	}

	private void EnsureCapacity(int needed)
	{
		if (needed > m_matrices.Length)
		{
			int num;
			for (num = m_matrices.Length; num < needed; num *= 2)
			{
			}
			Array.Resize(ref m_collectPlayers, num);
			Array.Resize(ref m_matrices, num);
			Array.Resize(ref m_sortKeys, num);
		}
	}

	private static bool ResolveGpuShader(out Shader shader)
	{
		if (!s_gpuShaderResolved)
		{
			s_gpuShaderResolved = true;
			s_gpuShader = Shader.Find("Custom/SpineSkeleton_URP_GPUAnim");
		}
		shader = s_gpuShader;
		return (Object)(object)shader != (Object)null;
	}

	private static bool ResolveGpuAdditiveShader(out Shader shader)
	{
		if (!s_gpuAdditiveShaderResolved)
		{
			s_gpuAdditiveShaderResolved = true;
			s_gpuAdditiveShader = Shader.Find("Custom/SpineSkeleton_URP_GPUAnim_Additive");
			if ((Object)(object)s_gpuAdditiveShader == (Object)null && zbAssetLoader.Instance != null)
			{
				s_gpuAdditiveShader = zbAssetLoader.Instance.LoadResouce<Shader>("effect/shader/SpineSkeletonURP_GPUAnimAdditive.shader");
			}
			if ((Object)(object)s_gpuAdditiveShader == (Object)null)
			{
				zbPerfLog.LogWarning("[zbSpineGpuAnimRenderer] 叠加固定混合 shader 缺失：effect/shader/SpineSkeletonURP_GPUAnimAdditive.shader（重打 effect/shader AB 后生效），本场叠加 mega 回落 919.0 属性驱动混合");
			}
		}
		shader = s_gpuAdditiveShader;
		return (Object)(object)shader != (Object)null;
	}

	private static Material ResolveTemplateMaterial()
	{
		if (!s_templateResolved)
		{
			s_templateResolved = true;
			s_templateMaterial = ((zbAssetLoader.Instance != null) ? zbAssetLoader.Instance.LoadResouce<Material>("effect/material/mt_spine_gpu_anim.mat") : null);
			if ((Object)(object)s_templateMaterial == (Object)null)
			{
				zbPerfLog.Log("[zbSpineGpuAnimRenderer] 模板材质缺失：effect/material/mt_spine_gpu_anim.mat（编辑器创建：New Material → 指定 Custom/SpineSkeleton_URP_GPUAnim → 存为该路径并随 AB 打包）");
			}
		}
		return s_templateMaterial;
	}
}
