using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameFile
{
    public string chapterName = "chapter_start";
    public int chapterProgress = 0;

    public string cachedLastSpeaker = string.Empty;

    public string currentTextSystemSpeakerDisplayText = string.Empty;
    public string currentTextSystemDisplayText = string.Empty;

    public List<CHARACTERDATA> charactersInScene = new List<CHARACTERDATA>();

    public Texture background = null;
    public Texture foreground = null;

    public AudioClip music = null;

    public GameFile()
    {
        this.chapterName = "chapter_start";
        this.chapterProgress = 0;
        this.cachedLastSpeaker = string.Empty;
        charactersInScene = new List<CHARACTERDATA>();
    }

    [System.Serializable]
    public class CHARACTERDATA
    {
        public string characterName;
        public bool enabled = true;
        public string bodyExpression = "";
        public Vector2 position = new Vector2(0.5f,0);

        public CHARACTERDATA(Character character)
        {   
            this.characterName = character.characterName;
            this.enabled = character.isVisibleInScene;
            this.bodyExpression = character.renderers.bodyRenderer.sprite.name;
            this.position = character._targetPosition;
        }
    }
}
