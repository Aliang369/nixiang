using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFormationGunItem : VisualElement
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteHero;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private VisualElement m_current;

	private VisualElement m_lock;

	private zbFormationWnd m_wndParent;

	public zbPlayerGunCsvRecord m_record;

	private VisualElement[] m_spriteStars = (VisualElement[])(object)new VisualElement[5];

	public zbFormationGunItem(zbFormationWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/formation/tb_formationGun.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteHero = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_current = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_OK", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_lock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_lock", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "iconstar" + i, (string)null);
		}
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
	}

	public void Refresh(zbPlayerGunCsvRecord record)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
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
		m_current.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.GetGunId() != m_record.id) ? 1 : 0));
		for (int i = 0; i < 5; i++)
		{
			Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/suitGun/star_0.png");
			m_spriteStars[i].style.backgroundImage = StyleBackground.op_Implicit(val3);
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

	public void OnTouch()
	{
		if (zbSelf.Instance.suitGunHandler.GetPlayerGunDetail(m_record.id) == null && !zbSelf.Instance.suitGunHandler.CanGunActiveOrUpgrade(m_record.id))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_record.propId, 1L);
			return;
		}
		zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.10729"), delegate(bool isOK)
		{
			if (isOK)
			{
				zbUIMgr.Instance.ShowPlayerGun();
			}
		});
	}
}
