using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbTruckWinLoseWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private VisualElement m_lbBgWin;

	private VisualElement m_lbBgLose;

	private Label m_lbWinDesc;

	private zbItem m_item;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbBgWin = UQueryExtensions.Q<VisualElement>(m_RootElement, "bgWin", (string)null);
		m_lbBgLose = UQueryExtensions.Q<VisualElement>(m_RootElement, "bgLose", (string)null);
		m_lbWinDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_windesc", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item0", (string)null);
		m_item.OnEnable(this);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_award.ogg");
		zbCombatSystem.Instance.combatRecorder.ClearRecord();
		if (zbCombatSystem.Instance.battleResultInfo.ResultScore == EBattleResultScore.Fail)
		{
			m_lbBgWin.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_lbBgLose.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			return;
		}
		m_lbBgWin.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_lbBgLose.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		using (IEnumerator<AwardDetail> enumerator = zbCombatSystem.Instance.battleAwardList.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				AwardDetail current = enumerator.Current;
				m_item.Refresh(new zbItemData(current.Type, current.Tid, current.Count));
			}
		}
		if (zbSelf.Instance.truckHandler.bBestPrize)
		{
			m_item.AddBestPrizeTag(vis: true);
			((VisualElement)m_lbWinDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			m_item.AddBestPrizeTag(vis: false);
			((VisualElement)m_lbWinDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
		zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
		zbSelf.Instance.truckHandler.bBestPrize = false;
		zbSelf.Instance.truckHandler.bFromBalance = true;
		zbLinkHelper.OpenTruck();
	}
}
