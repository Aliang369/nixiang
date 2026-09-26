using System;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFuncPreviewItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbFuncPreviewItem>
	{
	}

	private zbFuncPreviewWnd m_wndParent;

	private zbFunctionUnlockCsvRecord m_record;

	private Label m_lbName;

	private Label m_lbDesc;

	private zbItem m_item;

	private VisualElement m_iconFunc;

	private VisualElement m_grayMask;

	private VisualElement m_funcUnlockDesc;

	private Button m_btnTouch;

	private VisualElement m_leftTime;

	private VisualElement m_iconLock;

	private Label m_lbLeftTime;

	public zbFuncPreviewItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/comm/tb_funcPreview.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbFuncPreviewWnd wndParent)
	{
		m_wndParent = wndParent;
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "award", (string)null);
		m_item.OnEnable(wndParent);
		m_iconFunc = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_func", (string)null);
		m_grayMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "gray_mask", (string)null);
		m_funcUnlockDesc = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "funcUnlockDesc", (string)null);
		m_leftTime = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "leftTime", (string)null);
		m_lbLeftTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_leftTime", (string)null);
		m_iconLock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_lock", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
	}

	public void Init(zbFunctionUnlockCsvRecord record)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		bool isUnLock = !zbSelf.Instance.funHandler.IsLock((EFuncType)m_record.id, bPopTip: false);
		bool flag = !isUnLock && HasLeftTime();
		m_leftTime.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (flag)
		{
			((TextElement)m_lbLeftTime).text = zbUtil.FormatTimeHHMMSS(zbSelf.Instance.funHandler.GetFuncOpenLeftTime(m_record.serverHours));
		}
		m_iconFunc.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.icon));
		((TextElement)m_lbName).text = zbUtil.Language(m_record.name);
		((TextElement)m_lbDesc).text = zbUtil.Language(m_record.desc);
		bool flag2 = m_record.awardList.Length != 0;
		((VisualElement)m_item).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		if (!flag2)
		{
			return;
		}
		zbItemData data = new zbItemData((EResourceType)m_record.awardList[0][0], m_record.awardList[0][1], m_record.awardList[0][2]);
		m_item.Refresh(data);
		bool hadReceive = isUnLock && zbServerData.sGameData.AuxAwardInfo.FuncList.Contains(m_record.id);
		m_item.SetGettedVisible(hadReceive);
		bool flag3 = isUnLock && !zbServerData.sGameData.AuxAwardInfo.FuncList.Contains(m_record.id);
		m_item.SetSpineVisible(flag3);
		m_item.SetTouchCallback(delegate
		{
			if (!isUnLock | hadReceive)
			{
				zbUIMgr.Instance.ShowTip((EResourceType)m_record.awardList[0][0], m_record.awardList[0][1], 0L);
			}
			else
			{
				RepeatedField<int> funcList = zbServerData.sGameData.AuxAwardInfo.FuncList;
				int[] selectList = (from r in zbDataConfig.Instance.functionUnlockCsvTable.GetRecords().records.Values
					where r.unlockType == 2 && r.unlockVal <= zbServerData.sGameData.PeerageInfo.PeerageId
					select r.id into id
					where !funcList.Contains(id)
					select id).ToArray();
				zbWsMsgHandler.Instance.Aux_FuncUnlockAward_Req(selectList);
			}
		});
		m_grayMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!isUnLock | flag3) ? 1 : 0));
		m_funcUnlockDesc.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(isUnLock ? 1 : 0));
		if (!isUnLock)
		{
			((TextElement)UQueryExtensions.Q<Label>(m_funcUnlockDesc, "lb_unlockDesc", (string)null)).text = (HasLeftTime() ? zbUtil.Language("lang.10973") : string.Format(zbUtil.Language("lang.10732"), m_record.unlockVal));
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Update()
	{
		if (HasLeftTime())
		{
			((TextElement)m_lbLeftTime).text = zbUtil.FormatTimeHHMMSS(zbSelf.Instance.funHandler.GetFuncOpenLeftTime(m_record.serverHours));
		}
	}

	public bool HasLeftTime()
	{
		return zbSelf.Instance.funHandler.GetFuncOpenLeftTime(m_record.serverHours) > 0;
	}

	private void OnTouch()
	{
		zbSelf.Instance.funHandler.IsLockAndTip((EFuncType)m_record.id);
	}
}
