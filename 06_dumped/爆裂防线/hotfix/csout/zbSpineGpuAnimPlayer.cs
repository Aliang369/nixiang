using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class zbSpineGpuAnimPlayer
{
	private const float kMonsterLayerZDepth = 50f;

	private zbSpineGpuAnimBakedType m_baked;

	private zbMonster m_monster;

	private zbSpineGpuAnimRenderer m_renderer;

	internal Transform spineTf;

	internal zbSpineGpuAnimRenderer.MegaBatch.TypeRecord attachedType;

	private string m_currentClip;

	private bool m_currentLoop;

	private float m_currentTime;

	private string m_prevClip;

	private float m_prevTime;

	private float m_crossRemain;

	private float m_crossDuration;

	private readonly List<string> m_queuedClips;

	private readonly List<bool> m_queuedLoops;

	public float timeScale;

	private Color m_tint;

	private float m_skeletonAlpha;

	private Matrix4x4 m_manualMatrix;

	private float m_manualSortY;

	private const float kCrossFadeDuration = 0.2f;

	public bool isFallback { get; private set; }

	public bool hasPlayed => m_currentClip != null;

	public float frameA0 { get; private set; }

	public float frameA1 { get; private set; }

	public float blendA { get; private set; }

	public float frameB0 { get; private set; }

	public float frameB1 { get; private set; }

	public float blendB { get; private set; }

	public float crossWeight { get; private set; }

	public Vector4 tintVector
	{
		get
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			return new Vector4(m_tint.r, m_tint.g, m_tint.b, m_tint.a * m_skeletonAlpha);
		}
	}

	public void SetTint(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		m_tint = color;
	}

	public void SetSkeletonAlpha(float alpha)
	{
		m_skeletonAlpha = alpha;
	}

	public void Bind(zbSpineGpuAnimBakedType baked, zbMonster monster, zbSpineGpuAnimRenderer renderer, zbSpineGpuAnimRenderer.MegaBatch.TypeRecord typeRecord)
	{
		bool num = m_baked == baked;
		m_baked = baked;
		m_monster = monster;
		m_renderer = renderer;
		attachedType = typeRecord;
		spineTf = (((Object)(object)monster != (Object)null && (Object)(object)monster.m_skeletonRenderer != (Object)null) ? ((Component)monster.m_skeletonRenderer).transform : null);
		isFallback = false;
		if (!num)
		{
			Reset();
		}
		renderer?.Register(this);
	}

	public void Bind(zbSpineGpuAnimBakedType baked, Transform spineTransform, zbSpineGpuAnimRenderer renderer, zbSpineGpuAnimRenderer.MegaBatch.TypeRecord typeRecord)
	{
		bool num = m_baked == baked;
		m_baked = baked;
		m_monster = null;
		m_renderer = renderer;
		attachedType = typeRecord;
		spineTf = spineTransform;
		isFallback = false;
		if (!num)
		{
			Reset();
		}
		renderer?.Register(this);
	}

	public bool ShouldDrawInstance()
	{
		if (isFallback || !hasPlayed || m_baked == null)
		{
			return false;
		}
		if ((Object)(object)spineTf != (Object)null && !((Component)spineTf).gameObject.activeInHierarchy)
		{
			return false;
		}
		if ((Object)(object)m_monster != (Object)null)
		{
			if (m_monster.isGoDestroyed || m_monster.isUsingSprite)
			{
				return false;
			}
			if (m_monster.GetCurrentSpineTier() == zbMonster.ESpineUpdateTier.Culled)
			{
				return false;
			}
		}
		return true;
	}

	public void Unbind()
	{
		if (m_renderer != null)
		{
			m_renderer.Unregister(this);
		}
	}

	public void Reset()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		m_currentClip = null;
		m_currentLoop = false;
		m_currentTime = 0f;
		m_prevClip = null;
		m_prevTime = 0f;
		m_crossRemain = 0f;
		m_crossDuration = 0f;
		m_queuedClips.Clear();
		m_queuedLoops.Clear();
		timeScale = 1f;
		m_tint = Color.white;
		m_skeletonAlpha = 1f;
		isFallback = false;
	}

	public bool PlayClip(string clipName, bool loop)
	{
		return PlayClipInternal(clipName, loop, 0.2f, clearQueue: true);
	}

	public void PlaySetupPose()
	{
		PlayClipInternal("<setup>", loop: false, 0f, clearQueue: true);
	}

	private bool PlayClipInternal(string clipName, bool loop, float crossFade, bool clearQueue)
	{
		if (m_baked == null || !m_baked.clips.TryGetValue(clipName, out var _))
		{
			return false;
		}
		if (clearQueue)
		{
			m_queuedClips.Clear();
			m_queuedLoops.Clear();
		}
		if (hasPlayed && crossFade > 0f)
		{
			m_prevClip = m_currentClip;
			m_prevTime = m_currentTime;
			m_crossDuration = crossFade;
			m_crossRemain = crossFade;
		}
		else
		{
			m_prevClip = null;
			m_crossRemain = 0f;
			m_crossDuration = 0f;
		}
		m_currentClip = clipName;
		m_currentLoop = loop;
		m_currentTime = 0f;
		return true;
	}

	public void QueueClip(string clipName, bool loop)
	{
		if (m_baked != null && m_baked.clips.ContainsKey(clipName))
		{
			m_queuedClips.Add(clipName);
			m_queuedLoops.Add(loop);
		}
	}

	public void RandomizeTime()
	{
		if (m_baked != null && hasPlayed && m_baked.clips.TryGetValue(m_currentClip, out var value))
		{
			m_currentTime = ((value.duration > 0f) ? Rand.Range(0f, value.duration) : 0f);
		}
	}

	public void SetTime(float time)
	{
		m_currentTime = time;
	}

	public string GetCurrentClipName()
	{
		return m_currentClip;
	}

	public float GetCurrentClipDuration()
	{
		if (m_baked != null && hasPlayed && m_baked.clips.TryGetValue(m_currentClip, out var value))
		{
			return value.duration;
		}
		return 0f;
	}

	public void Advance(float deltaTime)
	{
		if (m_baked == null || !hasPlayed)
		{
			return;
		}
		float num = deltaTime * timeScale;
		m_currentTime += num;
		if (m_crossRemain > 0f)
		{
			m_crossRemain -= num;
			m_prevTime += num;
		}
		if (m_baked.clips.TryGetValue(m_currentClip, out var value))
		{
			if (m_currentLoop)
			{
				if (value.duration > 0f)
				{
					float currentTime = m_currentTime;
					m_currentTime = Mathf.Repeat(m_currentTime, value.duration);
					if (m_queuedClips.Count > 0 && currentTime >= value.duration)
					{
						DequeueNextClip();
					}
				}
			}
			else if (m_currentTime >= value.duration)
			{
				if (m_queuedClips.Count > 0)
				{
					DequeueNextClip();
				}
				else
				{
					m_currentTime = value.duration;
				}
			}
		}
		if (m_crossRemain <= 0f)
		{
			m_prevClip = null;
		}
		ComputeFrame(m_currentClip, m_currentTime, out var row, out var row2, out var blend);
		frameA0 = row;
		frameA1 = row2;
		blendA = blend;
		if (m_prevClip != null && m_crossRemain > 0f && m_crossDuration > 0f)
		{
			ComputeFrame(m_prevClip, m_prevTime, out var row3, out var row4, out var blend2);
			frameB0 = row3;
			frameB1 = row4;
			blendB = blend2;
			crossWeight = Mathf.Clamp01(m_crossRemain / m_crossDuration);
		}
		else
		{
			frameB0 = 0f;
			frameB1 = 0f;
			blendB = 0f;
			crossWeight = 0f;
		}
	}

	public void GetRenderInfo(out Matrix4x4 matrix, out float sortZ)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)spineTf != (Object)null)
		{
			matrix = spineTf.localToWorldMatrix;
			sortZ = (((Object)(object)m_monster != (Object)null) ? (50f + m_monster.cachPositionY * 0.1f) : (50f + spineTf.position.y * 0.1f));
		}
		else
		{
			matrix = m_manualMatrix;
			sortZ = 50f + m_manualSortY * 0.1f;
		}
	}

	public void SetManualTransform(Vector3 position, float scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		m_manualMatrix = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(scale, scale, scale));
		m_manualSortY = position.y;
	}

	public void FallbackToSpine()
	{
		if (isFallback || (Object)(object)m_monster == (Object)null)
		{
			return;
		}
		isFallback = true;
		Unbind();
		if (m_monster.anim != null && m_monster.anim.spineColorChange != null)
		{
			m_monster.anim.spineColorChange.gpuTintSink = null;
		}
		SkeletonAnimation val = ((m_monster.anim != null && m_monster.anim.spineAnimation != null) ? m_monster.anim.spineAnimation.skeletonAnimation : null);
		if ((Object)(object)val != (Object)null)
		{
			if (m_currentClip == "<setup>")
			{
				val.AnimationState.ClearTrack(0);
				val.AnimationState.SetEmptyAnimation(0, 0f);
			}
			else if (m_currentClip != null && ((SkeletonRenderer)val).skeleton.Data.FindAnimation(m_currentClip) != null)
			{
				val.AnimationState.ClearTrack(0);
				val.AnimationState.SetAnimation(0, m_currentClip, m_currentLoop).TrackTime = m_currentTime;
			}
			((Behaviour)val).enabled = true;
			((SkeletonRenderer)val).UpdateMode = (UpdateMode)3;
			val.UpdateTiming = (UpdateTiming)1;
			m_monster.InvalidateSpineTier();
			MeshRenderer component = ((Component)val).GetComponent<MeshRenderer>();
			if ((Object)(object)component != (Object)null)
			{
				((Renderer)component).enabled = true;
			}
			val.Update(0f);
			((SkeletonRenderer)val).LateUpdate();
		}
		m_monster.m_spineGpuTakenOver = false;
		zbSpineGpuAnimRuntime.RegisterPendingReconvert(m_monster);
		m_monster = null;
		m_renderer = null;
		spineTf = null;
	}

	private void DequeueNextClip()
	{
		string clipName = m_queuedClips[0];
		bool loop = m_queuedLoops[0];
		m_queuedClips.RemoveAt(0);
		m_queuedLoops.RemoveAt(0);
		PlayClipInternal(clipName, loop, 0.2f, clearQueue: false);
	}

	private void ComputeFrame(string clipName, float time, out float row0, out float row1, out float blend)
	{
		zbSpineGpuAnimBakedType.ClipInfo clipInfo = m_baked.clips[clipName];
		float num = ((clipInfo.duration > 0f) ? (time * m_baked.fps) : 0f);
		float num2 = Mathf.Floor(num);
		blend = num - num2;
		int num3 = (int)num2;
		if (num3 > clipInfo.frameCount - 1)
		{
			num3 = clipInfo.frameCount - 1;
		}
		if (num3 < 0)
		{
			num3 = 0;
		}
		int num4 = num3 + 1;
		if (num4 > clipInfo.frameCount - 1)
		{
			num4 = clipInfo.frameCount - 1;
		}
		row0 = clipInfo.rowStart + num3;
		row1 = clipInfo.rowStart + num4;
	}

	public zbSpineGpuAnimPlayer()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		m_queuedClips = new List<string>(2);
		m_queuedLoops = new List<bool>(2);
		timeScale = 1f;
		m_tint = Color.white;
		m_skeletonAlpha = 1f;
		m_manualMatrix = Matrix4x4.identity;
		base._002Ector();
	}
}
