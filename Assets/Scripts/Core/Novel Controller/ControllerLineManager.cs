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

            public void Run()
            {
                if (running != null)
                    NovelManager.instance.StopCoroutine(running);
                running = NovelManager.instance.StartCoroutine(Running());
            }

            IEnumerator Running()
            {
                if(line.speaker != "narrator")
                {
                    Character character = CharacterManager.instance.GetCharacter(line.speaker);
                    character.Say(dialogue, pretext != "");
                }
                else
                {
                    DialogueSystem.instance.Say(dialogue, line.speaker, pretext != "");
                }

                architect = DialogueSystem.instance.textArchitect;

                while (architect.isConstructing)
                    yield return new WaitForEndOfFrame();

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
                    architect.ForceFinish();
            }
        }
    }
}
