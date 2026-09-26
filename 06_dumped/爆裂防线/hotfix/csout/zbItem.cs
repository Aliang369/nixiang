using System;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbItem : zbViewBaseItem
{
	public VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	public VisualElement m_spriteFrag;

	public Label m_lbName;

	public Label m_lbCount;

	private Button m_btnTouch;

	private VisualElement m_spriteGetted;

	private VisualElement m_spriteLock;

	private VisualElement m_spriteLoss;

	private VisualElement m_spriteTruckLossItem;

	private SpineVisualElement m_spineRare;

	private SpineVisualElement m_spineEffect;

	private SpineVisualElement m_zhenguiEffect;

	private VisualElement m_spritePropTag;

	private VisualElement m_spriteBestPrizeTag;

	private zbBaseWnd m_wndParent;

	private zbItemData m_award;

	private Action<ClickEvent> m_touchCallback;

	private VisualElement m_wrapper;

	private string noItemBubble = "";

	public zbItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/ctrl/tb_item.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbBaseWnd wndParent)
	{
		m_wndParent = wndParent;
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_spriteFrag = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frag", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lbName", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lbCount", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "touch", (string)null);
		m_wrapper = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "wrapper", (string)null);
		m_spineRare = null;
		m_spineEffect = null;
		m_zhenguiEffect = null;
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbItemData data, bool isNameVis = false, bool isCountVis = true, bool isTouchVis = true, bool isFrameVis = true, bool isRareVis = true, string _noItemBubble = "")
	{
		m_award = data;
		noItemBubble = _noItemBubble;
		zbResourceHelper.RefreshResourceItem(m_award.type, m_award.id, m_award.count, m_spriteFrame, m_spriteIcon, m_lbName, m_lbCount);
		m_spriteFrag.visible = m_award.type == EResourceType.HeroFrag;
		((VisualElement)m_lbName).visible = isNameVis;
		((VisualElement)m_lbCount).visible = isCountVis;
		((VisualElement)m_btnTouch).visible = isTouchVis;
		m_spriteFrame.visible = isFrameVis;
		SetGettedVisible(vis: false);
		SetLockVisible(vis: false);
		SetSpineVisible(vis: false);
		SetLossVisible(vis: false);
		AddPropOperateGetWayTag();
	}

	public void SetNameNeedHave(string haveColor, string haveString, string needColor, string needString)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_lbName).visible = true;
		((VisualElement)m_lbName).style.fontSize = StyleLength.op_Implicit(26f);
		((TextElement)m_lbName).text = " <color=" + haveColor + ">" + haveString + "</color><color=" + needColor + ">" + needString + "</color>";
	}

	public void SetGettedVisible(bool vis)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		if (m_spriteGetted == null)
		{
			if (!vis)
			{
				return;
			}
			m_spriteGetted = new VisualElement();
			m_spriteGetted.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/common/Check.png"));
			m_spriteGetted.style.width = StyleLength.op_Implicit(80f);
			m_spriteGetted.style.height = StyleLength.op_Implicit(80f);
			m_spriteGetted.style.position = StyleEnum<Position>.op_Implicit((Position)1);
			m_spriteGetted.style.top = StyleLength.op_Implicit(22f);
			m_spriteGetted.style.left = StyleLength.op_Implicit(20f);
			m_spriteGetted.pickingMode = (PickingMode)1;
			((VisualElement)this).Add(m_spriteGetted);
		}
		m_spriteGetted.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
	}

	public void SetLockVisible(bool vis)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		if (m_spriteLock == null)
		{
			if (!vis)
			{
				return;
			}
			m_spriteLock = new VisualElement();
			m_spriteLock.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/peerage/passport_lock.png"));
			m_spriteLock.style.width = StyleLength.op_Implicit(40f);
			m_spriteLock.style.height = StyleLength.op_Implicit(40f);
			m_spriteLock.style.position = StyleEnum<Position>.op_Implicit((Position)1);
			m_spriteLock.style.top = StyleLength.op_Implicit(-5f);
			m_spriteLock.style.right = StyleLength.op_Implicit(-5f);
			m_spriteLock.pickingMode = (PickingMode)1;
			((VisualElement)this).Add(m_spriteLock);
		}
		m_spriteLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
	}

	public void SetRareVisible(bool vis)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		if ((m_spineRare == null) & vis)
		{
			if (!vis)
			{
				return;
			}
			m_spineRare = new SpineVisualElement();
			m_spineRare.skeletonDataAsset = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>("animation/ui/ui_zhengui/ui_zhengui_SkeletonData.asset");
			((VisualElement)m_spineRare).style.position = StyleEnum<Position>.op_Implicit((Position)1);
			((VisualElement)m_spineRare).style.width = StyleLength.op_Implicit(110f);
			((VisualElement)m_spineRare).style.height = StyleLength.op_Implicit(110f);
			((VisualElement)m_spineRare).style.left = StyleLength.op_Implicit(15f);
			((VisualElement)m_spineRare).style.top = StyleLength.op_Implicit(15f);
			((VisualElement)m_spineRare).pickingMode = (PickingMode)1;
			m_wrapper.style.overflow = StyleEnum<Overflow>.op_Implicit((Overflow)1);
			m_wrapper.Add((VisualElement)(object)m_spineRare);
		}
		if (m_spineRare != null)
		{
			if (vis)
			{
				m_spineRare.AnimationState.SetAnimation(0, "animation", true);
			}
			((VisualElement)m_spineRare).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		}
	}

	public void SetSpineVisible(bool vis)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		if ((m_spineEffect == null) & vis)
		{
			m_spineEffect = new SpineVisualElement();
			m_spineEffect.skeletonDataAsset = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>("animation/ui/ui_kelingqu/ui_kelingqu_SkeletonData.asset");
			((VisualElement)m_spineEffect).style.position = StyleEnum<Position>.op_Implicit((Position)1);
			((VisualElement)m_spineEffect).style.width = StyleLength.op_Implicit(210f);
			((VisualElement)m_spineEffect).style.height = StyleLength.op_Implicit(210f);
			((VisualElement)m_spineEffect).style.top = StyleLength.op_Implicit(-35f);
			((VisualElement)m_spineEffect).style.flexShrink = StyleFloat.op_Implicit(0f);
			((VisualElement)m_spineEffect).style.flexGrow = StyleFloat.op_Implicit(0f);
			((VisualElement)m_spineEffect).style.alignSelf = StyleEnum<Align>.op_Implicit((Align)2);
			((VisualElement)m_spineEffect).pickingMode = (PickingMode)1;
			m_wrapper.style.overflow = StyleEnum<Overflow>.op_Implicit((Overflow)1);
			m_wrapper.Add((VisualElement)(object)m_spineEffect);
		}
		if (m_spineEffect != null)
		{
			if (vis)
			{
				m_spineEffect.AnimationState.SetAnimation(0, "animation", true);
			}
			((VisualElement)m_spineEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		}
	}

	public void SetZhenGuiVisible(bool vis)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		if ((m_zhenguiEffect == null) & vis)
		{
			m_zhenguiEffect = new SpineVisualElement();
			m_zhenguiEffect.skeletonDataAsset = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>("animation/ui/ui_zhengui2/ui_zhengui2_SkeletonData.asset");
			((VisualElement)m_zhenguiEffect).style.position = StyleEnum<Position>.op_Implicit((Position)1);
			((VisualElement)m_zhenguiEffect).style.width = StyleLength.op_Implicit(210f);
			((VisualElement)m_zhenguiEffect).style.height = StyleLength.op_Implicit(210f);
			((VisualElement)m_zhenguiEffect).style.top = StyleLength.op_Implicit(-45f);
			((VisualElement)m_zhenguiEffect).style.left = StyleLength.op_Implicit(-38f);
			((VisualElement)m_zhenguiEffect).pickingMode = (PickingMode)1;
			((VisualElement)this).Add((VisualElement)(object)m_zhenguiEffect);
		}
		if (m_zhenguiEffect != null)
		{
			if (vis)
			{
				m_zhenguiEffect.AnimationState.SetAnimation(0, "animation", true);
			}
			((VisualElement)m_zhenguiEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		}
	}

	public void SetLossVisible(bool vis)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		if ((m_spriteLoss == null) & vis)
		{
			m_spriteLoss = new VisualElement();
			m_spriteLoss.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/dungeon/icon_loss.png"));
			m_spriteLoss.style.width = StyleLength.op_Implicit(120f);
			m_spriteLoss.style.height = StyleLength.op_Implicit(120f);
			m_spriteLoss.style.position = StyleEnum<Position>.op_Implicit((Position)1);
			m_spriteLoss.style.top = StyleLength.op_Implicit(0f);
			m_spriteLoss.style.left = StyleLength.op_Implicit(0f);
			m_spriteLoss.pickingMode = (PickingMode)1;
			((VisualElement)this).Add(m_spriteLoss);
		}
		if (m_spriteLoss != null)
		{
			m_spriteLoss.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		}
		SetGray(vis);
	}

	public void SetTruckLossItemVisible(bool vis)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		if ((m_spriteTruckLossItem == null) & vis)
		{
			m_spriteTruckLossItem = new VisualElement();
			m_spriteTruckLossItem.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truckLossItem.png"));
			m_spriteTruckLossItem.style.width = StyleLength.op_Implicit(120f);
			m_spriteTruckLossItem.style.height = StyleLength.op_Implicit(120f);
			m_spriteTruckLossItem.style.position = StyleEnum<Position>.op_Implicit((Position)1);
			m_spriteTruckLossItem.style.top = StyleLength.op_Implicit(0f);
			m_spriteTruckLossItem.style.left = StyleLength.op_Implicit(0f);
			m_spriteTruckLossItem.pickingMode = (PickingMode)1;
			m_spriteTruckLossItem.style.opacity = StyleFloat.op_Implicit(0.8f);
			((VisualElement)this).Add(m_spriteTruckLossItem);
		}
		if (m_spriteTruckLossItem != null)
		{
			m_spriteTruckLossItem.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		}
		SetGray(vis);
	}

	private void AddPropOperateGetWayTag()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		if (m_spritePropTag == null)
		{
			m_spritePropTag = new VisualElement();
			m_spritePropTag.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/frame/prop_operate.png"));
			m_spritePropTag.style.width = StyleLength.op_Implicit(32f);
			m_spritePropTag.style.height = StyleLength.op_Implicit(32f);
			m_spritePropTag.style.position = StyleEnum<Position>.op_Implicit((Position)1);
			m_spritePropTag.style.top = StyleLength.op_Implicit(0f);
			m_spritePropTag.style.right = StyleLength.op_Implicit(0f);
			m_spriteIcon.Add(m_spritePropTag);
		}
		bool flag = false;
		if (m_award.type == EResourceType.Prop)
		{
			flag = zbDataConfig.Instance.propCsvTable.GetRecordByKey((int)m_award.id).getWay == 1;
		}
		if (m_spritePropTag != null)
		{
			m_spritePropTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		}
	}

	public void AddBestPrizeTag(bool vis)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		if (!vis)
		{
			if (m_spriteBestPrizeTag != null)
			{
				m_spriteBestPrizeTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
			}
			return;
		}
		if (m_spriteBestPrizeTag == null)
		{
			m_spriteBestPrizeTag = new VisualElement();
			m_spriteBestPrizeTag.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truckBg/bestPrize.png"));
			m_spriteBestPrizeTag.style.width = StyleLength.op_Implicit(32f);
			m_spriteBestPrizeTag.style.height = StyleLength.op_Implicit(32f);
			m_spriteBestPrizeTag.style.position = StyleEnum<Position>.op_Implicit((Position)1);
			m_spriteBestPrizeTag.style.top = StyleLength.op_Implicit(0f);
			m_spriteBestPrizeTag.style.right = StyleLength.op_Implicit(0f);
			m_spriteIcon.Add(m_spriteBestPrizeTag);
		}
		if (m_spriteBestPrizeTag != null)
		{
			m_spriteBestPrizeTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		}
	}

	public void SetGray(bool vis)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		m_spriteFrame.style.unityBackgroundImageTintColor = (StyleColor)(vis ? new StyleColor(Color.gray) : StyleColor.op_Implicit((StyleKeyword)1));
		m_spriteIcon.style.unityBackgroundImageTintColor = (StyleColor)(vis ? new StyleColor(Color.gray) : StyleColor.op_Implicit((StyleKeyword)1));
		m_spriteFrag.style.unityBackgroundImageTintColor = (StyleColor)(vis ? new StyleColor(Color.gray) : StyleColor.op_Implicit((StyleKeyword)1));
	}

	public void SetTouchCallback(Action<ClickEvent> callback)
	{
		m_touchCallback = callback;
	}

	public void OnTouch(ClickEvent evt)
	{
		if (m_touchCallback != null)
		{
			m_touchCallback(evt);
		}
		else if ((Object)(object)m_wndParent != (Object)null && m_award != null && m_award.id > 0)
		{
			zbUIMgr.Instance.ShowTip(m_award.type, m_award.id, 0L);
		}
		else if (noItemBubble != "")
		{
			zbUIMgr.Instance.BubbleInfo(noItemBubble);
		}
	}
}
