using UnityEngine;

public class zbSeasonBuild : MonoBehaviour
{
	protected Transform m_buildRoot;

	public int m_buildId;

	public int m_areaId;

	public ESeasonBuildType m_buildType;

	protected zbSeasonHudBase m_hud;

	protected zbSeasonMapController m_seasonMapCtrl;

	protected SpriteRenderer m_spriteRenderer;

	protected virtual void Awake()
	{
		m_buildRoot = ((Component)this).transform.Find("buildroot");
		m_spriteRenderer = ((Component)this).gameObject.GetComponent<SpriteRenderer>();
	}

	public virtual void Init(int buildId, ESeasonBuildType type, int areaId)
	{
		m_buildId = buildId;
		m_areaId = areaId;
		m_buildType = type;
		switch (m_buildType)
		{
		case ESeasonBuildType.Normal:
			m_hud = new zbSeasonHudBuild(((Component)this).transform, m_buildId);
			break;
		case ESeasonBuildType.Force:
			m_hud = new zbSeasonHudForceBuild(((Component)this).transform, m_buildId);
			break;
		case ESeasonBuildType.Resource:
			m_hud = new zbSeasonHudResourceBuild(((Component)this).transform, m_buildId);
			break;
		case ESeasonBuildType.Center:
			m_hud = new zbSeasonHudBuild(((Component)this).transform, m_buildId);
			break;
		default:
			m_hud = new zbSeasonHudBuild(((Component)this).transform, m_buildId);
			break;
		}
		m_hud.Init();
		zbSeasonHudMgr.Instance.AddHud(m_hud);
	}

	public virtual void Update()
	{
	}

	protected virtual void OnDestroy()
	{
		if (m_hud != null)
		{
			zbSeasonHudMgr.Instance.RemoveHud(m_hud);
			m_hud = null;
		}
	}

	public virtual void OnClick()
	{
	}
}
