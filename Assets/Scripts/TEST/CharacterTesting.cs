using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTesting : MonoBehaviour
{
    public Character Tachibana;
    void Start()
    {
        Tachibana = CharacterManager.instance.GetCharacter("Tachibana", enableCreatedCharacterOnStart: false);
    }

    public string[] speech;
    int i = 0;
    public Vector2 moveTarget;
    public float moveSpeed;
    public bool smooth;

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
        if (Input.GetKeyDown(KeyCode.M))
        {
            Tachibana.MoveTo(moveTarget, moveSpeed, smooth);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Tachibana.StopMoving(true);
        }
    }
}
