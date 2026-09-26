using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbModDetailModItem : zbViewBaseItem
{
	private Button m_btnTouch;

	private Label m_lbName;

	private VisualElement m_spLock;

	private VisualElement m_spCheck;

	private VisualElement m_spRootBg;

	private readonly Sprite m_notUnlocked;

	private readonly Sprite m_Unlocked;

	private zbModDetailWnd m_wndParent;

	private int m_modId;

	public zbModDetailModItem(zbModDetailWnd wndParent, VisualTreeAsset tAsset)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_wndParent = wndParent;
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_spLock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "sp_lock", (string)null);
		m_spRootBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "rootBg", (string)null);
		m_spCheck = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "sp_check", (string)null);
		m_notUnlocked = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/encyclopedia/modbook/modbook_requireBg0.png");
		m_Unlocked = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/encyclopedia/modbook/modbook_requireBg1.png");
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(int modId, zbModHandbookCsvRecord mod)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		m_modId = modId;
		zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(m_modId);
		zbModHandbookCsvRecord recordByKey2 = zbDataConfig.Instance.modHandbookCsvTable.GetRecordByKey(recordByKey.handbookId);
		if (recordByKey2 != null)
		{
			((TextElement)m_lbName).text = recordByKey2.name;
			bool flag = zbSelf.Instance.handbookHandler.CanModActive(recordByKey2.id);
			bool flag2 = zbSelf.Instance.handbookHandler.CanModDone(recordByKey2.id);
			bool flag3 = mod.unlock == 1 || (mod.unlock == 0 && (flag | flag2));
			m_spLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag3 ? 1 : 0));
			m_spCheck.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
			m_spRootBg.style.backgroundImage = new StyleBackground((mod.unlock == 1 || (mod.unlock == 0 && (flag | flag2))) ? m_Unlocked : m_notUnlocked);
		}
	}

	public void OnTouch(ClickEvent evt)
	{
		zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(m_modId);
		m_wndParent.ShowTip(evt, recordByKey.desc);
	}
}
