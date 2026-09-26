using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbInfiniteGiftItem : zbViewBaseItem
{
	private VisualElement m_root;

	private VisualElement m_bgCur;

	private zbItem[] m_items;

	private Label m_lblPrice;

	private VisualElement m_lock;

	private VisualElement m_score;

	private VisualElement m_scoreFly;

	private Label m_lblScore;

	private VisualElement m_btnTouch;

	private zbInfiniteGiftWnd m_wndParent;

	private bool isFlying;

	private string m_uuid;

	private InfiniteGiftGoodsDetail m_giftGoodsDetail1;

	public zbInfiniteGiftItem(zbInfiniteGiftWnd parent)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		m_items = new zbItem[2];
		base._002Ector();
		((VisualElement)this).style.width = StyleLength.op_Implicit(324f);
		((VisualElement)this).style.height = StyleLength.op_Implicit(192f);
		((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		((VisualElement)this).style.left = StyleLength.op_Implicit(0f);
		((VisualElement)this).style.top = StyleLength.op_Implicit(0f);
		m_wndParent = parent;
		VisualElement val = (VisualElement)(object)zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_infinitegiftItem.uxml").Instantiate();
		m_root = UQueryExtensions.Q<VisualElement>(val, "item", (string)null);
		((VisualElement)this).Add(m_root);
		m_bgCur = UQueryExtensions.Q<VisualElement>(m_root, "bg_cur", (string)null);
		m_bgCur.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(m_root, "awarditems", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(val2, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		m_lblPrice = UQueryExtensions.Q<Label>(m_root, "lblprice", (string)null);
		m_lock = UQueryExtensions.Q<VisualElement>(m_root, "lock", (string)null);
		m_score = UQueryExtensions.Q<VisualElement>(m_root, "score", (string)null);
		m_scoreFly = UQueryExtensions.Q<VisualElement>(m_root, "score_fly", (string)null);
		m_lblScore = UQueryExtensions.Q<Label>(m_score, "lblscore", (string)null);
		m_btnTouch = UQueryExtensions.Q<VisualElement>(m_root, "btntouch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>(m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(InfiniteGiftGoodsDetail giftGoodsDetail, string UUID)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		m_uuid = UUID;
		m_giftGoodsDetail1 = giftGoodsDetail;
		if (m_giftGoodsDetail1 != null)
		{
			InfiniteGiftGoodsDetail infiniteGiftCurGood = zbSelf.Instance.operateHandler.GetInfiniteGiftCurGood(m_uuid);
			if (infiniteGiftCurGood != null && infiniteGiftCurGood.Index == m_giftGoodsDetail1.Index)
			{
				m_bgCur.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_lock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			else
			{
				m_bgCur.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_lock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_giftGoodsDetail1.State == ECommonState.End));
			}
			m_root.SetEnabled(m_giftGoodsDetail1.State != ECommonState.End);
			if (m_giftGoodsDetail1.GoodsId == 0)
			{
				((TextElement)m_lblPrice).text = zbUtil.Language("lang.10326");
			}
			else
			{
				string priceByGoodsId = zbSelf.Instance.vipHandler.GetPriceByGoodsId(m_giftGoodsDetail1.GoodsId);
				((TextElement)m_lblPrice).text = priceByGoodsId;
			}
			if (m_giftGoodsDetail1.State == ECommonState.End)
			{
				((TextElement)m_lblPrice).text = zbUtil.Language("lang.10148");
			}
			RefreshAward();
		}
	}

	private void RefreshAward()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 2; i++)
		{
			bool flag = i < m_giftGoodsDetail1.AwardList.Count;
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				AwardDetail awardDetail = m_giftGoodsDetail1.AwardList[i];
				m_items[i].Refresh(new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count));
			}
		}
		bool flag2 = m_giftGoodsDetail1.Score > 0;
		m_score.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		m_scoreFly.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		((TextElement)m_lblScore).text = "+" + m_giftGoodsDetail1.Score;
	}

	private void OnTouch()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (m_giftGoodsDetail1 == null || m_uuid == null || m_giftGoodsDetail1.State == ECommonState.End)
		{
			return;
		}
		if ((int)m_lock.resolvedStyle.display == 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10327"));
		}
		else if (m_giftGoodsDetail1.GoodsId == 0)
		{
			((Action)delegate
			{
				zbWsMsgHandler.Instance.Operate_InfiniteGiftAward_Req(m_uuid, m_giftGoodsDetail1.Index);
				FlyToTarget(m_scoreFly, m_wndParent.icon_flyTarget, m_wndParent.GetRootElement());
			})();
		}
		else
		{
			zbChannel_Pay.OnPay(m_giftGoodsDetail1.GoodsId);
		}
	}

	private void FlyToTarget(VisualElement from, VisualElement to, VisualElement container)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		if (!isFlying)
		{
			isFlying = true;
			VisualElement flying = new VisualElement();
			flying.style.width = StyleLength.op_Implicit(from.resolvedStyle.width);
			flying.style.height = StyleLength.op_Implicit(from.resolvedStyle.height);
			flying.style.backgroundImage = StyleBackground.op_Implicit(from.resolvedStyle.backgroundImage);
			flying.style.position = StyleEnum<Position>.op_Implicit((Position)1);
			container.Add(flying);
			Rect worldBound = from.worldBound;
			float x = ((Rect)(ref worldBound)).center.x;
			worldBound = from.worldBound;
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(x, ((Rect)(ref worldBound)).center.y, 0f);
			worldBound = to.worldBound;
			float num = ((Rect)(ref worldBound)).center.x - 15f;
			worldBound = to.worldBound;
			Vector3 val2 = default(Vector3);
			((Vector3)(ref val2))._002Ector(num, ((Rect)(ref worldBound)).center.y - 15f, 0f);
			Vector3 position = Vector2.op_Implicit(VisualElementExtensions.WorldToLocal(container, Vector2.op_Implicit(val)));
			Vector3 val3 = Vector2.op_Implicit(VisualElementExtensions.WorldToLocal(container, Vector2.op_Implicit(val2)));
			flying.transform.position = position;
			flying.experimental.animation.Position(val3, 1500);
			flying.schedule.Execute((Action)delegate
			{
				container.Remove(flying);
				isFlying = false;
			}).ExecuteLater(1500L);
		}
	}
}
