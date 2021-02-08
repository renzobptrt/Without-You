using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Character
{
    public string characterName;
    public RectTransform root;
    public Renderers renderers = new Renderers();
    public bool isMultiLayerCharacter { get { return renderers.renderer == null; } }
    public bool enabled { get { return root.gameObject.activeInHierarchy; } set { root.gameObject.SetActive(value); } }
    DialogueSystem dialogue;
    public void Say(string speech, bool add = false)
    {   if (!enabled)
            enabled = true;
        if (!add)
            dialogue.Say(speech, characterName);
        else
            dialogue.SayAdd(speech, characterName);
    }

    public Character(string _characterName, bool enableOnStart = true)
    {
        CharacterManager cn = CharacterManager.instance;
        GameObject prefab = Resources.Load("Characters/Character[" + _characterName + "]") as GameObject;
        GameObject ob = GameObject.Instantiate(prefab, cn.characterPanel);
        root = ob.GetComponent<RectTransform>();
        this.characterName = _characterName;
        
        // Get the renderers
        renderers.renderer = ob.GetComponentInChildren<RawImage>();
        if (isMultiLayerCharacter)
        {
            renderers.bodyRenderer = ob.transform.Find("bodyLayer").GetComponent<Image>();
        }

        dialogue = DialogueSystem.instance;

        enabled = enableOnStart;
    }
    [System.Serializable]
    public class Renderers
    {
        public RawImage renderer;
        public Image bodyRenderer;
    }
}
