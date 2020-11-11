using UnityEngine;

// Class which handles the single-item UI at the high level.
public class ItemUI : MonoBehaviour
{
    private GameObject itemUIContainer;
    private GameObject cassetteUIContainer;
    void Awake()
    {
        itemUIContainer = gameObject.transform.GetChild(0).gameObject;
        itemUIContainer.SetActive(false);

        cassetteUIContainer = gameObject.transform.GetChild(1).gameObject;
        cassetteUIContainer.SetActive(false);

        //GetComponent<ItemAcquireUI>().enabled = false;
    }

    public void AcquireItem(string itemName)
    {
        itemUIContainer.SetActive(true);
        GetComponent<ItemAcquireUI>().GetItem();
        //GetComponent<ItemAcquireUI>().itemName = itemName;
    }

    public void NoItem()
    {
        itemUIContainer.SetActive(false);
        //GetComponent<ItemAcquireUI>().enabled = false;
    }

    public void AcquireTape(string itemName)
    {
        cassetteUIContainer.SetActive(true);
        GetComponent<ItemAcquireUI>().GetTape();
        //GetComponent<ItemAcquireUI>().itemName = itemName;
    }

    public void NoTape()
    {
        cassetteUIContainer.SetActive(false);
        //GetComponent<ItemAcquireUI>().enabled = false;
    }

    // Space for functions like UseItem(), etc.
}
