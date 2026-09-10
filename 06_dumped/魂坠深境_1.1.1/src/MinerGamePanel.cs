using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using DG.Tweening;
using GameRandom;
using UnityEngine;

public class MinerGamePanel : BonusLocationPanel.GamePanel
{
	[SerializeField]
	private BonusLocationBonusCell cellPrefab;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private List<RectTransform> spawnPivots;

	[SerializeField]
	private RectTransform shootLine;

	[SerializeField]
	private RectTransform aim;

	[SerializeField]
	private float aimEndSize;

	[SerializeField]
	public float aimRotatePeriod;

	[SerializeField]
	private GameButton switchPanelButton;

	[NonSerialized]
	private CellUIPrefabPool<BonusLocationBonusCell, BonusLocationBonus> cellPool;

	[NonSerialized]
	private Vector2 initShootLineSizeDelta;

	[NonSerialized]
	private Sequence rotateSeq;

	[NonSerialized]
	private bool rotateStopped;

	protected override void Init()
	{
		base.Init();
		GetPrefabPool(ref cellPool, cellPrefab, base.transform, UIHelper.GetDefaultCommonInfoPanel());
		(parent as BonusLocationPanel).ConfigInfoPanel(cellPool);
		confirmButton.onClick.AddListener(_OnConfirmButtonClick);
		switchPanelButton.onClick.AddListener(_OnSwitchPanelButtonClick);
		initShootLineSizeDelta = shootLine.sizeDelta;
	}

	public override void Setup(BonusLocationEvent bonusEvent)
	{
		base.Setup(bonusEvent);
		rotateStopped = false;
		confirmButton.interactable = true;
		base.gameObject.SetActive(value: true);
		if (bonusEvent.gameSwitched || !UIBase.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableSwitchBonusLocationGame))
		{
			switchPanelButton.gameObject.SetActive(value: false);
		}
		SpawnBonus();
		UIBase.gameState.StartCoroutine(AimCor);
	}

	public override void Unset()
	{
		base.Unset();
		cellPool.DespawnAll();
		rotateStopped = false;
		shootLine.sizeDelta = initShootLineSizeDelta;
		aim.rotation = Quaternion.Euler(Vector3.zero);
		if (rotateSeq != null)
		{
			rotateSeq.Kill();
		}
		rotateSeq = null;
		base.gameObject.SetActive(value: false);
	}

	private IEnumerator AimCor()
	{
		bool variableFlag = UIBase.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableUpgradeBonusLocationGame);
		rotateSeq = DOTween.Sequence();
		aim.rotation = Quaternion.Euler(Vector3.zero);
		rotateSeq.Append(aim.DORotate(new Vector3(0f, 0f, 180f), aimRotatePeriod).SetEase(variableFlag ? Ease.Linear : Ease.OutQuart));
		rotateSeq.Append(aim.DORotate(new Vector3(0f, 0f, 0f), aimRotatePeriod).SetEase(variableFlag ? Ease.Linear : Ease.OutQuart));
		rotateSeq.SetLoops(-1);
		rotateSeq.Play();
		while (!rotateStopped)
		{
			yield return new WaitForEndOfFrame();
		}
		rotateSeq.Kill();
		rotateSeq = null;
		yield return new WaitForSeconds(0.3f);
		DOTweenModuleUI.DOSizeDelta(shootLine, new Vector2(aimEndSize, shootLine.GetHeight()), 0.5f, false);
		yield return new WaitForSeconds(0.5f);
		List<BonusLocationBonusCell> getBonus = new List<BonusLocationBonusCell>();
		foreach (BonusLocationBonusCell activeElement in cellPool.GetActiveElements())
		{
			if (IsOverlap(activeElement.GetComponent<RectTransform>(), shootLine))
			{
				activeElement.NotifyGet();
				getBonus.Add(activeElement);
			}
		}
		yield return new WaitForSeconds(1.5f);
		UIBase.gameState.UIManager.bonusLocationPanel.FinishBonusLocation(new List<BonusLocationBonus>(getBonus.ConvertAll((BonusLocationBonusCell b) => b.bonus)));
	}

	private void SpawnBonus()
	{
		List<BonusLocationBonus> list = bonusEvent.GenerateRandomBonus();
		if (cellPool == null)
		{
			Debug.LogError("MinerGamePanel spawn failed: cellPool is null.", this);
			return;
		}
		if (list == null || list.Count == 0)
		{
			Debug.LogError("MinerGamePanel spawn failed: no bonuses generated.", this);
			return;
		}
		List<BonusLocationBonus> list2 = list.FindAll((BonusLocationBonus bonus) => bonus?.bonus != null);
		if (list2.Count != list.Count)
		{
			Debug.LogError(string.Format("{0} ignored {1} invalid bonuses.", "MinerGamePanel", list.Count - list2.Count), this);
		}
		List<RectTransform> list3 = spawnPivots?.Where((RectTransform pivot) => pivot != null).ToList();
		if (list3 == null || list3.Count == 0)
		{
			Debug.LogError("MinerGamePanel spawn failed: no valid spawn pivots.", this);
			return;
		}
		foreach (BonusLocationBonus item in list2)
		{
			if (list3.Count == 0)
			{
				Debug.LogError("MinerGamePanel has fewer spawn pivots than valid bonuses.", this);
				break;
			}
			RectTransform rectTransform = list3.SelectRandom(UIBase.gameState.randomManager.NextDouble);
			BonusLocationBonusCell bonusLocationBonusCell = cellPool.Spawn(item, rectTransform);
			if (bonusLocationBonusCell == null)
			{
				Debug.LogError("MinerGamePanel failed to spawn bonus cell.", this);
				list3.Remove(rectTransform);
			}
			else
			{
				bonusLocationBonusCell.transform.position = rectTransform.position;
				list3.Remove(rectTransform);
			}
		}
	}

	private bool IsOverlap(RectTransform ta, RectTransform tb)
	{
		return IsPolygonsIntersecting(ta.GetWorldCorners().ToList().ConvertAll((Converter<Vector3, Vector2>)((Vector3 t) => t))
			.ToArray(), tb.GetWorldCorners().ToList().ConvertAll((Converter<Vector3, Vector2>)((Vector3 t) => t))
			.ToArray());
	}

	private bool IsPolygonsIntersecting(Vector2[] a, Vector2[] b)
	{
		foreach (Vector2[] item in new List<Vector2[]> { a, b })
		{
			for (int i = 0; i < item.Length; i++)
			{
				int num = (i + 1) % item.Length;
				Vector2 vector = item[i];
				Vector2 vector2 = item[num];
				Vector2 vector3 = new Vector2(vector2.y - vector.y, vector.x - vector2.x);
				double? num2 = null;
				double? num3 = null;
				Vector2[] array = a;
				for (int j = 0; j < array.Length; j++)
				{
					Vector2 vector4 = array[j];
					float num4 = vector3.x * vector4.x + vector3.y * vector4.y;
					if (!num2.HasValue || (double)num4 < num2)
					{
						num2 = num4;
					}
					if (!num3.HasValue || (double)num4 > num3)
					{
						num3 = num4;
					}
				}
				double? num5 = null;
				double? num6 = null;
				array = b;
				for (int j = 0; j < array.Length; j++)
				{
					Vector2 vector5 = array[j];
					float num7 = vector3.x * vector5.x + vector3.y * vector5.y;
					if (!num5.HasValue || (double)num7 < num5)
					{
						num5 = num7;
					}
					if (!num6.HasValue || (double)num7 > num6)
					{
						num6 = num7;
					}
				}
				if (num3 < num5 || num6 < num2)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void _OnConfirmButtonClick()
	{
		rotateStopped = true;
		confirmButton.interactable = false;
	}

	private void _OnSwitchPanelButtonClick()
	{
		bonusEvent.SwitchToRandomBonusPanel();
	}
}
