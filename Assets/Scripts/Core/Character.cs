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
    private Vector2 targetPosition;
    private Coroutine moving;
    public Vector2 anchorPadding { get { return root.anchorMax - root.anchorMin; } }
    private bool isMoving { get { return moving != null; } }

    public void Say(string speech, bool add = false)
    {   if (!enabled)
            enabled = true;
        if (!add)
            dialogue.Say(speech, characterName);
        else
            dialogue.SayAdd(speech, characterName);
    }

    public void MoveTo(Vector2 target, float speed, bool smoth)
    {
        StopMoving();
        moving = CharacterManager.instance.StartCoroutine(Moving(target, speed, smoth));
    }

    public void StopMoving(bool arriveAtTargetPositionInmediately = false)
    {
        if (isMoving)
        {
            CharacterManager.instance.StopCoroutine(moving);
            if (arriveAtTargetPositionInmediately) SetPosition(targetPosition);
        }
        moving = null;
    }

    public void SetPosition(Vector2 target)
    {
        targetPosition = target;
        Vector2 padding = anchorPadding;
        float maxX = 1f - padding.x;
        float maxY = 1f - padding.y;

        Vector2 minAnchorTarget = new Vector2(maxX * targetPosition.x, maxY * targetPosition.y);

        root.anchorMin = minAnchorTarget;
        root.anchorMax = root.anchorMin + padding;
    }

    IEnumerator Moving(Vector2 target, float speed, bool smoth)
    {
        targetPosition = target;
        Vector2 padding = anchorPadding;
        float maxX = 1f - padding.x;
        float maxY = 1f - padding.y;

        Vector2 minAnchorTarget = new Vector2(maxX * targetPosition.x, maxY * targetPosition.y);
        speed *= Time.deltaTime;
        while(root.anchorMin != minAnchorTarget)
        {
            root.anchorMin = (!smoth) ? Vector2.MoveTowards(root.anchorMin, minAnchorTarget, speed) : Vector2.Lerp(root.anchorMin,minAnchorTarget,speed);
            root.anchorMax = root.anchorMin + padding;
            yield return new WaitForEndOfFrame();
        }
        StopMoving();
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
