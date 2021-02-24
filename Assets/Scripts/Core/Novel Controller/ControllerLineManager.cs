using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerLineManager : MonoBehaviour
{   
    public static LINE Interpret(string rawLine)
    {
        return new LINE(rawLine);
    }
    public class LINE
    {
        public string speaker = "";

        public List<SEGMENT> segments = new List<SEGMENT>();
        public List<string> actions = new List<string>();

        public string lastSegmentsWholeDialogue = "";

        public LINE(string rawLine)
        {
            string[] dialogueAndActions = rawLine.Split('"');
            char actionSplitter = ' ';
            string[] actionArr = dialogueAndActions.Length == 3 ? dialogueAndActions[2].Split(actionSplitter) :
                dialogueAndActions[0].Split(actionSplitter);

            if(dialogueAndActions.Length == 3) //Contains dialogue
            {
                speaker = dialogueAndActions[0] == "" ? NovelManager.instance.cachedLastSpeaker :
                    dialogueAndActions[0];

                if(speaker[speaker.Length-1] == ' ')
                {
                    speaker = speaker.Remove(speaker.Length - 1);
                }

                //cache the speaker
                NovelManager.instance.cachedLastSpeaker = speaker;

                //segment the dialogue
                SegmentDialogue(dialogueAndActions[1]);

            }
            //now handle actions. Just capture all actions into the line
            for(int i = 0; i < actions.Count; i++)
            {
                actions.Add(actionArr[i]);
            }
        }
        
        void SegmentDialogue(string dialogue)
        {
            segments.Clear();
            string[] parts = dialogue.Split('{', '}');

            for(int i=0; i < parts.Length; i++)
            {
                SEGMENT newSegment = new SEGMENT();
                bool isOdd = i % 2 != 0;

                if (isOdd)
                {
                    string[] commandData = parts[i].Split(' ');
                    switch (commandData[0])
                    {
                        case "c": //wait for input and clear
                            newSegment.trigger = SEGMENT.TRIGGER.waitClickClear;
                            break;
                        case "a": //wait for input and append
                            newSegment.trigger = SEGMENT.TRIGGER.waitClick;
                            newSegment.pretext = segments.Count > 0 ? segments[segments.Count - 1].dialogue : "";
                            break;
                        case "w": //wait for set time and clear
                            newSegment.trigger = SEGMENT.TRIGGER.autoDelay;
                            newSegment.autoDelay = float.Parse(commandData[1]);
                            break;
                        case "wa"://wait for set time and append
                            newSegment.trigger = SEGMENT.TRIGGER.autoDelay;
                            newSegment.autoDelay = float.Parse(commandData[1]);
                            newSegment.pretext = segments.Count > 0 ? segments[segments.Count - 1].dialogue : "";
                            break;
                    }
                    i++;
                }

                newSegment.dialogue = parts[i];
                newSegment.line = this;

                segments.Add(newSegment);
            }
        }

        public class SEGMENT
        {
            public LINE line;
            public string dialogue = "";
            public string pretext = "";
            public enum TRIGGER
            {
                waitClick,
                waitClickClear,
                autoDelay
            }

            public TRIGGER trigger = TRIGGER.waitClickClear;

            public float autoDelay = 0f;

            Coroutine running = null;
            public bool isRunning { get { return running != null; } }

            public TextArchitect architect = null;

            List<string> allCurrentlyExecutedEvents = new List<string>();

            public void Run()
            {
                if (running != null)
                    NovelManager.instance.StopCoroutine(running);
                running = NovelManager.instance.StartCoroutine(Running());
            }

            IEnumerator Running()
            {
                allCurrentlyExecutedEvents.Clear();

                TagManager.Inject(ref dialogue);

                string[] parts = dialogue.Split('[', ']');

                for (int i = 0; i < parts.Length; i++)
                {
                    bool isOdd = i % 2 != 0;

                    if (isOdd)
                    {
                        DialogueEvents.HandleEvent(parts[i], this);
                        allCurrentlyExecutedEvents.Add(parts[i]);
                        i++;
                    }

                    string targDialogue = parts[i];

                    bool isIgnore = NovelManager.instance.IsSpeakerIgnore(line.speaker);

                    if (!isIgnore)
                    {
                        Character character = CharacterManager.instance.GetCharacter(line.speaker);
                        character.Say(targDialogue, i > 0 ? true : pretext != "");
                    }
                    else
                    {
                        print(line.speaker);
                        DialogueSystem.instance.Say(targDialogue, line.speaker, i > 0 ? true : pretext != "");
                    }

                    architect = DialogueSystem.instance.textArchitect;

                    while (architect.isConstructing)
                        yield return new WaitForEndOfFrame();
                }

                running = null;
            }

            public void ForceFinish()
            {
                if (running != null)
                {
                    NovelManager.instance.StopCoroutine(running);
                }

                running = null;
                if (architect != null)
                {
                    architect.ForceFinish();

                    if(pretext == "")
                        line.lastSegmentsWholeDialogue= "";

                    string[] parts = dialogue.Split('[', ']');

                    for(int i = 0; i < parts.Length; i++)
                    {
                        bool isOdd = i % 2 != 0;
                        if (isOdd)
                        {
                            string e = parts[i];
                            if (allCurrentlyExecutedEvents.Contains(e))
                            {
                                allCurrentlyExecutedEvents.Remove(e);
                            }
                            else
                            {
                                DialogueEvents.HandleEvent(e, this);
                            }
                            i++;
                        }
                        line.lastSegmentsWholeDialogue += parts[i];
                    }
                    architect.ShowText(line.lastSegmentsWholeDialogue);
                }

            }
        }
    }
}
