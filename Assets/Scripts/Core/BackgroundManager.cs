using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager instance;
    public LAYER background = new LAYER();
    public LAYER foreground = new LAYER();

    void Awake()
    {
        instance = this;   
    }

    [System.Serializable]
    public class LAYER
    {
        public GameObject root;
        public GameObject newImageObjectReference;
        public RawImage activeImage;
        public List<RawImage> allImages = new List<RawImage>();

        Coroutine transitioningTexture;
        public bool isTransitioning { get { return transitioningTexture != null; } }

        public Texture GetTexture(string nameTexture)
        {
            Texture newTexture = Resources.Load("images/UI/Backdrops/Still/" + nameTexture) as Texture;
            return newTexture;
        }

        public void SetTexture(Texture newTexture)
        {
            if (newTexture != null)
            {
                if (activeImage == null)
                    CreateNewActiveImage();
                activeImage.texture = newTexture;
                activeImage.color = GlobalFunction.SetAlpha(activeImage.color, 1f);
            }
            else
            {
                if (activeImage != null)
                {
                    allImages.Remove(activeImage);
                    GameObject.DestroyImmediate(activeImage.gameObject);
                    activeImage = null;
                }
            }
        }

        public void TransitionToTexture(Texture newTexture, float speed = 2f, bool smooth = false)
        {   
            if(activeImage!=null && activeImage.texture == newTexture)
            {
                return;
            }
            StopTransition();
            transitioningTexture = BackgroundManager.instance.StartCoroutine(TransitionTexture(newTexture,speed,smooth));

        }

        void StopTransition()
        {
            if (isTransitioning)
                BackgroundManager.instance.StopCoroutine(transitioningTexture);
            transitioningTexture = null;
        }
        
        IEnumerator TransitionTexture(Texture newTexture, float speed, bool smooth)
        {   
            if(newTexture != null)
            {
                for (int i = 0; i < allImages.Count; i++)
                {
                    RawImage newImage = allImages[i];
                    if (newImage.texture == newTexture)
                    {
                        activeImage = newImage;
                        break;
                    }
                }

                if (activeImage == null || activeImage.texture != newTexture)
                {
                    CreateNewActiveImage();
                    activeImage.texture = newTexture;
                    activeImage.color = GlobalFunction.SetAlpha(activeImage.color, 0f);
                }
            }
            else
            {
                activeImage = null;
            }

            while (GlobalFunction.TransitionRawImages(ref activeImage, ref allImages, speed, smooth))
                yield return new WaitForEndOfFrame();

            StopTransition();
        }

        void CreateNewActiveImage()
        {
            GameObject ob = Instantiate(newImageObjectReference, root.transform) as GameObject;
            ob.SetActive(true);
            RawImage raw = ob.GetComponent<RawImage>();
            activeImage = raw;
            allImages.Add(raw);
        }
    }
    public class BACKGROUNDSSTILL
    {
        public string classroom = "Classroom";
    }

    public static BACKGROUNDSSTILL backgroundStill = new BACKGROUNDSSTILL();
}
