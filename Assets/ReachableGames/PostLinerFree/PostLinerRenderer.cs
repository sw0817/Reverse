//-------------------
// Copyright 2019
// Reachable Games, LLC
//-------------------

using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ReachableGames
{
	namespace PostLinerFree
	{
		[ExecuteInEditMode]
		public class PostLinerRenderer : MonoBehaviour
		{
			[HideInInspector]
			public int _outlineLayer;

			private Camera             _hiddenCamera = null;
			private RenderTexture      _renderTexture = null;
			private static int         _globalTextureId = Shader.PropertyToID("_OutlineDepth");
			private HashSet<Transform> _outlineObjects = new HashSet<Transform>();  // track these
			private List<int>          _objectLayers = new List<int>();

#if UNITY_EDITOR
			[NonSerialized]
			private bool               _sceneViewHooked = false;
			private void Update()
			{
				if (!_sceneViewHooked)
				{
#if UNITY_2019_1_OR_NEWER
					SceneView.duringSceneGui += OnSceneViewUpdate;
#else
					SceneView.onSceneGUIDelegate += OnSceneViewUpdate;
#endif
					_sceneViewHooked = true;
				}
			}
#endif

			//-------------------
			// Access to the outliner object.
			static public PostLinerRenderer Instance = null;

			void Awake()
			{
				Instance = null;
			}

			void Start()
			{
				Instance = this;
			}

			void OnDestroy()
			{
				Instance = null;
#if UNITY_EDITOR
#if UNITY_2019_1_OR_NEWER
				SceneView.duringSceneGui -= OnSceneViewUpdate;
#else
				SceneView.onSceneGUIDelegate -= OnSceneViewUpdate;
#endif
				_sceneViewHooked = false;
#endif
			}

			//-------------------
			// Remove ALL data, in case this matters.
			public void ClearAllOutlines()
			{
				_recursiveList.Clear();
				_outlineObjects.Clear();
				_objectLayers.Clear();
			}

			// Call this to push an object hierarchy into the outliner effect
			public void AddToOutlines(Transform t)
			{
				DoRecursive(t, (Transform o) => { _outlineObjects.Add(o); });
			}

			public void RemoveFromOutlines(Transform t)
			{
				DoRecursive(t, (Transform o) => { _outlineObjects.Remove(o); });
			}

			//-------------------
			// Perform recursive action on game object
			delegate void DoAction(Transform g);
			private Queue<Transform> _recursiveList = new Queue<Transform>();
			private void DoRecursive(Transform root, DoAction action)  // trade deep stack for queue
			{
				_recursiveList.Clear();
				_recursiveList.Enqueue(root);

				while (_recursiveList.Count>0)
				{
					Transform t = _recursiveList.Dequeue();
					foreach (Transform child in t)
					{
						_recursiveList.Enqueue(child);
					}
					action(t);
				}
			}

			//-------------------

#if UNITY_EDITOR
			public void OnSceneViewUpdate(SceneView sv)
			{
				UpdateRenderTexture(sv.camera);
			}
#endif

			public void OnPreRender()
			{
				UpdateRenderTexture(Camera.current);
			}

			private void UpdateRenderTexture(Camera c)
			{
				// Parent camera must have depth, otherwise there's no way for us to do the hidden object effect
				if (c.depthTextureMode == DepthTextureMode.None)
					c.depthTextureMode = DepthTextureMode.Depth;

				// Every frame, walk over and re-capture the layers for each object (in case they change dynamically for some reason), and force them to the Outline layer temporarily
				_recursiveList.Clear();  // collect "null" transforms to remove from the HashSet
				_objectLayers.Clear();
				foreach (Transform t in _outlineObjects)
				{
					if (t==null)
						_recursiveList.Enqueue(t);  // take this out of the set
					else
					{
						_objectLayers.Add(t.gameObject.layer);
						t.gameObject.layer = _outlineLayer;
					}
				}
				while (_recursiveList.Count>0)
					_outlineObjects.Remove(_recursiveList.Dequeue());  // remove nulls

				// Make a hidden camera if we don't already have one
				if (_hiddenCamera==null)
				{
					GameObject hiddenCameraGO = new GameObject("OutlineCamera");
					hiddenCameraGO.hideFlags = HideFlags.HideAndDontSave;
					hiddenCameraGO.transform.SetParent(transform);
					_hiddenCamera = hiddenCameraGO.AddComponent<Camera>();
				}

				// Given the editor lets you see things from the SceneView, and we want to support outlines 
				// there where you can tumble the camera better, I have to make it possible to override the camera.
				_hiddenCamera.CopyFrom(c);

				// Handle resizing of cameras gracefully
				if (_renderTexture==null || _hiddenCamera.pixelWidth != _renderTexture.width || _hiddenCamera.pixelHeight!=_renderTexture.height)
				{
					if (_renderTexture!=null)
						_renderTexture.Release();
					// It is actually critical to NOT increase depth buffer precision beyond 16.  The DepthNormals texture has 16-bit normals and 16-bit depth, so the increased
					// precision here would actually cause comparison problems.
					_renderTexture = new RenderTexture(_hiddenCamera.pixelWidth, _hiddenCamera.pixelHeight, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.Default);
					_renderTexture.hideFlags = HideFlags.HideAndDontSave;
				}

				// Modify the settings we need to control
				_hiddenCamera.enabled = false;
				_hiddenCamera.depthTextureMode = DepthTextureMode.Depth;
				_hiddenCamera.clearFlags = CameraClearFlags.Depth;
				_hiddenCamera.targetTexture = _renderTexture;
				_hiddenCamera.forceIntoRenderTexture = true;
				_hiddenCamera.rect = new Rect(0, 0, 1, 1);  // always inherit parent camera's matrix

				// Just draw the outline objects into the depth buffer all at once.
				_hiddenCamera.cullingMask = 1<<_outlineLayer;
				_hiddenCamera.Render();

				Shader.SetGlobalTexture(_globalTextureId, _renderTexture);

				//-------------------
				// Reset everything to its original layer
				int i=0;
				foreach (Transform t in _outlineObjects)
				{
					t.gameObject.layer = _objectLayers[i++];
				}
			}
		}

#if UNITY_EDITOR
		// Basic custom inspector so we can use a nice layer picker
		[CustomEditor(typeof(PostLinerRenderer))]
		public class PostLinerRenderObjectsInspector : Editor
		{
			SerializedProperty _outlineLayer;

			private void OnEnable()
			{
				_outlineLayer = serializedObject.FindProperty("_outlineLayer");
			}

			public override void OnInspectorGUI()
			{
				base.DrawDefaultInspector();
				serializedObject.Update();

				// Select rendering layer for outline camera
				_outlineLayer.intValue = EditorGUILayout.LayerField(new GUIContent("Outline Layer", "Select a layer to use for rendering outlines.  Either make a layer just for outlines or reuse one that is only used for physics or non-renderables."), _outlineLayer.intValue);

				PostLinerRenderer plr = (PostLinerRenderer)target;
				if (plr!=null)
				{
					// Handle drag and drop for adding an outline
					Color bgc = GUI.backgroundColor;

					GUIStyle addBoxStyle = new GUIStyle(GUI.skin.box);
					addBoxStyle.alignment = TextAnchor.MiddleCenter;
					addBoxStyle.fontStyle = FontStyle.Italic;
					addBoxStyle.fontSize = 12;
					GUI.skin.box = addBoxStyle;
					if (EditorGUIUtility.isProSkin)
						addBoxStyle.normal.textColor = new Color(0.75f, 0.75f, 1.0f, 1.0f);
					else GUI.backgroundColor = new Color(0.75f, 0.75f, 1.0f, 1.0f);

					Event mouseEvent = Event.current;
					Rect addRect = GUILayoutUtility.GetRect(0,20,GUILayout.ExpandWidth(true));
					GUI.Box(addRect,"Drag here to add outline",addBoxStyle);
					if (addRect.Contains(Event.current.mousePosition))
					{
						if (mouseEvent.type == EventType.DragUpdated)
						{
							DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
						}
						else if (mouseEvent.type == EventType.DragPerform)
						{
							foreach (UnityEngine.Object o in DragAndDrop.objectReferences)
							{
								if (o is GameObject)
								{
									plr.AddToOutlines(((GameObject)o).transform);
								}
							}
							mouseEvent.Use ();
							UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
						}
					}

					GUIStyle remBoxStyle = new GUIStyle(addBoxStyle);
					GUI.skin.box = remBoxStyle;
					if (EditorGUIUtility.isProSkin)
						remBoxStyle.normal.textColor = new Color(1.0f, 0.75f, 0.75f, 1.0f);
					else GUI.backgroundColor = new Color(1.0f, 0.75f, 0.75f, 1.0f);

					Rect remRect = GUILayoutUtility.GetRect(0,20,GUILayout.ExpandWidth(true));
					GUI.Box(remRect,"Drag here to remove outline",remBoxStyle);
					if (remRect.Contains(Event.current.mousePosition))
					{
						if (mouseEvent.type == EventType.DragUpdated)
						{
							DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
						}
						else if (mouseEvent.type == EventType.DragPerform)
						{
							foreach (UnityEngine.Object o in DragAndDrop.objectReferences)
							{
								if (o is GameObject)
								{
									plr.RemoveFromOutlines(((GameObject)o).transform);
								}
							}
							mouseEvent.Use ();
							UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
						}
					}
					GUI.backgroundColor = bgc;  // reset background color
				}
				serializedObject.ApplyModifiedProperties();
			}
		}
#endif
	}
}