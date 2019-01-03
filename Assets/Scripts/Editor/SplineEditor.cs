using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor Script that lets us build and draw the spline with a button and add points via mouse click.
/// Updates points' positions when they are dragged around.
/// </summary>
[CustomEditor(typeof(SplineCreator))]
public class SplineEditor : Editor
{
	public float SplineThickness = 2.0f;

	SplineCreator SplineCreatorRef;
	Spline TargetSpline;

	int ActiveIndex = -1;
	PointType ActiveType = PointType.Anchor;

	private void OnEnable()
	{
		SplineCreatorRef = (SplineCreator)target;
		if (SplineCreatorRef.Spline == null)
		{
			SplineCreatorRef.CreateSpline();
		}
		TargetSpline = SplineCreatorRef.Spline;
	}

	private void OnSceneGUI()
	{
		DrawSpline();
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (GUILayout.Button("Create/Reset Spline"))
		{
			RecordUndo(SplineCreatorRef, "Create/Reset Spline");
			SplineCreatorRef.CreateSpline();
			TargetSpline = SplineCreatorRef.Spline;
		}

		if (GUILayout.Button("Add Point to End"))
		{
			RecordUndo(SplineCreatorRef, "Add Point to End.");
			TargetSpline.AddPointToEnd();
		}

		if (GUILayout.Button("Insert After Active Point"))
		{
			RecordUndo(SplineCreatorRef, "Insert point.");
			TargetSpline.InsertPoint(ActiveIndex);
		}

		if (GUILayout.Button("Remove Active Point"))
		{
			RecordUndo(SplineCreatorRef, "Remove point.");
			TargetSpline.RemovePoint(ActiveIndex);
		}


		TargetSpline.isLooping = EditorGUILayout.Toggle("Loop", TargetSpline.isLooping);
	}

	/// <summary>
	/// Draws the points of the spline (anchor and control points) as well as the bezier curves connecting them.
	/// Draws an extra segment if loop is enabled
	/// </summary>
	void DrawSpline()
	{
		for (int i = 0; i < TargetSpline.TotalPoints; i++)
		{
			DrawPoint(i, PointType.Anchor);
			DrawPoint(i, PointType.LeftControl);
			DrawPoint(i, PointType.RightControl);
			DrawControlPointLines(i);
		}

		for (int i = 0; i < TargetSpline.TotalSegments; i++)
		{
			DrawBezierCurve(i);
		}

		if (TargetSpline.isLooping)
		{
			DrawBezierCurve(TargetSpline.TotalPoints-1);
		}
	}

	/// <summary>
	/// Draw the lines from the anchor point to each one of the control points.
	/// </summary>
	/// <param name="pointIndex">Index of point to be drawn</param>
	void DrawControlPointLines(int pointIndex)
	{
		Handles.color = Color.white;
		Handles.DrawLine(TargetSpline.GetPointAt(pointIndex, PointType.Anchor, SplineCreatorRef.transform),
						TargetSpline.GetPointAt(pointIndex, PointType.RightControl, SplineCreatorRef.transform));
		Handles.DrawLine(TargetSpline.GetPointAt(pointIndex, PointType.Anchor, SplineCreatorRef.transform),
						TargetSpline.GetPointAt(pointIndex, PointType.LeftControl, SplineCreatorRef.transform));
	}

	/// <summary>
	/// Draw curve for the segment starting with startingPointIndex
	/// </summary>
	/// <param name="startingPointIndex"> Index of the first point of the segment we want to draw</param>
	void DrawBezierCurve(int startingPointIndex)
	{
		Handles.DrawBezier(TargetSpline.GetPointAt(startingPointIndex, PointType.Anchor, SplineCreatorRef.transform),
							TargetSpline.GetPointAt((startingPointIndex + 1) % TargetSpline.TotalPoints, PointType.Anchor, SplineCreatorRef.transform),
							TargetSpline.GetPointAt(startingPointIndex, PointType.RightControl, SplineCreatorRef.transform),
							TargetSpline.GetPointAt((startingPointIndex + 1)%TargetSpline.TotalPoints, PointType.LeftControl, SplineCreatorRef.transform),
							Color.green,
							null,
							SplineThickness);
	}

	/// <summary>
	/// Draw anchor and control points. When an anchor point is clicked, show a x,y,z handle to move it around
	/// (handle points at control pt if local space is enabled on the editor). Control points can be moved
	/// around by clicking and dragging them around.
	/// </summary>
	/// <param name="index"> Index of the point to be drawn</param>
	/// <param name="type"> Which point should be moved? Anchor, Control Left or Control Right? </param>
	void DrawPoint(int index, PointType type)
	{
		Vector3 pointPos = TargetSpline.GetPointAt(index, type, SplineCreatorRef.transform);
		Handles.color = type == PointType.Anchor ? Color.green : Color.red;

		if (Handles.Button(pointPos, Quaternion.identity, 0.1f, 0.06f, Handles.SphereHandleCap))
		{
			ActiveIndex = index;
			ActiveType = type;
		}


		Quaternion handleRotation = Quaternion.identity;

		if (Tools.pivotRotation == PivotRotation.Local && type == PointType.Anchor)
		{
			handleRotation=Quaternion.LookRotation(TargetSpline.GetPointsAt(index).GetForwardDirection(type, SplineCreatorRef.transform));
		}

		Vector3 newPosition = Vector3.zero;

		if (type == PointType.Anchor)
		{
			if (ActiveIndex != index || ActiveType != type) return;
			newPosition = Handles.DoPositionHandle(pointPos, handleRotation);
		}
		else
		{
			newPosition = Handles.FreeMoveHandle(pointPos, Quaternion.identity, .1f, Vector2.zero, Handles.SphereHandleCap);
		}

		if (pointPos != newPosition)
		{
			RecordUndo(SplineCreatorRef, "Move " + type.ToString() + " Point");
			TargetSpline.SetPointPosition(index, type, SplineCreatorRef.transform.InverseTransformPoint( newPosition));
		}
	}

	/// <summary>
	/// Record action in the undo list and acknowledge that changes have been made on the scene.
	/// </summary>
	/// <param name="objectToUndo">Object whose actions have to be undone</param>
	/// <param name="message"> Message to show in the undo list. </param>
	void RecordUndo(Object objectToUndo, string message)
	{
		Undo.RecordObject(objectToUndo, message);
		EditorUtility.SetDirty(objectToUndo);
	}

	int GetNearestPointindex(Vector3 position)
	{
		float minDistance = Mathf.Infinity;
		int index = -1;
		for (int i = 0; i < TargetSpline.TotalPoints; i++)
		{
			float newDistance = Vector3.Distance(TargetSpline.GetPointAt(i, PointType.Anchor), position);
			if (newDistance< minDistance)
			{
				minDistance = newDistance;
				index = i;
			}
		}
		return index;
	}

}

