using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GachaEffectPlayer : MonoBehaviour
{
	public GameObject root;

	public ParticleSystem rank1;

	public ParticleSystem rank2;

	public ParticleSystem rank3;

	public ParticleSystem rank4;

	public ParticleSystem rank5;

	public GachaRoomCollisionCube collisionCube;

	public void Play(List<GlobalRewardInfo> rewards)
	{
		root.gameObject.SetActive(value: true);
		SetRank(QualityLevel.Base, rank1);
		SetRank(QualityLevel.Common, rank2);
		SetRank(QualityLevel.Uncommon, rank3);
		SetRank(QualityLevel.Rare, rank4);
		SetRank(QualityLevel.Epic, rank5);
		collisionCube.ResetCollision(new List<ParticleSystem> { rank1, rank2, rank3, rank4, rank5 });
		void SetRank(QualityLevel quality, ParticleSystem rankObj)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			int num = rewards.Count((GlobalRewardInfo r) => ((IGlobalItemDisplaySetup)r).GlobalItemQuality == quality);
			if (num == 0)
			{
				((Component)(object)rankObj).gameObject.SetActive(value: false);
			}
			else
			{
				((Component)(object)rankObj).gameObject.SetActive(value: true);
				EmissionModule emission = rankObj.emission;
				Burst burst = ((EmissionModule)(ref emission)).GetBurst(0);
				((Burst)(ref burst)).cycleCount = num;
				((EmissionModule)(ref emission)).SetBurst(0, burst);
			}
		}
	}
}
