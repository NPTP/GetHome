using UnityEngine;

/// <summary>
/// Super simple monobehaviour to get passed from a QUIT in a level
/// to the main menu, to indicate that we just came from a level.
/// Right now does literally nothing else, could do more if needed later.
/// </summary>
public class ReturnFromLevel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
