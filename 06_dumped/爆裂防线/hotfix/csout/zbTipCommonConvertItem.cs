using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTipCommonConvertItem : zbViewBaseItem
{
	private zbTipCommonWnd m_wndParent;

	private Label m_lbName;

	private zbItem m_item;

	private Button m_btnConvert;

	private Label m_lbCost;

	private Button m_btnBatch;

	private zbResourceConvertCsvRecord m_record;

	private stResorceIconInfo m_convertInfo;

	private stResorceIconInfo m_costInfo;

	private readonly int[] BATCHCOUNT = new int[4] { 1, 2, 5, 10 };

	private int m_batchCount = 1;

	public zbTipCommonConvertItem(zbTipCommonWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/tip/tb_tipCommon_convert.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_btnConvert = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_convert", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_cost", (string)null);
		m_btnBatch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_batch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConvert, (Action<ClickEvent>)OnConvert, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBatch, (Action)OnBatch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbResourceConvertCsvRecord record)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_convertInfo = zbResourceHelper.GetResorceIconPath((EResourceType)m_record.convertType, m_record.convertId);
		m_costInfo = zbResourceHelper.GetResorceIconPath((EResourceType)m_record.resType, m_record.resId);
		m_item.Refresh(new zbItemData((EResourceType)m_record.convertType, m_record.convertId, m_record.convertCount), isNameVis: false, isCountVis: false, isTouchVis: false);
		((VisualElement)m_lbCost).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_costInfo.strIconPath));
		RefreshBatch();
	}

	public void RefreshDetail()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbName).text = m_convertInfo.strName + "X" + m_record.convertCount * m_batchCount;
		((TextElement)m_lbCost).text = (m_record.resCount * m_batchCount).ToString();
		if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)m_record.resType, m_record.resId, m_record.resCount * m_batchCount))
		{
			((VisualElement)m_lbCost).style.color = StyleColor.op_Implicit(Color.red);
		}
		else
		{
			((VisualElement)m_lbCost).style.color = StyleColor.op_Implicit(Color.white);
		}
	}

	private void RefreshBatch()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		int canConvertCount = GetCanConvertCount();
		if (canConvertCount > 1)
		{
			if (m_batchCount > canConvertCount)
			{
				m_batchCount = 1;
				for (int num = BATCHCOUNT.Length - 1; num >= 0; num--)
				{
					if (BATCHCOUNT[num] <= canConvertCount)
					{
						m_batchCount = BATCHCOUNT[num];
						break;
					}
				}
			}
			((VisualElement)m_btnBatch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnBatch).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/vip/bar/bar_score{m_batchCount}.png"));
		}
		else
		{
			((VisualElement)m_btnBatch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_batchCount = 1;
		}
		RefreshDetail();
	}

	private void OnConvert(ClickEvent evt)
	{
		if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)m_record.resType, m_record.resId, m_record.resCount))
		{
			zbUIMgr.Instance.ShowTip((EResourceType)m_record.resType, m_record.resId, m_record.resCount);
		}
		else
		{
			zbWsMsgHandler.Instance.Aux_ResourceConvert_Req(m_record.id, m_batchCount);
		}
	}

	private int GetCanConvertCount()
	{
		return (int)zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)m_record.resType, m_record.resId) / m_record.resCount;
	}

	private void OnBatch()
	{
		int canConvertCount = GetCanConvertCount();
		int num = Array.IndexOf(BATCHCOUNT, m_batchCount);
		if (num < 0)
		{
			num = 0;
		}
		for (int i = 1; i <= BATCHCOUNT.Length; i++)
		{
			int num2 = (num + i) % BATCHCOUNT.Length;
			int num3 = BATCHCOUNT[num2];
			if (num3 <= canConvertCount)
			{
				m_batchCount = num3;
				break;
			}
		}
		RefreshBatch();
	}
}
