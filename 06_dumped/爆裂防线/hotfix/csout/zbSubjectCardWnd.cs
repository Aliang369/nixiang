using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSubjectCardWnd : zbBaseWnd
{
	private SpineVisualElement m_spine;

	private VisualElement m_bg;

	private Label m_lbTitle;

	private VisualElement m_btnTip;

	private VisualElement m_spriteOk;

	private Button m_btnTouch;

	private VisualElement m_weekCard;

	private Label m_lbWeekDiamond;

	private Label m_lbWeekState;

	private zbItem[] m_weekItems = new zbItem[4];

	private Button m_btnWeekBuy;

	private Label m_lbWeekBuyDay;

	private Label m_lbWeekBuyPrice;

	private VisualElement m_nodeWeekBought;

	private VisualElement m_progressWeek;

	private Label m_lbProgressWeek;

	private Button m_btnMainQuest;

	private Button m_btnElite;

	private Button m_btnLegendary;

	private Button m_btnFacility;

	private int m_goodsId;

	private int m_day;

	private VipCardShopDetail m_weekCardInfo;

	private Action<bool> m_callback;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_btnTip = UQueryExtensions.Q<VisualElement>(m_RootElement, "tip", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnTip, (Action<ClickEvent>)OnTip, "", (TrickleDown)0);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_weekCard = UQueryExtensions.Q<VisualElement>(m_RootElement, "week", (string)null);
		m_lbWeekDiamond = UQueryExtensions.Q<Label>(m_weekCard, "lb_diamond", (string)null);
		m_lbWeekState = UQueryExtensions.Q<Label>(m_weekCard, "lb_state", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_weekItems[i] = UQueryExtensions.Q<zbItem>(m_weekCard, "item" + i, (string)null);
			m_weekItems[i].OnEnable(this);
		}
		m_btnWeekBuy = UQueryExtensions.Q<Button>(m_weekCard, "btn_buy", (string)null);
		m_lbWeekBuyDay = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnWeekBuy, "lb_buyDay", (string)null);
		m_lbWeekBuyPrice = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnWeekBuy, "lb_buyPrice", (string)null);
		m_nodeWeekBought = UQueryExtensions.Q<VisualElement>(m_weekCard, "bought", (string)null);
		m_progressWeek = UQueryExtensions.Q<VisualElement>(m_nodeWeekBought, "progress", (string)null);
		m_lbProgressWeek = UQueryExtensions.Q<Label>(m_nodeWeekBought, "lb_progress", (string)null);
		m_btnMainQuest = UQueryExtensions.Q<Button>(m_weekCard, "btn_mainQuest", (string)null);
		m_btnElite = UQueryExtensions.Q<Button>(m_weekCard, "btn_elite", (string)null);
		m_btnLegendary = UQueryExtensions.Q<Button>(m_weekCard, "btn_legendary", (string)null);
		m_btnFacility = UQueryExtensions.Q<Button>(m_weekCard, "btn_facility", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnWeekBuy, (Action)OnWeekBuy, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMainQuest, (Action<ClickEvent>)delegate
		{
			OnDoubleTip(EFuncType.MainQuest);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnElite, (Action<ClickEvent>)delegate
		{
			OnDoubleTip(EFuncType.EliteQuest);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLegendary, (Action<ClickEvent>)delegate
		{
			OnDoubleTip(EFuncType.Legendary);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFacility, (Action<ClickEvent>)delegate
		{
			OnDoubleTip(EFuncType.Facility);
		}, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, OnEventUpdate);
		InitRecord();
		AddDark();
		AddDark();
	}

	public void RefreshCustom(Action<bool> callback)
	{
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		m_callback = callback;
		m_weekCardInfo = zbSelf.Instance.vipHandler.GetVipCardShopDetail(m_goodsId);
		if (m_weekCardInfo != null && m_weekCardInfo.DailyAward != EActiveState.Pending)
		{
			((VisualElement)m_btnWeekBuy).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeWeekBought.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			int num = (int)Math.Ceiling((double)(m_weekCardInfo.ExpireTm - zbSelf.Instance.timeHandler.GetServerTime()) / 86400.0);
			float num2 = Mathf.Clamp((float)num / (float)m_day, 0f, 1f);
			m_progressWeek.style.width = StyleLength.op_Implicit(new Length(num2 * 636f, (LengthUnit)0));
			((TextElement)m_lbProgressWeek).text = string.Format(zbUtil.Language("lang.10604"), num);
			if (m_weekCardInfo.DailyAward == EActiveState.Active)
			{
				((TextElement)m_lbWeekState).text = zbUtil.Language("lang.10606");
			}
			else
			{
				((TextElement)m_lbWeekState).text = zbUtil.Language("lang.10607");
			}
			for (int i = 0; i < 4; i++)
			{
				m_weekItems[i].SetSpineVisible(m_weekCardInfo.DailyAward == EActiveState.Active);
				m_weekItems[i].SetGettedVisible(m_weekCardInfo.DailyAward != EActiveState.Active);
				if (m_weekCardInfo.DailyAward == EActiveState.Active)
				{
					m_weekItems[i].SetTouchCallback(delegate
					{
						OnWeekGet();
					});
				}
				else
				{
					m_weekItems[i].SetTouchCallback(null);
				}
			}
		}
		else
		{
			((VisualElement)m_btnWeekBuy).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_nodeWeekBought.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbWeekState).text = zbUtil.Language("lang.10605");
		}
		m_weekCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_weekCard.style.opacity = StyleFloat.op_Implicit(0f);
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spine.AnimationState.SetAnimation(0, "in", false);
		m_spine.AnimationState.AddAnimation(0, "animation", true, 0f);
		zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
		((MonoBehaviour)this).StartCoroutine(FadeInEx());
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_award.ogg");
		((MonoBehaviour)this).StartCoroutine(FadeInEx());
	}

	private IEnumerator FadeInEx()
	{
		yield return (object)new WaitForSeconds(4f / 15f);
		((MonoBehaviour)this).StartCoroutine(FadeCoroutine(m_weekCard, 0f, 1f, 17f / 30f));
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private IEnumerator FadeCoroutine(VisualElement element, float startOpacity, float endOpacity, float duration)
	{
		float time = 0f;
		while (time < duration)
		{
			float num = Mathf.SmoothStep(startOpacity, endOpacity, time / duration);
			element.style.opacity = StyleFloat.op_Implicit(num);
			time += Time.deltaTime;
			yield return null;
		}
		element.style.opacity = StyleFloat.op_Implicit(endOpacity);
	}

	private void InitRecord()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 4; i++)
		{
			((VisualElement)m_weekItems[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		m_goodsId = (int)zbSelf.Instance.vipHandler.m_vipBaseRecord["vipCard"][(object)"resource"][(object)"goodsId"];
		m_day = (int)zbSelf.Instance.vipHandler.m_vipBaseRecord["vipCard"][(object)"resource"][(object)"cardDays"];
		foreach (JToken item in (JArray)zbSelf.Instance.vipHandler.m_vipBaseRecord["vipCard"][(object)"resource"][(object)"dailyAward"])
		{
			if (num < m_weekItems.Length)
			{
				((VisualElement)m_weekItems[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				zbItemData data = new zbItemData((EResourceType)(int)item[(object)0], (int)item[(object)1], (int)item[(object)2]);
				m_weekItems[num].Refresh(data);
			}
			num++;
		}
		zbVipShopCsvRecord recordByKey = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(m_goodsId);
		if (recordByKey != null)
		{
			int[] array = recordByKey.awardList[0];
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)array[0], array[1]);
			((VisualElement)m_lbWeekDiamond).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
			((TextElement)m_lbWeekDiamond).text = "x" + array[2];
		}
		((TextElement)m_lbWeekBuyPrice).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(m_goodsId);
		((TextElement)m_lbWeekBuyDay).text = string.Format(zbUtil.Language("lang.10603"), m_day);
	}

	private void OnClose()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (m_spriteOk.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			zbGameSave.Instance.SetString("SubjectDate" + zbSelf.Instance.GetRoleId(), zbSelf.Instance.timeHandler.GetServerDateStr());
		}
		m_callback?.Invoke(obj: true);
		m_weekCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		Hide();
	}

	private void OnWeekBuy()
	{
		if (m_weekCardInfo != null)
		{
			zbChannel_Pay.OnPay(m_weekCardInfo.GoodsId);
		}
	}

	private void OnWeekGet()
	{
		zbWsMsgHandler.Instance.RechargeShop_VipCardAward_Req(m_weekCardInfo.GoodsId);
	}

	private void OnDoubleTip(EFuncType type)
	{
		if (type == EFuncType.MainQuest)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11359"), zbUtil.Language("lang.11054")));
			return;
		}
		zbFunctionUnlockCsvRecord recordByKey = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey((int)type);
		if (recordByKey != null)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11359"), recordByKey.name));
		}
	}

	private void OnTip(ClickEvent evt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = m_spriteOk.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
	}

	private void OnEventUpdate(object data)
	{
		OnClose();
	}
}
