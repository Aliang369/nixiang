using System.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSnowVeilGetItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbSnowVeilGetItem>
	{
	}

	private zbSnowVeilWnd m_wndParent;

	private zbSnowVeilMap m_parent;

	private VisualElement m_spriteIcon;

	private Label m_lbCount;

	private Coroutine m_coroutine;

	public bool m_isAction;

	private Vector2 m_targetPos;

	public zbSnowVeilGetItem()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		m_targetPos = new Vector2(15f, 400f);
		base._002Ector();
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/snowVeil/tb_snowVeilGet.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbSnowVeilWnd wndParent, zbSnowVeilMap parent)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_parent = parent;
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(SnowVeilLandDetail landDetail)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent.m_isGetPropsAck = false;
		Vector2 startPos = GetStartPos(landDetail.Index - 1);
		((VisualElement)this).style.left = StyleLength.op_Implicit(startPos.x);
		((VisualElement)this).style.top = StyleLength.op_Implicit(startPos.y);
		if (m_coroutine != null)
		{
			((MonoBehaviour)m_wndParent).StopCoroutine(m_coroutine);
		}
		if (landDetail.LandType == ESnowVeilLandType.Turntable)
		{
			zbUIMgr.Instance.ShowSnowVeilTurntable(m_wndParent.m_data);
		}
		else if (landDetail.LandType == ESnowVeilLandType.Rolling)
		{
			zbUIMgr.Instance.ShowSnowVeilRolling(m_wndParent.m_data);
		}
		else if (landDetail.LandType == ESnowVeilLandType.Score || landDetail.LandType == ESnowVeilLandType.HighScore)
		{
			string resName = "ui/image/operate/snowVeil/snowVeil_type_score.png";
			if (landDetail.LandType == ESnowVeilLandType.HighScore)
			{
				resName = "ui/image/operate/snowVeil/snowVeil_type_highScore.png";
			}
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
			((TextElement)m_lbCount).text = "+" + landDetail.Score * zbSelf.Instance.snowVeilHandler.m_goAckfactor;
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_coroutine = ((MonoBehaviour)m_wndParent).StartCoroutine(AnimationFloat(startPos));
			m_wndParent.RefreshTarget();
			m_wndParent.m_spineProgress.AnimationState.SetAnimation(0, "animation", false);
		}
		else if (landDetail.LandType == ESnowVeilLandType.Item)
		{
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(landDetail.Item.Type, landDetail.Item.Tid);
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
			((TextElement)m_lbCount).text = "+" + landDetail.Item.Count * zbSelf.Instance.snowVeilHandler.m_goAckfactor;
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_coroutine = ((MonoBehaviour)m_wndParent).StartCoroutine(AnimationFloat(startPos));
		}
	}

	private IEnumerator AnimationFloat(Vector2 startPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		m_isAction = true;
		float duration = 0.8f / m_wndParent.m_speed;
		float elapsed = 0f;
		Vector2 endPos = startPos + new Vector2(0f, -100f);
		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			float num2 = Mathf.SmoothStep(0f, 1f, num);
			Vector2 val = Vector2.Lerp(startPos, endPos, num2);
			((VisualElement)this).style.left = StyleLength.op_Implicit(val.x);
			((VisualElement)this).style.top = StyleLength.op_Implicit(val.y);
			yield return null;
		}
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_isAction = false;
	}

	public IEnumerator AnimationMoveTo(Vector2 startPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		m_isAction = true;
		yield return null;
		m_targetPos = GetTargetPos();
		float duration = 0.8f / m_wndParent.m_speed;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			float num2 = Mathf.SmoothStep(0f, 1f, num);
			Vector2 val = Vector2.Lerp(startPos, m_targetPos, num2);
			((VisualElement)this).style.left = StyleLength.op_Implicit(val.x);
			((VisualElement)this).style.top = StyleLength.op_Implicit(val.y);
			yield return null;
		}
		m_wndParent.RefreshTarget();
		m_wndParent.m_spineProgress.AnimationState.SetAnimation(0, "animation", false);
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_isAction = false;
	}

	private Vector2 GetStartPos(int spriteRendererIndex)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(((Component)m_parent.m_spriteRenderers[spriteRendererIndex]).transform.position);
		Vector2 val2 = RuntimePanelUtils.CameraTransformWorldToPanel(m_wndParent.GetRootElement().panel, Vector2.op_Implicit(val), Camera.main);
		return VisualElementExtensions.WorldToLocal(((VisualElement)this).parent, val2) + new Vector2(-25f, -25f);
	}

	private Vector2 GetTargetPos()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Rect worldBound = m_wndParent.m_spriteScore.worldBound;
		Vector2 position = ((Rect)(ref worldBound)).position;
		return VisualElementExtensions.WorldToLocal(((VisualElement)this).parent, position);
	}
}
