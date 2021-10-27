//-------------------
// Copyright 2019
// Reachable Games, LLC
//-------------------

using UnityEngine;

namespace ReachableGames
{
	namespace PostLinerFree
	{
		// Shows a message if an asset package is missing.
		public class IfMissingPackageShow : MonoBehaviour
		{
			public Object     _checkAsset = null;
			public GameObject _showIfMissing = null;
			void OnEnable()
			{
				if (_checkAsset==null)
				{
					_showIfMissing.SetActive(true);
				}
			}
		}
	}
}