using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarSettlePreviewWnd : zbBaseWnd
{
	private SpineVisualElement m_spinePrivilege;

	private Button m_btnClose;

	private ELegionWarForce eLegionWarForce;

	private VisualElement m_bgState;

	private Label m_lbDesc;

	private int redScoreCount;

	private int blueScoreCount;

	private int winScoreCount;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_bgState = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_state", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_temp", (string)null);
		m_spinePrivilege = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "SpineVisualElement", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(ELegionWarForce _eLegionWarForce, int _redScoreCount, int _blueScoreCount)
	{
		eLegionWarForce = _eLegionWarForce;
		redScoreCount = _redScoreCount;
		blueScoreCount = _blueScoreCount;
		if (_eLegionWarForce == ELegionWarForce.Red)
		{
			winScoreCount = redScoreCount;
		}
		else
		{
			winScoreCount = blueScoreCount;
		}
		((MonoBehaviour)this).StartCoroutine(ShootRepeater());
	}

	public IEnumerator ShootRepeater()
	{
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/legionWar/bg_win.png");
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/legionWar/bg_lose.png");
		int num = Extensions.Value<int>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["warWinScore"]);
		((VisualElement)m_btnClose).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_bgState.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId == eLegionWarForce)
		{
			zbAudioMgr.Instance.PlayAudio("audio/ui/legionwar_win.ogg");
			if ((Object)(object)val != (Object)null)
			{
				m_bgState.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
				m_bgState.MarkDirtyRepaint();
			}
			if (winScoreCount >= num)
			{
				((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.11243"), zbUtil.ConvertToTBMK(num));
			}
			else
			{
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.11245");
			}
			TrackEntry val3 = ((eLegionWarForce != ELegionWarForce.Red) ? m_spinePrivilege.AnimationState.SetAnimation(0, "blue_win", false) : m_spinePrivilege.AnimationState.SetAnimation(0, "red_win", false));
			yield return (object)new WaitForSeconds(val3.AnimationEnd);
			if (eLegionWarForce == ELegionWarForce.Red)
			{
				m_spinePrivilege.AnimationState.SetAnimation(0, "red_win2", true);
			}
			else
			{
				m_spinePrivilege.AnimationState.SetAnimation(0, "blue_win2", true);
			}
		}
		else
		{
			zbAudioMgr.Instance.PlayAudio("audio/ui/legionwar_lose.ogg");
			if ((Object)(object)val2 != (Object)null)
			{
				m_bgState.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
				m_bgState.MarkDirtyRepaint();
			}
			if (winScoreCount >= num)
			{
				((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.11244"), zbUtil.ConvertToTBMK(num));
			}
			else
			{
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.11246");
			}
			TrackEntry val3 = ((zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId != ELegionWarForce.Red) ? m_spinePrivilege.AnimationState.SetAnimation(0, "blue_loss", false) : m_spinePrivilege.AnimationState.SetAnimation(0, "red_loss", false));
			yield return (object)new WaitForSeconds(val3.AnimationEnd);
			if (zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId == ELegionWarForce.Red)
			{
				m_spinePrivilege.AnimationState.SetAnimation(0, "red_loss2", true);
			}
			else
			{
				m_spinePrivilege.AnimationState.SetAnimation(0, "blue_loss2", true);
			}
		}
		m_bgState.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnClose).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private void OnClose()
	{
		zbUIMgr.Instance.ShowLegionWarSettle();
		Hide();
	}
}
