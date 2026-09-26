using System.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEndlessMineGetItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbEndlessMineGetItem>
	{
	}

	private zbEndlessMineWnd m_wndParent;

	private zbEndlessMineMap m_parent;

	private VisualElement m_spriteIcon;

	private Label m_lbCount;

	private Coroutine m_coroutine;

	public bool m_isAction;

	private Vector2 m_targetPos;

	public zbEndlessMineGetItem()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		m_targetPos = new Vector2(15f, 690f);
		base._002Ector();
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/endlessMine/tb_endlessMineGet.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbEndlessMineWnd wndParent, zbEndlessMineMap parent)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_parent = parent;
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(AwardDetail award, Vector2 pos)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(award.Type, award.Tid);
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
		((TextElement)m_lbCount).text = "+" + award.Count;
		Vector2 startPos = ConvertToRootPos(pos);
		((VisualElement)this).style.left = StyleLength.op_Implicit(pos.x);
		((VisualElement)this).style.top = StyleLength.op_Implicit(pos.y);
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (m_coroutine != null)
		{
			((MonoBehaviour)m_wndParent).StopCoroutine(m_coroutine);
		}
		if (award.Type == EResourceType.Prop && award.Tid == m_wndParent.PropTypeIds[EPropType.Ore])
		{
			m_coroutine = ((MonoBehaviour)m_wndParent).StartCoroutine(AnimationMoveTo(startPos));
		}
		else
		{
			m_coroutine = ((MonoBehaviour)m_wndParent).StartCoroutine(AnimationFloat(startPos));
		}
	}

	private Vector2 ConvertToRootPos(Vector2 scrollPos)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = VisualElementExtensions.LocalToWorld(((VisualElement)m_parent.m_scrollView).contentContainer, scrollPos);
		return VisualElementExtensions.WorldToLocal(((VisualElement)this).parent, val);
	}

	private IEnumerator AnimationFloat(Vector2 startPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		m_isAction = true;
		float duration = 0.8f;
		float elapsed = 0f;
		Vector2 endPos = startPos + new Vector2(0f, -50f);
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
		m_wndParent.m_isGetPropsAck = false;
		m_wndParent.RefreshExchange();
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_isAction = false;
	}

	private IEnumerator AnimationMoveTo(Vector2 startPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		m_isAction = true;
		yield return null;
		m_targetPos = GetTargetPos();
		float duration = 0.8f;
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
		m_wndParent.m_spineProgress.AnimationState.SetAnimation(0, "animation", false);
		m_wndParent.m_isGetPropsAck = false;
		m_wndParent.RefreshExchange();
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_isAction = false;
	}

	private Vector2 GetTargetPos()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Rect worldBound = m_wndParent.m_spriteOre.worldBound;
		Vector2 position = ((Rect)(ref worldBound)).position;
		return VisualElementExtensions.WorldToLocal(((VisualElement)this).parent, position);
	}
}
