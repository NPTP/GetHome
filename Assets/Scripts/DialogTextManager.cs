using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  Class that will hold dialogue so that it can all be handled in one central place
 */
public class DialogTextManager : MonoBehaviour
{

    private Dictionary<string, Dialog> dialogText;
    public void Start()
    {
        dialogText = new Dictionary<string, Dialog>();

        dialogText.Add("tape1", new Dialog {
            header = "This is the first tape",
            subtitle1 = "Subtitle1",
            subtitle2 = "Second Subtitle",
            paragraphs = new string[]{ "This is the first paragraph", "This is the second paragraph" }
        });

        dialogText.Add("tape2", new Dialog
        {
            header = "This is the secont tape",
            subtitle1 = "2Subtitle1",
            subtitle2 = "2Second Subtitle",
            paragraphs = new string[] { "This is the first second paragraph", "This is the second second paragraph" }
        });
    }

    public Dialog GetDialog(string id)
    {
        if (dialogText.ContainsKey(id))
        {
            Debug.Log("Got dialog with id " + id);
            Debug.Log(dialogText[id].header);
            return dialogText[id];
        }
        // Can't find dialog, this is an error!
        Debug.Log("Can't find dialog with key " + id);
        return null;
    }
}
