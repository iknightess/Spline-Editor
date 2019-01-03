using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineFollow : MonoBehaviour
{
	public SplineCreator SplineCreatorRef;

	Spline SplineToFollow;

	float elapsedtime = 0;
	public float timeToTraverse = 10f;

	void Start()
	{
		SplineToFollow = SplineCreatorRef.Spline;
	}

	void Update()
	{
		FollowSpline();
	}

	void FollowSpline()
	{
		elapsedtime += Time.deltaTime;

		if (!SplineToFollow.isLooping && elapsedtime >= timeToTraverse) return;

		transform.position = SplineToFollow.GetPositionForTime(elapsedtime / timeToTraverse, SplineCreatorRef.transform);
		transform.rotation = Quaternion.LookRotation(SplineToFollow.GetDirectionForTime(elapsedtime / timeToTraverse, SplineCreatorRef.transform));
	}


}
