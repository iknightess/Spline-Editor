using UnityEngine;

/// <summary>
/// Describes the three point types
/// </summary>
public enum PointType
{
	Anchor, 
	LeftControl,
	RightControl
}

/// <summary>
/// Represents and anchor point with two control points
/// </summary>
[System.Serializable]
public class CurvePoint {

	[SerializeField,HideInInspector] Vector3[] Points;

	/// <summary>
	/// Lets you get one of the three points by type rather than by index
	/// </summary>
	/// <param name="pointType"> The type of the point to get </param>
	/// <returns></returns>
	public Vector3 this[PointType pointType]
	{
		get
		{
			return Points[(int)pointType];
		}

		set
		{
			Points[(int)pointType] = value;
		}
	}

	public CurvePoint(Vector3 anchorPt, Vector3 controlPtLeft, Vector3 controlPtRight)
	{
		Points = new Vector3[] { anchorPt, controlPtLeft, controlPtRight };
	}

	/// <summary>
	/// Gets wherever the control point or anchor is pointing. If a transform is passed, it converts the direction
	/// to that transform's local space.
	/// </summary>
	/// <param name="type"> Anchor, left control or right control point? </param>
	/// <returns></returns>
	public Vector3 GetForwardDirection(PointType type, Transform parentTransform=null)
	{
		Vector3 worldDirection = (this[type] - this[PointType.LeftControl]).normalized;
		Vector3 transformedDirection =(parentTransform.TransformPoint(this[type]) - parentTransform.TransformPoint(this[PointType.LeftControl])).normalized;

		return (parentTransform == null) ? worldDirection : transformedDirection;
	}
}
