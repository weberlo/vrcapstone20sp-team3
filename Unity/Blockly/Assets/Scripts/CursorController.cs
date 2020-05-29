﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public GameObject blockPrefab;  // prefab for blocks, used when emitted
    public GameObject moduleCreationBlockPrefab;  // prefab for the temporary blocks that show up during module creation

    public GameObject recordButton;
    private ModuleController moduleController;

    private HashSet<Vector3> blockPositions;  // the positions for all blocks existing on grid right now

    public AudioClip emitSound;
    public AudioClip moveSound;

    public const float GRID_SIZE = 0.5f;
    public const float MIN_POSITION = 0;  // inclusive
    public const float MAX_POSITION = 4.5f;  // inclusive

    private AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        moduleController = recordButton.GetComponent<ModuleController>();
        blockPositions = new HashSet<Vector3>();
    }

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public void OnRecognizeGesture(string gestureName)
    {
        if (gestureName == "CreateModule")
        {
            moduleController.OnPressRecord();
            return;
        }

        Vector3 oldPosition = this.gameObject.transform.position;
        if (gestureName == "Emit")
        {
            Emit();
            source.PlayOneShot(emitSound);
        }
        else
        {
            source.PlayOneShot(moveSound);
            if (gestureName == "Left")
            {
                MoveLeft();
            }
            else if (gestureName == "Right")
            {
                MoveRight();
            }
            else if (gestureName == "Backward")
            {
                MoveBackward();
            }
            else if (gestureName == "Forward")
            {
                MoveForward();
            }
            else if (gestureName == "Up")
            {
                MoveUp();
            }
            else if (gestureName == "Down")
            {
                MoveDown();
            }
        }

        if (moduleController.IsRecording())
        {
            Debug.Log("OnRecognizeGesture: " + gestureName);

            // only record if the move actually happens (don't record if cursor is at edge of valid region and doesn't actually move)
            if (gestureName != "Emit" && this.gameObject.transform.position == oldPosition)
            {
                return;
            }
            moduleController.AddStatement(gestureName);
        }

        // if (Input.GetKeyDown(KeyCode.Delete))
        // {
        //     Delete();
        // }
    }

    public void MoveRight()
    {
        Vector3 position = this.gameObject.transform.position;
        position.x += GRID_SIZE;
        if (position.x > MAX_POSITION)
        {
            return;
        }
        this.gameObject.transform.position = position;
    }

    public void MoveLeft()
    {
        Vector3 position = this.gameObject.transform.position;
        Debug.Log("old pos = " + this.gameObject.transform.position);
        position.x -= GRID_SIZE;
        if (position.x < MIN_POSITION)
        {
            return;
        }
        this.gameObject.transform.position = position;
        Debug.Log("new pos = " + this.gameObject.transform.position);
    }

    public void MoveUp()
    {
        Vector3 position = this.gameObject.transform.position;
        position.y += GRID_SIZE;
        if (position.y > MAX_POSITION)
        {
            return;
        }
        this.gameObject.transform.position = position;
    }

    public void MoveDown()
    {
        Vector3 position = this.gameObject.transform.position;
        position.y -= GRID_SIZE;
        if (position.y < MIN_POSITION)
        {
            return;
        }
        this.gameObject.transform.position = position;
    }

    public void MoveBackward()
    {
        Vector3 position = this.gameObject.transform.position;
        position.z -= GRID_SIZE;
        if (position.z < MIN_POSITION)
        {
            return;
        }
        this.gameObject.transform.position = position;
    }

    public void MoveForward()
    {
        Vector3 position = this.gameObject.transform.position;
        position.z += GRID_SIZE;
        if (position.z > MAX_POSITION)
        {
            return;
        }
        this.gameObject.transform.position = position;
    }

    // emit a new block at the cursor's current position, if there isn't a block there already
    // if there is a block there, deletes it
    public void Emit()
    {
        bool blockExisted = false;
        Vector3 blockPosition = this.gameObject.transform.position;
        Collider[] colliders = Physics.OverlapSphere(blockPosition, CursorController.GRID_SIZE / 3);
        foreach (Collider collider in colliders)
        {
            if (this.moduleController.IsRecording() ? collider.gameObject.tag == "Module Creation Block" : collider.gameObject.tag == "Block")
            {
                Vector3 cursorCopy = new Vector3(blockPosition.x, blockPosition.y, blockPosition.z);
                if (blockPositions.Contains(cursorCopy))
                {
                    blockExisted = true;
                    Destroy(collider.gameObject);
                    blockPositions.Remove(cursorCopy);

                }
                break;
            }
        }

        if (!blockExisted)
        {
            GameObject prefab = this.moduleController.IsRecording() ? this.moduleCreationBlockPrefab : this.blockPrefab;
            GameObject obj = Instantiate(prefab, blockPosition, Quaternion.identity) as GameObject;
            blockPositions.Add(new Vector3(blockPosition.x, blockPosition.y, blockPosition.z));
        }
    }

    // delete the block at the cursor's current position, if there is one
    public void Delete()
    {
        Collider[] colliders = Physics.OverlapSphere(this.gameObject.transform.position, GRID_SIZE / 3);
        foreach (Collider collider in colliders)
        {
            // only destroy emitted blocks, not the cursor/region
            // TODO: look into layer masks
            if (collider.gameObject.tag == "Block")
            {
                Destroy(collider.gameObject);
            }
        }
    }

    // calls appropriate functions to move/delete/emit based on keypress
    // TODO: eventually remove this as this will be replaced by gesture control
    private void Move()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            moduleController.OnPressRecord();
        }
        /*
        if (Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            MoveRight();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            MoveBackward();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            MoveForward();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            MoveDown();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            MoveUp();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Emit();
        }
        */
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            Delete();
        }
    }

    // returns true if there is not currently a block at the given position
    private bool isGridSpaceEmpty(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(this.gameObject.transform.position, GRID_SIZE / 3);
        foreach (Collider collider in colliders)
        {
            // TODO: look into layer masks
            if (collider.gameObject.tag == "Block" && !this.moduleController.IsRecording())
            {
                return false;
            }
        }
        return true;
    }

    public Vector3 CursorPosition()
    {
        return this.gameObject.transform.position;
    }
}
