using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGrowthGiftWnd : zbBaseWnd
{
	private VisualElement m_ui;

	private Label m_lblTitle;

	private Button m_btnClose;

	private Button m_btnBuy;

	private Button m_btnLeft;

	private Button m_btnRight;

	private Label m_lblTime;

	private Label m_lbBonus;

	private zbItem[] m_items = new zbItem[3];

	private VisualElement m_tabs;

	private Button[] m_btnTabs = (Button[])(object)new Button[3];

	private SpineVisualElement[] m_tabsEffect = (SpineVisualElement[])(object)new SpineVisualElement[3];

	private Label[] m_lbTabsTime = (Label[])(object)new Label[3];

	private Button m_btnTouch;

	private VisualElement m_dark;

	private GrowthGiftShopDetail m_giftShopDetail;

	private SpineVisualElement m_spine;

	private Coroutine m_stateCoroutine;

	private bool m_isAnimating;

	private VisualElement m_descWrapper;

	private zbItem[] m_descItems = new zbItem[3];

	private VisualElement[] m_desces = (VisualElement[])(object)new VisualElement[3];

	private Label[] m_lbDesces = (Label[])(object)new Label[3];

	private bool m_needHide;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_ui = UQueryExtensions.Q<VisualElement>(m_RootElement, "ui", (string)null);
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_right", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbBonus = UQueryExtensions.Q<Label>(m_RootElement, "lb_bonus", (string)null);
		m_lblTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lblTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_dark = UQueryExtensions.Q<VisualElement>(m_RootElement, "dark", (string)null);
		m_tabs = UQueryExtensions.Q<VisualElement>(m_RootElement, "tabs", (string)null);
		m_descWrapper = UQueryExtensions.Q<VisualElement>(m_RootElement, "descWrapper", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "awarditems", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(val, "item" + i, (string)null);
			m_items[i].OnEnable(this);
			m_btnTabs[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
			m_tabsEffect[i] = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btnTabs[i], "spineEffect", (string)null);
			m_lbTabsTime[i] = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTabs[i], "lb_time", (string)null);
			m_desces[i] = UQueryExtensions.Q<VisualElement>(m_descWrapper, "desc" + i, (string)null);
			m_descItems[i] = UQueryExtensions.Q<zbItem>(m_descWrapper, "item" + i, (string)null);
			m_descItems[i].OnEnable(this);
			m_lbDesces[i] = UQueryExtensions.Q<Label>(m_descWrapper, "lb_desc" + i, (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTabs[i], (Action)delegate
			{
				OnTab(tempIndex);
			}, "", (TrickleDown)0);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action)OnBtnLeft, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action)OnBtnRight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action)OnBtnBuy, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_GrowthGift, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnEventUpdate);
	}

	public void RefreshCustom(int goodsId)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		m_needHide = false;
		if (zbSelf.Instance.growthGiftHandler.GetGrowthGiftList().Count == 0)
		{
			Hide();
			return;
		}
		bool flag = goodsId == 501001;
		m_descWrapper.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (flag)
		{
			int[][] awardList = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(goodsId).awardList;
			for (int i = 0; i < 3; i++)
			{
				bool flag2 = i < awardList.Count();
				m_desces[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
				if (flag2)
				{
					zbItemData data = new zbItemData((EResourceType)awardList[i][0], awardList[i][1], awardList[i][2]);
					m_descItems[i].Refresh(data);
					stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)awardList[i][0], awardList[i][1]);
					((TextElement)m_lbDesces[i]).text = resorceIconPath.strDesc;
				}
			}
		}
		m_dark.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_ui.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbSelf.Instance.growthGiftHandler.hasNotify = false;
		m_giftShopDetail = zbSelf.Instance.growthGiftHandler.GetGrowthGiftList().FirstOrDefault((GrowthGiftShopDetail gift) => gift.GoodsId == goodsId);
		if (m_giftShopDetail == null)
		{
			m_giftShopDetail = zbSelf.Instance.growthGiftHandler.GetGrowthGiftList()[0];
		}
		RefreshDetail();
		zbGiftTaskCsvRecord zbGiftTaskCsvRecord2 = zbDataConfig.Instance.giftTaskCsvTable.GetRecords().records.FirstOrDefault((zbGiftTaskCsvRecord record) => record.goodsId == m_giftShopDetail.GoodsId);
		m_spine.skeletonDataAsset = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(zbGiftTaskCsvRecord2.animPath);
		m_spine.Initialize(true);
		m_spine.AnimationState.ClearTrack(0);
		m_spine.AnimationState.SetAnimation(0, "in", false);
		m_spine.AnimationState.AddAnimation(0, "loop", true, 0f);
		((TextElement)m_lbBonus).text = zbGiftTaskCsvRecord2.bonus;
		zbAudioMgr.Instance.PlayAudio("audio/ui/rise_normal.ogg");
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		m_isAnimating = true;
		m_stateCoroutine = ((MonoBehaviour)this).StartCoroutine(FadeIn());
	}

	private IEnumerator FadeIn()
	{
		yield return (object)new WaitForSeconds(0.5f);
		m_ui.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		yield return null;
		yield return null;
		yield return null;
		m_isAnimating = false;
		m_stateCoroutine = null;
		if (m_needHide)
		{
			m_needHide = false;
			StartHideAnimation();
		}
	}

	private void RefreshDetail()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		List<GrowthGiftShopDetail> growthGiftList = zbSelf.Instance.growthGiftHandler.GetGrowthGiftList();
		bool flag = growthGiftList.Count > 1;
		((VisualElement)m_btnLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_btnRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		zbVipShopCsvRecord recordByKey = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(m_giftShopDetail.GoodsId);
		((TextElement)m_lblTitle).text = zbUtil.Language(recordByKey.goodsName);
		for (int i = 0; i < 3; i++)
		{
			bool flag2 = i < recordByKey.awardList.Count();
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			if (flag2)
			{
				int[] array = recordByKey.awardList[i];
				zbItemData data = new zbItemData((EResourceType)array[0], array[1], array[2]);
				m_items[i].Refresh(data);
			}
		}
		((TextElement)m_btnBuy).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(m_giftShopDetail.GoodsId);
		bool flag3 = growthGiftList.Count > 1;
		m_tabs.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
		if (!flag3)
		{
			return;
		}
		int j;
		for (j = 0; j < 3; j++)
		{
			bool flag4 = j < growthGiftList.Count;
			((VisualElement)m_btnTabs[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag4));
			if (flag4)
			{
				zbGiftTaskCsvRecord zbGiftTaskCsvRecord2 = zbDataConfig.Instance.giftTaskCsvTable.GetRecords().records.FirstOrDefault((zbGiftTaskCsvRecord record) => record.goodsId == zbSelf.Instance.growthGiftHandler.GetGrowthGiftList()[j].GoodsId);
				bool flag5 = growthGiftList[j].GoodsId == m_giftShopDetail.GoodsId;
				((VisualElement)m_tabsEffect[j]).style.scale = StyleScale.op_Implicit(new Vector2(flag5 ? 1.5f : 1.2f, flag5 ? 1.5f : 1.2f));
				m_tabsEffect[j].skeletonDataAsset = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(zbGiftTaskCsvRecord2.iconPath);
				m_tabsEffect[j].Initialize(true);
				m_tabsEffect[j].AnimationState.ClearTrack(0);
				m_tabsEffect[j].AnimationState.SetAnimation(0, flag5 ? "animation2" : "animation", true);
			}
		}
	}

	private void OnBtnLeft()
	{
		RefreshCustom(GetPreGoodsId());
	}

	private void OnBtnRight()
	{
		RefreshCustom(GetNextGoodsId());
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		RefreshLeftTime();
		RefreshTabsTime();
	}

	private void RefreshTabsTime()
	{
		List<GrowthGiftShopDetail> growthGiftList = zbSelf.Instance.growthGiftHandler.GetGrowthGiftList();
		if (growthGiftList.Count < 1)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			if (i < growthGiftList.Count)
			{
				((TextElement)m_lbTabsTime[i]).text = zbUtil.FormatTimeHHMMSS(growthGiftList[i].ExpireTm - zbSelf.Instance.timeHandler.GetServerTime());
			}
		}
	}

	private void RefreshLeftTime()
	{
		long num = m_giftShopDetail.ExpireTm - zbSelf.Instance.timeHandler.GetServerTime();
		if (num < 0)
		{
			GrowthGiftShopDetail growthGiftShopDetail = zbSelf.Instance.growthGiftHandler.FindEarliestExpiringGrowthGift();
			if (growthGiftShopDetail != null)
			{
				RefreshCustom(growthGiftShopDetail.GoodsId);
			}
			else
			{
				StartHideAnimation();
			}
		}
		else
		{
			((TextElement)m_lblTime).text = zbUtil.FormatTimeHHMMSS(num);
		}
	}

	private int GetNextGoodsId()
	{
		List<GrowthGiftShopDetail> growthGiftList = zbSelf.Instance.growthGiftHandler.GetGrowthGiftList();
		if (growthGiftList.Count == 0 || growthGiftList.Count == 1)
		{
			return -1;
		}
		int num = growthGiftList.FindIndex((GrowthGiftShopDetail gift) => gift.GoodsId == m_giftShopDetail.GoodsId);
		return growthGiftList[(num + 1 < growthGiftList.Count) ? (num + 1) : 0].GoodsId;
	}

	private int GetPreGoodsId()
	{
		List<GrowthGiftShopDetail> growthGiftList = zbSelf.Instance.growthGiftHandler.GetGrowthGiftList();
		if (growthGiftList.Count == 0 || growthGiftList.Count == 1)
		{
			return -1;
		}
		int num = growthGiftList.FindIndex((GrowthGiftShopDetail gift) => gift.GoodsId == m_giftShopDetail.GoodsId);
		return growthGiftList[(num - 1 < 0) ? (growthGiftList.Count - 1) : (num - 1)].GoodsId;
	}

	private void OnBtnBuy()
	{
		if (m_giftShopDetail != null)
		{
			zbChannel_Pay.OnPay(m_giftShopDetail.GoodsId);
		}
	}

	private void OnEventUpdate(object data)
	{
		GrowthGiftShopDetail growthGiftShopDetail = zbSelf.Instance.growthGiftHandler.FindEarliestExpiringGrowthGift();
		if (growthGiftShopDetail != null)
		{
			RefreshCustom(growthGiftShopDetail.GoodsId);
			return;
		}
		m_needHide = true;
		StartHideAnimation();
	}

	private void OnTab(int index)
	{
		if (m_giftShopDetail.GoodsId != zbSelf.Instance.growthGiftHandler.GetGrowthGiftList()[index].GoodsId)
		{
			RefreshCustom(zbSelf.Instance.growthGiftHandler.GetGrowthGiftList()[index].GoodsId);
		}
	}

	private void OnClose(ClickEvent evt)
	{
		StartHideAnimation();
	}

	private void StartHideAnimation()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (m_isAnimating)
		{
			m_needHide = true;
			return;
		}
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		Vector2 targetWorldPos = Vector2.zero;
		if ((Object)(object)zbMainLevelWnd2 != (Object)null)
		{
			Rect worldBound = ((VisualElement)zbMainLevelWnd2.m_btnGrowthGift).worldBound;
			targetWorldPos = ((Rect)(ref worldBound)).center;
		}
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(DoHideAnim(targetWorldPos));
	}

	private IEnumerator DoHideAnim(Vector2 targetWorldPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		m_isAnimating = true;
		m_dark.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		float duration = 0.35f;
		float elapsed = 0f;
		Rect worldBound = m_ui.worldBound;
		Vector2 center = ((Rect)(ref worldBound)).center;
		Vector3 targetDelta = Vector2.op_Implicit(targetWorldPos - center);
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			m_ui.style.opacity = StyleFloat.op_Implicit(1f - num);
			((VisualElement)m_spine).style.opacity = StyleFloat.op_Implicit(1f - num);
			float num2 = Mathf.Lerp(1f, 0.1f, num);
			m_ui.style.transformOrigin = StyleTransformOrigin.op_Implicit(new TransformOrigin(Length.Percent(50f), Length.Percent(50f)));
			((VisualElement)m_spine).style.transformOrigin = StyleTransformOrigin.op_Implicit(new TransformOrigin(Length.Percent(50f), Length.Percent(50f)));
			m_ui.style.scale = StyleScale.op_Implicit(new Scale(new Vector3(num2, num2, 1f)));
			((VisualElement)m_spine).style.scale = StyleScale.op_Implicit(new Scale(new Vector3(num2, num2, 1f)));
			float num3 = Mathf.Lerp(0f, targetDelta.x, num);
			float num4 = Mathf.Lerp(0f, targetDelta.y, num);
			m_ui.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(num3), Length.op_Implicit(num4), 0f));
			((VisualElement)m_spine).style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(num3), Length.op_Implicit(num4), 0f));
			yield return null;
		}
		m_isAnimating = false;
		Hide();
		zbGameSave.Instance.SetLong(zbSelf.Instance.growthGiftHandler.growthGiftPopupKey, zbSelf.Instance.timeHandler.GetServerTime());
		ResetUIStyle();
	}

	private void ResetUIStyle()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		m_ui.style.opacity = StyleFloat.op_Implicit(1f);
		((VisualElement)m_spine).style.opacity = StyleFloat.op_Implicit(1f);
		m_ui.style.scale = new StyleScale(new Scale(Vector3.one));
		((VisualElement)m_spine).style.scale = new StyleScale(new Scale(Vector3.one));
		m_ui.style.translate = new StyleTranslate(new Translate(Length.op_Implicit(0f), Length.op_Implicit(0f), 0f));
		((VisualElement)m_spine).style.translate = new StyleTranslate(new Translate(Length.op_Implicit(0f), Length.op_Implicit(0f), 0f));
	}

	private void OnTouch()
	{
		if (!m_isAnimating)
		{
			StartHideAnimation();
		}
	}
}
