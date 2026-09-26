using System;
using System.Collections;
using System.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbChallengeWnd : zbBaseWnd
{
	private EFuncType[] m_funcTyps = new EFuncType[5]
	{
		EFuncType.ElementTrial,
		EFuncType.WorldLair,
		EFuncType.LegionBoss,
		EFuncType.WorldBoss,
		EFuncType.EndlessHorde
	};

	public zbChallengeItem[] m_items = new zbChallengeItem[5];

	private Button m_btnShenmi;

	public ScrollView m_scView;

	public EFuncType m_scrollToFuncType;

	public float itemHeight = 310f;

	protected override void Awake()
	{
		base.Awake();
		m_funcTyps = m_funcTyps.OrderBy((EFuncType t) => zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey((int)t).unlockVal).ToArray();
		for (int num = 0; num < m_funcTyps.Length; num++)
		{
			m_items[num] = new zbChallengeItem();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < m_funcTyps.Length; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbChallengeItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this, m_funcTyps[i]);
		}
		m_btnShenmi = UQueryExtensions.Q<Button>(m_RootElement, "btn_shenmi", (string)null);
		m_scView = UQueryExtensions.Q<ScrollView>(m_RootElement, "list", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShenmi, (Action)OnShenmi, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Feature_Update, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WorldLairData, OnWorldLair);
		CreateMainBottom(6);
	}

	public void RefreshCustom(EFuncType scrollToFuncType = EFuncType.LegionBoss)
	{
		RefreshChallenges();
		ActionDelayScrollTo(scrollToFuncType);
	}

	private void RefreshChallenges()
	{
		int num = 0;
		for (int i = 0; i < m_funcTyps.Length; i++)
		{
			EFuncType eFuncType = m_funcTyps[i];
			bool flag = zbSelf.Instance.funHandler.IsLockNoTip(eFuncType);
			if (zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey((int)eFuncType).unlockVal == 99)
			{
				m_items[i].Hide();
			}
			else if (flag)
			{
				if (num < 1)
				{
					m_items[i].Refresh();
					num++;
				}
				else
				{
					m_items[i].Hide();
				}
			}
			else
			{
				m_items[i].Refresh();
			}
		}
	}

	public void ShowGuideByFuncType(EFuncType eFuncType)
	{
		int tabItemIndex = GetTabItemIndex(eFuncType);
		m_items[tabItemIndex].ShowGuide();
	}

	public void ActionDelayScrollTo(EFuncType funcType, Action _onFinished = null)
	{
		if (!zbGuide.Instance.IsGuiding() || _onFinished != null)
		{
			m_scrollToFuncType = funcType;
			int tabItemIndex = GetTabItemIndex(funcType);
			((MonoBehaviour)this).StartCoroutine(DelayScrollTo(tabItemIndex, _onFinished));
		}
	}

	public IEnumerator DelayScrollTo(int index, Action _onFinished = null)
	{
		yield return null;
		zbUtil.ScrollVerticalToIndex(m_scView, 2, ((VisualElement)m_scView).childCount, itemHeight, index);
		_onFinished?.Invoke();
	}

	private void OnEventUpdate(object data)
	{
		RefreshChallenges();
	}

	private void OnShenmi()
	{
		zbLinkHelper.OpenShop(1);
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		for (int i = 0; i < m_items.Length; i++)
		{
			m_items[i].Update();
		}
	}

	private void OnWorldLair(object data)
	{
		zbUIMgr.Instance.ShowWorldLair();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		zbWsMsgHandler.Instance.Feature_List_Req();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		zbWsMsgHandler.Instance.Feature_List_Req();
	}

	public int GetTabItemIndex(EFuncType funcType)
	{
		int num = 0;
		EFuncType[] funcTyps = m_funcTyps;
		foreach (EFuncType eFuncType in funcTyps)
		{
			if (funcType == eFuncType)
			{
				return num;
			}
			num++;
		}
		return num;
	}
}
