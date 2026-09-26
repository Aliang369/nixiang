using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerGunItem : VisualElement
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteHero;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private VisualElement m_select;

	private VisualElement m_current;

	private VisualElement m_lock;

	private zbPlayerGunWnd m_parent;

	public zbPlayerGunCsvRecord m_record;

	private VisualElement[] m_spriteStars = (VisualElement[])(object)new VisualElement[5];

	public zbPlayerGunItem(zbPlayerGunWnd parent)
	{
		m_parent = parent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_gun.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteHero = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_select = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_sel", (string)null);
		m_current = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_OK", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_lock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_lock", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "iconstar" + i, (string)null);
		}
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
	}

	public void RefreshBase(zbPlayerGunCsvRecord record)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(record.icon);
		if ((Object)(object)val != (Object)null)
		{
			m_spriteHero.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_spriteHero.MarkDirtyRepaint();
		}
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex((EQuality)m_record.quality);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceFrameTex);
		if ((Object)(object)val2 != (Object)null)
		{
			m_spriteFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteFrame.MarkDirtyRepaint();
		}
	}

	public void Refresh()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		m_current.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.GetGunId() != m_record.id) ? 1 : 0));
		for (int i = 0; i < 5; i++)
		{
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/suitGun/star_0.png");
			m_spriteStars[i].style.backgroundImage = StyleBackground.op_Implicit(val);
		}
		PlayerGunDetail playerGunDetail = zbSelf.Instance.suitGunHandler.GetPlayerGunDetail(m_record.id);
		if (playerGunDetail == null)
		{
			m_spriteFrame.style.unityBackgroundImageTintColor = StyleColor.op_Implicit(new Color(0.5f, 0.5f, 0.5f, 1f));
			m_spriteHero.style.unityBackgroundImageTintColor = StyleColor.op_Implicit(new Color(0.5f, 0.5f, 0.5f, 1f));
			m_lock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			zbUIEffect.SetStars(m_spriteStars, playerGunDetail.Star);
			m_spriteFrame.style.unityBackgroundImageTintColor = StyleColor.op_Implicit(new Color(1f, 1f, 1f, 1f));
			m_spriteHero.style.unityBackgroundImageTintColor = StyleColor.op_Implicit(new Color(1f, 1f, 1f, 1f));
			m_lock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.suitGunHandler.CanGunActiveOrUpgrade(m_record.id)));
	}

	public void RefreshSelect(bool flag)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_select.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	public void OnTouch()
	{
		m_parent.m_selectGunId = m_record.id;
		m_parent.RefreshCustom();
	}
}
