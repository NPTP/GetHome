using System;
using UnityEngine;

// State machine for the whole game, based primarily on player state.
public class StateManager : MonoBehaviour
{
    public StateManager.State state;
    public enum State
    {
        Normal,     /* Take normal inputs. */
        Looking,    /* Take only movement inputs, but no interactions. */
        Dialog,     /* Disallow movement/interaction, hit prompts only. */
        Inert,      /* Block all inputs. Use for e.g. switching characters.  */
        Crate,      // Not set up yet
        Flipping    // Not set up yet
    }

    GameObject selectedCharacter;   /* Use to keep track of player/robot selection. */
    bool isGravityFlipped = false;  /* Use to keep track of orientation (floor is bottom: false, ceiling is bottom: true) */
    bool readyToFlip = true;        /* Use to allow/disallow another gravity flip, cool down from the last. */
    bool isPaused = false;

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
        OnState?.Invoke(this, new StateArgs { state = newState });
    }

    public StateManager.State GetState()
    {
        return this.state;
    }

    public void SetSelected(GameObject selected)
    {
        selectedCharacter = selected;
    }

    public GameObject GetSelected()
    {
        return selectedCharacter;
    }

    public void ToggleGravityOrientation()
    {
        isGravityFlipped = !isGravityFlipped;
    }

    public bool IsGravityFlipped()
    {
        return isGravityFlipped;
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public void SetPaused(bool value)
    {
        isPaused = value;
    }

    public void SetReadyToFlip(bool value)
    {
        readyToFlip = value;
    }

    public bool CheckReadyToFlip()
    {
        return readyToFlip;
    }

    public void EndInert()
    {
        if (this.state == State.Inert)
            SetState(State.Normal);
        // Else let the current state be.
    }

}
