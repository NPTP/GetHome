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
                "* a door creaks open* \n\n* a moment of silence * \n\nAll: HAPPY BIRTHDAY RAY!",
                "R: Wow, guys. What a surprise. Didn't realize I was turning 7. \n\nS: Come on, Ray, we all knew you were worried we forgot. \n\nP: Also do you have any idea how long it took to put up these streamers-- \n\nM: But now that you're here, you're not gonna believe what we got you! Open it up!",
                "* rustling sounds * \n\nR: Is this -- \n\nS: Yes. Yes it is.",
                "R: A 2089 vintage Bordeaux?! \n\nP: It took some digging, but hell yeah it is. \n\nR: But this stuff's ancient! How did you guys even manage to get it? \n\nP: Intimidation, blackmail, Sam lost a thumb. \n\n R: This, this really means a lot guys, thanks.",
                "M: Let's get this party started, I'll pick the era! \n\nS: Man, glad I got that all on tape, Cas'll love to hear what a sap Ray turned out to be.",
                " -- END OF TAPE -- "}
        });

        dialogText.Add("C2", new Dialog
        {
            header = "Tape 2 of 9",
            subtitle1 = "",
            subtitle2 = "",
            paragraphs = new string[] {
                "M: -- and so he would lock himself in his quarters, pour dirt on the ground and just lay there, for days. We only eventually noticed because of the smell. \n\nP: See, that's just nasty. \n\nM: I think he was trying to escape somehow? \n\nR: Not everyone is cut out for this line of work.",
                "C: Do you remember what the last crew we met up with said, the one that came back this way? \n\nR: Yeah, but they were loonies. \n\nC: The burly one, Jaha, told me that there was some type of interference that hijacked their radar and comms. \n\nM: Could it have been pirates? It's pretty far out, but still a possibility, right?",
                "P: Hard to say. Not sure what else could've caused it. \n\nS: Pretty sure they tried to pin it on space demons. \n\nR: Exactly, loonies.",
                "C: Supposedly it's common along this freight route, but I couldn't track down any other logs to back that up. \n\nS: Well, they had some lofty conspiracy theories about that one. \n\nR: We all saw they were cooked up nearly 24/7, so they're seriously lacking credibility here. Besides, there's a million other explanations.",
                "C: Well, I'll check in on the systems more often while we're in the dead zone, just in case. \n\nP: I, for one, welcome our space demon overlords.",
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
                "* walky talky buzz *",
                "M: Cassy, come in Cas, please don't do this-- \n\nR: Get back out here. NOW. \n\nC: Look, guys. The longer we wait, the more dangerous it gets. Trust me, I know how to fix this thing. \n\nR: We need to run more tests to make sure it won't rip us all apart, Cas. \n\nC: It won't.",
                "P: You know what, f*** you, you don't just get to make that decision for all of us. Open the goddamn door before that reactor blasts us all into the empty! \n\nR: Get that door open, Sam. \n\nS: Already on it.",
                "* click *",
                "C: I -- I think I got it. Guys! I think I got it! And you were all so--",
                "* a loud blast is followed by screams * \n\n* alarms sounding *",
                "S: Oh god, my arm, oh f***, my arm it's--",
                " -- END OF TAPE -- "
            }
        });

        dialogText.Add("C5", new Dialog
        {
            header = "Tape 5 of 9",
            subtitle1 = "",
            subtitle2 = "",
            paragraphs = new string[] {
                "S: Love how she just gets to sleep through this peacefully, meanwhile my arm is busted all to hell. \n\nM: She's thrown up at least 4 times now, while unconscious. \n\nP: So, she hit her head? \n\nM: Well, a lot more got hit than just her head, in theory. But, other than the bile I've been cleaning up, she seems... remarkably unscathed.",
                "P: I've been fiddling with the medtech but it's too fried, I wouldn't put her in there to run a scan. \n\nR: The only option is to leave her. \n\nM: Ray!",
                "P: He's not wrong. At this point, we're honestly lucky to have life support. \n\nS: And how long will that last, again? \n\nP: Three days, if nothing else happens. \n\nS: Great, that's just, great...",
                "R: We can't count on that time. We head back down there immediately-- \n\n* electrical whir * \n\nM: Hey, uh, what was that? \n\nP: That-- that would be the auxiliary power.",
                " -- END OF TAPE -- "
            }
        });

        dialogText.Add("C6", new Dialog
        {
            header = "Tape 6 of 9",
            subtitle1 = "",
            subtitle2 = "",
            paragraphs = new string[] {
                "S: Hey. \n\nIt's about 3am. Can't sleep. \n\nAnd not because of the pain either. \n\nI saw -- well, I don't know what I saw.",
                "Sometimes I look outside the window and I swear ... I swear I see some of the stars just, going out. \n\nSo maybe it's just some trick of the light, or maybe I'm just really losing it.",
                "Without the radar we can't even see if there's anything out there, or in here. But that's a much worse thought.",
                "There's maybe... a day and a half of life support left? \n\nPewter and Ray have been doing everything they can to reroute power to the elevators, get us down again, but nothing's been working so far.",
                "It's really starting to look like, this might be it. \n\n* rustling  noise * \n\nCas?",
                "Oh god, you're okay, how are you even -- \n\nCas? \n\n* distorted crackle *",
                " -- END OF TAPE -- "
            }
        });

        dialogText.Add("C7", new Dialog
        {
            header = "Tape 7 of 9",
            subtitle1 = "",
            subtitle2 = "",
            paragraphs = new string[] {
                "R: -- don't care what you think you saw or heard, you were wrong. \n\nS: I'm telling you, that ISN'T. HER. \n\nM: We've heard you going off at night, talking to yourself, and that tape. We're worried, Sammy.",
                "S: We need to tie her up, just chain her to something just until we figure this out. That's reasonable, right? \n\nP: We're not chaining her up -- do you hear yourself? She's unconscious, for god's sake, what is wrong with you? \n\n S: YOU DIDN'T SEE WHAT I SAW.",
                "M: Well, I'm looking at her right now, and all I see is our injured friend. \n\nS: . . . \n\nR: There's no time for this. Hand me the recorder, Sam.",
                "S: No. \n\nR: Don't make me do this the hard way. \n\nS: Are you... Are you with her? Are you in on this?",
                "P: No one is in on anything, Sam. You're just letting the low oxygen levels get to your head. \n\nR: You know what, this is done, If you can't pull yourself together--",
                "M: Wait.I know where we can put her, where she'll be safe, and away from us, just until we get through this."
            }
        });

        dialogText.Add("C8", new Dialog
        {
            header = "Tape 8 of 9",
            subtitle1 = "",
            subtitle2 = "",
            paragraphs = new string[] {
                "S: Hey. \n\nRay did end up taking my tape player, not that it matters now. \n\nHe already went.",
                "First, he started... Glitching. There’s no way to describe what it looks like, really. \n\nSome parts just started going missing, like he was half here, and half somewhere else.",
                "Then his voice started to go, and ... That was that. \n\nPewter figured it all out, but it was too late for her, and Mira.",
                "Looks like the reactor ripped us a new dimension, after whatever Cas did to it, and it's attacking both us and the ship like a foreign virus.",
                "When we got to the reactor, we found her body. \n\n It disappeared though, along with the others. \n\nLooks like whatever's up there wasn't her after all."
            }
        });


        dialogText.Add("C9", new Dialog
        {
            header = "Tape 9 of 9",
            subtitle1 = "",
            subtitle2 = "",
            paragraphs = new string[] {
                "S: There's ... one hour left. Of oxygen.",
                "I checked outside, the stars are disappearing faster. \n\nLike the dimension is all coming in a big crunch. \n\nIt's hard to tell what'll get me first.",
                "According to Pewter, the only thing that could fix all this is sending the dimensional disturbance back where she came from, whatever, or whoever she is.",
                "If you're out there, Cas, or whoever you are... I'm sure you know how to fix it this time.",
                "And if not, then ... I'll ... I'll miss everyone, a whole lot. \n\nAnd I hope to see them all again, real soon."
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
