using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Class which handles all the UI events of when an item is first acquired.
public class ItemAcquireUI : MonoBehaviour
{
    // Name of the item. Set by ItemUI.
    public string itemName = "Default";
    public string tapeName = "Tape";

    // How fast the icon rotates on its own axis.
    public float iconRotationSpeed = 100f;

    // How fast the icon slides from the center of the screen to its final resting position.
    public float iconSlideSpeed = 1f;

    // How long the icon stays at the center of the screen before moving.
    public float waitAtCenterTime = 0.5f;

    // Initially false until initial routine (moving into position, etc) is complete.
    private bool doneInitialItemRoutine = false;
    private bool doneInitialTapeRoutine = false;

    private Vector3 desiredItemPosition;
    private GameObject itemUIContainer;
    private Text itemText;
    private GameObject itemModel;

    private Vector3 desiredTapePosition;
    private GameObject tapeUIContainer;
    private Text tapeText;
    private GameObject tapeModel;

    //void Awake()
    //{
    //    enabled = false;
    //}

    void Start()
    {
        itemUIContainer = gameObject.transform.GetChild(0).gameObject;
        itemUIContainer.SetActive(true);
        itemText = itemUIContainer.transform.GetChild(0).gameObject.GetComponent<Text>();
        itemModel = itemUIContainer.transform.GetChild(1).gameObject;

        desiredItemPosition = new Vector3(itemModel.transform.localPosition.x, itemModel.transform.localPosition.y, itemModel.transform.localPosition.z);
        itemModel.transform.localPosition = new Vector3(0f, 0f, 0f);
        itemText.enabled = false;
        itemText.text = itemName.ToUpper();


        tapeUIContainer = gameObject.transform.GetChild(1).gameObject;
        tapeUIContainer.SetActive(true);
        tapeText = tapeUIContainer.transform.GetChild(0).gameObject.GetComponent<Text>();
        tapeModel = tapeUIContainer.transform.GetChild(1).gameObject;

        desiredTapePosition = new Vector3(tapeModel.transform.localPosition.x, tapeModel.transform.localPosition.y, tapeModel.transform.localPosition.z);
        tapeModel.transform.localPosition = new Vector3(0f, 0f, 0f);
        tapeText.enabled = false;
        tapeText.text = tapeName.ToUpper();

    }

    public void GetTape()
    {
        if (!doneInitialTapeRoutine)
            StartCoroutine("InitialTapeRoutine");
    }

    public void GetItem()
    {
        if (!doneInitialItemRoutine)
            StartCoroutine("InitialItemRoutine");
    }

    void Update()
    {
        if (doneInitialItemRoutine)
        {
            itemModel.transform.Rotate(0f, 0f, -iconRotationSpeed * Time.deltaTime, Space.Self);
        }
        if (doneInitialTapeRoutine)
        {
            tapeModel.transform.Rotate(0f, 0f, -iconRotationSpeed * Time.deltaTime, Space.Self);
        }

    }

    IEnumerator InitialItemRoutine()
    {
        Debug.Log("In InitialItemRoutine");
        Debug.Log("desitredItemPosition:" + desiredItemPosition);
        yield return new WaitForSeconds(waitAtCenterTime);
        while (itemModel.transform.localPosition != desiredItemPosition)
        {
            itemModel.transform.localPosition = Vector3.MoveTowards(itemModel.transform.localPosition, desiredItemPosition, iconSlideSpeed * Time.deltaTime);
            yield return null;
        }
        doneInitialItemRoutine = true;
        itemText.enabled = true;
    }

    IEnumerator InitialTapeRoutine()
    {
        Debug.Log("In InitialTapeRoutine");
        Debug.Log("desireTapePosition:" + desiredTapePosition);
        yield return new WaitForSeconds(waitAtCenterTime);
        while (tapeModel.transform.localPosition != desiredTapePosition)
        {
            tapeModel.transform.localPosition = Vector3.MoveTowards(tapeModel.transform.localPosition, desiredTapePosition, iconSlideSpeed * Time.deltaTime);
            yield return null;
        }
        doneInitialTapeRoutine = true;
        tapeText.enabled = true;
    }
}
