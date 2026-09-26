using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbScratchCardWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnClose;

	private Label m_lbTime;

	private Label m_lbName;

	private Label m_lbDesc;

	private Button[] m_btnCards = (Button[])(object)new Button[9];

	private SpineVisualElement[] m_spines = (SpineVisualElement[])(object)new SpineVisualElement[9];

	private ListView m_listView;

	private Button m_btnResetFree;

	private Button m_btnResetDiamond;

	public ScratchCardDetail m_scratchCardDetail;

	private Sprite m_icon0;

	private Sprite m_icon1;

	private Sprite m_icon2;

	private Sprite m_icon3;

	private bool m_isScratched;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_scratechCard.uxml");
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnResetFree = UQueryExtensions.Q<Button>(m_RootElement, "btn_resetFree", (string)null);
		m_btnResetDiamond = UQueryExtensions.Q<Button>(m_RootElement, "btn_resetDiamond", (string)null);
		for (int i = 0; i < 9; i++)
		{
			int tempIndex = i;
			m_btnCards[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_card" + i, (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCards[i], (Action<ClickEvent>)delegate
			{
				OnClickCard(tempIndex + 1);
			}, "", (TrickleDown)0);
			m_spines[i] = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btnCards[i], "spine", (string)null);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnResetFree, (Action)OnReset, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnResetDiamond, (Action)OnReset, "", (TrickleDown)0);
		m_icon0 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/scratchCard/scratchCard_item_bg.png");
		m_icon1 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/scratchCard/scratchCard_item2.png");
		m_icon2 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/scratchCard/scratchCard_item1.png");
		m_icon3 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/scratchCard/scratchCard_item0.png");
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnUpdateEventOperate);
	}

	public void RefreshCustom(ScratchCardDetail scratchCardDetail)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		m_scratchCardDetail = scratchCardDetail;
		RefreshMoneyPanel();
		OperateScratchCardDetail[] targetList = ((IEnumerable<OperateScratchCardDetail>)scratchCardDetail.TargetList).OrderByDescending((OperateScratchCardDetail t) => t.State == ECommonState.Yes).ToArray();
		UpdateListView(targetList);
		for (int num = 0; num < 9; num++)
		{
			((VisualElement)m_btnCards[num]).style.backgroundImage = new StyleBackground(GetIconById(m_scratchCardDetail.ScratchList[num]));
			((VisualElement)m_spines[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!IsCardSpineShow(num)));
		}
		bool flag = m_scratchCardDetail.ResetDiamond == 0;
		((VisualElement)m_btnResetFree).visible = flag;
		((VisualElement)m_btnResetDiamond).visible = !flag;
		if (!flag)
		{
			((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnResetDiamond, "lb_diamondCost", (string)null)).text = m_scratchCardDetail.ResetDiamond.ToString();
		}
		m_isScratched = false;
		foreach (int scratch in m_scratchCardDetail.ScratchList)
		{
			if (scratch > 0)
			{
				m_isScratched = true;
				break;
			}
		}
	}

	private void RefreshMoneyPanel()
	{
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		long propId = m_scratchCardDetail.PropId;
		long countByPropTid = zbSelf.Instance.itemHandler.GetCountByPropTid((int)propId);
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Prop, propId);
		m_moneyPanel.RefreshOther(1, resorceIconPath.strIconPath, countByPropTid, delegate
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Prop, propId, 0L);
		});
	}

	private void UpdateListView(OperateScratchCardDetail[] targetList)
	{
		((VisualElement)m_listView).Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbScratchCardItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbScratchCardItem).Init(targetList[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = targetList;
	}

	protected override void UpdateAtShow()
	{
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		long startTm = m_scratchCardDetail.BaseInfo.StartTm;
		long endTm = m_scratchCardDetail.BaseInfo.EndTm;
		bool flag = startTm < serverTime && serverTime < endTm;
		((TextElement)m_lbTime).text = (flag ? zbUtil.FormatTimeHHMMSS(endTm - serverTime) : zbUtil.Language("lang.10043"));
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnClickCard(int index)
	{
		if (m_scratchCardDetail.ScratchList[index - 1] <= 0)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, m_scratchCardDetail.PropId, 1))
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_scratchCardDetail.PropId, 1L);
				return;
			}
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_CLICK);
			zbWsMsgHandler.Instance.Operate_ScratchCard_Req(m_scratchCardDetail.BaseInfo.UUID, index);
		}
	}

	private void OnUpdateEventOperate(object data)
	{
		ScratchCardDetail scratchCardInfoByUUID = zbSelf.Instance.operateHandler.GetScratchCardInfoByUUID(m_scratchCardDetail.BaseInfo.UUID);
		RefreshCustom(scratchCardInfoByUUID);
	}

	private void OnReset()
	{
		if (!m_isScratched)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10993"));
			return;
		}
		foreach (OperateScratchCardDetail target in m_scratchCardDetail.TargetList)
		{
			if (target.State == ECommonState.Yes)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10994"));
				return;
			}
		}
		if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 2, m_scratchCardDetail.ResetDiamond))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 2L, m_scratchCardDetail.ResetDiamond);
		}
		else
		{
			zbWsMsgHandler.Instance.Operate_ScratchCardReset_Req(m_scratchCardDetail.BaseInfo.UUID);
		}
	}

	public Sprite GetIconById(int id)
	{
		return (Sprite)(id switch
		{
			0 => m_icon0, 
			1 => m_icon1, 
			2 => m_icon2, 
			3 => m_icon3, 
			_ => m_icon1, 
		});
	}

	public bool IsCardSpineShow(int index)
	{
		int[][] obj = new int[8][]
		{
			new int[3] { 0, 1, 2 },
			new int[3] { 3, 4, 5 },
			new int[3] { 6, 7, 8 },
			new int[3] { 0, 3, 6 },
			new int[3] { 1, 4, 7 },
			new int[3] { 2, 5, 8 },
			new int[3] { 0, 4, 8 },
			new int[3] { 2, 4, 6 }
		};
		List<int[]> list = new List<int[]>();
		int[][] array = obj;
		foreach (int[] array2 in array)
		{
			if (IsMatch(array2))
			{
				list.Add(array2);
			}
		}
		foreach (int[] item in list)
		{
			for (int i = 0; i < item.Length; i++)
			{
				if (item[i] == index)
				{
					return true;
				}
			}
		}
		return false;
		bool IsMatch(int[] line)
		{
			foreach (OperateScratchCardDetail target in m_scratchCardDetail.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					int num = line[1];
					if (m_scratchCardDetail.ScratchList[num] == target.CardList[1])
					{
						int num2 = line[0];
						int num3 = line[2];
						int num4 = target.CardList[0];
						int num5 = target.CardList[2];
						int num6 = m_scratchCardDetail.ScratchList[num2];
						int num7 = m_scratchCardDetail.ScratchList[num3];
						if ((num6 == num4 && num7 == num5) || (num6 == num5 && num7 == num4))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
