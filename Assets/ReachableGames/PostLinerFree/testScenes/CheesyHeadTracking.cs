//-------------------
// Copyright 2019
// Reachable Games, LLC
//-------------------

using UnityEngine;

namespace ReachableGames
{
	namespace PostLinerFree
	{
		// Cheesy head tracking.
		public class CheesyHeadTracking : MonoBehaviour
		{
			public Quaternion startRot;
			private float trackRate = 0.1f;
			private float trackDelay = 2.0f;
			private float nextTrackStart = 0.0f;
			private void Start()
			{
				startRot = transform.rotation;
				trackRate *= Random.value + 0.25f;  // give some variation
				nextTrackStart = Time.time + Random.value * trackDelay;
			}

			void Update()
			{
				if (Time.time > nextTrackStart)
				{
					Quaternion q = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);
					Quaternion aimRot = q * startRot;
					if (Quaternion.Angle(aimRot , transform.rotation) > 0.01f)
					{
						transform.rotation = Quaternion.Lerp(transform.rotation, aimRot, trackRate);
					}
					else
					{
						nextTrackStart = Time.time + Random.value * trackDelay;
					}
				}
			}
		}
	}
}