﻿using UnityEngine;
using UnityEngine.Assertions;
using OculusSampleFramework;

namespace Blockly
{

// TODO rename to LibraryModule
public class BlocklyLibraryModuleController : MonoBehaviour
{
	[SerializeField] private GameObject _startStopButton = null;
	[SerializeField] float _maxSpeed = 10f;
	[SerializeField] private SelectionCylinder _selectionCylinder = null;

	[SerializeField]
	private GameObject _moduleMeshObj;
	[SerializeField]
	private GameObject _dragModulePrefab;

	public int moduleName = -1;

	private InteractableTool _toolInteractingWithMe = null;

	private void Awake()
	{
		Debug.Assert(moduleName != -1);
		Assert.IsNotNull(_startStopButton);
		Assert.IsNotNull(_selectionCylinder);
	}

	private void OnEnable()
	{
		_startStopButton.GetComponent<Interactable>().InteractableStateChanged.AddListener(StartStopStateChanged);
	}

	private void OnDisable()
	{
		if (_startStopButton != null)
		{
			_startStopButton.GetComponent<Interactable>().InteractableStateChanged.RemoveListener(StartStopStateChanged);
		}
	}

	private void StartStopStateChanged(InteractableStateArgs obj)
	{
		if (obj.Tool == null) return;

		if (_toolInteractingWithMe == null) {
			_toolInteractingWithMe = obj.Tool;
		}

		if (obj.Tool == _toolInteractingWithMe) {
			if (obj.NewInteractableState > InteractableState.Default) {
				_selectionCylinder.CurrSelectionState = SelectionCylinder.SelectionState.Selected;
			}

			if (obj.Tool.ToolInputState == ToolInputState.PrimaryInputDown || obj.Tool.ToolInputState == ToolInputState.PrimaryInputDownStay) {
				// finger is pinching
				if ((obj.OldInteractableState == InteractableState.ProximityState || obj.OldInteractableState == InteractableState.ContactState)
					&& obj.NewInteractableState == InteractableState.Default) {
					// ray went outside of proximity zone while pinching (i.e., the module was dragged out of the library)
					_moduleMeshObj.GetComponent<Renderer>().material.color = Color.red;
					_selectionCylinder.CurrSelectionState = SelectionCylinder.SelectionState.Off;
					GameObject dragModObj = Instantiate(_dragModulePrefab);
					BlocklyDragModule dragMod = dragModObj.GetComponent<BlocklyDragModule>();
					dragMod.toolInteractingWithMe = obj.Tool;
					dragMod.moduleName = moduleName;
					_toolInteractingWithMe = null;
				} else {
					_selectionCylinder.CurrSelectionState = SelectionCylinder.SelectionState.Highlighted;
					_moduleMeshObj.GetComponent<Renderer>().material.color = Color.cyan;
				}
			} else if (obj.Tool.ToolInputState == ToolInputState.PrimaryInputUp || obj.Tool.ToolInputState == ToolInputState.Inactive) {
				// finger is not pinching
				if (obj.NewInteractableState == InteractableState.Default) {
					_toolInteractingWithMe = null;
				}
			}
		}

		if (_toolInteractingWithMe == null) {
			// tool stopped interacting with us
			_moduleMeshObj.GetComponent<Renderer>().material.color = Color.white;
			_selectionCylinder.CurrSelectionState = SelectionCylinder.SelectionState.Off;

			if (obj.NewInteractableState > InteractableState.Default) {
				// a new object is interacting with us
				_toolInteractingWithMe = obj.Tool;
			}
		}
	}
}

}
