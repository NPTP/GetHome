using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Class which handles all the UI events of when an item is first acquired.
public class ItemAcquireUI : MonoBehaviour
{
    // Name of the item. Set by ItemUI.
    public string itemName = "Default";
    // How fast the icon rotates on its own axis.
    public float iconRotationSpeed = 100f;
    // How fast the icon slides from the center of the screen to its final resting position.
    public float iconSlideSpeed = 1f;
    // How long the icon stays at the center of the screen before moving.
    public float waitAtCenterTime = 0.5f;
    // Initially false until initial routine (moving into position, etc) is complete.
    private bool doneInitialRoutine = false;
    private Vector3 desiredPosition;
    private GameObject itemUIContainer;
    private Text itemText;
    private GameObject itemModel;

    void Awake()
    {
        enabled = false;
    }

    void Start()
    {
        itemUIContainer = gameObject.transform.GetChild(0).gameObject;
        itemUIContainer.SetActive(true);
        itemText = itemUIContainer.transform.GetChild(0).gameObject.GetComponent<Text>();
        itemModel = itemUIContainer.transform.GetChild(1).gameObject;

        desiredPosition = new Vector3(itemModel.transform.localPosition.x, itemModel.transform.localPosition.y, itemModel.transform.localPosition.z);
        itemModel.transform.localPosition = new Vector3(0f, 0f, 0f);
        itemText.enabled = false;
        itemText.text = itemName.ToUpper();

        StartCoroutine("InitialRoutine");
        Debug.Log("Started item acquire UI!");
    }
    void Update()
    {
        if (doneInitialRoutine)
            itemModel.transform.Rotate(0f, 0f, -iconRotationSpeed * Time.deltaTime, Space.Self);
    }

    IEnumerator InitialRoutine()
    {
        yield return new WaitForSeconds(waitAtCenterTime);
        while (itemModel.transform.localPosition != desiredPosition)
        {
            itemModel.transform.localPosition = Vector3.MoveTowards(itemModel.transform.localPosition, desiredPosition, iconSlideSpeed * Time.deltaTime);
            yield return null;
        }
        doneInitialRoutine = true;
        itemText.enabled = true;
    }
}
