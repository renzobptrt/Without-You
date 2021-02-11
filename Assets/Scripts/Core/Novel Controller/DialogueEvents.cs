using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueEvents : MonoBehaviour
{
    public static void HandleEvent(string _event, ControllerLineManager.LINE.SEGMENT segment)
    {
        if (_event.Contains("("))
        {
            string[] actions = _event.Split(' ');

            for (int i = 0; i < actions.Length; i++)
            {
                NovelManager.instance.HandleAction(actions[i]);
            }
            return;
        }

        string[] eventData = _event.Split(' ');

        switch (eventData[0])
        {
            case "txtSpd":
                EVENT_TxtSpd(eventData[1],segment);
                break;
            case "/txtSpd":
                segment.architect.speed = 1f;
                segment.architect.charactersPerFrame = 1;
                break;
        }
    }

    public static void EVENT_TxtSpd(string data, ControllerLineManager.LINE.SEGMENT seg)
    {
        string[] parts = data.Split(',');
        float delay = float.Parse(parts[0]);
        int characterPerFrame = int.Parse(parts[1]);

        seg.architect.speed = delay;
        seg.architect.charactersPerFrame = characterPerFrame;
    } 
}
