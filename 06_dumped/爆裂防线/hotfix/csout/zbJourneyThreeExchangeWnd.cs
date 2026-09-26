using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbJourneyThreeExchangeWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbTime;

	private Label m_lbItemName;

	private Button m_btnClose;

	private ScrollView m_scrollView;

	private VisualElement m_container;

	private zbItem m_showAward;

	private Button m_btnBuy;

	private Label m_lbPrice;

	private VisualElement m_iconMoneyInBtn;

	private Button m_btnTouch;

	private Button m_btnBag;

	private Button m_btnLeft;

	private Button m_btnRight;

	private VisualElement m_btnTip;

	private VisualElement m_spriteOk;

	private zbPlayerAvatar m_suitAvatar;

	private SkeletonAnimation m_gunAvatar;

	private SkeletonAnimation m_heroAvatar;

	private SkeletonAnimation m_animationChange;

	private SkeletonAnimation m_mecha;

	private Coroutine m_rotateCoroutine;

	private Coroutine m_resumeCoroutine;

	private float m_interval = 6f;

	private bool m_isPause;

	private int m_Select;

	private int m_curIndex;

	private JourneyCountRecordDetail m_data;

	private zbChapterThreeExchangeCsvRecord m_record;

	private Coroutine m_animCoroutine;

	protected override void OnEnable()
	{
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_suitAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		m_gunAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("GunAnimation")).GetComponent<SkeletonAnimation>();
		m_heroAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("heroAvatar")).GetComponent<SkeletonAnimation>();
		m_animationChange = ((Component)m_GameObjectSceneRoot.transform.Find("animationChange")).GetComponent<SkeletonAnimation>();
		m_mecha = ((Component)m_GameObjectSceneRoot.transform.Find("mecha")).GetComponent<SkeletonAnimation>();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.11174");
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbItemName = UQueryExtensions.Q<Label>(m_RootElement, "lb_itemName", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnBag = UQueryExtensions.Q<Button>(m_RootElement, "btn_bag", (string)null);
		m_iconMoneyInBtn = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnBuy, "icon_money", (string)null);
		m_lbPrice = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnBuy, "lb_price", (string)null);
		m_showAward = UQueryExtensions.Q<zbItem>(m_RootElement, "showAward", (string)null);
		m_showAward.OnEnable(this);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollView", (string)null);
		m_scrollView.mode = (ScrollViewMode)1;
		((VisualElement)m_scrollView).contentContainer.style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)2);
		m_container = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_scrollView, "unity-content-container", (string)null);
		m_btnTip = UQueryExtensions.Q<VisualElement>(m_RootElement, "tip", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_right", (string)null);
		m_rotateCoroutine = ((MonoBehaviour)this).StartCoroutine(UpdateAwardShow());
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action)OnBuy, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBag, (Action)OnBag, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnTip, (Action<ClickEvent>)OnTip, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action)OnPre, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action)OnNext, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.CrystalBall });
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_JourneyInfo, OnEventUpdate);
	}

	private IEnumerator UpdateAwardShow()
	{
		RefreshAnimation();
		while (true)
		{
			if (m_isPause)
			{
				yield return null;
				continue;
			}
			yield return (object)new WaitForSeconds(m_interval);
			if (!m_isPause)
			{
				int[] array = m_record?.showList;
				if (array != null && array.Length != 0)
				{
					m_curIndex = (m_curIndex + 1) % array.Length;
					RefreshAnimation();
				}
			}
		}
	}

	private void OnTouch()
	{
		m_isPause = true;
		if (m_record == null)
		{
			return;
		}
		switch (m_record.showType)
		{
		case 1:
			((Component)m_heroAvatar).gameObject.SetActive(true);
			zbUIMgr.Instance.ShowPreview(EPreviewType.Hero, m_record.showList[m_curIndex]);
			break;
		case 2:
		{
			((Component)m_suitAvatar).gameObject.SetActive(true);
			zbPlayerSuitCsvRecord recordByKey2 = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(m_record.showList[m_curIndex]);
			if (recordByKey2 != null)
			{
				zbUIMgr.Instance.ShowPreview(EPreviewType.Suit, recordByKey2.propId);
			}
			break;
		}
		case 3:
		{
			((Component)m_gunAvatar).gameObject.SetActive(true);
			zbPlayerGunCsvRecord recordByKey3 = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(m_record.showList[m_curIndex]);
			if (recordByKey3 != null)
			{
				zbUIMgr.Instance.ShowPreview(EPreviewType.Gun, recordByKey3.propId);
			}
			break;
		}
		case 4:
		{
			((Component)m_mecha).gameObject.SetActive(true);
			zbMechaAvatarCsvRecord recordByKey = zbDataConfig.Instance.mechaAvatarCsvTable.GetRecordByKey(m_record.showList[m_curIndex]);
			if (recordByKey != null)
			{
				zbUIMgr.Instance.ShowPreview(EPreviewType.Mecha, recordByKey.avatarId);
			}
			break;
		}
		default:
			zbUIMgr.Instance.ShowTip((EResourceType)m_record.award[0], m_record.award[1], 0L);
			break;
		}
	}

	private void RefreshShowAward()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		if (m_record == null)
		{
			return;
		}
		((TextElement)m_lbItemName).text = zbUtil.Language(zbResourceHelper.GetResorceIconPath((EResourceType)m_record.award[0], m_record.award[1]).strName);
		GameObject[] array = (GameObject[])(object)new GameObject[4]
		{
			((Component)m_gunAvatar).gameObject,
			((Component)m_heroAvatar).gameObject,
			((Component)m_suitAvatar).gameObject,
			((Component)m_mecha).gameObject
		};
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
		((VisualElement)m_showAward).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		switch (m_record.showType)
		{
		case 1:
		{
			((Component)m_heroAvatar).gameObject.SetActive(true);
			zbHeroCsvRecord recordByKey4 = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_record.showList[m_curIndex]);
			zbHeroAvatarCsvRecord recordByKey5 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey4.avatarId);
			if (recordByKey5 != null)
			{
				string spinePath2 = recordByKey5.frontBone + "_SkeletonData.asset";
				zbUtil.RefreshSpine(m_heroAvatar, spinePath2, "idle", loop: true);
			}
			break;
		}
		case 2:
		{
			((Component)m_suitAvatar).gameObject.SetActive(true);
			zbPlayerSuitCsvRecord recordByKey2 = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(m_record.showList[m_curIndex]);
			if (recordByKey2 != null)
			{
				m_suitAvatar.RefreshCustom(recordByKey2.avatarId, zbSelf.Instance.GetGunId());
			}
			break;
		}
		case 3:
		{
			((Component)m_gunAvatar).gameObject.SetActive(true);
			zbPlayerGunCsvRecord recordByKey3 = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(m_record.showList[m_curIndex]);
			if (recordByKey3 != null)
			{
				zbUtil.RefreshSpine(m_gunAvatar, recordByKey3.frontBone + "_SkeletonData.asset", "animation", loop: true);
			}
			break;
		}
		case 4:
		{
			((Component)m_mecha).gameObject.SetActive(true);
			zbMechaAvatarCsvRecord recordByKey = zbDataConfig.Instance.mechaAvatarCsvTable.GetRecordByKey(m_record.showList[m_curIndex]);
			if (recordByKey != null)
			{
				string spinePath = recordByKey.fightBone + "_SkeletonData.asset";
				zbUtil.RefreshSpine(m_mecha, spinePath, "idle", loop: true);
			}
			break;
		}
		default:
		{
			zbItemData data = new zbItemData((EResourceType)m_record.award[0], m_record.award[1], m_record.award[2]);
			((VisualElement)m_showAward).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_showAward.Refresh(data);
			break;
		}
		}
	}

	private void RefreshAnimation()
	{
		if (m_animCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_animCoroutine);
		}
		m_animCoroutine = ((MonoBehaviour)this).StartCoroutine(SpineAnimation());
	}

	private IEnumerator SpineAnimation()
	{
		m_animationChange.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSeconds(0.4f);
		RefreshShowAward();
	}

	public override void Refresh()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.Refresh();
		m_Select = 0;
		m_curIndex = 0;
		m_scrollView.scrollOffset = new Vector2(0f, 0f);
		RefreshCurrentData();
		if (m_rotateCoroutine == null)
		{
			m_rotateCoroutine = ((MonoBehaviour)this).StartCoroutine(UpdateAwardShow());
		}
		UpdateScrollView();
		RefreshLeftTime();
	}

	private void RefreshCurrentData()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		RepeatedField<JourneyCountRecordDetail> val = zbServerData.sGameData?.JourneyInfo?.ChapterThree?.Exchange?.ExchangeList;
		if (val == null || m_Select < 0 || m_Select >= val.Count)
		{
			m_data = null;
			m_record = null;
			m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			m_data = val[m_Select];
			m_record = zbDataConfig.Instance.chapterThreeExchangeCsvTable.GetRecordByKey(m_data.Id);
			bool exchangeRemindState = zbSelf.Instance.journeyThreeHandler.GetExchangeRemindState(m_data.Id);
			m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!exchangeRemindState));
		}
	}

	private void UpdateScrollView()
	{
		((VisualElement)m_scrollView).Clear();
		RepeatedField<JourneyCountRecordDetail> val = zbServerData.sGameData?.JourneyInfo?.ChapterThree?.Exchange?.ExchangeList;
		if (val != null && val.Count != 0)
		{
			for (int i = 0; i < val.Count; i++)
			{
				zbJourneyThreeExchangeItem zbJourneyThreeExchangeItem2 = new zbJourneyThreeExchangeItem();
				zbJourneyThreeExchangeItem2.OnEnable(this);
				zbJourneyThreeExchangeItem2.Init(val[i], i);
				((VisualElement)m_scrollView).Add((VisualElement)(object)zbJourneyThreeExchangeItem2);
			}
			SetSelect(m_Select);
		}
	}

	public void SetSelect(int index)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		m_Select = index;
		m_curIndex = 0;
		if (m_scrollView != null && m_container != null && m_container.childCount > 0)
		{
			List<zbJourneyThreeExchangeItem> list = UQueryExtensions.Query<zbJourneyThreeExchangeItem>(m_container, (string)null, (string)null).ToList();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].SetSelectBg(i == index);
			}
		}
		RefreshCurrentData();
		RefreshCurrentItem();
	}

	private void RefreshCurrentItem()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (m_record != null)
		{
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Money, m_record.moneyType);
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath);
			m_iconMoneyInBtn.style.backgroundImage = new StyleBackground(val);
			((TextElement)m_lbPrice).text = "X" + m_record.price;
			bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, m_record.moneyType, m_record.price);
			((VisualElement)m_lbPrice).style.color = StyleColor.op_Implicit(flag ? Color.white : Color.red);
			RefreshAnimation();
			((VisualElement)m_btnLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_record.showList.Length <= 1));
			((VisualElement)m_btnRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_record.showList.Length <= 1));
		}
	}

	private void OnBuy()
	{
		if (m_record != null && m_data != null)
		{
			if (m_record.limitNum - m_data.Count == 0)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10278"));
			}
			else if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(m_record.moneyType, m_record.price))
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, m_record.moneyType, m_record.price);
			}
			else
			{
				zbWsMsgHandler.Instance.Journey_ChapterThreeExchange_Req(m_data.Id, 1);
			}
		}
	}

	private void OnEventUpdate(object data)
	{
		m_curIndex = 0;
		RefreshCurrentData();
		if (m_rotateCoroutine == null)
		{
			m_rotateCoroutine = ((MonoBehaviour)this).StartCoroutine(UpdateAwardShow());
		}
		UpdateScrollView();
		RefreshLeftTime();
	}

	private void OnTip(ClickEvent evt)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (m_data != null)
		{
			bool flag = !zbSelf.Instance.journeyThreeHandler.GetExchangeRemindState(m_data.Id);
			zbSelf.Instance.journeyThreeHandler.UpdateJourneyThreeExchangeRemind(m_data.Id, flag);
			m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			UpdateScrollView();
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshLeftTime();
	}

	private void RefreshLeftTime()
	{
		long endLeftTime = zbSelf.Instance.journeyThreeHandler.GetEndLeftTime();
		if (endLeftTime > 0)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(endLeftTime);
		}
		else
		{
			OnClose();
		}
	}

	private void OnClose()
	{
		if (m_rotateCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_rotateCoroutine);
			m_rotateCoroutine = null;
		}
		Hide();
	}

	private void OnPre()
	{
		m_isPause = true;
		ChangeIndex(-1);
		StartResumeTimer();
	}

	private void OnNext()
	{
		m_isPause = true;
		ChangeIndex(1);
		StartResumeTimer();
	}

	private void ChangeIndex(int delta)
	{
		if (m_record?.showList != null && m_record.showList.Length != 0)
		{
			m_curIndex = (m_curIndex + delta + m_record.showList.Length) % m_record.showList.Length;
			RefreshAnimation();
		}
	}

	private void StartResumeTimer()
	{
		if (m_resumeCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_resumeCoroutine);
		}
		m_resumeCoroutine = ((MonoBehaviour)this).StartCoroutine(ResumeAfterDelay(3f));
	}

	private IEnumerator ResumeAfterDelay(float delay)
	{
		yield return (object)new WaitForSeconds(delay);
		ResumeRotate();
	}

	private void OnBag()
	{
		zbUIMgr.Instance.ShowBag();
	}

	private void ResumeRotate()
	{
		m_isPause = false;
		if (m_rotateCoroutine == null)
		{
			m_rotateCoroutine = ((MonoBehaviour)this).StartCoroutine(UpdateAwardShow());
		}
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		ResumeRotate();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		ResumeRotate();
	}
}
