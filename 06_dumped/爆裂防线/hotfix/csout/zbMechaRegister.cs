using System.Collections.Generic;
using UnityEngine;

public class zbMechaRegister
{
	private Dictionary<int, string> mechaMap = new Dictionary<int, string>();

	public void Init()
	{
		RegisterMecha(101, "prefab/combat/extra_attacker/mecha/attacker_mechaDian.prefab");
		RegisterMecha(102, "prefab/combat/extra_attacker/mecha/attacker_mechaHuo.prefab");
		RegisterMecha(103, "prefab/combat/extra_attacker/mecha/attacker_mechaBing.prefab");
		RegisterMecha(104, "prefab/combat/extra_attacker/mecha/attacker_mechaWuli.prefab");
		RegisterMecha(105, "prefab/combat/extra_attacker/mecha/attacker_mechaNengliang.prefab");
		RegisterMecha(106, "prefab/combat/extra_attacker/mecha/attacker_mechaFeng.prefab");
	}

	private void RegisterMecha(int mechaId, string prefabStr)
	{
		if (!mechaMap.ContainsKey(mechaId))
		{
			mechaMap.Add(mechaId, prefabStr);
		}
	}

	public zbMechaBase CreateMecha(int mechaId)
	{
		if (!mechaMap.TryGetValue(mechaId, out var value))
		{
			Debug.LogError((object)$"未注册的机甲ID: {mechaId}");
			return null;
		}
		return Object.Instantiate<GameObject>(zbAssetLoader.Instance.LoadResouce<GameObject>(value)).GetComponent<zbMechaBase>();
	}

	public zbMechaSkillHandleBase CreateSkill(int skillId)
	{
		return new zbMechaSkillHandleBase(skillId);
	}
}
