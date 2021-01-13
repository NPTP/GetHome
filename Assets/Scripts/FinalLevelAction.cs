using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FinalLevelAction : MonoBehaviour, IObjectAction
{
    StateManager stateManager;

    // ATTACH TO THE TRIGGER
    public FinalForcefieldController forcefield;
    public Material attachedWire;

    float blockingMoveVolumeScale = 0.5f;
    [Header("Blocking geo lowering details")]
    [SerializeField] float distanceToLower = 4f;
    [SerializeField] float timeToLower = 1f;
    [SerializeField] AudioClip blockingMoveSound;
    [Header("Floor geo in the way of solved puzzle")]
    [SerializeField] GameObject blockingLower;
    [Header("Ceiling geo in the way of solved puzzle")]
    [SerializeField] GameObject blockingUpper;

    private bool isActivated = false;
    private MusicLayerBuilder musicLayerManager;

    void Start()
    {
        attachedWire.DisableKeyword("_EMISSION");
        musicLayerManager = GameObject.FindGameObjectWithTag("Music").GetComponent<MusicLayerBuilder>();
        stateManager = FindObjectOfType<StateManager>();
    }

    public void action()
    {
        if (!isActivated)
        {
            forcefield.addCompleted();
            attachedWire.EnableKeyword("_EMISSION");
            isActivated = true;
            musicLayerManager.playNextLayer();

            if (blockingLower && blockingUpper)
                StartCoroutine(RemoveBlocking());
        }
    }

    IEnumerator RemoveBlocking()
    {
        stateManager.SetState(StateManager.State.Inert);

        // Wait for powerup to hit first, before moving blocking geometry
        yield return new WaitForSeconds(1f);

        int up, down;
        if (stateManager.IsGravityFlipped())
        {
            up = -1;
            down = 1;
        }
        else // Normal gravity
        {
            up = 1;
            down = -1;
        }

        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource) { audioSource.PlayOneShot(blockingMoveSound, blockingMoveVolumeScale); }

        Tween t;
        blockingLower.transform.DOMoveY(distanceToLower * down, timeToLower).SetRelative();
        t = blockingUpper.transform.DOMoveY(distanceToLower * up, timeToLower).SetRelative();
        t.OnComplete(() =>
            {
                Destroy(blockingLower);
                Destroy(blockingUpper);
                stateManager.SetState(StateManager.State.Normal);
            }
        );
    }
}
