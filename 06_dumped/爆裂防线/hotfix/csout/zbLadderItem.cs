using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLadderItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbLadderItem>
	{
	}

	private SpineVisualElement m_spineLadder;

	private VisualElement[] m_spriteStars = (VisualElement[])(object)new VisualElement[5];

	private SpineVisualElement[] spineStars = (SpineVisualElement[])(object)new SpineVisualElement[5];

	private SpineVisualElement m_spineUpgrade;

	private Label m_lbLadder;

	private zbBaseWnd m_wndParent;

	private int m_ladderId;

	public zbLadderItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/crossLadder/tb_ladder.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbBaseWnd wndParent)
	{
		m_wndParent = wndParent;
		m_spineLadder = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)this, "spine_ladder", (string)null);
		m_spineUpgrade = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)this, "spine_upgrade", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "star" + i, (string)null);
			spineStars[i] = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)this, "spine_star" + i, (string)null);
		}
		m_lbLadder = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_ladder", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(int ladderId, bool visName = false, bool isAction = false, int lastLadderId = 0)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (lastLadderId == 0)
		{
			lastLadderId = ladderId;
		}
		zbLadderCsvRecord recordByKey = zbDataConfig.Instance.ladderCsvTable.GetRecordByKey(lastLadderId);
		zbLadderCsvRecord recordByKey2 = zbDataConfig.Instance.ladderCsvTable.GetRecordByKey(ladderId);
		if (recordByKey2 == null)
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		((VisualElement)m_spineUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (isAction)
		{
			((MonoBehaviour)m_wndParent).StartCoroutine(ShootRepeater(recordByKey, recordByKey2, ladderId));
		}
		else
		{
			RefreshIconStars(recordByKey2, ladderId);
		}
		((TextElement)m_lbLadder).text = recordByKey2.name;
		((VisualElement)m_lbLadder).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visName));
	}

	public IEnumerator ShootRepeater(zbLadderCsvRecord lastRecord, zbLadderCsvRecord record, int ladderId)
	{
		if (lastRecord.quality == record.quality)
		{
			zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
			ActionIconStars(record, ladderId);
			yield break;
		}
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)spineStars[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_spriteStars[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			spineStars[i].AnimationState.SetAnimation(0, "loop", false);
		}
		zbUtil.RefreshSpineUI(m_spineLadder, lastRecord.spine + "_SkeletonData.asset", "loop", loop: true);
		yield return (object)new WaitForSeconds(0.5f);
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
		TrackEntry val = m_spineLadder.AnimationState.SetAnimation(0, "exit", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		((VisualElement)m_spineUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spineUpgrade.AnimationState.SetAnimation(0, "animation", false);
		ActionIconStars(record, ladderId);
	}

	public void RefreshIconStars(zbLadderCsvRecord record, int ladderId)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		zbUtil.RefreshSpineUI(m_spineLadder, record.spine + "_SkeletonData.asset", "loop", loop: true);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((ladderId == zbSelf.Instance.crossLadderHandler.maxLadderId) ? 1 : 0));
			if (record.id != zbSelf.Instance.crossLadderHandler.maxLadderId)
			{
				((VisualElement)spineStars[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((i >= record.star) ? 1 : 0));
			}
		}
	}

	public void ActionIconStars(zbLadderCsvRecord record, int ladderId)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		zbUtil.RefreshSpineUI(m_spineLadder, record.spine + "_SkeletonData.asset", "loop", loop: true);
		m_spineLadder.AnimationState.SetAnimation(0, "enter", false);
		m_spineLadder.AnimationState.AddAnimation(0, "loop", true, 0f);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((ladderId == zbSelf.Instance.crossLadderHandler.maxLadderId) ? 1 : 0));
			if (record.id != zbSelf.Instance.crossLadderHandler.maxLadderId)
			{
				((VisualElement)spineStars[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((i >= record.star) ? 1 : 0));
				if (i < record.star)
				{
					spineStars[i].AnimationState.SetAnimation(0, "enter2", false);
					spineStars[i].AnimationState.AddAnimation(0, "loop", true, 0f);
				}
			}
		}
	}
}
