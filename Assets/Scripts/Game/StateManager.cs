﻿using System;
using UnityEngine;

// State machine for the whole game, based primarily on player state.
public class StateManager : MonoBehaviour
{
    public StateManager.State state;
    public enum State
    {
        Normal,     /* Take normal inputs. */
        Dialog,     /* Disallow movement/interaction, hit prompts only. */
        Inert,      /* Block all inputs. Use for e.g. switching characters.  */
        Paused,     // Not set up yet
        Crate,      // Not set up yet
        Flipping    // Not set up yet
    }

    GameObject selectedCharacter;   /* Use to keep track of player/robot selection. */

    public event EventHandler<StateArgs> OnState;
    public class StateArgs : EventArgs
    {
        public StateManager.State state;
    }

    void Start()
    {
        SetState(State.Normal);
        selectedCharacter = GameObject.FindWithTag("Player");
    }

    // This method is for the other managers to tell us what state we're in.
    public void SetState(StateManager.State newState)
    {
        this.state = newState;
        // Debug.Log(">>>>>>>>>> STATE SET TO: " + this.state);
        OnState?.Invoke(this, new StateArgs { state = newState });
    }

    public StateManager.State GetState()
    {
        return this.state;
    }

    public void SetSelected(GameObject selected)
    {
        selectedCharacter = selected;
        // Debug.Log(">>>>>>>>>> SELECTED: " + selected.tag);
    }

    public GameObject GetSelected()
    {
        return selectedCharacter;
    }

    public void EndInert()
    {
        if (this.state == State.Inert)
            SetState(State.Normal);
        // Else let the current state be.
    }

}