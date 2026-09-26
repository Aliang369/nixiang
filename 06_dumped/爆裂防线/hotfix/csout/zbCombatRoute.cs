using Proto.Enums;
using UnityEngine;

public class zbCombatRoute
{
	private Transform tsRouteLine;

	public float routeWidth;

	public float routeY;

	public float tsRouteLinePosX;

	private zbCombatData combatData;

	public void Init(zbCombatData _combatData)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		combatData = _combatData;
		tsRouteLine = ((Component)combatData.nodeRoot.Find("SceneRoute")).transform.Find("routeLine");
		routeY = ((Component)tsRouteLine).transform.position.y;
		routeWidth = 7.2f;
		tsRouteLine.localScale = new Vector3(routeWidth, tsRouteLine.localScale.y, tsRouteLine.localScale.z);
		((Component)tsRouteLine).gameObject.SetActive(false);
		tsRouteLinePosX = ((Component)tsRouteLine).transform.position.x;
	}

	public void SetRouteWidth()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (zbCombatSystem.Instance.GetDefenseType() != EDefenseType.CarDefense)
		{
			return;
		}
		float num = 999f;
		float num2 = -999f;
		foreach (zbHero hero in combatData.masterTeam.heroList)
		{
			if (num > ((Component)hero).transform.position.x)
			{
				num = ((Component)hero).transform.position.x;
			}
			if (num2 < ((Component)hero).transform.position.x)
			{
				num2 = ((Component)hero).transform.position.x;
			}
		}
		num -= 0.4f;
		num2 += 0.4f;
		routeWidth = num2 - num;
		tsRouteLine.localScale = new Vector3(routeWidth, tsRouteLine.localScale.y, tsRouteLine.localScale.z);
		((Component)tsRouteLine).transform.position = new Vector3((num2 + num) / 2f, ((Component)tsRouteLine).transform.position.y, ((Component)tsRouteLine).transform.position.x);
		tsRouteLinePosX = ((Component)tsRouteLine).transform.position.x;
	}

	public void Free()
	{
	}
}
