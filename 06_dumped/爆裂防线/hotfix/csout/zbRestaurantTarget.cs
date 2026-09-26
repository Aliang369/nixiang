using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRestaurantTarget
{
	private zbRestaurantWnd m_wndParent;

	private SkeletonAnimation m_heroAvatar;

	private SpineVisualElement m_spine;

	private VisualElement m_nodeTalk;

	private Label m_lbTalk;

	private zbItem[] m_awardTaskAwards;

	private VisualElement m_nodeTargetList;

	public VisualElement[] m_spriteTargets;

	private VisualElement[] m_spriteFoods;

	private Label[] m_lbFoodLvs;

	private VisualElement[] m_spriteFoodGets;

	public VisualElement[] m_btnCanOffers;

	private VisualElement m_nodeNormal;

	private zbItem[] m_normalAwards;

	private readonly Vector3 OuterPosition;

	private readonly Vector3 InerPosition;

	private int m_targetId;

	private bool m_isGrand;

	public void OnEnable(zbRestaurantWnd wndParent, SkeletonAnimation heroAvatar, VisualElement root)
	{
		m_wndParent = wndParent;
		m_heroAvatar = heroAvatar;
		m_spine = UQueryExtensions.Q<SpineVisualElement>(root, "spine", (string)null);
		m_nodeTalk = UQueryExtensions.Q<VisualElement>(root, "node_talk", (string)null);
		m_lbTalk = UQueryExtensions.Q<Label>(m_nodeTalk, "lb_talk", (string)null);
		m_nodeTargetList = UQueryExtensions.Q<VisualElement>(root, "targetList", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_awardTaskAwards[i] = UQueryExtensions.Q<zbItem>(m_nodeTalk, $"award{i}", (string)null);
			m_awardTaskAwards[i].OnEnable(m_wndParent);
		}
		for (int j = 0; j < 3; j++)
		{
			m_spriteTargets[j] = UQueryExtensions.Q<VisualElement>(m_nodeTargetList, $"taskTarget{j}", (string)null);
			m_spriteFoods[j] = UQueryExtensions.Q<VisualElement>(m_spriteTargets[j], "target_food", (string)null);
			m_lbFoodLvs[j] = UQueryExtensions.Q<Label>(m_spriteTargets[j], "lb_lv", (string)null);
			m_spriteFoodGets[j] = UQueryExtensions.Q<VisualElement>(m_spriteTargets[j], "icon_hasFood", (string)null);
			m_btnCanOffers[j] = (VisualElement)(object)UQueryExtensions.Q<Button>(m_spriteTargets[j], "btn_canOffer", (string)null);
			zbUIEffect.CreateSwingPauseAction(m_btnCanOffers[j]);
			int tempIndex = j;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>(m_btnCanOffers[j], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnOffers(evt, tempIndex);
			}, "", (TrickleDown)0);
		}
		m_nodeNormal = UQueryExtensions.Q<VisualElement>(root, "node_normal", (string)null);
		for (int num = 0; num < 4; num++)
		{
			m_normalAwards[num] = UQueryExtensions.Q<zbItem>(m_nodeNormal, $"award{num}", (string)null);
			m_normalAwards[num].OnEnable(m_wndParent);
		}
	}

	public void Refresh()
	{
		OperateRestaurantTargetDetail targetDetail = m_wndParent.m_detail.TargetDetail;
		m_isGrand = m_wndParent.m_detail.TargetNextGrand.CurrentIndex == m_wndParent.m_detail.TargetNextGrand.GrandIndex;
		RefreshAwards(targetDetail);
		RefreshTargets(targetDetail);
		if (m_targetId == targetDetail.Id)
		{
			return;
		}
		m_targetId = targetDetail.Id;
		zbRestaurantTargetCsvRecord recordByKey = zbDataConfig.Instance.restaurantTargetCsvTable.GetRecordByKey(targetDetail.Id);
		if (recordByKey != null)
		{
			zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
			if (recordByKey2 != null)
			{
				string spinePath = recordByKey2.frontBone + "_SkeletonData.asset";
				zbUtil.RefreshSpine(m_heroAvatar, spinePath, "idle", loop: true);
			}
			((TextElement)m_lbTalk).text = recordByKey.desc;
		}
	}

	private void RefreshAwards(OperateRestaurantTargetDetail targetDetail)
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (m_isGrand)
		{
			for (int i = 0; i < 4; i++)
			{
				bool flag = i < targetDetail.AwardList.Count;
				((VisualElement)m_awardTaskAwards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
				if (!flag)
				{
					continue;
				}
				zbItemData data = new zbItemData(targetDetail.AwardList[i].Type, targetDetail.AwardList[i].Tid, targetDetail.AwardList[i].Count);
				m_awardTaskAwards[i].Refresh(data);
				if (targetDetail.State == ECommonState.Yes)
				{
					m_awardTaskAwards[i].SetTouchCallback(delegate
					{
						zbWsMsgHandler.Instance.Operate_RestaurantTarget_Req(m_wndParent.m_detail.BaseInfo.UUID, ((IEnumerable<int>)m_wndParent.m_detail.SlotList).ToArray());
					});
				}
				else
				{
					m_awardTaskAwards[i].SetTouchCallback(null);
				}
				m_awardTaskAwards[i].SetSpineVisible(targetDetail.State == ECommonState.Yes);
			}
			return;
		}
		for (int num = 0; num < 4; num++)
		{
			bool flag2 = num < targetDetail.AwardList.Count;
			((VisualElement)m_normalAwards[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			if (!flag2)
			{
				continue;
			}
			zbItemData data2 = new zbItemData(targetDetail.AwardList[num].Type, targetDetail.AwardList[num].Tid, targetDetail.AwardList[num].Count);
			m_normalAwards[num].Refresh(data2);
			if (targetDetail.State == ECommonState.Yes)
			{
				m_normalAwards[num].SetTouchCallback(delegate
				{
					zbWsMsgHandler.Instance.Operate_RestaurantTarget_Req(m_wndParent.m_detail.BaseInfo.UUID, ((IEnumerable<int>)m_wndParent.m_detail.SlotList).ToArray());
				});
			}
			else
			{
				m_normalAwards[num].SetTouchCallback(null);
			}
			m_normalAwards[num].SetSpineVisible(targetDetail.State == ECommonState.Yes);
		}
	}

	private void RefreshTargets(OperateRestaurantTargetDetail targetDetail)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			bool flag = i < targetDetail.Target.Count;
			m_spriteTargets[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			Sprite val = (zbSelf.Instance.restaurantHandler.IsOfferedByTargetIndex(m_wndParent.m_detail, i) ? m_wndParent.m_OfferBg : m_wndParent.m_UnlockBg);
			m_spriteTargets[i].style.backgroundImage = new StyleBackground(val);
			if (flag)
			{
				int num = targetDetail.Target[i];
				m_spriteFoods[i].style.backgroundImage = new StyleBackground(m_wndParent.m_foodIcons[num - 1]);
				m_spriteFoods[i].style.opacity = StyleFloat.op_Implicit(zbSelf.Instance.restaurantHandler.IsOfferedByTargetIndex(m_wndParent.m_detail, i) ? 1f : 0.7f);
				((TextElement)m_lbFoodLvs[i]).text = num.ToString();
				m_spriteFoodGets[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.restaurantHandler.IsOfferedByTargetIndex(m_wndParent.m_detail, i)));
				m_btnCanOffers[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.restaurantHandler.IsOfferedByTargetIndex(m_wndParent.m_detail, i) || !zbSelf.Instance.restaurantHandler.CanOfferByTargetIndex(m_wndParent.m_detail, i)) ? 1 : 0));
			}
		}
	}

	private void SetFacing(bool left)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localScale = ((Component)m_heroAvatar).transform.localScale;
		localScale.x = (left ? Mathf.Abs(localScale.x) : (0f - Mathf.Abs(localScale.x)));
		((Component)m_heroAvatar).transform.localScale = localScale;
	}

	private void OnOffers(ClickEvent evt, int index)
	{
		int num = m_wndParent.m_detail.TargetDetail.Target[index];
		int num2 = 0;
		using (IEnumerator<int> enumerator = m_wndParent.m_detail.SlotList.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current != num)
			{
				num2++;
			}
		}
		zbSelf.Instance.restaurantHandler.m_offerSlotIndex = num2;
		zbSelf.Instance.restaurantHandler.m_offerTargetIndex = index;
		zbWsMsgHandler.Instance.Operate_RestaurantOfferPiece_Req(m_wndParent.m_detail.BaseInfo.UUID, ((IEnumerable<int>)m_wndParent.m_detail.SlotList).ToArray(), num2 + 1);
	}

	public void PlayInAnimation()
	{
		((MonoBehaviour)m_wndParent).StartCoroutine(PlayIn());
	}

	public void PlayOutAndInAnimation(OperateRestaurantTargetAck ack)
	{
		((MonoBehaviour)m_wndParent).StartCoroutine(PlayOutAndIn(ack));
	}

	private IEnumerator PlayIn()
	{
		Refresh();
		((Component)m_heroAvatar).transform.position = OuterPosition;
		SetFacing(left: true);
		((VisualElement)m_spine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodeTalk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodeTargetList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodeNormal.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbUIMgr.Instance.ShowModel();
		yield return MoveTo(InerPosition);
		yield return TalkPop();
		zbUIMgr.Instance.HideModel();
		if (!zbSelf.Instance.restaurantHandler.HasAwardNotify(m_wndParent.m_detail))
		{
			m_wndParent.RefreshGuideFinger(forceRefresh: true);
		}
	}

	private IEnumerator PlayOutAndIn(OperateRestaurantTargetAck ack)
	{
		zbUIMgr.Instance.ShowModel();
		yield return MoveAndScaleTo(m_nodeTargetList, new Vector2(475f, 200f), 0.3f);
		zbSelf.Instance.dataHandler.DataChange(ack.DataChange, ((IEnumerable<AwardDetail>)ack.AwardList).ToArray());
		zbServerData.sGameData.OperateInfo.RestaurantList.Clear();
		zbServerData.sGameData.OperateInfo.RestaurantList.AddRange((IEnumerable<RestaurantDetail>)ack.RestaurantList);
		m_wndParent.m_detail = zbSelf.Instance.operateHandler.GetRestaurantByUUID(m_wndParent.m_detail.BaseInfo.UUID);
		m_wndParent.InitSlotList();
		yield return (object)new WaitForSeconds(0.1f);
		((VisualElement)m_spine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodeTalk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodeTargetList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodeNormal.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodeTargetList.style.left = StyleLength.op_Implicit(315f);
		m_nodeTargetList.style.top = StyleLength.op_Implicit(300f);
		m_nodeTargetList.transform.scale = Vector3.one;
		yield return MoveTo(OuterPosition);
		m_wndParent.RefreshCustom(m_wndParent.m_detail);
		Refresh();
		yield return MoveTo(InerPosition);
		yield return TalkPop();
		zbUIMgr.Instance.HideModel();
	}

	private IEnumerator MoveTo(Vector3 targetPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		m_heroAvatar.AnimationState.SetAnimation(0, "walk", true);
		SetFacing(((Component)m_heroAvatar).transform.position.x > targetPos.x);
		while (Vector3.Distance(((Component)m_heroAvatar).transform.position, targetPos) > 0.01f)
		{
			((Component)m_heroAvatar).transform.position = Vector3.MoveTowards(((Component)m_heroAvatar).transform.position, targetPos, 2f * Time.deltaTime);
			yield return null;
		}
		m_heroAvatar.AnimationState.SetAnimation(0, "idle", true);
	}

	private IEnumerator MoveAndScaleTo(VisualElement element, Vector2 targetPos, float duration)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		element.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		Vector2 startPos = new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);
		float startScale = 1f;
		float endScale = 0.2f;
		float time = 0f;
		while (time < duration)
		{
			time += Time.deltaTime;
			float num = time / duration;
			float num2 = Mathf.SmoothStep(0f, 1f, num);
			Vector2 val = Vector2.Lerp(startPos, targetPos, num2);
			float num3 = Mathf.Lerp(startScale, endScale, num2);
			element.style.left = StyleLength.op_Implicit(val.x);
			element.style.top = StyleLength.op_Implicit(val.y);
			element.transform.scale = new Vector3(num3, num3, 1f);
			yield return null;
		}
		element.style.left = StyleLength.op_Implicit(targetPos.x);
		element.style.top = StyleLength.op_Implicit(targetPos.y);
		element.transform.scale = new Vector3(endScale, endScale, 1f);
	}

	private IEnumerator TalkPop()
	{
		if (m_isGrand)
		{
			((VisualElement)m_spine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_spine.AnimationState.SetAnimation(0, "animation", false);
			m_nodeTalk.style.scale = StyleScale.op_Implicit(new Scale(new Vector3(0f, 0f, 1f)));
			m_nodeTalk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			m_nodeNormal.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		yield return ScaleTo(m_nodeTalk, 1f, 0.3f);
		m_nodeTargetList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private IEnumerator ScaleTo(VisualElement element, float scaleTo, float duration)
	{
		float startScale = element.transform.scale.x;
		float time = 0f;
		while (time < duration)
		{
			time += Time.deltaTime;
			float num = time / duration;
			float num2 = Mathf.SmoothStep(0f, 1f, num);
			float num3 = Mathf.Lerp(startScale, scaleTo, num2);
			element.transform.scale = new Vector3(num3, num3, 1f);
			yield return null;
		}
		element.transform.scale = new Vector3(scaleTo, scaleTo, 1f);
	}

	public zbRestaurantTarget()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		m_awardTaskAwards = new zbItem[4];
		m_spriteTargets = (VisualElement[])(object)new VisualElement[3];
		m_spriteFoods = (VisualElement[])(object)new VisualElement[3];
		m_lbFoodLvs = (Label[])(object)new Label[3];
		m_spriteFoodGets = (VisualElement[])(object)new VisualElement[3];
		m_btnCanOffers = (VisualElement[])(object)new Button[3];
		m_normalAwards = new zbItem[4];
		OuterPosition = new Vector3(4.9f, 0.2f, 0f);
		InerPosition = new Vector3(2.4f, 0.2f, 0f);
		base._002Ector();
	}
}
