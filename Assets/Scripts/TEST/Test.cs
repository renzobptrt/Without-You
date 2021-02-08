using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    DialogueSystem dialogue;
    public string[] s = new string[]
    {
        "I...: Avira",
        "It's lovely weather today",
        "To be honest..."
    };

    int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        dialogue = DialogueSystem.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!dialogue._isSpeaking || dialogue._isWaitingForUserInput)
            {
                if (index >= s.Length)
                {
                    return;
                }
                Say(s[index]);
                index++;
            }
        }

    }

    void Say(string s)
    {
        string[] parts = s.Split(':');
        string speech = parts[0];
        string speaker = (parts.Length >= 2) ? parts[1] : "";
        dialogue.Say(speech, speaker);
    }
}
