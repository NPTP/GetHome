using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 *  Class that will hold dialogue so that it can all be handled in one central place/
 *  Use the labels C1-C9 as per the google sheet the text comes from.
 */
public class DialogTextManager : MonoBehaviour
{

    private Dictionary<string, Dialog> dialogText;
    public void Start()
    {
        dialogText = new Dictionary<string, Dialog>();
        AddTestDialogs();

        AddDialogsForScene(SceneManager.GetActiveScene().name);
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


    void AddDialogsForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Level1-1":
                Level11();
                break;

            case "Level1-2":
                Level12();
                break;

            case "Level2-1":
                Level21();
                break;

            case "Level2-2":
                Level22();
                break;

            case "Level3-1":
                Level31();
                break;

            case "Level3-2":
                Level32();
                break;

            default:
                print("Scene " + sceneName + " unknown.");
                break;
        }
    }

    void Level11()
    {
        dialogText.Add("C1", new Dialog
        {
            header = "Tape 1 of 9",
            subtitle1 = "",
            subtitle2 = "",
            paragraphs = new string[] {
                "S: Alright, alright, guys, I'm recording, he's on his way, any second now...",
                "P: Uh-huh, just like you said five minutes ago.",
                "S: Hey, it's for real this time, I swear--",
                "M: Hush up, I hear him!",
                "- a door creaks open-",
                "- a moment of silence before sounds of party",
                "SMP, in partial unison: HAPPY BIRTHDAY RAY!!",
                "- commotion dies down -",
                "R: Wow, wow. What a surprise, truly. I definitely didn't see Sam booking it back to my quarters just now.",
                "S: Yeah, I was probably the wrong person for the job, to be honest.",
                "P: Alright, for the record, it's really hard to plan a surprise party on a heap of scrap metal with only 5 people on it --",
                "M: But you're not gonna believe what we got you!! Open it up!",
                "- rustling sounds -",
                "R: Is this --",
                "S: Yes. Yes it is.",
                "R: A 2089 Bordeaux?!",
                "P: It took some digging, but hell yeah it is.",
                "R: But this stuff's ancient, how did you guys even manage to--",
                "M: Hard work and dedication, chief. Cas also really scrummed up a good bargain.",
                "R: This, this really means a lot guys, thanks.",
                "P: Let's get this bad boy open!!",
                "S: The night is young, my friends! Glad I got that all on tape, Cas'll love to hear what a sap Ray turned out to be."}
        });

        dialogText.Add("C2", new Dialog
        {
            header = "Tape 2 of 9",
            subtitle1 = "",
            subtitle2 = "",
            paragraphs = new string[] {
                "M: -- and so he would lock himself in his quarters, pour dirt on the ground and just lay there, for days. We only eventually noticed because of the smell.",
                "P: See, that's just nasty.",
                "M: I think he was trying to escape somehow.",
                "R: Not everyone is cut out for this line of work.",
                "C: Do you remember what the last crew we met up with said, the one that came back this way?",
                "R: Yeah, but they were loonies.",
                "C: The burly one, Jaha, he told me that there was some type of interference that hijacked their radar and comms.",
                "M: Could it have been pirates? It's pretty far out, but still a possibility, right?",
                "P: Hard to say. Barring space demons, not sure what else could've caused it. Though they really did try to pin it on space demons.",
                "R: Exactly, loonies.",
                "C: Supposedly it's common along this freight route, but I couldn't track down any other logs to back that up.",
                "S: Well, they had some lofty conspiracy theories about that one, that the higher-ups wanted to keep it all hush-hush.",
                "R: We all saw they were cooked up nearly 24/7, so they're seriously lacking credibility there. Besides, there's a million other explanations.",
                "M: No harm in being on the lookout, I'll check in on the systems more often while we're in the dead zone.",
                "P: I'm fine with space demons, personally.",
                "M: Yeah, having shared a ship with a guy that ate dirt until his organs failed, I find people scarier."
            }
        });
    }

    void Level12()
    {

    }

    void Level21()
    {

    }

    void Level22()
    {

    }

    void Level31()
    {

    }

    void Level32()
    {

    }

    void AddTestDialogs()
    {
        dialogText.Add("tape1", new Dialog
        {
            header = "This is the first tape",
            subtitle1 = "Subtitle1",
            subtitle2 = "Second Subtitle",
            paragraphs = new string[] { "This is the first paragraph", "This is the second paragraph" }
        });

        dialogText.Add("tape2", new Dialog
        {
            header = "This is the secont tape",
            subtitle1 = "2Subtitle1",
            subtitle2 = "2Second Subtitle",
            paragraphs = new string[] { "This is the first second paragraph", "This is the second second paragraph" }
        });
    }
}
