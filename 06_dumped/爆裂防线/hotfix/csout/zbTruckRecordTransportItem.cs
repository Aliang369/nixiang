using System;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTruckRecordTransportItem : zbViewBaseItem
{
	private SpineVisualElement m_spineTruck;

	private Label m_lbTruckTime;

	private zbItem[] m_items = new zbItem[10];

	private Button m_btnTouch;

	private TruckTransportInfo m_detail;

	public zbTruckRecordTransportItem(VisualTreeAsset tAsset, zbTruckRecordWnd wndParent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_spineTruck = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)this, "spine_che", (string)null);
		m_lbTruckTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_time", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		for (int i = 0; i < 10; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, $"item{i}", (string)null);
			m_items[i]?.OnEnable(wndParent);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(TruckTransportInfo detail)
	{
		m_detail = detail;
		InitTruckInfo();
		RefreshPauseAward();
	}

	private void InitTruckInfo()
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbTruckTime).text = zbUtil.Language("lang.11122") + ":" + zbUtil.FormatUnixTimeToUtcString(m_detail.TransportInfo.ArriveTm);
		m_spineTruck.AnimationState.SetAnimation(0, "che_" + (int)m_detail.TransportInfo.Quality, true);
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_title" + (int)m_detail.TransportInfo.Quality + ".png");
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_quality", (string)null).style.backgroundImage = new StyleBackground(val);
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_smallbg" + (int)m_detail.TransportInfo.Quality + ".png");
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null).style.backgroundImage = new StyleBackground(val2);
	}

	public void RefreshPauseAward()
	{
		int num = 0;
		foreach (AwardDetail award in m_detail.TransportInfo.AwardList)
		{
			zbItemData data = new zbItemData(award.Type, award.Tid, award.Count);
			m_items[num].Refresh(data);
			if (num == 0)
			{
				m_items[num].AddBestPrizeTag(vis: true);
			}
			else
			{
				m_items[num].AddBestPrizeTag(vis: false);
			}
			m_items[num].SetTruckLossItemVisible(m_detail.TransportInfo.RobList.Contains(num));
			num++;
		}
	}

	private void OnTouch(object data = null)
	{
		zbUIMgr.Instance.ShowTruckBalance(m_detail, _isRuning: false);
	}
}
