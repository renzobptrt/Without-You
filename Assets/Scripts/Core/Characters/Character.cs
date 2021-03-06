﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Character
{
    public string characterName;
    public RectTransform root;
    public Renderers renderers = new Renderers();
    public bool enabled { get { return root.gameObject.activeInHierarchy; } set { root.gameObject.SetActive(value); visibleInScene = value; } }
    DialogueSystem dialogue;
    [SerializeField] private Vector2 targetPosition = new Vector2(0.5f,0);
    private Coroutine moving;
    private Coroutine transitioningBody;
    private Sprite lastBodySprite = null;
   
    public Vector2 anchorPadding { get { return root.anchorMax - root.anchorMin; } }
    private bool isMoving { get { return moving != null; } }
    private bool isTransitioningBody { get { return transitioningBody != null; } }

    public void Say(string speech, bool add = false)
    {   
        if (!enabled)
            enabled = true;
      
         dialogue.Say(speech, characterName,add);
    }

    public Vector2 _targetPosition
    {
        get { return targetPosition; }
    }

    public void MoveTo(Vector2 target, float speed, bool smoth = true)
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

    public bool isVisibleInScene
    {
        get { return visibleInScene; }
    }
    bool visibleInScene = true;

    public void FadeOut(float speed = 3, bool smooth = false)
    {
        Sprite alphaSprite = Resources.Load<Sprite>("images/AlphaOnly");
        lastBodySprite = renderers.bodyRenderer.sprite;

        TransitionBody(alphaSprite, speed, smooth);
        visibleInScene = false;
    }

    public void FadeIn(float speed = 3, bool smooth = false)
    {
        if(lastBodySprite != null)
        {
            TransitionBody(lastBodySprite, speed, smooth);
            if (enabled)
                visibleInScene = true;
        }
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
    
    //Begin Transition Images

    public Sprite GetSprite(string status)
    {
        Sprite newSprite = Resources.Load<Sprite>("Images/Characters/" + characterName + "/" + characterName + "_" + status);
        return newSprite;
    }

    public void SetNewEmotion(string status)
    {
        if (status == "AlphaOnly")
            SetNewEmotion(Resources.Load<Sprite>("images/AlphaOnly"));
        else
            renderers.bodyRenderer.sprite = GetSprite(status);
    }

    public void SetNewEmotion(Sprite newSprite)
    {
        renderers.bodyRenderer.sprite = newSprite;
    }

    public void TransitionBody(Sprite newSprite, float speed, bool isSmooth = true)
    {
        StopTransitionBody();
        transitioningBody = CharacterManager.instance.StartCoroutine(TransitioningBody(newSprite,speed,isSmooth));
    }

    private void StopTransitionBody()
    {
        if (isTransitioningBody)
            CharacterManager.instance.StopCoroutine(transitioningBody);
        transitioningBody = null;
    }

    public IEnumerator TransitioningBody(Sprite newSprite,float speed, bool isSmooth)
    {
        for(int i=0; i < renderers.allBodyRenderers.Count; i++)
        {
            Image newImage = renderers.allBodyRenderers[i];
            if(newImage.sprite == newSprite)
            {
                renderers.bodyRenderer = newImage;
                break;
            }
        }

        if(renderers.bodyRenderer.sprite != newSprite)
        {
            Image image = GameObject.Instantiate(renderers.bodyRenderer.gameObject,
                            renderers.bodyRenderer.transform.parent).GetComponent<Image>();
            renderers.allBodyRenderers.Add(image);
            renderers.bodyRenderer = image;
            image.color = GlobalFunction.SetAlpha(image.color, 0f);
            image.sprite = newSprite;
        }

        while (GlobalFunction.TransitionImages(ref renderers.bodyRenderer, ref renderers.allBodyRenderers, speed, isSmooth,true))
            yield return new WaitForEndOfFrame();

        StopTransitionBody();
    }

    //End Transition Images

    // Create new Character
    public Character(string _characterName, bool enableOnStart = true)
    {
        CharacterManager cn = CharacterManager.instance;
        GameObject prefab = Resources.Load("Characters/Character[" + _characterName + "]") as GameObject;
        GameObject ob = GameObject.Instantiate(prefab, cn.characterPanel);
        root = ob.GetComponent<RectTransform>();
        this.characterName = _characterName;
        
        // Get the renderers
        renderers.bodyRenderer = ob.transform.Find("BodyLayer").GetComponentInChildren<Image>();
        renderers.allBodyRenderers.Add(renderers.bodyRenderer);

        dialogue = DialogueSystem.instance;

        enabled = enableOnStart;
        visibleInScene = enabled;
    }
    [System.Serializable]
    public class Renderers
    {
        public Image bodyRenderer;

        public List<Image> allBodyRenderers = new List<Image>();
    }
}
