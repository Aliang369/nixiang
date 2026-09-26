using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWallSubjectBarWnd : zbBaseWnd
{
	private SkeletonAnimation m_spinePP;

	private SpriteRenderer[] m_spritePPs = (SpriteRenderer[])(object)new SpriteRenderer[10];

	private SpriteRenderer[] m_spriteNews = (SpriteRenderer[])(object)new SpriteRenderer[10];

	private SkeletonAnimation[] m_spineConverts = (SkeletonAnimation[])(object)new SkeletonAnimation[10];

	private Label m_lbTitle;

	private Label m_lbTime;

	private Button m_btnOne;

	private Label m_lbOne;

	private Button m_btnTen;

	private Label m_lbTen;

	private Button m_btnRelic;

	private Label m_lbTip;

	private Button m_btnClose;

	private Button m_btnTouch;

	private VisualElement m_nodeToggle;

	private VisualElement m_spriteOk;

	private VisualElement m_newRelic;

	private VisualElement m_notifyOne;

	private VisualElement m_notifyTen;

	private WallSubjectDetail m_data;

	private bool m_skipAnimationEnabled;

	public bool m_isPlayingAnimation;

	private bool m_hasConvert;

	private int[] m_ackRelicIds;

	private int[] m_ackNewRelicIds;

	private bool m_skipAnimation;

	private Coroutine m_animCoroutine;

	private HashSet<int> m_oldRelics;

	protected override void OnEnable()
	{
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		GameObject gameObject = ((Component)GetMyGameObjectSceneRoot().transform.Find("spine_pp")).gameObject;
		m_spinePP = gameObject.GetComponent<SkeletonAnimation>();
		((Component)m_spinePP).gameObject.SetActive(false);
		for (int i = 0; i < 10; i++)
		{
			m_spritePPs[i] = ((Component)GetMyGameObjectSceneRoot().transform.Find("sprite_pp" + i)).GetComponent<SpriteRenderer>();
			m_spriteNews[i] = ((Component)((Component)m_spritePPs[i]).transform.Find("sprite_new" + i)).GetComponent<SpriteRenderer>();
			m_spineConverts[i] = ((Component)((Component)m_spritePPs[i]).transform.Find("spine_convert" + i)).GetComponent<SkeletonAnimation>();
			((Component)m_spritePPs[i]).gameObject.SetActive(false);
			((Component)m_spineConverts[i]).gameObject.SetActive(false);
		}
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_btnOne = UQueryExtensions.Q<Button>(m_RootElement, "btn_one", (string)null);
		m_lbOne = UQueryExtensions.Q<Label>(m_RootElement, "lb_one", (string)null);
		m_btnTen = UQueryExtensions.Q<Button>(m_RootElement, "btn_ten", (string)null);
		m_lbTen = UQueryExtensions.Q<Label>(m_RootElement, "lb_ten", (string)null);
		m_btnRelic = UQueryExtensions.Q<Button>(m_RootElement, "btn_relic", (string)null);
		m_lbTip = UQueryExtensions.Q<Label>(m_RootElement, "lb_tip", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_nodeToggle = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_toggle", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_nodeToggle, "sprite_ok", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>(m_nodeToggle, (Action)OnToggle, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOne, (Action<ClickEvent>)delegate
		{
			OnBar(1);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTen, (Action<ClickEvent>)delegate
		{
			OnBar(10);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRelic, (Action<ClickEvent>)OnRelic, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_newRelic = zbUtil.CreateNewTag((VisualElement)(object)m_btnRelic, (Vector2?)new Vector2(-3f, -3f), 1f);
		m_notifyOne = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnOne, (Vector2?)new Vector2(5f, 5f), 1f, true);
		m_notifyTen = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTen, (Vector2?)new Vector2(5f, 5f), 1f, true);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Prop, RefreshMoneyPanel);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WallSubject, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_WallSubjectBarDraw_Ack, OnWallSubjectBarDrawAck);
	}

	public void RefreshCustom(WallSubjectDetail data)
	{
		m_data = data;
		((VisualElement)m_btnTouch).visible = false;
		RefreshDetail();
		ChangeAnimPlay();
		RefreshTime();
		RefreshMoneyPanel();
	}

	private void RefreshMoneyPanel(object data = null)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		CreateMoneyPanel(new EMoneyType[0]);
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, zbSelf.Instance.wallSubjectHandler.m_barPropId);
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Prop, zbSelf.Instance.wallSubjectHandler.m_barPropId);
		m_moneyPanel.RefreshOther(0, resorceIconPath.strIconPath, resourceCount, delegate
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Prop, zbSelf.Instance.wallSubjectHandler.m_barPropId, 0L);
		});
		((VisualElement)m_lbOne).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
		((VisualElement)m_lbTen).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
	}

	private void RefreshTime()
	{
		if (m_data != null)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(m_data.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
	}

	public void RefreshDetail()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		m_newRelic.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallSubjectHandler.HasRelicNew()));
		m_notifyOne.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.itemHandler.HasEnoughProp(zbSelf.Instance.wallSubjectHandler.m_barPropId, 10L)));
		m_notifyTen.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.itemHandler.HasEnoughProp(zbSelf.Instance.wallSubjectHandler.m_barPropId, 100L)));
		bool flag = zbSelf.Instance.itemHandler.HasEnoughProp(zbSelf.Instance.wallSubjectHandler.m_barPropId, 10L);
		((VisualElement)m_lbOne).style.color = StyleColor.op_Implicit(flag ? Color.white : Color.red);
		bool flag2 = zbSelf.Instance.itemHandler.HasEnoughProp(zbSelf.Instance.wallSubjectHandler.m_barPropId, 100L);
		((VisualElement)m_lbTen).style.color = StyleColor.op_Implicit(flag2 ? Color.white : Color.red);
	}

	private void OnBar(int count)
	{
		m_oldRelics = new HashSet<int>((IEnumerable<int>)m_data.BarInfo.RelicList);
		if (!m_isPlayingAnimation)
		{
			if (!zbSelf.Instance.itemHandler.HasEnoughProp(zbSelf.Instance.wallSubjectHandler.m_barPropId, 10 * count))
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, zbSelf.Instance.wallSubjectHandler.m_barPropId, 10 * count);
			}
			else
			{
				zbWsMsgHandler.Instance.Operate_WallSubjectBarDraw_Req(m_data.BaseInfo.UUID, count);
			}
		}
	}

	public void OnRelic(ClickEvent evt)
	{
		if (!m_isPlayingAnimation)
		{
			zbUIMgr.Instance.ShowWallSubjectRelic(m_data);
		}
	}

	public void OnClose(ClickEvent evt)
	{
		if (!m_isPlayingAnimation)
		{
			Hide();
		}
	}

	private void OnTouch(ClickEvent evt)
	{
		if (!m_hasConvert)
		{
			ConvertRepeatRelics();
		}
		m_skipAnimation = true;
	}

	private void ChangeAnimPlay()
	{
		m_spriteOk.visible = m_skipAnimationEnabled;
	}

	private void OnToggle()
	{
		if (!m_isPlayingAnimation)
		{
			m_skipAnimationEnabled = !m_skipAnimationEnabled;
			ChangeAnimPlay();
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		OnEventUpdate(null);
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		OnEventUpdate(null);
	}

	private void OnEventUpdate(object obj)
	{
		if (m_data != null)
		{
			WallSubjectDetail wallSubjectDetail = zbSelf.Instance.wallSubjectHandler.GetWallSubjectDetail();
			RefreshCustom(wallSubjectDetail);
		}
	}

	private void OnWallSubjectBarDrawAck(object data)
	{
		if (!(data is OperateWallSubjectBarDrawAck operateWallSubjectBarDrawAck))
		{
			return;
		}
		m_ackRelicIds = ((IEnumerable<int>)operateWallSubjectBarDrawAck.RelicList).ToArray();
		List<int> list = new List<int>();
		foreach (int relic in operateWallSubjectBarDrawAck.RelicList)
		{
			if (m_oldRelics.Add(relic))
			{
				zbSelf.Instance.newTagHandler.AddNewRelic(relic);
				list.Add(relic);
			}
		}
		m_ackNewRelicIds = list.ToArray();
		if (m_animCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_animCoroutine);
			m_animCoroutine = null;
		}
		m_isPlayingAnimation = true;
		m_animCoroutine = ((MonoBehaviour)this).StartCoroutine(SpineAnimation(operateWallSubjectBarDrawAck));
	}

	private IEnumerator SpineAnimation(OperateWallSubjectBarDrawAck ack)
	{
		m_hasConvert = false;
		m_skipAnimation = false;
		((VisualElement)m_btnTouch).visible = !m_skipAnimationEnabled;
		bool flag = ack.RelicList.Count == 1;
		string text = (flag ? "pull_in" : "pull10_in");
		string text2 = (flag ? "pull_loop" : "pull10_loop");
		string outAnim = (flag ? "pull_out" : "pull10_out");
		((Component)m_spinePP).gameObject.SetActive(true);
		((SkeletonRenderer)m_spinePP).Initialize(true, false);
		RefreshAckRelicIcons(m_ackRelicIds, m_ackNewRelicIds);
		if (m_skipAnimationEnabled)
		{
			yield return PlayAnimation(outAnim);
		}
		else
		{
			m_spinePP.AnimationState.SetAnimation(0, text, false);
			m_spinePP.AnimationState.AddAnimation(0, text2, true, 0f).Start += (TrackEntryDelegate)delegate
			{
				zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
				ConvertRepeatRelics();
			};
			while (!m_skipAnimation)
			{
				yield return null;
			}
			ConvertRepeatRelics();
			yield return PlayAnimation(outAnim);
		}
		zbAudioMgr.Instance.PlayAudio("audio/ui/rise_normal.ogg");
		yield return CollectRelicIcons();
		EndSpineAnimation(ack);
	}

	private IEnumerator PlayAnimation(string animationName)
	{
		TrackEntry entry = m_spinePP.AnimationState.SetAnimation(0, animationName, false);
		while (entry.TrackTime < entry.Animation.Duration)
		{
			yield return null;
		}
	}

	private void RefreshAckRelicIcons(int[] reliclist, int[] newReliclist)
	{
		for (int i = 0; i < 10; i++)
		{
			((Component)m_spritePPs[i]).gameObject.SetActive(false);
		}
		int num = 0;
		foreach (int relicId in reliclist)
		{
			if (num < m_spritePPs.Length)
			{
				zbWallSubjectRelicCsvRecord zbWallSubjectRelicCsvRecord2 = zbSelf.Instance.wallSubjectHandler.m_relicRecords.FirstOrDefault((zbWallSubjectRelicCsvRecord r) => r.subjectId == m_data.SubjectId && r.id == relicId);
				if (zbWallSubjectRelicCsvRecord2 != null)
				{
					((Component)m_spritePPs[num]).gameObject.SetActive(true);
					m_spritePPs[num].sprite = zbAssetLoader.Instance.LoadSpriteAsset(zbWallSubjectRelicCsvRecord2.icon);
					((Component)m_spriteNews[num]).gameObject.SetActive(Enumerable.Contains(newReliclist, relicId));
				}
			}
			num++;
		}
	}

	private void ConvertRepeatRelics()
	{
		if (!m_hasConvert)
		{
			m_hasConvert = true;
			RefreshLoopRelicIcons();
		}
	}

	private void RefreshLoopRelicIcons()
	{
		int i;
		for (i = 0; i < m_ackRelicIds.Length && i < m_spritePPs.Length; i++)
		{
			if (!Enumerable.Contains(m_ackNewRelicIds, m_ackRelicIds[i]))
			{
				zbWallSubjectRelicCsvRecord zbWallSubjectRelicCsvRecord2 = zbSelf.Instance.wallSubjectHandler.m_relicRecords.FirstOrDefault((zbWallSubjectRelicCsvRecord r) => r.subjectId == m_data.SubjectId && r.id == m_ackRelicIds[i]);
				if (zbWallSubjectRelicCsvRecord2 != null)
				{
					stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)zbWallSubjectRelicCsvRecord2.convertAward[0], zbWallSubjectRelicCsvRecord2.convertAward[1]);
					((MonoBehaviour)this).StartCoroutine(PlayConvertEffect(i, zbAssetLoader.Instance.LoadSpriteAsset(resorceIconPath.strIconPath)));
				}
			}
		}
	}

	private IEnumerator PlayConvertEffect(int index, Sprite newSprite)
	{
		SkeletonAnimation effect = m_spineConverts[index];
		((Component)effect).gameObject.SetActive(true);
		((Component)effect).transform.position = ((Component)m_spritePPs[index]).transform.position;
		((Component)effect).transform.rotation = ((Component)m_spritePPs[index]).transform.rotation;
		((Component)effect).transform.localScale = ((Component)m_spritePPs[index]).transform.lossyScale;
		((SkeletonRenderer)effect).Initialize(true, false);
		effect.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSecondsRealtime(0.2f);
		m_spritePPs[index].sprite = newSprite;
		yield return (object)new WaitForSecondsRealtime(8f / 15f);
		((Component)effect).gameObject.SetActive(false);
	}

	private void EndSpineAnimation(OperateWallSubjectBarDrawAck ack)
	{
		m_isPlayingAnimation = false;
		((Component)m_spinePP).gameObject.SetActive(false);
		for (int i = 0; i < 10; i++)
		{
			((Component)m_spritePPs[i]).gameObject.SetActive(false);
		}
		zbUIMgr.Instance.ShowAward(((IEnumerable<AwardDetail>)ack.AwardList).ToArray());
		WallSubjectDetail wallSubjectDetail = zbSelf.Instance.wallSubjectHandler.GetWallSubjectDetail();
		RefreshCustom(wallSubjectDetail);
		m_skipAnimation = false;
		m_animCoroutine = null;
	}

	private IEnumerator CollectRelicIcons()
	{
		float num = 0.35f;
		for (int i = 0; i < m_spritePPs.Length && i < m_ackRelicIds.Length; i++)
		{
			SpriteRenderer val = m_spritePPs[i];
			if (((Component)val).gameObject.activeSelf)
			{
				if (!Enumerable.Contains(m_ackNewRelicIds, m_ackRelicIds[i]))
				{
					((Component)val).gameObject.SetActive(false);
					continue;
				}
				GameObject val2 = new GameObject("FlyRelic");
				SpriteRenderer val3 = val2.AddComponent<SpriteRenderer>();
				val3.sprite = val.sprite;
				((Renderer)val3).material = ((Renderer)val).sharedMaterial;
				val3.color = val.color;
				((Renderer)val3).sortingLayerID = ((Renderer)val).sortingLayerID;
				((Renderer)val3).sortingOrder = ((Renderer)val).sortingOrder;
				val2.transform.position = ((Component)val).transform.position;
				val2.transform.rotation = ((Component)val).transform.rotation;
				val2.transform.localScale = ((Component)val).transform.lossyScale;
				((MonoBehaviour)this).StartCoroutine(Fly(val3));
				((Component)val).gameObject.SetActive(false);
			}
		}
		yield return (object)new WaitForSecondsRealtime(num);
	}

	private IEnumerator Fly(SpriteRenderer clone)
	{
		Vector3 start = ((Component)clone).transform.position;
		Vector3 target = new Vector3(-3f, -6.4f, -1f);
		Vector3 startScale = ((Component)clone).transform.localScale;
		float duration = 1f;
		float timer = 0f;
		while (timer < duration)
		{
			timer += Time.unscaledDeltaTime;
			float num = Mathf.Clamp01(timer / duration);
			num = 1f - Mathf.Pow(1f - num, 3f);
			((Component)clone).transform.position = Vector3.Lerp(start, target, num);
			((Component)clone).transform.localScale = Vector3.Lerp(startScale, Vector3.zero, num);
			yield return null;
		}
		Object.Destroy((Object)(object)((Component)clone).gameObject);
	}
}
