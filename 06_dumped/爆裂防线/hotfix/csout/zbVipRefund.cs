using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbVipRefund : zbBaseWnd
{
	private ChargebackInfo m_chargebackInfo;

	private VisualElement m_layerWarning;

	private VisualElement m_layerDanger;

	private Label m_lblTime;

	private Label m_lblMoney;

	private Button m_btnClose;

	private Button[] m_btnMoney = (Button[])(object)new Button[6];

	private List<int> m_goodsId = new List<int> { 620001, 620005, 620015, 620030, 620050, 620100 };

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_layerWarning = UQueryExtensions.Q<VisualElement>(m_RootElement, "layer1", (string)null);
		m_layerDanger = UQueryExtensions.Q<VisualElement>(m_RootElement, "layer2", (string)null);
		m_lblTime = UQueryExtensions.Q<Label>(m_layerDanger, "lbltime", (string)null);
		m_lblMoney = UQueryExtensions.Q<Label>(m_layerDanger, "lblmoeny", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_layerDanger, "btnpnl", (string)null);
		for (int i = 0; i < 6; i++)
		{
			m_btnMoney[i] = UQueryExtensions.Q<Button>(val, $"btn{i + 1}", (string)null);
			int tmpIdx = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMoney[i], (Action<ClickEvent>)delegate
			{
				OnRecharge(tmpIdx);
			}, "", (TrickleDown)0);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)Hide, "", (TrickleDown)0);
		AddDark();
	}

	public override void Refresh()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		base.Refresh();
		if (zbServerData.sGameData.RechargeActiveInfo != null && zbServerData.sGameData.RechargeActiveInfo.ChargebackInfo != null)
		{
			m_chargebackInfo = zbServerData.sGameData.RechargeActiveInfo.ChargebackInfo;
		}
		if (m_chargebackInfo == null || m_chargebackInfo.State == EChargebackState.None)
		{
			Hide();
			return;
		}
		m_layerWarning.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_chargebackInfo.State != EChargebackState.Warning));
		m_layerDanger.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_chargebackInfo.State != EChargebackState.Danger));
		if (m_chargebackInfo.State == EChargebackState.Danger)
		{
			RefreshDangerInfo();
		}
	}

	private void RefreshDangerInfo()
	{
		string text = zbUtil.FormatTimeSmart(m_chargebackInfo.ExpireTm - zbSelf.Instance.timeHandler.GetServerTime());
		((TextElement)m_lblTime).text = text + zbUtil.Language("lang.10495");
		((TextElement)m_lblMoney).text = string.Format("{0}:{1}/{2}", zbUtil.Language("lang.10496"), m_chargebackInfo.Repayment, m_chargebackInfo.VoidAmount);
		for (int i = 0; i < m_goodsId.Count; i++)
		{
			if (i < m_btnMoney.Length && m_btnMoney[i] != null)
			{
				string priceByGoodsId = zbSelf.Instance.vipHandler.GetPriceByGoodsId(m_goodsId[i]);
				((TextElement)m_btnMoney[i]).text = priceByGoodsId;
			}
		}
	}

	private void OnRecharge(int idx)
	{
		zbChannel_Pay.OnPayWithMoney(m_goodsId[idx]);
	}
}
