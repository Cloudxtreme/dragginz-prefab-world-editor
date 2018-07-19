﻿//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

//using System;
//using System.Collections;
using System.Collections.Generic;

using UnityEngine;
//using UnityEngine.EventSystems;

using AssetsShared;

//using Gamekit3D.WorldBuilding;

namespace PrefabWorldEditor
{
	public class LevelController : Singleton<LevelController>
    {
		#region PublicStructs

		public struct LevelElement
		{
			public GameObject go;
			public Globals.PartList part;
            public int overwriteGravity;
            public float shaderSnow;
            public float lightIntensity;

            public bool gravity() {
                return (overwriteGravity == 0 ? PrefabLevelEditor.Instance.parts[part].usesGravity : (overwriteGravity == 1 ? true : false));
            }
        };

		public struct ElementGroup
		{
			public string groupType;
			public PrefabLevelEditor.Part part;
			public List<GameObject> gameObjects;
			//
			public PlacementTool.PlacementMode placement;
			public DungeonTool.DungeonPreset dungeon;
			public RoomTool.RoomPattern room;
			//
			public int width;
			public int height;
			public int depth;
			public bool ceiling;
			//
			public int radius;
			public int interval;
			public int density;
			public bool inverse;
		};

		#endregion

		// ------------------------------------------------------------------------

		#region PrivateAttributes

		//private Transform _container;

		private Dictionary<string, LevelElement> _levelElements;

		private List<ElementGroup> _aElementGroups;
		private int _iSelectedGroupIndex;

		private LevelElement _selectedElement;
		private Bounds _selectedElementBounds;
		private List<MeshRenderer> _selectedMeshRenderers;
        private List<Material> _selectedMaterials;

        private List<GameObject> _listOfChildren;

		#endregion

		// ------------------------------------------------------------------------

		#region Getters

		public Dictionary<string, LevelElement> levelElements {
			get { return _levelElements; }
		}

		public List<ElementGroup> aElementGroups {
			get { return _aElementGroups; }
		}

		public int iSelectedGroupIndex {
			get { return _iSelectedGroupIndex; }
			set { _iSelectedGroupIndex = value; }
		}

		public LevelElement selectedElement {
			get { return _selectedElement; }
			set { _selectedElement = value; }
		}

		public Bounds selectedElementBounds {
			get { return _selectedElementBounds; }
		}

		#endregion

		#region PublicMethods

		// ------------------------------------------------------------------------
		public void init(Transform container)
		{
			//_container = container;

			_levelElements = new Dictionary<string, LevelElement> ();

			_aElementGroups = new List<ElementGroup> ();
			_iSelectedGroupIndex = -1;

			_selectedMeshRenderers = new List<MeshRenderer> ();

            _selectedMaterials = new List<Material>();

            _listOfChildren = new List<GameObject> ();
		}

		// ------------------------------------------------------------------------
		public void clearLevel()
		{
			foreach (KeyValuePair<string, LevelElement> element in _levelElements) {
				GameObject.Destroy (element.Value.go);
			}

			_levelElements.Clear ();
			_aElementGroups.Clear ();
		}

		// ------------------------------------------------------------------------
		public void setMeshCollider (GameObject go, bool state) {

			_listOfChildren.Clear ();
			getChildrenRecursive (go);

			int i, len = _listOfChildren.Count;
			for (i = 0; i < len; ++i) {
				if (_listOfChildren [i].GetComponent<Collider> ()) {
					_listOfChildren [i].GetComponent<Collider> ().enabled = state;
				}
			}
		}		

		// ------------------------------------------------------------------------
		public void setMeshColliders (bool state)
		{
			_listOfChildren.Clear ();

			foreach (KeyValuePair<string, LevelController.LevelElement> element in _levelElements)
			{
				getChildrenRecursive (element.Value.go);
			}

			int i, len = _listOfChildren.Count;
			for (i = 0; i < len; ++i) {
				if (_listOfChildren [i].GetComponent<Collider> ()) {
					_listOfChildren [i].GetComponent<Collider> ().enabled = state;
				}
			}
		}

		// ------------------------------------------------------------------------
		public void setRigidBody (GameObject go, bool state) {

			if (go.GetComponent<Rigidbody>()) {
				go.GetComponent<Rigidbody>().useGravity = state;
			}
			else {
				foreach (Transform child in go.transform) {
					if (child.gameObject.GetComponent<Rigidbody> ()) {
						child.gameObject.GetComponent<Rigidbody> ().useGravity = state;
					}
				}
			}
		}

        // ------------------------------------------------------------------------
        public void setSnowLevel(GameObject go, float value) {

            float shaderValue = 1f - (2f * value);

            _listOfChildren.Clear();
            getChildrenRecursive(go);

            MeshRenderer renderer;
            int i, len = _listOfChildren.Count;
            for (i = 0; i < len; ++i) {
                renderer = _listOfChildren[i].GetComponent<MeshRenderer>();
                if (renderer != null) {
                    if (renderer.material.shader.name == Globals.snowShaderName) {
                        renderer.material.SetFloat("_SnowLevel", shaderValue);
                    }
                }
            }
        }

        // ------------------------------------------------------------------------
        public void selectElement (string name)
		{
			_selectedElement = _levelElements [name];

            setMeshCollider(_selectedElement.go, false);
			setRigidBody (_selectedElement.go, false);

			getSelectedMeshRenderers (_selectedElement.go, _iSelectedGroupIndex);
			getSelectedMeshRendererBounds ();

            // snow shader
            _selectedMaterials.Clear();
            int i, len = _selectedMeshRenderers.Count;
            for (i = 0; i < len; ++i) {
                if (_selectedMeshRenderers[i].material.shader.name == Globals.snowShaderName) {
                    _selectedMaterials.Add(_selectedMeshRenderers[i].material);
                    //break;
                }
            }

            //PweMainMenu.Instance.showSnowLevelPanel(_selectedMaterials.Count > 0);
		}

        // ------------------------------------------------------------------------
        public void saveSelectElement() {

            if (_selectedElement.go != null) {
                string name = _selectedElement.go.name;
                if (_levelElements.ContainsKey(name)) {
                    _levelElements[name] = _selectedElement;
                }
            }
        }

        // ------------------------------------------------------------------------
        public void changeSnowLevel(float value) {

            float shaderValue = 1f - (2f * value);
            int i, len = _selectedMaterials.Count;
            for (i = 0; i < len; ++i) {
                _selectedMaterials[i].SetFloat("_SnowLevel", shaderValue);
            }
        }

        // ------------------------------------------------------------------------
        public void changeLightIntensity(float value) {

            if (_selectedElement.go == null) {
                return;
            }

            float lightIntensity = 0.5f + (2f * value);

            _listOfChildren.Clear();
            getChildrenRecursive(_selectedElement.go);

            int i, len = _listOfChildren.Count;
            for (i = 0; i < len; ++i) {
                if (_listOfChildren[i].GetComponent<Light>() != null) {
                    _listOfChildren[i].GetComponent<Light>().intensity = lightIntensity;
                }
            }
        }

        // ------------------------------------------------------------------------
        public void deleteSelectedElement ()
		{
			if (_selectedElement.go != null) {
				if (_levelElements.ContainsKey (_selectedElement.go.name)) {
					_levelElements.Remove (_selectedElement.go.name);
				}
				GameObject.Destroy (_selectedElement.go);
				_selectedElement.go = null;
			}
		}

		// ------------------------------------------------------------------------
		public void resetSelectedElement()
		{
			_selectedElement = new LevelElement();
			_selectedElement.part = Globals.PartList.End_Of_List;
            _selectedElement.overwriteGravity = 0;
            _selectedElement.shaderSnow = 0;
            _selectedElement.lightIntensity = 0;

            _selectedMeshRenderers.Clear ();
			_selectedElementBounds = new Bounds();
		}

		// ------------------------------------------------------------------------
		public void resetElementComponents()
		{
			if (_selectedElement.go != null) {

				PrefabLevelEditor.Part part = PrefabLevelEditor.Instance.parts [_selectedElement.part];

				setMeshCollider(_selectedElement.go, true);

                bool gravity = (_selectedElement.overwriteGravity == 0 ? part.usesGravity : (_selectedElement.overwriteGravity == 1 ? true : false));
                setRigidBody (_selectedElement.go, gravity);
			}
		}

		// ------------------------------------------------------------------------
		public void updatedSelectedObjectPosition(Vector3 posChange)
		{
			getSelectedMeshRendererBounds ();

			if (_iSelectedGroupIndex != -1) {

				Vector3 pos;
				int index = _iSelectedGroupIndex;
				int i, len = _aElementGroups [index].gameObjects.Count;
				for (i = 0; i < len; ++i) {
					if (_aElementGroups [index].gameObjects [i] != null && _aElementGroups [index].gameObjects [i] != _selectedElement.go) {
						pos = _aElementGroups [index].gameObjects [i].transform.position + posChange;
						_aElementGroups [index].gameObjects [i].transform.position = pos;
					}
				}
			}
		}

		// ------------------------------------------------------------------------
		public void getSelectedMeshRenderers (GameObject go, int iSelectedGroupIndex)
		{
			_selectedMeshRenderers.Clear ();

			_listOfChildren.Clear ();

			int i, len;

			if (iSelectedGroupIndex != -1)
			{
				len = _aElementGroups [iSelectedGroupIndex].gameObjects.Count;
				for (i = 0; i < len; ++i) {
					getChildrenRecursive (_aElementGroups [iSelectedGroupIndex].gameObjects [i]);
				}
			}
			else
			{
				getChildrenRecursive (go);
			}

			len = _listOfChildren.Count;
			for (i = 0; i < len; ++i) {
				if (_listOfChildren [i].GetComponent<MeshRenderer> ()) {
					_selectedMeshRenderers.Add(_listOfChildren [i].GetComponent<MeshRenderer> ());
				}
			}
		}

		// ------------------------------------------------------------------------
		public void getSelectedMeshRendererBounds()
		{
			if (_selectedElement.go != null) {
				if (_selectedElement.part != Globals.PartList.End_Of_List) {

                    if (_selectedMeshRenderers.Count > 0) {
						_selectedElementBounds = _selectedMeshRenderers [0].bounds;
						int i, len = _selectedMeshRenderers.Count;
						for (i = 1; i < len; ++i) {
							_selectedElementBounds.Encapsulate (_selectedMeshRenderers [i].bounds);
                        }
					}
				}
			}
		}

		// ------------------------------------------------------------------------
		public int findElementInGroup(GameObject go)
		{
			int index = -1;
			int i, len = _aElementGroups.Count;
			for (i = 0; i < len; ++i) {
				int j, len2 = _aElementGroups [i].gameObjects.Count;
				for (j = 0; j < len2; ++j) {
					if (_aElementGroups [i].gameObjects [j] == go) {
						index = i;
						i = len;
						break;
					}
				}
			}

			return index;
		}

		// ------------------------------------------------------------------------
		/*public void placeDungeonPrefab (int index)
		{
			Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 4f;
			int partId = (int)Globals.PartList.Dungeon_Floor + index;

			LevelElement element = new LevelController.LevelElement ();
			element.part = (Globals.PartList)partId;
			element.go = PrefabLevelEditor.Instance.createPartAt (element.part, pos.x, pos.y, pos.z);

			setMeshCollider (element.go, true);
			setRigidBody (element.go, false);

			element.go.AddComponent<Draggable> ();

			_levelElements.Add (element.go.name, element);
		}*/

		#endregion

		#region PrivateMethods

		// ------------------------------------------------------------------------
		// Private Methods
		// ------------------------------------------------------------------------
		private void getChildrenRecursive(GameObject go)
		{
			if (go == null) {
				return;
			}

			_listOfChildren.Add (go);

			foreach (Transform child in go.transform)
			{
				if (child != null) {
					_listOfChildren.Add (child.gameObject);
					getChildrenRecursive (child.gameObject);
				}
			}
		}

		#endregion
	}
}