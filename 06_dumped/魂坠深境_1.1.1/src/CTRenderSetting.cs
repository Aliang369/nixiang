using UnityEngine;

public static class CTRenderSetting
{
	public static Color FarFogColor
	{
		get
		{
			return Shader.GetGlobalColor("_FarFogColor");
		}
		set
		{
			Shader.SetGlobalColor("_FarFogColor", value);
		}
	}

	public static Color MiddleFogColor1
	{
		get
		{
			return Shader.GetGlobalColor("_MiddleFogColor1");
		}
		set
		{
			Shader.SetGlobalColor("_MiddleFogColor1", value);
		}
	}

	public static Color MiddleFogColor2
	{
		get
		{
			return Shader.GetGlobalColor("_MiddleFogColor2");
		}
		set
		{
			Shader.SetGlobalColor("_MiddleFogColor2", value);
		}
	}

	public static Color NearFogColor
	{
		get
		{
			return Shader.GetGlobalColor("_NearFogColor");
		}
		set
		{
			Shader.SetGlobalColor("_NearFogColor", value);
		}
	}

	public static float MiddleFogFactor1
	{
		get
		{
			return Shader.GetGlobalFloat("_MiddleFogFactor1");
		}
		set
		{
			Shader.SetGlobalFloat("_MiddleFogFactor1", value);
		}
	}

	public static float MiddleFogFactor2
	{
		get
		{
			return Shader.GetGlobalFloat("_MiddleFogFactor2");
		}
		set
		{
			Shader.SetGlobalFloat("_MiddleFogFactor2", value);
		}
	}

	public static bool DepthFogColor
	{
		get
		{
			return Shader.IsKeywordEnabled("DEPTH_FOG_COLOR");
		}
		set
		{
			if (value)
			{
				Shader.EnableKeyword("DEPTH_FOG_COLOR");
			}
			else
			{
				Shader.DisableKeyword("DEPTH_FOG_COLOR");
			}
		}
	}

	public static float HeightFogBottom
	{
		get
		{
			return Shader.GetGlobalFloat("_HeightFogBottom");
		}
		set
		{
			Shader.SetGlobalFloat("_HeightFogBottom", value);
		}
	}

	public static float HeightFogTop
	{
		get
		{
			return Shader.GetGlobalFloat("_HeightFogTop");
		}
		set
		{
			Shader.SetGlobalFloat("_HeightFogTop", value);
		}
	}

	public static Color BaseColorMultiply
	{
		get
		{
			return Shader.GetGlobalColor("_BaseColorMultiply");
		}
		set
		{
			Shader.SetGlobalColor("_BaseColorMultiply", value);
		}
	}

	public static Color RoomAmbient
	{
		get
		{
			return Shader.GetGlobalColor("_RoomAmbient");
		}
		set
		{
			Shader.SetGlobalColor("_RoomAmbient", value);
		}
	}

	public static Color RoomAmbient2
	{
		get
		{
			return Shader.GetGlobalColor("_RoomAmbient2");
		}
		set
		{
			Shader.SetGlobalColor("_RoomAmbient2", value);
		}
	}

	public static float RoomAmbient2Occlusion
	{
		get
		{
			return Shader.GetGlobalFloat("_RoomAmbient2Occlusion");
		}
		set
		{
			Shader.SetGlobalFloat("_RoomAmbient2Occlusion", value);
		}
	}
}
