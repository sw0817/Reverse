//-------------------
// Copyright 2019
// Reachable Games, LLC
//-------------------

using UnityEngine;
using System.Collections;

namespace ReachableGames
{
	namespace PostLinerFree
	{
		// Simply add this component to an object and it gets an outline.  
		// Disable or remove it and the outline goes away.
		public class PostLinerOutline : MonoBehaviour
		{
			private Coroutine _inProcess = null;
			private bool      _isQuitting = false;  // avoids an error when disabling on quit, coroutine will never run

			private void OnEnable()
			{
				if (_inProcess!=null)
				{
					StopCoroutine(_inProcess);
				}
				if (PostLinerRenderer.Instance!=null)
				{
					PostLinerRenderer.Instance.AddToOutlines(transform);  // do it immediately if we can
				}
				else
				{
					_inProcess = StartCoroutine(Add());
				}
			}

			private void OnDisable()
			{
				if (_inProcess!=null)
					StopCoroutine(_inProcess);
				if (PostLinerRenderer.Instance!=null)
				{
					PostLinerRenderer.Instance.RemoveFromOutlines(transform);  // do it immediately if we can
				}
				else
				{
					if (!_isQuitting)
						_inProcess = StartCoroutine(Remove());
				}
			}
			private void OnApplicationQuit()
			{
				_isQuitting = true;
			}

			private IEnumerator Add()
			{
				yield return new WaitUntil(() => PostLinerRenderer.Instance!=null);
				PostLinerRenderer.Instance.AddToOutlines(transform);
			}

			private IEnumerator Remove()
			{
				yield return new WaitUntil(() => PostLinerRenderer.Instance!=null);
				PostLinerRenderer.Instance.RemoveFromOutlines(transform);
			}
		}
	}
}