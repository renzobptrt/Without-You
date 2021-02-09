using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTesting : MonoBehaviour
{
    public Character Tachibana;

    public string[] speech;
    int i = 0;
    public Vector2 moveTarget;
    public float moveSpeed;
    public bool smooth;

    public string newEmotion = "Normal";
    public float speed = 5f;
    public bool smoothTransitions = false;
    void Start()
    {
        Tachibana = CharacterManager.instance.GetCharacter("Tachibana", enableCreatedCharacterOnStart: false);
        
    }

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
        if (Input.GetKeyDown(KeyCode.T))
            Tachibana.TransitionBody(Tachibana.GetSprite(CharacterManager.characterExpressions.shocked), speed, smooth);
        if (Input.GetKeyDown(KeyCode.E))
        {
            Tachibana.SetNewEmotion(CharacterManager.characterExpressions.normal);
        }
    }
}
