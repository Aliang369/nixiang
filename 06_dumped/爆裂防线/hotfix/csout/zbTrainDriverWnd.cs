using System;
using System.Collections;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTrainDriverWnd : zbBaseWnd
{
	private Button m_btnBack;

	private Button m_btnOk;

	private VisualElement[] m_trainDetal = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_veOk = (VisualElement[])(object)new VisualElement[3];

	private Button[] m_btnChecks = (Button[])(object)new Button[3];

	private SpineVisualElement[] m_spineTrains = (SpineVisualElement[])(object)new SpineVisualElement[3];

	private Button[] m_btnSelects = (Button[])(object)new Button[3];

	private int currSelectIndex;

	private zbItem[] m_items = new zbItem[20];

	private Label m_lbTruckTime;

	private Label[] lb_costs = (Label[])(object)new Label[2];

	private VisualElement[] ve_icons = (VisualElement[])(object)new VisualElement[2];

	private VisualElement ve_faChe;

	private VisualElement ve_cost;

	private Label ve_unLock;

	private float _lastTop = -1f;

	private Button m_btnRule;

	protected override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < 3; i++)
		{
			m_trainDetal[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "train" + i, (string)null);
			m_veOk[i] = UQueryExtensions.Q<VisualElement>(m_trainDetal[i], "sprite_ok", (string)null);
			m_btnSelects[i] = UQueryExtensions.Q<Button>(m_trainDetal[i], "btn_one", (string)null);
			m_btnChecks[i] = UQueryExtensions.Q<Button>(m_trainDetal[i], "node_toggle", (string)null);
			m_spineTrains[i] = UQueryExtensions.Q<SpineVisualElement>(m_trainDetal[i], "spine_che", (string)null);
			int index = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSelects[i], (Action<ClickEvent>)delegate
			{
				onSelects(index);
			}, "", (TrickleDown)0);
		}
		m_lbTruckTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardPreview0", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardPreview1", (string)null);
		for (int num = 0; num < 10; num++)
		{
			m_items[num] = UQueryExtensions.Q<zbItem>(val, $"item{num}", (string)null);
			m_items[num]?.OnEnable(this);
			m_items[num + 10] = UQueryExtensions.Q<zbItem>(val2, $"item{num}", (string)null);
			m_items[num + 10]?.OnEnable(this);
		}
		for (int num2 = 0; num2 < 2; num2++)
		{
			ve_icons[num2] = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon" + num2, (string)null);
			lb_costs[num2] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + num2, (string)null);
		}
		ve_faChe = UQueryExtensions.Q<VisualElement>(m_RootElement, "Label", (string)null);
		ve_cost = UQueryExtensions.Q<VisualElement>(m_RootElement, "cost", (string)null);
		ve_unLock = UQueryExtensions.Q<Label>(m_RootElement, "unLock", (string)null);
		m_btnOk = UQueryExtensions.Q<Button>(m_RootElement, "button_fight", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOk, (Action<ClickEvent>)OnOk, "", (TrickleDown)0);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.TrainTicket
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_TrainTransportOpen_Ack, TrainTransportAward);
		m_gameEevent.RegisterEvent(EGameEvent.Event_TrainData_Ack, OnEventUpdate);
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom();
	}

	public void RefreshCustom()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			m_trainDetal[i].style.top = StyleLength.op_Implicit(90f);
			if (currSelectIndex == i)
			{
				((VisualElement)m_btnSelects[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_btnChecks[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else
			{
				((VisualElement)m_btnSelects[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_btnChecks[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			m_spineTrains[i].AnimationState.SetAnimation(0, "track1_che_" + (i + 1), true);
			m_spineTrains[i].AnimationState.SetAnimation(1, "track2_idle", true);
			if (i == 2)
			{
				int num = i + 3;
				if (zbSelf.Instance.truckHandler.trainInfo.UnlockList.Contains(num))
				{
					((TextElement)m_btnSelects[i]).text = zbUtil.Language("选择");
					Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_yellow.png");
					((VisualElement)m_btnSelects[i]).style.backgroundImage = new StyleBackground(val);
				}
				else
				{
					((TextElement)m_btnSelects[i]).text = zbUtil.Language("lang.10786");
					Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_green.png");
					((VisualElement)m_btnSelects[i]).style.backgroundImage = new StyleBackground(val2);
				}
			}
		}
		RefreshPauseAward();
	}

	public void RefreshPauseAward()
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		int num = currSelectIndex + 3;
		zbTrainCsvRecord recordByKey = zbDataConfig.Instance.trainCsvTable.GetRecordByKey(num);
		int num2 = 0;
		int[][] driverAwardList = recordByKey.driverAwardList;
		foreach (int[] array in driverAwardList)
		{
			zbItemData data = new zbItemData((EResourceType)array[0], array[1], array[2]);
			m_items[num2].Refresh(data);
			if (num2 == 0)
			{
				m_items[num2].AddBestPrizeTag(vis: true);
			}
			else
			{
				m_items[num2].AddBestPrizeTag(vis: false);
			}
			num2++;
		}
		num2 = 0;
		driverAwardList = recordByKey.costList;
		foreach (int[] array2 in driverAwardList)
		{
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)array2[0], array2[1]);
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath);
			ve_icons[num2].style.backgroundImage = new StyleBackground(val);
			((TextElement)lb_costs[num2]).text = array2[2].ToString();
			num2++;
		}
		((TextElement)m_lbTruckTime).text = zbUtil.Language("lang.11122") + ":" + zbUtil.FormatTimeHHMMSS(recordByKey.runTime * 60);
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_title" + num + ".png");
		UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_quality", (string)null).style.backgroundImage = new StyleBackground(val2);
		Sprite val3 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_smallbg" + num + ".png");
		UQueryExtensions.Q<VisualElement>(m_RootElement, "detail", (string)null).style.backgroundImage = new StyleBackground(val3);
		if (!zbSelf.Instance.truckHandler.trainInfo.UnlockList.Contains(num))
		{
			ve_faChe.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			ve_cost.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)ve_unLock).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)ve_unLock).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(recordByKey.goodsId) + "\n" + zbUtil.Language("lang.10786");
		}
		else
		{
			ve_faChe.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			ve_cost.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)ve_unLock).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void onSelects(int i)
	{
		currSelectIndex = i;
		RefreshCustom();
	}

	private void OnOk(object data = null)
	{
		int quality = currSelectIndex + 3;
		zbTrainCsvRecord recordByKey = zbDataConfig.Instance.trainCsvTable.GetRecordByKey(quality);
		if (!zbSelf.Instance.truckHandler.trainInfo.UnlockList.Contains(quality))
		{
			zbUIMgr.Instance.ShowTrainGoldTrain(recordByKey);
			return;
		}
		int[][] costList = recordByKey.costList;
		foreach (int[] array in costList)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)array[0], array[1], array[2]))
			{
				zbUIMgr.Instance.ShowTip((EResourceType)array[0], array[1], array[2]);
				return;
			}
		}
		zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.11371"), delegate(bool isOK)
		{
			if (isOK)
			{
				zbWsMsgHandler.Instance.Train_TransportOpen_Req((EQuality)quality);
			}
		});
	}

	public void OnRule()
	{
		string[] langKeys = new string[7] { "lang.11374", "", "lang.11375", "", "lang.11381", "", "lang.11382" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnClose(object data = null)
	{
		Hide();
	}

	public void TrainTransportAward(object data)
	{
		((MonoBehaviour)this).StartCoroutine(RefreshNewTrain(data));
	}

	public IEnumerator RefreshNewTrain(object data)
	{
		zbAudioMgr.Instance.PlayAudio("audio/combat/motuo_2.mp3");
		int index = (int)(zbSelf.Instance.truckHandler.trainInfo.SelfTrain.Quality - 3);
		((VisualElement)m_btnSelects[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnChecks[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spineTrains[index].AnimationState.SetAnimation(1, "track2_move", true);
		do
		{
			StyleLength top = m_trainDetal[index].style.top;
			Length value = ((StyleLength)(ref top)).value;
			float value2 = ((Length)(ref value)).value;
			zbUtil.SetStyleIfChanged(ref _lastTop, value2 - 15f, delegate(float val)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				m_trainDetal[index].style.top = StyleLength.op_Implicit(val);
			});
			yield return null;
		}
		while (!(_lastTop <= -900f));
		OnClose();
	}
}
