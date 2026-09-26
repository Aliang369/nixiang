using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine;

public class zbFootholdRegister
{
	private Dictionary<int, Type> footholdMap = new Dictionary<int, Type>();

	public void Init()
	{
		RegisterFoothold(7021, typeof(zbFoothold_201));
		RegisterFoothold(7031, typeof(zbFoothold_301));
		RegisterFoothold(7041, typeof(zbFoothold_401));
		RegisterFoothold(7051, typeof(zbFoothold_501));
		RegisterFoothold(7061, typeof(zbFoothold_601));
		RegisterFoothold(7071, typeof(zbFoothold_701));
		RegisterFoothold(7081, typeof(zbFoothold_801));
		RegisterFoothold(7091, typeof(zbFoothold_901));
		RegisterFoothold(7101, typeof(zbFoothold_1001));
		RegisterFoothold(7111, typeof(zbFoothold_1101));
		RegisterFoothold(7121, typeof(zbFoothold_1201));
		RegisterFoothold(7131, typeof(zbFoothold_1301));
		RegisterFoothold(7141, typeof(zbFoothold_1401));
		RegisterFoothold(7151, typeof(zbFoothold_1501));
		RegisterFoothold(7161, typeof(zbFoothold_1601));
		RegisterFoothold(7171, typeof(zbFoothold_1701));
		RegisterFoothold(7181, typeof(zbFoothold_1801));
		RegisterFoothold(7191, typeof(zbFoothold_1901));
		RegisterFoothold(7201, typeof(zbFoothold_2001));
		RegisterFoothold(7211, typeof(zbFoothold_2101));
		RegisterFoothold(7221, typeof(zbFoothold_2201));
	}

	private void RegisterFoothold(int footholdId, Type prefabStr)
	{
		if (!footholdMap.ContainsKey(footholdId))
		{
			footholdMap.Add(footholdId, prefabStr);
		}
	}

	public zbFootholdBase CreateFoothold(BattleTeamFootholdInfo footholdInfo, GameObject heroInstance)
	{
		zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(footholdInfo.FootholdId);
		if (recordByKey == null || recordByKey.footholdEffectId1 <= 0)
		{
			return null;
		}
		zbFootholdEffectCsvRecord footholdEffectRecord = zbSelf.Instance.guardHandler.GetFootholdEffectRecord(footholdInfo.FootholdId, footholdInfo.Star);
		if (footholdEffectRecord == null)
		{
			return null;
		}
		if (!footholdMap.TryGetValue(footholdEffectRecord.skillIndex, out var value))
		{
			Debug.LogError((object)$"未注册的机甲ID: {footholdInfo.FootholdId}");
			return null;
		}
		heroInstance.AddComponent(value);
		return heroInstance.GetComponent<zbFootholdBase>();
	}
}
