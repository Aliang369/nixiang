using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbJourneyOneMainWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Label m_lbTitle;

	private Label m_lbTime;

	private readonly Button[] m_btns = (Button[])(object)new Button[5];

	private readonly Label[] m_lbs = (Label[])(object)new Label[5];

	private readonly VisualElement[] m_notify = (VisualElement[])(object)new VisualElement[5];

	private readonly VisualElement[] m_sprites = (VisualElement[])(object)new VisualElement[5];

	private zbJourneyOneCitizen[] m_citizens = new zbJourneyOneCitizen[3];

	private zbJourneyOneZombie[] m_zombies = new zbJourneyOneZombie[4];

	protected override void OnEnable()
	{
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
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
		for (int num = 0; num < 3; num++)
		{
			m_citizens[num] = ((Component)m_GameObjectSceneRoot.transform.Find("citizen" + num)).GetComponent<zbJourneyOneCitizen>();
		}
		for (int num2 = 0; num2 < 4; num2++)
		{
			m_zombies[num2] = ((Component)m_GameObjectSceneRoot.transform.Find("zombie" + num2)).GetComponent<zbJourneyOneZombie>();
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
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		base.Start();
		Vector3[] array = (Vector3[])(object)new Vector3[3]
		{
			new Vector3(-1.6f, -0.7f, 0f),
			new Vector3(-2f, -1.4f, 0f),
			new Vector3(1.4f, -0.8f, 0f)
		};
		JArray val = (JArray)zbDataConfig.Instance.chapterOneBaseJsonTable.GetRecord()["avatar"][(object)"citizen"];
		for (int i = 0; i < 3; i++)
		{
			m_citizens[i].Refresh((int)val[i], array[i]);
		}
		JArray zombieList = (JArray)zbDataConfig.Instance.chapterOneBaseJsonTable.GetRecord()["avatar"][(object)"zombie"];
		((MonoBehaviour)this).StartCoroutine(ZombieWalkCrossScreen(zombieList));
	}

	private IEnumerator ZombieWalkCrossScreen(JArray zombieList)
	{
		while (true)
		{
			int zombieCount = Rand.Range(1, 5);
			int[] zombieIds = (from x in ((IEnumerable<JToken>)zombieList).OrderBy((JToken x) => Rand.value).Take(zombieCount)
				select (int)x).ToArray();
			for (int i = 0; i < zombieCount; i++)
			{
				Vector3 startPos = ((Rand.value > 0.5f) ? new Vector3(-3.6f, 1.63f, 1.63f) : new Vector3(3.6f, 1.66f, 1.66f));
				m_zombies[i].Refresh(zombieIds[i], startPos);
				yield return (object)new WaitForSeconds(Rand.Range(2f, 3f));
			}
			yield return (object)new WaitForSeconds(Rand.Range(67f, 72f));
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
		zbLinkHelper.OpenJourneyOne((EJourneyOneMenu)index);
	}

	private void UpdateTime()
	{
		long valueOrDefault = (zbServerData.sGameData?.JourneyInfo?.ChapterOne?.BaseInfo?.EndTm).GetValueOrDefault();
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		((TextElement)m_lbTime).text = ((valueOrDefault > serverTime) ? zbUtil.FormatTimeHHMMSS(valueOrDefault - serverTime) : zbUtil.Language("lang.10345"));
	}

	private void UpdateNotify()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		m_notify[2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyOneHandler.HasBarRedPoint()));
		m_notify[3].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyOneHandler.HasSignRedPoint()));
		m_notify[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyOneHandler.HasRechargeSumRedPoint()));
		m_notify[4].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyOneHandler.HasZhanlingRedPoint()));
		m_notify[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyOneHandler.HasShopRedPoint()));
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
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.10806");
		((TextElement)m_lbs[0]).text = zbUtil.Language("lang.10807");
		((TextElement)m_lbs[1]).text = zbUtil.Language("lang.10808");
		((TextElement)m_lbs[2]).text = zbUtil.Language("lang.10809");
		((TextElement)m_lbs[3]).text = zbUtil.Language("lang.10810");
		((TextElement)m_lbs[4]).text = zbUtil.Language("lang.10811");
	}
}
