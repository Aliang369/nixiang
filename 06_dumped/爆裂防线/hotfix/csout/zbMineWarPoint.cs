using System;
using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbMineWarPoint : MonoBehaviour
{
	public SpriteRenderer m_spriteRenderer;

	private SkeletonAnimation m_spineAnimation;

	private GameObject m_spineZhaozi;

	private GameObject m_spineBaoza;

	public zbSpriteColorChange spriteColorChange;

	protected ESortMainLayerType m_sortType = ESortMainLayerType.Elevator;

	private int currX;

	private int layerNum;

	[NonSerialized]
	public zbMineWarHud mineWarHud;

	private MineWarPointDetail mineWarPointDetail;

	private void Awake()
	{
		zbSortMainMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
	}

	public void Init(int i)
	{
		Transform val = ((Component)this).transform.Find("visibleObject");
		m_spriteRenderer = ((Component)val.Find("lightbg")).GetComponent<SpriteRenderer>();
		m_spineAnimation = ((Component)val.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
		m_spineZhaozi = ((Component)val.Find("spineZhaozi")).gameObject;
		m_spineBaoza = ((Component)val.Find("spineBaoza")).gameObject;
		spriteColorChange = new zbSpriteColorChange((MonoBehaviour)(object)this);
		currX = i;
		layerNum = zbSelf.Instance.mineWarHandler.GetLayerByPoint(currX);
		mineWarHud = new zbMineWarHud();
		mineWarHud.Init(this);
		string text = "animation/mineWar/kuafu_feichuan/kuafu_feichuan";
		if (layerNum == 2)
		{
			text = "animation/mineWar/kuafu_jianzhu_1/kuafu_jianzhu_1";
		}
		else if (layerNum == 3)
		{
			text = "animation/mineWar/kuafu_jianzhu_2/kuafu_jianzhu_2";
		}
		else if (layerNum == 4)
		{
			text = "animation/mineWar/kuafu_jianzhu_3/kuafu_jianzhu_3";
		}
		zbUtil.RefreshSpine(m_spineAnimation, text + "_SkeletonData.asset", "animation", loop: true);
	}

	public void Refresh(MineWarPointDetail _mineWarPointDetail)
	{
		mineWarPointDetail = _mineWarPointDetail;
		mineWarHud.Refresh(mineWarPointDetail);
		if (mineWarPointDetail.DestroyTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			m_spineAnimation.state.SetAnimation(0, "animation2", true);
		}
		else
		{
			m_spineAnimation.state.SetAnimation(0, "animation", true);
		}
		if (mineWarPointDetail.ProtectTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			m_spineZhaozi.SetActive(true);
			m_spineBaoza.SetActive(false);
		}
		else if (mineWarPointDetail.LockedTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			m_spineBaoza.SetActive(true);
			m_spineZhaozi.SetActive(false);
		}
		else
		{
			m_spineBaoza.SetActive(false);
			m_spineZhaozi.SetActive(false);
		}
		string strAssetPath = "ui/image/mineWar/mine_selfbg.png";
		if (zbSelf.Instance.mineWarHandler.currServerId != zbSelf.Instance.GetServerId())
		{
			strAssetPath = ((layerNum != zbSelf.Instance.mineWarHandler.canCuihuiLayer) ? "ui/image/mineWar/mine_redbg.png" : ((mineWarPointDetail.DestroyTm <= zbSelf.Instance.timeHandler.GetServerTime()) ? "ui/image/mineWar/mine_lightbg.png" : "ui/image/mineWar/mine_redbg.png"));
		}
		else if (mineWarPointDetail.PlayerId == 0)
		{
			strAssetPath = "ui/image/mineWar/mine_lightbg.png";
		}
		else if (mineWarPointDetail.PlayerId != zbSelf.Instance.GetRoleId())
		{
			strAssetPath = ((layerNum < zbSelf.Instance.mineWarHandler.selfLayerNum - 1) ? "ui/image/mineWar/mine_redbg.png" : ((mineWarPointDetail.DestroyTm <= zbSelf.Instance.timeHandler.GetServerTime() && mineWarPointDetail.ProtectTm <= zbSelf.Instance.timeHandler.GetServerTime() && mineWarPointDetail.LockedTm <= zbSelf.Instance.timeHandler.GetServerTime()) ? "ui/image/mineWar/mine_lightbg.png" : "ui/image/mineWar/mine_redbg.png"));
		}
		Sprite val = zbAssetLoader.Instance.LoadCombatSpriteAsset(strAssetPath);
		if ((Object)(object)val != (Object)null)
		{
			m_spriteRenderer.sprite = val;
		}
	}

	public void Pick()
	{
		if (mineWarPointDetail.PlayerId == zbSelf.Instance.GetRoleId())
		{
			zbWsMsgHandler.Instance.MineWar_PointData_Req(mineWarPointDetail.PointId);
		}
		else if (zbSelf.Instance.mineWarHandler.currServerId == zbSelf.Instance.GetServerId())
		{
			zbWsMsgHandler.Instance.MineWar_PointData_Req(mineWarPointDetail.PointId);
		}
		else
		{
			zbWsMsgHandler.Instance.CrossMine_PointData_Req(mineWarPointDetail.PointId, zbSelf.Instance.mineWarHandler.currServerId);
		}
	}

	public void BlinkTimes()
	{
		spriteColorChange.LerpChangeColor();
	}

	private void Update()
	{
		if (mineWarHud != null)
		{
			mineWarHud.Update();
		}
	}
}
