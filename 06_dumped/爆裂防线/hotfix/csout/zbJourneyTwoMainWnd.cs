using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbJourneyTwoMainWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Label m_lbTitle;

	private Label m_lbTime;

	private readonly Button[] m_btns = (Button[])(object)new Button[5];

	private readonly Label[] m_lbs = (Label[])(object)new Label[5];

	private readonly VisualElement[] m_notify = (VisualElement[])(object)new VisualElement[5];

	private readonly VisualElement[] m_sprites = (VisualElement[])(object)new VisualElement[5];

	protected override void OnEnable()
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		for (int i = 0; i < 5; i++)
		{
			int index = i;
			m_btns[i] = UQueryExtensions.Q<Button>(m_RootElement, $"btn_{i}", (string)null);
			m_lbs[i] = UQueryExtensions.Q<Label>((VisualElement)(object)m_btns[i], "lb_operateName", (string)null);
			m_notify[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btns[i], "sprite", (string)null);
			m_sprites[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btns[i], "sprite", (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btns[i], (Action<ClickEvent>)delegate
			{
				OnGoto(index);
			}, "", (TrickleDown)0);
		}
		m_notify[0] = zbUtil.CreateNotifyTag(m_sprites[0], (Vector2?)new Vector2(5f, 10f), 1f, true);
		m_notify[1] = zbUtil.CreateNotifyTag(m_sprites[1], (Vector2?)new Vector2(5f, 10f), 1f, true);
		m_notify[2] = zbUtil.CreateNotifyTag(m_sprites[2], (Vector2?)new Vector2(5f, 10f), 1f, true);
		m_notify[3] = zbUtil.CreateNotifyTag(m_sprites[3], (Vector2?)new Vector2(5f, 10f), 1f, true);
		m_notify[4] = zbUtil.CreateNotifyTag(m_sprites[4], (Vector2?)new Vector2(5f, 10f), 1f, true);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		SetUIText();
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Tili
		});
	}

	public override void Refresh()
	{
		zbWsMsgHandler.Instance.Journey_Data_Req();
		UpdateTime();
		UpdateNotify();
	}

	private void OnGoto(int index)
	{
		zbLinkHelper.OpenJourneyTwo((EJourneyTwoMenu)index);
	}

	private void UpdateTime()
	{
		long valueOrDefault = (zbServerData.sGameData?.JourneyInfo?.ChapterTwo?.BaseInfo?.EndTm).GetValueOrDefault();
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		((TextElement)m_lbTime).text = ((valueOrDefault > serverTime) ? zbUtil.FormatTimeHHMMSS(valueOrDefault - serverTime) : zbUtil.Language("lang.10345"));
	}

	private void UpdateNotify()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		m_notify[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyTwoHandler.HasBarNotify()));
		m_notify[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyTwoHandler.HasDiamondCostNotify()));
		m_notify[2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyTwoHandler.HasExchangeNotify()));
		m_notify[3].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyTwoHandler.HasActiveNotify()));
		m_notify[4].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.truckHandler.HasRedPoint(isArenaMain: false)));
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		UpdateTime();
		UpdateNotify();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		UpdateNotify();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		UpdateNotify();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void SetUIText()
	{
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.11097");
		((TextElement)m_lbs[0]).text = zbUtil.Language("lang.11098");
		((TextElement)m_lbs[1]).text = zbUtil.Language("lang.11099");
		((TextElement)m_lbs[2]).text = zbUtil.Language("lang.11100");
		((TextElement)m_lbs[3]).text = zbUtil.Language("lang.11101");
		((TextElement)m_lbs[4]).text = zbUtil.Language("lang.11102");
	}
}
