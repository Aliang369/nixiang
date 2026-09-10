using System;
using PathCreation;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class BezierLineDisplay : MonoBehaviour
{
	[SerializeField]
	private EffectLineRenderer uiLineRenderer;

	[SerializeField]
	private float endPointOffset;

	[SerializeField]
	private float endPointLineOffset;

	[SerializeField]
	private GameObject Arrow;

	[Header("BeizerCreatorData")]
	[SerializeField]
	private BezierLineHelper.BeizerType type;

	[SerializeField]
	private BezierLineHelper.BeizerControlDirType controlType;

	[SerializeField]
	private Vector3 startPointControlVector;

	[SerializeField]
	private Vector3 endPointControlVector;

	[SerializeField]
	private float controlScale;

	[SerializeField]
	private bool controlInvertByScreen;

	[Header("VertextLineConfig")]
	[SerializeField]
	private float vertexSpacing;

	[NonSerialized]
	private PathCreation.BezierPath bezierPath;

	[NonSerialized]
	private Vector2 m_endPoint;

	[NonSerialized]
	private Vector2 m_startPoint;

	[NonSerialized]
	private VertexPath vertexPath;

	private void OnDrawGizmos()
	{
		if (bezierPath != null)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawLine(bezierPath.points[0], bezierPath.points[3]);
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(bezierPath.points[0], bezierPath.points[1]);
			Gizmos.DrawLine(bezierPath.points[2], bezierPath.points[3]);
		}
	}

	public void Setup(Vector3 startPoint, Vector3 endPoint)
	{
		if (vertexPath == null)
		{
			vertexPath = new VertexPath(base.transform.root);
		}
		base.transform.localScale = base.transform.localScale / base.transform.lossyScale.x;
		base.transform.position = Vector3.zero;
		m_startPoint = startPoint;
		m_endPoint = endPoint;
		UpdateContent();
	}

	public void UpdateContent()
	{
		ComputeRenderPath();
	}

	private void ComputeRenderPath()
	{
		bezierPath = BezierLineHelper.GetBezier(new BezierLineHelper.BeizerCreatorData
		{
			type = type,
			anchors = new Vector3[2] { m_startPoint, m_endPoint },
			controlDirType = controlType,
			startControl = startPointControlVector,
			endControl = endPointControlVector,
			controlScale = controlScale,
			controlInvertByScreen = controlInvertByScreen,
			camera = Camera.main
		});
		vertexPath.ComputeBySection(bezierPath, vertexSpacing);
		float dst = vertexPath.length - endPointOffset;
		Arrow.transform.position = vertexPath.GetPointAtDistance(dst);
		Vector3 eulerAngles = vertexPath.GetRotationAtDistance(dst).eulerAngles;
		eulerAngles.z = ((eulerAngles.y < 180f) ? (270f - eulerAngles.x) : (eulerAngles.x + 90f));
		eulerAngles.x = 0f;
		eulerAngles.y = 0f;
		Quaternion rotation = Quaternion.Euler(eulerAngles);
		Arrow.transform.rotation = rotation;
		uiLineRenderer.Points = new Vector2[Mathf.Min(vertexPath.NumPoints, 100)];
		float num = 1f - endPointLineOffset / vertexPath.length;
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < uiLineRenderer.Points.Length; i++)
		{
			if (vertexPath.times[i] <= num)
			{
				vector = vertexPath.GetPoint(i);
			}
			uiLineRenderer.Points[i] = vector;
		}
	}
}
