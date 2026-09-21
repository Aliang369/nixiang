using PathCreation;
using UnityEngine;

public class BezierLineHelper
{
	public enum BeizerControlDirType
	{
		Fixed,
		VerticalToLine
	}

	public enum BeizerControlLengthType
	{
		Fixed,
		ScaledByDistance
	}

	public enum BeizerType
	{
		TwoPoint
	}

	public class BeizerCreatorData
	{
		public Vector3[] anchors;

		public Camera camera;

		public BeizerControlDirType controlDirType;

		public bool controlInvertByScreen;

		public BeizerControlLengthType controlLengthType;

		public float controlScale;

		public Vector3 endControl;

		public Vector3 startControl;

		public BeizerType type;
	}

	public static BezierPath GetBezier(BeizerCreatorData data)
	{
		if (data.type == BeizerType.TwoPoint)
		{
			return GetTwoPointBeizer(data);
		}
		return null;
	}

	private static BezierPath GetTwoPointBeizer(BeizerCreatorData data)
	{
		if (data.anchors == null || data.anchors.Length != 2)
		{
			return null;
		}
		Vector3[] array = new Vector3[4]
		{
			data.anchors[0],
			default(Vector3),
			default(Vector3),
			data.anchors[1]
		};
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		switch (data.controlDirType)
		{
		case BeizerControlDirType.Fixed:
			vector = data.startControl.normalized;
			vector2 = data.endControl.normalized;
			break;
		case BeizerControlDirType.VerticalToLine:
			vector.x = 0f - (array[3] - array[0]).y;
			vector.y = (array[3] - array[0]).x;
			vector = vector.normalized;
			vector2 = vector;
			break;
		}
		if (data.controlInvertByScreen)
		{
			bool flag = isMidPointInLeftScreen(data);
			vector.x = (flag ? (0f - vector.x) : vector.x);
			vector2.x = (flag ? (0f - vector2.x) : vector2.x);
		}
		switch (data.controlLengthType)
		{
		case BeizerControlLengthType.Fixed:
			vector *= data.startControl.magnitude * data.controlScale;
			vector2 *= data.endControl.magnitude * data.controlScale;
			break;
		case BeizerControlLengthType.ScaledByDistance:
		{
			float num = Vector3.Distance(data.anchors[0], data.anchors[1]);
			vector *= data.startControl.magnitude * num * data.controlScale;
			vector2 *= data.endControl.magnitude * num * data.controlScale;
			break;
		}
		}
		array[1] = array[0] + vector;
		array[2] = array[3] + vector2;
		BezierPath bezierPath = new BezierPath(data.anchors);
		for (int i = 0; i < array.Length; i++)
		{
			bezierPath.SetPoint(i, array[i]);
		}
		return bezierPath;
	}

	private static bool isMidPointInLeftScreen(BeizerCreatorData data)
	{
		return ((data.anchors[0] + data.anchors[1]) / 2f).x - (float)(Screen.width / 2) < 0f;
	}
}
