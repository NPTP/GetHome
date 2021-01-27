using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FinalLevelAction : MonoBehaviour, IObjectAction
{
    StateManager stateManager;

    // ATTACH TO THE TRIGGER
    public FinalForcefieldController forcefield;
    public Material attachedWire;
    private Color originalColor;
    private float emissionFadeTime = 1f;

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
        // Gets the color we need and disables the emission.
        attachedWire.EnableKeyword("_EMISSION");
        originalColor = attachedWire.GetColor("_EmissionColor");
        attachedWire.DisableKeyword("_EMISSION");

        musicLayerManager = GameObject.FindGameObjectWithTag("Music").GetComponent<MusicLayerBuilder>();
        stateManager = FindObjectOfType<StateManager>();
    }

    public void action()
    {
        if (!isActivated)
        {
            forcefield.addCompleted();
            StartCoroutine(EmissionFadeUp());
            attachedWire.EnableKeyword("_EMISSION");
            isActivated = true;
            musicLayerManager.playNextLayer();

            if (blockingLower && blockingUpper)
                StartCoroutine(RemoveBlocking());
        }
    }

    IEnumerator EmissionFadeUp()
    {
        Color startColor = new Color(0, 0, 0, 1);
        attachedWire.EnableKeyword("_EMISSION");
        attachedWire.SetColor("_EmissionColor", startColor);

        float elapsed = 0f;

        while (elapsed < emissionFadeTime)
        {
            float t = elapsed / emissionFadeTime;
            t = Mathf.Pow(t, 3);    // Cubic ease-in

            attachedWire.SetColor(
                "_EmissionColor",
                Color.Lerp(startColor, originalColor, t)
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        attachedWire.SetColor("_EmissionColor", originalColor);
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
