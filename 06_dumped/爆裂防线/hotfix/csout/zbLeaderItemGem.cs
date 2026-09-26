using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLeaderItemGem : zbViewBaseItem
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private Label m_lbCount;

	private VisualElement m_spriteSelect;

	private VisualElement m_spriteLock;

	private VisualElement m_iconNew;

	private VisualElement m_spriteMask;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private zbBaseWnd m_wndParent;

	private zbLeaderTabGem m_tabParent;

	private ResourceDetail m_data;

	private VisualElement m_spritePlan;

	public zbLeaderItemGem(zbBaseWnd wndParent, zbLeaderTabGem tabParent)
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		m_wndParent = wndParent;
		m_tabParent = tabParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/leader/tb_leader_gem.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lbCount", (string)null);
		m_spriteSelect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "select", (string)null);
		m_spriteMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "mask", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "touch", (string)null);
		m_spriteLock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "lock", (string)null);
		m_iconNew = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_new", (string)null);
		m_spritePlan = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "sp_plan", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, null, 1f, scaleAnim: false);
		m_spritePlan.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(ResourceDetail data)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex(zbResourceHelper.GetResourceQuality(EResourceType.Gem, m_data.Tid));
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceFrameTex);
		if ((Object)(object)val != (Object)null && (Object)(object)val != (Object)null)
		{
			m_spriteFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_spriteFrame.MarkDirtyRepaint();
		}
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Gem, m_data.Tid);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resorceIconPath.strIconPath);
		if ((Object)(object)val2 != (Object)null && (Object)(object)val2 != (Object)null)
		{
			m_spriteIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteIcon.MarkDirtyRepaint();
		}
		((TextElement)m_lbCount).text = zbSelf.Instance.equipHandler.getGemCount(m_data.Tid).ToString();
		m_spriteLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.equipHandler.IsGemLock(m_data.Tid)));
		bool flag = zbSelf.Instance.equipHandler.CanGemInsert(m_data.Tid) || zbSelf.Instance.equipHandler.CanGemReplace(m_data.Tid);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_iconNew.visible = zbSelf.Instance.newTagHandler.IsNewGem(data.Tid.ToString()) && !flag;
		bool flag2 = zbSelf.Instance.equipHandler.GetGroupGemIndex(m_data.Tid) >= 0 && !zbSelf.Instance.equipHandler.CanGemReplace(m_data.Tid);
		m_spriteMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		bool flag3 = false;
		if (!zbSelf.Instance.equipHandler.IsGemInsert(m_data.Tid, zbServerData.sGameData.EquipSlotInfo.GemPlanId))
		{
			flag3 = zbSelf.Instance.equipHandler.getGemCount(m_data.Tid) == 1 && zbSelf.Instance.equipHandler.IsGemByPlan(m_data.Tid, zbServerData.sGameData.EquipSlotInfo.GemPlanId);
		}
		m_spritePlan.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
	}

	public void RefreshSelect(int selectedId)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_data.Tid != selectedId) ? 1 : 0));
	}

	public void OnTouch(ClickEvent evt)
	{
		zbSelf.Instance.newTagHandler.SetNewGemViewed(m_data.Tid.ToString());
		m_iconNew.visible = false;
		if (m_tabParent.m_selectTid == m_data.Tid && zbUIMgr.Instance.IsShow(EWndId.GemInsert))
		{
			m_tabParent.RefreshSelect(-1);
			zbUIMgr.Instance.HideWnd(EWndId.GemInsert);
		}
		else
		{
			m_tabParent.RefreshSelect(m_data.Tid);
			zbUIMgr.Instance.ShowGemInsert(m_data);
		}
	}
}
