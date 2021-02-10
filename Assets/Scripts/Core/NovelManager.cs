﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovelManager : MonoBehaviour
{

    List<string> data = new List<string>();

    int progress = 0;
    string cachedLastSpeaker = "";

    void Start()
    {
        LoadChapterFile("chapter0_start");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            HandleLine(data[progress]);
            progress++;
        }
    }

    public void LoadChapterFile(string fileName)
    {
        data = FileManager.LoadFile(FileManager.savPath + "Resources/Story/"+fileName);
        progress = 0;
        cachedLastSpeaker = "";
    }

    void HandleLine(string line)
    {
        string[] dialogueAndActions = line.Split('"');

        if(dialogueAndActions.Length == 3)
        {
            HandleDialogue(dialogueAndActions[0], dialogueAndActions[1]);
            HandleEventsFromLine(dialogueAndActions[2]);
        }
        else
        {
            HandleEventsFromLine(dialogueAndActions[0]);
        }
    }

    void HandleDialogue(string dialogueDetails,string dialogue)
    {
        string speaker = cachedLastSpeaker;
        bool additive = dialogueDetails.Contains("+");
        if (additive)
            dialogueDetails = dialogueDetails.Remove(dialogueDetails.Length - 1);
        if (dialogueDetails.Length > 0)
        {
            if (dialogueDetails[dialogueDetails.Length - 1] == ' ')
            {
                dialogueDetails = dialogueDetails.Remove(dialogueDetails.Length - 1);
            }
            speaker = dialogueDetails;
            cachedLastSpeaker = speaker;
        }

        if(speaker != "narrator")
        {
            Character character = CharacterManager.instance.GetCharacter(speaker);
            character.Say(dialogue, additive);
        }
        else
        {
            DialogueSystem.instance.Say(dialogue, speaker, additive);
        }
    }

    void HandleEventsFromLine(string events)
    {

        string[] actions = events.Split(' ');
        foreach(string action in actions)
        {
            HandleAction(action);
        }
    }

    void HandleAction(string action)
    {
        print("Accion: " + action);
        string[] data = action.Split('(', ')');
        if (data[0] == "setBackground")
        {
            Command_SetLayerImage(data[1], BackgroundManager.instance.background);
            return;
        }
        if (data[0] == "setForeground")
        {
            Command_SetLayerImage(data[1], BackgroundManager.instance.foreground);
            return;
        }
        if (data[0] == "playSfx")
        {
            Command_PlaySfx(data[1]);
            return;
        }
        if (data[0] == "playMusic")
        {
            Command_PlayMusic(data[1]);
            return;
        }
        if (data[0] == "moveCharacter")
        {
            Command_MoveCharacter(data[1]);
            return;
        }
        if (data[0] == "setPosition")
        {
            Command_SetPosition(data[1]);
            return;
        }
        if (data[0] == "setExpression")
        {
            Command_ChangeExpression(data[1]);
            return;
        }
    }

    void Command_SetLayerImage(string data,BackgroundManager.LAYER layer)
    {
        string textureName = data.Contains(",") ? data.Split(',')[0] : data;
        Texture2D tex = textureName == "null" ? null : Resources.Load("images/UI/Backdrops/" + textureName) as Texture2D;
        float spd = 2f;
        bool smooth = false;

        if (data.Contains(","))
        {
            string[] parameters = data.Split(',');
            foreach(string p in parameters)
            {
                float fVal = 0;
                bool bVal = false;
                
                if(float.TryParse(p,out fVal))
                {
                    spd = fVal; continue;
                }
                if(bool.TryParse(p, out bVal))
                {
                    smooth = bVal; continue;
                }
            }
        }

        layer.TransitionToTexture(tex, spd, smooth);
    }

    void Command_PlaySfx(string data)
    {
        AudioClip clip = Resources.Load("Audio/SFX/" + data) as AudioClip;
        if (clip != null)
            AudioManager.instance.PlaySfx(clip);
        else
            Debug.LogError("Clip does not exits: " + data);
    }

    void Command_PlayMusic(string data)
    {
        AudioClip clip = Resources.Load("Audio/Music/" + data) as AudioClip;
        if (clip != null)
            AudioManager.instance.PlayMusic(clip);
        else
            Debug.LogError("Clip does not exits: " + data);
    }

    void Command_MoveCharacter(string data)
    {
        string[] parameters = data.Split(',');
        string character = parameters[0];
        float locationX = float.Parse(parameters[1]);
        float locationY = float.Parse(parameters[2]);
        float speed = parameters.Length >= 4 ? float.Parse(parameters[3]) : 1f;
        bool smooth = parameters.Length == 5 ? bool.Parse(parameters[4]) : true;
        Character c = CharacterManager.instance.GetCharacter(character);
        c.MoveTo(new Vector2(locationX, locationY), speed,smooth);
    }
    void Command_SetPosition(string data)
    {
        string[] parameters = data.Split(',');
        string character = parameters[0];
        float locationX = float.Parse(parameters[1]);
        float locationY = float.Parse(parameters[2]);
        Character c = CharacterManager.instance.GetCharacter(character);
        c.SetPosition(new Vector2(locationX, locationY));
    }
    void Command_ChangeExpression(string data)
    {
        string[] parameters = data.Split(',');
        string character = parameters[0];
        string region = parameters[1];
        string expression = parameters[2];
        float speed = parameters.Length == 4 ? float.Parse(parameters[3]) : 1f;

        Character c = CharacterManager.instance.GetCharacter(character);
        Sprite sprite = c.GetSprite(expression);

        if(region.ToLower() == "body")
        {
            //c.SetNewEmotion(expression);
            c.TransitionBody(sprite, speed,false);
        }
    }
}
