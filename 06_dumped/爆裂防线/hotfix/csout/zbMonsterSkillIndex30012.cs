using UnityEngine;

public class zbMonsterSkillIndex30012 : zbMonsterSkillBase
{
	private void SetSkeletonAlpha(float alpha)
	{
		monster.m_skeletonRenderer.skeleton.A = alpha;
		zbSpineGpuAnimPlayer zbSpineGpuAnimPlayer2 = ((monster.anim != null && monster.anim.spineAnimation != null) ? monster.anim.spineAnimation.gpuPlayer : null);
		if (zbSpineGpuAnimPlayer2 != null && !zbSpineGpuAnimPlayer2.isFallback)
		{
			zbSpineGpuAnimPlayer2.SetSkeletonAlpha(alpha);
		}
	}

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		SetSkeletonAlpha(1f);
		monster.monsterIndex30012.skillIndexTake = true;
		monster.monsterIndex30012.preLostHpPercent = monsterSkillRecord.param[0];
		monster.monsterIndex30012.duration = monsterSkillRecord.param[1];
		monster.monsterIndex30012.lastLostHp = 0L;
		monster.monsterIndex30012.everyTimeKeep = 0f;
		monster.monsterIndex30012Hide = false;
		monster.monsterIndex30012.skillBase = this;
	}

	public override void CallBackLostHp(long value)
	{
		if (!monster.monsterIndex30012.skillIndexTake)
		{
			return;
		}
		monster.monsterIndex30012.lastLostHp -= value;
		long num = (long)(monster.monsterIndex30012.preLostHpPercent * (float)(long)monster.maxHp);
		if (monster.monsterIndex30012.lastLostHp >= num)
		{
			while (monster.monsterIndex30012.lastLostHp >= num)
			{
				monster.monsterIndex30012.everyTimeKeep = monster.monsterIndex30012.duration;
				monster.monsterIndex30012.lastLostHp = monster.monsterIndex30012.lastLostHp - num;
				SetSkeletonAlpha(0.2f);
				monster.monsterIndex30012Hide = true;
			}
		}
	}

	public override void Update()
	{
		if (monster.monsterIndex30012.skillIndexTake)
		{
			monster.monsterIndex30012.everyTimeKeep -= Time.deltaTime;
			if (monster.monsterIndex30012.everyTimeKeep <= 0f)
			{
				monster.monsterIndex30012Hide = false;
				SetSkeletonAlpha(1f);
			}
		}
	}

	public override void ClearList()
	{
		monster.monsterIndex30012.skillIndexTake = false;
	}
}
