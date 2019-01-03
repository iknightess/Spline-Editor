using UnityEngine;

/// <summary>
/// This class will be referenced by the editor script to create and edit splines.
/// </summary>
public class SplineCreator : MonoBehaviour
{

	[HideInInspector] public Spline Spline;

	public void CreateSpline()
	{
		Spline = new Spline(transform.position, transform);
	}
}
