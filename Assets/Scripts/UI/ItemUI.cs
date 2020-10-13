using UnityEngine;

// Class which handles the single-item UI at the high level.
public class ItemUI : MonoBehaviour
{
    private GameObject itemUIContainer;
    void Awake()
    {
        itemUIContainer = gameObject.transform.GetChild(0).gameObject;
        itemUIContainer.SetActive(false);

        GetComponent<ItemAcquireUI>().enabled = false;
    }

    public void AcquireItem(string itemName)
    {
        itemUIContainer.SetActive(true);
        GetComponent<ItemAcquireUI>().enabled = true;
        GetComponent<ItemAcquireUI>().itemName = itemName;
    }

    public void NoItem()
    {
        itemUIContainer.SetActive(false);
        GetComponent<ItemAcquireUI>().enabled = false;
    }

    // Space for functions like UseItem(), etc.
}
