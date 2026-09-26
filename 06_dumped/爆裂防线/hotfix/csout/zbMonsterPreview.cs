using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMonsterPreview : MonoBehaviour
{
	public SkeletonAnimation m_spineAnimation;

	public zbMonsterCsvRecord record;

	private SpriteRenderer m_yingzi;

	protected void OnEnable()
	{
		m_spineAnimation = ((Component)((Component)this).transform.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
		m_yingzi = ((Component)((Component)this).transform.Find("yingzi")).GetComponent<SpriteRenderer>();
	}

	public void Refresh(int monsterTid)
	{
		((Component)this).gameObject.SetActive(false);
		if (monsterTid == 0)
		{
			return;
		}
		record = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterTid);
		if (record == null)
		{
			return;
		}
		zbMonsterAvatarCsvRecord recordByKey = zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(record.avatarId);
		if (recordByKey == null)
		{
			return;
		}
		if (recordByKey.shadow != null)
		{
			Sprite sprite = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.shadow);
			if ((Object)(object)m_yingzi == (Object)null)
			{
				m_yingzi = ((Component)((Component)this).transform.Find("yingzi")).GetComponent<SpriteRenderer>();
			}
			m_yingzi.sprite = sprite;
		}
		zbUtil.RefreshSpine(m_spineAnimation, recordByKey.fightBone + "_SkeletonData.asset", "idle", loop: true);
		((Component)this).gameObject.SetActive(true);
	}

	public void SetScale()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localScale = Vector2.op_Implicit(new Vector2(record.bodyScale, record.bodyScale));
	}

	public void RandomAnimation()
	{
		TrackEntry obj = m_spineAnimation.state.SetAnimation(0, "idle", true);
		float duration = obj.Animation.Duration;
		float trackTime = Rand.Range(0f, duration);
		obj.TrackTime = trackTime;
	}

	public zbMonsterCsvRecord GetMonsterRecord()
	{
		return record;
	}

	public zbHandbookCsvRecord GetHandBookRecord()
	{
		return zbDataConfig.Instance.handbookCsvTable.GetRecordByKey(record.handbookId);
	}
}
