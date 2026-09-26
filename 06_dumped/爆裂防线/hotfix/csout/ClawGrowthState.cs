public class ClawGrowthState
{
	public float cycleDuration = 30f;

	public bool attached;

	public float growthStartTime;

	private bool _isDelaying;

	private float _delayStartTime;

	private float _delayDuration;

	public int clawIndex;

	public zbMonster monster;

	public void StartDelayedAttach(float currentTime, float delayDuration)
	{
		_isDelaying = true;
		_delayStartTime = currentTime;
		_delayDuration = delayDuration;
		attached = false;
		if (clawIndex == 1)
		{
			monster.monsterSkillIndex30152.skill1LeftKeep = 0f;
		}
		else
		{
			monster.monsterSkillIndex30152.skill1RightKeep = 0f;
		}
	}

	public void UpdateDelay(float currentTime)
	{
		if (_isDelaying && currentTime >= _delayStartTime + _delayDuration)
		{
			_isDelaying = false;
			attached = true;
			growthStartTime = currentTime;
			if (clawIndex == 1)
			{
				monster.anim.spineAnimation.PlayTackAnimation(clawIndex + 5, "pincer_1", loop: false);
			}
			else
			{
				monster.anim.spineAnimation.PlayTackAnimation(clawIndex + 5, "pincer_2", loop: false);
			}
		}
	}

	public float GetExistenceTime(float currentTime)
	{
		if (!attached || _isDelaying || currentTime < growthStartTime)
		{
			return 0f;
		}
		return currentTime - growthStartTime;
	}
}
