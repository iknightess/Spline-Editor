using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class contains the list of points that make up the spline. Lets you create, get and move those points.
/// </summary>
[System.Serializable]
public class Spline
{

	[SerializeField] List<CurvePoint> Points;

	[HideInInspector] public bool isLooping = false;

	private float InitialDistanceToControlPts = 0.3f;

	public int TotalSegments
	{
		get
		{
			if (Points == null) {
				return 0; 
			}
			else
			{
				return (isLooping)? Points.Count : Points.Count - 1;
			}
		}
	}

	public int TotalPoints
	{
		get
		{
			return (Points == null) ? 0 : Points.Count;
		}
	}


	/// <summary>
	/// Initializes the point list with two points in a horizontal line.
	/// </summary>
	/// <param name="anchorPtPosition"> The position to create the first point in. </param>
	public Spline(Vector3 anchorPtPosition, Transform t=null)
	{
		Points = new List<CurvePoint>();
		AddPoint(Vector3.right * InitialDistanceToControlPts * 2f);
		AddPoint(Vector3.left * InitialDistanceToControlPts * 2f);
	}

	/// <summary>
	/// Gets the position of an anchor or control point at a certain index. If a transform is passed, it converts
	/// the position into the local space of that transform
	/// </summary>
	/// <param name="index"> Index of the point to fetch </param>
	/// <param name="type"> Wether it is an anchor, left control point or right control point </param>
	/// <returns></returns>
	public Vector3 GetPointAt(int index, PointType type, Transform parentTransform = null)
	{
		if (Points == null)
		{
			return Vector3.zero;
		}
		return (parentTransform == null) ? Points[index][type] : parentTransform.TransformPoint(Points[index][type]);
	}

	public CurvePoint GetPointsAt(int index)
	{
		return Points[index];
	}

	/// <summary>
	/// Add points matching previous anchor point's right handle direction. If we're adding the first point, add control
	/// points in a vertial line.
	/// </summary>
	/// <param name="anchorPtPosition">Position where the anchor point will be added</param>
	public void AddPoint(Vector3 anchorPtPosition)
	{
		if (Points.Count == 0)
		{
			Points.Add(new CurvePoint(
				anchorPtPosition,
				anchorPtPosition + Vector3.right * InitialDistanceToControlPts,
				anchorPtPosition + Vector3.left * InitialDistanceToControlPts
				));
		}

		else
		{
			Vector3 prevRightControlPtDir = ((anchorPtPosition - Points[TotalPoints - 1][PointType.RightControl]) / 2).normalized;

			Points.Add(new CurvePoint(
				anchorPtPosition,
				anchorPtPosition - prevRightControlPtDir * InitialDistanceToControlPts,
				anchorPtPosition + prevRightControlPtDir * InitialDistanceToControlPts
				));
		}
	}

	/// <summary>
	/// Removes anchor point at certain index from list with its control points
	/// </summary>
	/// <param name="pointindex">Index of point to remove.</param>
	public void RemovePoint(int pointindex)
	{
		Points.RemoveAt(pointindex);
	}

	/// <summary>
	/// Add a point to the end of the spline based on the previous point's right handle position.
	/// </summary>
	public void AddPointToEnd()
	{
		Vector3 prevRightControlPtDir = ((Points[TotalPoints - 1][PointType.Anchor] - Points[TotalPoints - 1][PointType.RightControl]) / 2).normalized;
		Vector3 newPosition = Points[TotalPoints - 1][PointType.RightControl] - prevRightControlPtDir;
		AddPoint(newPosition);
	}

	/// <summary>
	/// Inserts a point after the active point. Grabs the right control point of the previous point and the left one
	/// from the next point and puts the new anchor in the middle, with the control points pointing at these.
	/// </summary>
	/// <param name="index"> Index where to insert the new point after</param>
	public void InsertPoint(int index)
	{
		if (index == TotalPoints - 1) AddPointToEnd();

		Vector3 targetDirection = ((Points[index + 1][PointType.LeftControl] - Points[index][PointType.RightControl]) / 2f).normalized;

		Vector3 newPosition = Points[index][PointType.RightControl] + targetDirection;

		Points.Insert(index + 1, new CurvePoint(
			newPosition,
			newPosition - targetDirection * InitialDistanceToControlPts,
			newPosition + targetDirection * InitialDistanceToControlPts
			));
	}

	/// <summary>
	/// Change position of a certain point.When moving points, it makes sure that control points move with the 
	/// anchor point and that they always form a tangent
	/// - Anchor Point: Move control points with it
	/// - Control Points: Move the other control point in the same direction as the one you're moving while 
	/// keeping the distance.
	/// </summary>
	/// <param name="pointIndex"> Index of the point to move. </param>
	/// <param name="pointType">Anchor, Control Left or Control Right. </param>
	/// <param name="newPositiion"> Target Position of the point. </param>
	public void SetPointPosition(int pointIndex, PointType pointType, Vector3 newPositiion)
	{
		switch (pointType)
		{
			case PointType.Anchor:
				Vector3 distanceToMove = newPositiion - Points[pointIndex][PointType.Anchor];

				Points[pointIndex][PointType.LeftControl] += distanceToMove;
				Points[pointIndex][PointType.RightControl] += distanceToMove;
				break;
			case PointType.LeftControl:
			case PointType.RightControl:
				float distControlToAnchor = (Points[pointIndex][pointType] - Points[pointIndex][PointType.Anchor]).magnitude;
				Vector3 dirControlToAnchor = (Points[pointIndex][PointType.Anchor] - newPositiion).normalized;
				PointType otherControlEnum = (pointType == PointType.RightControl) ? PointType.LeftControl : PointType.RightControl;
				Points[pointIndex][otherControlEnum] = Points[pointIndex][PointType.Anchor] + dirControlToAnchor * distControlToAnchor;
				break;
		}

		Points[pointIndex][pointType] = newPositiion;
	}


	/// <summary>
	/// Calculate simple curve with only one handle for the first point
	/// </summary>
	/// <param name="firstAnchor">First anchor of segment</param>
	/// <param name="rightControl">Right Control Point of First Anchor</param>
	/// <param name="secondAnchor">First anchor of segment</param>
	/// <param name="t"></param>
	/// <returns></returns>
	Vector3 GetQuadraticCurve(Vector3 firstAnchor, Vector3 rightControl, Vector3 secondAnchor, float t)
	{
		Vector3 p0 = Vector3.Lerp(firstAnchor, rightControl, t);
		Vector3 p1 = Vector3.Lerp(rightControl, secondAnchor, t);
		return Vector3.Lerp(p0, p1, t);
	}

	/// <summary>
	/// Calculate cubic curve with 4 points per segment based on the quadratic curve
	/// </summary>
	/// <param name="firstAnchor">First anchor of segment</param>
	/// <param name="rightControl">Right Control Point of First Anchor</param>
	/// <param name="leftControl">Left Control Point of Second Anchor</param>
	/// <param name="secondAnchor">Second anchor of segment</param>
	/// <param name="t"></param>
	/// <returns></returns>
	Vector3 GetCubicCurve(Vector3 firstAnchor, Vector3 rightControl, Vector3 leftControl, Vector3 secondAnchor, float t)
	{
		Vector3 p0 = GetQuadraticCurve(firstAnchor, rightControl, leftControl, t);
		Vector3 p1 = GetQuadraticCurve(rightControl, leftControl, secondAnchor, t);
		return Vector3.Lerp(p0, p1, t);
	}

	/// <summary>
	/// Calculate the segment we're at and the position for the provided time. 
	/// </summary>
	/// <param name="time"> Point in time we want to get the position at [0-1]</param>
	/// <param name="parentTransform">Convert to this transform's local space</param>
	/// <returns></returns>
	public Vector3 GetPositionForTime(float time, Transform parentTransform)
	{
		float timeInSpline = time * TotalSegments;
		int segmentNumber = (int)timeInSpline;
		float timeInSegment = timeInSpline - segmentNumber;

		return parentTransform.TransformPoint(GetCubicCurve(Points[segmentNumber% TotalPoints][PointType.Anchor], Points[segmentNumber% TotalPoints][PointType.RightControl], Points[(segmentNumber + 1)% TotalPoints][PointType.LeftControl], Points[(segmentNumber + 1) % TotalPoints][PointType.Anchor], timeInSegment));
	}

	/// <summary>
	/// Get first derivative of the points of a segment to get tangent at a certain point in time
	/// </summary>
	/// <param name="firstAnchor">First anchor of segment</param>
	/// <param name="rightControl">Right Control Point of First Anchor</param>
	/// <param name="leftControl">Left Control Point of Second Anchor</param>
	/// <param name="secondAnchor">Second anchor of segment</param>
	/// <param name="t"></param>
	/// <returns></returns>
	public Vector3 GetFirstDerivative(Vector3 firstAnchor, Vector3 rightControl, Vector3 leftControl, Vector3 secondAnchor, float t)
	{
		return 3 * (1 - t) * (1 - t) * (rightControl - firstAnchor)
			+ 6 * (1 - t) * t * (leftControl - rightControl)
			+ 3 * t * t * (secondAnchor - leftControl);
	}

	/// <summary>
	/// Calculate segment we're at and get tangent for the provided time
	/// </summary>
	/// <param name="time">Time for which to get the velocity [0-1]</param>
	/// <param name="parentTransform">Convert to this transform's local space </param>
	/// <returns></returns>
	public Vector3 GetVelocityForTime(float time, Transform parentTransform)
	{
		float timeInSpline = time * TotalSegments;
		int segmentNumber = (int)timeInSpline;
		float timeInSegment = timeInSpline - segmentNumber;

		return parentTransform.TransformPoint(GetFirstDerivative(Points[segmentNumber%TotalPoints][PointType.Anchor], Points[segmentNumber % TotalPoints][PointType.RightControl], Points[(segmentNumber + 1) % TotalPoints][PointType.LeftControl], Points[(segmentNumber + 1) % TotalPoints][PointType.Anchor], timeInSegment)) -
			parentTransform.position;
	}

	/// <summary>
	/// Normalize the velocity vector to get a direction
	/// </summary>
	/// <param name="time">Time for which to get the direction</param>
	/// <param name="parentTransform"> Convert to this transform's local space </param>
	/// <returns></returns>
	public Vector3 GetDirectionForTime(float time, Transform parentTransform)
	{
		return GetVelocityForTime(time, parentTransform).normalized;
	}

}
