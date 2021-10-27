//-------------------
// Copyright 2019
// Reachable Games, LLC
//-------------------

using UnityEngine;

namespace ReachableGames
{
	namespace PostLinerFree
	{
		public class FlyCamera : MonoBehaviour
		{
			public float Speed = 1.0f;
			public float MouseSensitivity = 1.0f;
			public bool InvertMouse = false;

			// Start off in fly mode
			private void Start()
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}

			void Update()
			{
				if (Cursor.visible)
				{
					if (Input.GetKeyDown(KeyCode.BackQuote))
					{
						Cursor.visible = false;
						Cursor.lockState = CursorLockMode.Locked;
					}
				}
				else
				{
					Vector3 movement = Vector3.zero;
					if (Input.GetKey(KeyCode.W))
						movement.z += 1.0f;
					if (Input.GetKey(KeyCode.S))
						movement.z += -1.0f;
					if (Input.GetKey(KeyCode.A))
						movement.x += -1.0f;
					if (Input.GetKey(KeyCode.D))
						movement.x += 1.0f;
					transform.localPosition += transform.TransformVector(movement * Speed * Time.deltaTime);

					float pitch = Input.GetAxis("Mouse Y") * MouseSensitivity * (InvertMouse ? -1.0f : 1.0f);
					float yaw = Input.GetAxis("Mouse X") * MouseSensitivity;
					Vector3 angles = new Vector3(Mathf.LerpAngle(transform.localEulerAngles.x, transform.localEulerAngles.x+pitch, 1.0f), Mathf.LerpAngle(transform.localEulerAngles.y, transform.localEulerAngles.y+yaw, 1.0f), 0.0f);
					transform.localRotation = Quaternion.Euler(angles);

					if (Input.GetKeyDown(KeyCode.BackQuote))
					{
						Cursor.visible = true;
						Cursor.lockState = CursorLockMode.None;
					}
				}
			}
		}
	}
}