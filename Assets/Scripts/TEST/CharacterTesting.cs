using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTesting : MonoBehaviour
{
    public Character Tachibana;
    void Start()
    {
        Tachibana = CharacterManager.instance.GetCharacter("Tachibana", enableCreatedCharacterOnStart:false);
    }

    public string[] speech;
    int i = 0;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (i < speech.Length)
            {
                Tachibana.Say(speech[i]);
            }
            else
            {
                DialogueSystem.instance.Close();
            }
            i++;
        }
    }
}
