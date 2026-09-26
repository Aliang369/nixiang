using System;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbJourneyThreeMainWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Label m_lbTitle;

	private Label m_lbTime;

	private readonly Button[] m_btns = (Button[])(object)new Button[5];

	private readonly Label[] m_lbs = (Label[])(object)new Label[5];

	private readonly VisualElement[] m_notify = (VisualElement[])(object)new VisualElement[5];

	private readonly VisualElement[] m_sprites = (VisualElement[])(object)new VisualElement[5];

	private zbJourneyOneCitizen[] m_citizens = new zbJourneyOneCitizen[2];

	protected override void OnEnable()
	{
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
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
		for (int num = 0; num < 2; num++)
		{
			m_citizens[num] = ((Component)m_GameObjectSceneRoot.transform.Find("citizen" + num)).GetComponent<zbJourneyOneCitizen>();
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

	protected override void Start()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		Vector3[] array = (Vector3[])(object)new Vector3[2]
		{
			new Vector3(1.9f, -3f, 0f),
			new Vector3(3f, -2.9f, 0f)
		};
		JArray val = (JArray)zbDataConfig.Instance.chapterThreeBaseJsonTable.GetRecord()["citizen"];
		for (int i = 0; i < 2; i++)
		{
			m_citizens[i].Refresh((int)val[i], array[i]);
		}
	}

	public override void Refresh()
	{
		zbWsMsgHandler.Instance.Journey_Data_Req();
		UpdateTime();
		UpdateNotify();
	}

	private void OnGoto(int index)
	{
		zbLinkHelper.OpenJourneyThree((EJourneyThreeMenu)index);
	}

	private void UpdateTime()
	{
		long valueOrDefault = (zbServerData.sGameData?.JourneyInfo?.ChapterThree?.BaseInfo?.EndTm).GetValueOrDefault();
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
		m_notify[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyThreeHandler.HasBarNotify()));
		m_notify[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyThreeHandler.HasDrawCountNotify()));
		m_notify[2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyThreeHandler.HasExchangeNotify()));
		m_notify[3].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyThreeHandler.HasSignNotify()));
		m_notify[4].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.facilityHandler.HasRedPoint(isMainWnd: false)));
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
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.11171");
		((TextElement)m_lbs[0]).text = zbUtil.Language("lang.11172");
		((TextElement)m_lbs[1]).text = zbUtil.Language("lang.11173");
		((TextElement)m_lbs[2]).text = zbUtil.Language("lang.11174");
		((TextElement)m_lbs[3]).text = zbUtil.Language("lang.11175");
		((TextElement)m_lbs[4]).text = zbUtil.Language("lang.11176");
	}
}
