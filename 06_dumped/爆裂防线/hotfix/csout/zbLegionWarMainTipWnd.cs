using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarMainTipWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Button m_btnGoto;

	private Label m_lbRest;

	private zbLegionWarPointTarget renderTarget;

	private VisualElement m_foldIcon;

	private Image m_descImg;

	private Vector2 m_pos2;

	private Label m_lbAttackCount;

	private Label m_lbAttackTime;

	private Label m_lbActionCount;

	private Label m_lbActionTime;

	private zbLegionWarMapCsvRecord legionWarMapRecord;

	private int m_pointId;

	private int attackLimitNum;

	private int actionLimitNum;

	private int restCD;

	private float attackRecoverCD;

	private float actionRecoverCD;

	protected override void OnEnable()
	{
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Expected O, but got Unknown
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_lbRest = UQueryExtensions.Q<Label>(m_RootElement, "lb_rest", (string)null);
		m_foldIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "foldIcon", (string)null);
		m_lbAttackCount = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node0", (string)null), "lb_value", (string)null);
		m_lbAttackTime = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node0", (string)null), "lb_time", (string)null);
		m_lbActionCount = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node1", (string)null), "lb_value", (string)null);
		m_lbActionTime = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node1", (string)null), "lb_time", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action)OnGoto, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		renderTarget = ((Component)((Component)this).transform.Find("legionWarPointTarget")).GetComponent<zbLegionWarPointTarget>();
		if (m_descImg == null)
		{
			m_descImg = new Image();
			((VisualElement)m_descImg).style.left = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg).style.top = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg).style.width = StyleLength.op_Implicit(240f);
			((VisualElement)m_descImg).style.height = StyleLength.op_Implicit(240f);
			((VisualElement)m_descImg).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		}
		m_foldIcon.Add((VisualElement)(object)m_descImg);
		restCD = Extensions.Value<int>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["restCD"]);
		AddDark();
	}

	public void RefreshCustom(int pointId, Vector2 pos2)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		(int, int) tuple = zbSelf.Instance.legionWarHandler.GetAttackLimitNum();
		attackLimitNum = tuple.Item1;
		actionLimitNum = tuple.Item2;
		attackRecoverCD = (float)restCD / (float)attackLimitNum;
		actionRecoverCD = (float)restCD * (1f - zbSelf.Instance.legionWarHandler.GetActionRecoverPer()) / (float)actionLimitNum;
		m_pointId = pointId;
		m_pos2 = pos2;
		InitTarget();
		((TextElement)m_lbAttackTime).text = "";
		((TextElement)m_lbActionTime).text = "";
		legionWarMapRecord = zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(pointId);
		if (legionWarMapRecord.pointForce == (int)zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId)
		{
			((TextElement)m_lbAttackCount).text = zbSelf.Instance.legionWarHandler.legionWarSelfInfo.AttackNum + "/" + attackLimitNum;
			((TextElement)m_lbActionCount).text = zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ActionPoints + "/" + actionLimitNum;
			((VisualElement)m_btnGoto).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ToPointId == m_pointId || zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ArrivalTm > zbSelf.Instance.timeHandler.GetServerTime()) ? 1 : 0));
			((VisualElement)m_lbRest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ToPointId != m_pointId) ? 1 : 0));
		}
		else
		{
			((TextElement)m_lbAttackCount).text = "--/" + attackLimitNum;
			((TextElement)m_lbActionCount).text = "--/" + actionLimitNum;
			((VisualElement)m_btnGoto).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbRest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		UpdatePerSecond();
	}

	private void InitTarget()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		renderTarget.StartRender(m_descImg, m_pos2);
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (legionWarMapRecord.pointForce == (int)zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId && zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ToPointId == m_pointId)
		{
			bool flag = true;
			long num = zbSelf.Instance.timeHandler.GetServerMillTime() - zbSelf.Instance.legionWarHandler.legionWarSelfInfo.RestStartTm * 1000;
			int num2 = Math.Min(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.AttackNum + (int)((float)num / attackRecoverCD / 1000f), attackLimitNum);
			int num3 = Math.Min(zbSelf.Instance.legionWarHandler.legionWarSelfInfo.ActionPoints + (int)((float)num / actionRecoverCD / 1000f), actionLimitNum);
			((TextElement)m_lbAttackCount).text = num2 + "/" + attackLimitNum;
			((TextElement)m_lbActionCount).text = num3 + "/" + actionLimitNum;
			if (num2 >= attackLimitNum)
			{
				((TextElement)m_lbAttackTime).text = "";
			}
			else
			{
				flag = false;
				((TextElement)m_lbAttackTime).text = GetCDTime(num, attackRecoverCD);
			}
			if (num3 >= actionLimitNum)
			{
				((TextElement)m_lbActionTime).text = "";
			}
			else
			{
				flag = false;
				((TextElement)m_lbActionTime).text = GetCDTime(num, actionRecoverCD);
			}
			if (flag)
			{
				((TextElement)m_lbRest).text = zbUtil.Language("lang.11192");
			}
			else
			{
				((TextElement)m_lbRest).text = zbUtil.Language("lang.11193");
			}
		}
	}

	public static string GetCDTime(long elapsed, float cdSeconds)
	{
		long num = (long)(cdSeconds * 1000f);
		if (num <= 0)
		{
			return "00:000";
		}
		if (elapsed < 0)
		{
			elapsed = 0L;
		}
		long num2 = elapsed % num;
		long num3 = num - num2;
		if (num3 == num)
		{
			num3 = 0L;
		}
		long num4 = num3 / 1000;
		long num5 = num3 % 1000;
		return $"{num4:D2}:{num5:D3}";
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnGoto()
	{
		zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.11194"), delegate(bool isOK)
		{
			if (isOK)
			{
				OnClose();
				zbWsMsgHandler.Instance.LegionWar_Rest_Req();
			}
		});
	}
}
