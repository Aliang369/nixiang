using System;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTrainGoldTrainWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnLock;

	private zbItem[] m_items = new zbItem[20];

	private Label[] lb_costs = (Label[])(object)new Label[2];

	private VisualElement[] ve_icons = (VisualElement[])(object)new VisualElement[2];

	private zbTrainCsvRecord record;

	private SpineVisualElement m_spineTrain;

	private Label m_lbTitle;

	private Label m_lbTemp;

	private Label m_lbTime;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineTrain = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_che", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnLock = UQueryExtensions.Q<Button>(m_RootElement, "btn_lock", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLock, (Action)OnDriver, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardPreview0", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardPreview1", (string)null);
		for (int i = 0; i < 10; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(val, $"item{i}", (string)null);
			m_items[i]?.OnEnable(this);
			m_items[i + 10] = UQueryExtensions.Q<zbItem>(val2, $"item{i}", (string)null);
			m_items[i + 10]?.OnEnable(this);
		}
		for (int j = 0; j < 2; j++)
		{
			ve_icons[j] = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon" + j, (string)null);
			lb_costs[j] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + j, (string)null);
		}
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbTemp = UQueryExtensions.Q<Label>(m_RootElement, "lb_temp", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI(m_RootElement);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_TrainData_Ack, OnClose);
	}

	public void RefreshCustom(zbTrainCsvRecord _record)
	{
		record = _record;
		RefreshPauseAward();
		m_spineTrain.AnimationState.SetAnimation(0, "train_" + record.skinId, true);
	}

	public void RefreshPauseAward()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int[][] driverAwardList = record.driverAwardList;
		foreach (int[] array in driverAwardList)
		{
			zbItemData data = new zbItemData((EResourceType)array[0], array[1], array[2]);
			m_items[num].Refresh(data);
			if (num == 0)
			{
				m_items[num].AddBestPrizeTag(vis: true);
			}
			else
			{
				m_items[num].AddBestPrizeTag(vis: false);
			}
			num++;
		}
		num = 0;
		driverAwardList = record.costList;
		foreach (int[] array2 in driverAwardList)
		{
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)array2[0], array2[1]);
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath);
			ve_icons[num].style.backgroundImage = new StyleBackground(val);
			((TextElement)lb_costs[num]).text = array2[2].ToString();
			num++;
		}
		((TextElement)m_btnLock).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(record.goodsId);
	}

	private void OnDriver()
	{
		if (!zbSelf.Instance.truckHandler.trainInfo.UnlockList.Contains(record.quality))
		{
			zbChannel_Pay.OnPay(record.goodsId);
		}
	}

	private void OnClose(object data = null)
	{
		Hide();
	}
}
