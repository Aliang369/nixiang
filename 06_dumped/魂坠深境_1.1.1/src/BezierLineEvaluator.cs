using System.Collections.Generic;
using PathCreation;
using PathCreation.Utility;
using UnityEngine;

public class BezierLineEvaluator
{
	public class SegmentInfo
	{
		public int index;

		public float length;

		public Vector3[] points;
	}

	public BezierPath bezierPath;

	public List<SegmentInfo> segmentInfos;

	public float length { get; }

	public BezierLineEvaluator(BezierPath bezierPath)
	{
		this.bezierPath = bezierPath;
		segmentInfos = new List<SegmentInfo>();
		length = 0f;
		for (int i = 0; i < bezierPath.NumSegments; i++)
		{
			segmentInfos.Add(new SegmentInfo());
			segmentInfos[i].index = i;
			segmentInfos[i].points = bezierPath.GetPointsInSegment(i);
			segmentInfos[i].length = bezierPath.GetSegmentLength(i);
			length += segmentInfos[i].length;
		}
	}

	public SegmentInfo GetSegment(float t, out float remainT)
	{
		t = Mathf.Clamp(t, 0f, 1f);
		remainT = t;
		foreach (SegmentInfo segmentInfo in segmentInfos)
		{
			if (remainT <= segmentInfo.length / length)
			{
				return segmentInfo;
			}
			remainT -= segmentInfo.length / length;
		}
		return null;
	}

	public Vector3 GetPoint(float t)
	{
		SegmentInfo segment = GetSegment(t, out var remainT);
		if (segment == null)
		{
			return Vector3.zero;
		}
		return CubicBezierUtility.EvaluateCurve(segment.points, remainT);
	}

	public Vector3 GetNormal(float t)
	{
		SegmentInfo segment = GetSegment(t, out var remainT);
		if (segment == null)
		{
			return Vector3.zero;
		}
		return CubicBezierUtility.Normal(segment.points, remainT);
	}

	public Vector3 GetTangent(float t)
	{
		SegmentInfo segment = GetSegment(t, out var remainT);
		if (segment == null)
		{
			return Vector3.zero;
		}
		return CubicBezierUtility.EvaluateCurveDerivative(segment.points, remainT);
	}
}
