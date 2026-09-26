using Proto.Enums;
using UnityEngine;

public class zbSeasonMapDefine
{
	public static float minOrthographicSize;

	public static float maxOrthographicSize;

	public static int gridCount;

	public static float cellSize;

	public static Color[] forceColor;

	public static string[] forceBigFlagUrl;

	public static string[] forceSmallFlagUrl;

	public static string[] forceBtnFlagUrl;

	public static Color GetForceColor(ESeasonForce force)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return forceColor[(int)force];
	}

	public static string GetForceBigFlagUrl(ESeasonForce force)
	{
		return forceBigFlagUrl[(int)force];
	}

	public static string GetForceSmallFlagUrl(ESeasonForce force)
	{
		return forceSmallFlagUrl[(int)force];
	}

	public static string GetForceBtnFlagUrl(ESeasonForce force)
	{
		return forceBtnFlagUrl[(int)force];
	}

	static zbSeasonMapDefine()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		minOrthographicSize = 6f;
		maxOrthographicSize = 11f;
		gridCount = 10;
		cellSize = 10.24f;
		forceColor = (Color[])(object)new Color[5]
		{
			Color32.op_Implicit(new Color32((byte)158, (byte)158, (byte)177, byte.MaxValue)),
			Color32.op_Implicit(new Color32((byte)199, (byte)165, (byte)44, byte.MaxValue)),
			Color32.op_Implicit(new Color32((byte)75, (byte)170, (byte)75, byte.MaxValue)),
			Color32.op_Implicit(new Color32((byte)171, (byte)70, (byte)70, byte.MaxValue)),
			Color32.op_Implicit(new Color32((byte)75, (byte)120, (byte)175, byte.MaxValue))
		};
		forceBigFlagUrl = new string[5] { "ui/image/season/faction_flag4.png", "ui/image/season/faction_flag3.png", "ui/image/season/faction_flag2.png", "ui/image/season/faction_flag0.png", "ui/image/season/faction_flag1.png" };
		forceSmallFlagUrl = new string[5] { "ui/image/season/faction_smallFlag4.png", "ui/image/season/faction_smallFlag3.png", "ui/image/season/faction_smallFlag2.png", "ui/image/season/faction_smallFlag0.png", "ui/image/season/faction_smallFlag1.png" };
		forceBtnFlagUrl = new string[5] { "ui/image/season/btn_flag0.png", "ui/image/season/btn_flag3.png", "ui/image/season/btn_flag2.png", "ui/image/season/btn_flag0.png", "ui/image/season/btn_flag1.png" };
	}
}
