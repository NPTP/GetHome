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
        // AddTestDialogs();

        AddTapeDialogs();
    }

    public Dialog GetDialog(string id)
    {
        if (dialogText.ContainsKey(id))
        {
            return dialogText[id];
        }
        // Can't find dialog, this is an error!
        return null;
    }


    void AddTapeDialogs()
    {
        dialogText.Add("C1", new Dialog
        {
            header = "Tape 1 of 9",
            subtitle1 = "",
            subtitle2 = "",
            paragraphs = new string[] {
                "S: Alright, alright, guys, I'm recording, he's on his way, any second now... \n\nP: Uh-huh, just like you said five minutes ago.\n\nS: Hey, it's for real this time, I swear-- \n\nM: Hush up, I hear him!",
                "- a door creaks open- \n\n- a moment of silence before sounds of party -",
                "SMP, in partial unison: HAPPY BIRTHDAY RAY!!",
                "- commotion dies down -",
                "R: Wow, wow. What a surprise, truly. I definitely didn't see Sam booking it back to my quarters just now. \n\nS: Yeah, I was probably the wrong person for the job, to be honest. \n\nP: Alright, for the record, it's really hard to plan a surprise party on a heap of scrap metal with only 5 people on it -- \n\nM: But you're not gonna believe what we got you!! Open it up!",
                "* rustling sounds * \n\nR: Is this --",
                "S: Yes. Yes it is.",
                "R: A 2089 Bordeaux?! \n\nP: It took some digging, but hell yeah it is. \n\nR: But this stuff's ancient, how did you guys even manage to-- \n\nM: Hard work and dedication, chief. Cas also really scrummed up a good bargain. \n\nR: This, this really means a lot guys, thanks.",
                "P: Let's get this bad boy open!! \n\nS: The night is young, my friends! Glad I got that all on tape, Cas'll love to hear what a sap Ray turned out to be.",
                " -- END OF TAPE -- "}
        });

        dialogText.Add("C2", new Dialog
        {
            header = "Tape 2 of 9",
            subtitle1 = "",
            subtitle2 = "",
            paragraphs = new string[] {
                "M: -- and so he would lock himself in his quarters, pour dirt on the ground and just lay there, for days. We only eventually noticed because of the smell. \n\nP: See, that's just nasty. \n\nM: I think he was trying to escape somehow. \n\nR: Not everyone is cut out for this line of work.",
                "C: Do you remember what the last crew we met up with said, the one that came back this way? \n\nR: Yeah, but they were loonies. \n\nC: The burly one, Jaha, he told me that there was some type of interference that hijacked their radar and comms. \n\nM: Could it have been pirates? It's pretty far out, but still a possibility, right?",
                "P: Hard to say. Barring space demons, not sure what else could've caused it. Though they really did try to pin it on space demons. \n\nR: Exactly, loonies.",
                "C: Supposedly it's common along this freight route, but I couldn't track down any other logs to back that up. \n\nS: Well, they had some lofty conspiracy theories about that one, that the higher-ups wanted to keep it all hush-hush. \n\nR: We all saw they were cooked up nearly 24/7, so they're seriously lacking credibility there. Besides, there's a million other explanations.",
                "M: No harm in being on the lookout, I'll check in on the systems more often while we're in the dead zone. \n\nP: I'm fine with space demons, personally. \n\nM: Yeah, having shared a ship with a guy that ate dirt until his organs failed, I find people scarier.",
                " -- END OF TAPE -- "
            }
        });

        dialogText.Add("C3", new Dialog
        {
            header = "Tape 3 of 9",
            subtitle1 = "",
            subtitle2 = "",
            paragraphs = new string[] {
                "S: Hey! Hey, look what I found. The old cassette recorder, remember? \n\nP: Well, no real use for it now. Just keep at it with the nonperishables, yeah? We need to count everything before-- \n\nS: Hey, no wait, I think I got it working! Here, have anything to say for the good folks listening at home? \n\nP: How are you -- how are you still acting like everything's okay?! Did you hear anything I just said?",
                "S: I just-- \n\nP: The bots are going haywire, the replicators too, and I can only make a quick fix to the power grid, and -- god, I just, could you please just -- \n\nS: Yeah. Yeah I'm sorry, it's stupid. \n\nP: It's fine, let's just ... keep counting, okay?",
                " -- END OF TAPE -- "
            }
        });

        dialogText.Add("C4", new Dialog
        {
            header = "Tape 4 of 9",
            subtitle1 = "",
            subtitle2 = "",
            paragraphs = new string[] {
                "M: She's still not responding. \n\nR: Do it again.",
                "* walky talky noise *",
                "M: Cas, come in Cas, please don't do this-- \n\nR: Get back out here. NOW. \n\nC: Look, guys. The longer we wait, the more dangerous it gets. Trust me, I know how to fix this thing. \n\nR: We need to run more tests to make sure it won't rip us all apart, Cas. \n\nC: It won't.",
                "P: You know what, fuck you, you don't just get to make that decision for everyone. Fucking open the door before that reactor blasts us all into the empty. \n\nR: Get that door open, Sam. \n\nS: Already on it, chief.",
                "* click *",
                "C: I -- I think I got it. Guys! I think I got it! And you were all so--",
                "* loud blast * \n\n* screams * \n\n* alarms sounding *",
                "S: Oh god, my arm, oh fuck, my arm it's--",
                " -- END OF TAPE -- "
            }
        });

        dialogText.Add("C5", new Dialog
        {
            header = "Tape 5 of 9",
            subtitle1 = "",
            subtitle2 = "",
            paragraphs = new string[] {
                "S: Love how she just gets to sleep through this with barely a scratch, meanwhile my arm is busted all to hell. \n\nM: She's thrown up at least 4 times now, while unconscious. \n\nP: Do we know why? \n\nM: Well, it's a symptom of some kind of head trauma, not that I can find the source. But, you're right, Sammy, other than that, she seems... remarkably unscathed.",
                "P: I've been fiddling with the medtech but it's too fried, I wouldn't put her in there to run a scan. \n\nR: The only option is to leave her. \n\nM: Ray!",
                "P: He's not wrong. At this point, we're honestly lucky to have life support. \n\nS: And how long will that last, again? \n\nP: Three days, if nothing else happens. \n\nS: Great, that's just, great.",
                "R: We can't count on that time. We head back down there immediately-- \n\n* electrical whir * \n\nM: Hey, what's that sound? \n\nP: That, that would be the auxiliary power.",
                " -- END OF TAPE -- "
            }
        });

        dialogText.Add("C6", new Dialog
        {
            header = "Tape 6 of 9",
            subtitle1 = "",
            subtitle2 = "",
            paragraphs = new string[] {
                "S: Hey. \n\nIt's about 3am, and I can't sleep. \n\nIt's not because of the pain either. \n\nI saw -- well, I don't know what I saw.",
                "Sometimes I look outside the window and I swear ... I swear I see some of the stars just, going out. \n\nSo maybe it's just some trick of the light, or maybe I'm just really losing it.",
                "Without the radar we can't even see if there's anything out there. \n\nThere's maybe a day and a half of life support left. \n\nPewter and Raymond have been doing everything they can to restore power to the elevators so we can get down again, but nothing's been working so far.",
                "It's really starting to look like, we might just die here. Haha. \n\n* rustling  noise * \n\nCas?",
                "Oh god, you're okay, how are you even -- \n\nCas? \n\n* distorted crackle *",
                " -- END OF TAPE -- "
            }
        });
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
