using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using static Callbacks;


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public static SONG activeSong = null;
    public static List<SONG> allSongs = new List<SONG>();

    public float songTransitionSpeed = 2f;
    public bool songSmoothTransitions = true;

    public AudioMixer masterMixer;

    public Slider musicSlider;
    public Slider sfxSlider;

    public TextMeshProUGUI musicText;
    public TextMeshProUGUI sfxText;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AnimationCurve volumeCurve;

    public Animator TransitionMusic;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    public void SetSlidersAndText(Slider _musicSlider, Slider _sfxSlider,TextMeshProUGUI _musicTextVolume,TextMeshProUGUI _sfxTextVolume)
    {
        musicSlider = _musicSlider;
        sfxSlider = _sfxSlider;
        musicText = _musicTextVolume;
        sfxText = _sfxTextVolume;

        musicSlider.onValueChanged.AddListener(delegate { SetMusicVolume(); });
        sfxSlider.onValueChanged.AddListener(delegate { SetSfxVolume(); });

        LoadVolume();
    }

    public void SetMusicVolume()
    {
        masterMixer.SetFloat("MusicVolume", -80f + (80f * volumeCurve.Evaluate(musicSlider.value)));
        //masterMixer.SetFloat("MusicVolume", -80f + (80f * musicSlider.value));
        musicText.text = $"{(Mathf.RoundToInt(musicSlider.value * 100f))}%";
    }

    public void SetSfxVolume()
    {
        masterMixer.SetFloat("SfxVolume", -80f + (80f * volumeCurve.Evaluate(sfxSlider.value)));
        //masterMixer.SetFloat("SfxVolume", -80f + (80f * sfxSlider.value));
        sfxText.text = $"{(Mathf.RoundToInt(sfxSlider.value * 100f))}%";
    }
    
    public void SaveVolume()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SfxVolume", sfxSlider.value);
    }

    public void LoadVolume()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        if (PlayerPrefs.HasKey("SfxVolume"))
            sfxSlider.value = PlayerPrefs.GetFloat("SfxVolume");
    }

    public void PlaySfx(AudioClip effect,float volume=1f,float pitch = 1f)
    {
        //AudioSource source = CreateNewSource(string.Format("SFX [{0}]", effect.name));
        sfxSource.clip = effect;
        sfxSource.volume = volume;
        sfxSource.pitch = pitch;
        sfxSource.Play();

        //Destroy(source.gameObject, effect.length);
    }

    public void PlayMusic(AudioClip song, float maxVolume=1f, float maxPitch=1f,
        float startingVolume=1f,bool playOnStart = true, bool isLoop=true)
    {
        if (song != null)
        {
            for (int i = 0; i < allSongs.Count; i++)
            {
                SONG s = allSongs[i];
                if (s.clip == song)
                {
                    activeSong = s;
                    break;
                }
            }
            if(activeSong == null || activeSong.clip != song)
            {
                activeSong = new SONG(TransitionMusic, musicSource, song, maxVolume, maxPitch, startingVolume, playOnStart, isLoop);
            }
                
        }
        else
            activeSong = null;

        StopAllCoroutines();
        StartCoroutine(VolumeLeveling());
    }

    IEnumerator VolumeLeveling()
    {
        while (TransitionSongs())
            yield return new WaitForEndOfFrame();
    }

    private bool TransitionSongs()
    {
        bool anyValueChanged = false;

        float speed = songTransitionSpeed* Time.deltaTime;

        for (int i = allSongs.Count - 1; i >= 0; i--)
        {
            SONG song = allSongs[i];

            if (song == activeSong)
            {
                if (song.volume < 1f)
                {
                    song.volume = 1f;
                    /*song.volume = songSmoothTransitions ? Mathf.Lerp(0f, 1f, speed) :
                    Mathf.MoveTowards(0f, 1f, speed);*/
                    anyValueChanged = true;
                }

            }
            else
            {
                if (song.volume > 0)
                {
                    song.volume = 1f;
                    /*song.volume = songSmoothTransitions ? Mathf.Lerp(1f, 0f, speed) :
   Mathf.MoveTowards(1f, 0f, speed);*/
                    anyValueChanged = true;
                }

                else
                {
                    allSongs.RemoveAt(i);
                    song.DestroySong();
                    continue;
                }
            }
        }

        return anyValueChanged;
    }

    public static AudioSource CreateNewSource(string _name)
    {
        AudioSource newResource = new GameObject(_name).AddComponent<AudioSource>();
        newResource.transform.SetParent(instance.transform);
        return newResource;
    }

    [System.Serializable]
    public class SONG
    {
        public AudioSource source;
        public Animator Transition;
        public AudioClip clip { get { return source.clip; } set { source.clip = value; } }
        public float maxVolume = 1f;
        public float volume { get { return source.volume; } set { source.volume = value; } }
        public float pitch { get { return source.pitch; } set { source.pitch = value; } }
        public SONG(Animator newTransition,AudioSource newSource,AudioClip newClip, float newMaxVolume, float newPitch, float startVolume, bool playOnStart, bool isNewLoop)
        {
            //source = AudioManager.CreateNewSource(string.Format("SONG [{0}]", newClip.name));
            source = newSource;
            source.clip = newClip;
            source.volume = startVolume;
            maxVolume = newMaxVolume;
            source.pitch = newPitch;
            source.loop = isNewLoop;
            Transition = newTransition;

            AudioManager.allSongs.Add(this);

            /*if(source.clip != null)
            {
           
                print("Entro");

                AudioManager.instance.StartCoroutine(TransitionNewMusic(newClip,()=> {
                    //AudioManager.instance.StopAllCoroutines();
                }));
                //source.clip = newClip;
                //source.Play();
            }
            else
            {*/
                //source.clip = newClip;
                if (playOnStart)
                    source.Play();
            //}

        }

        public void Play()
        {
            source.Play();
        }

        public void Stop()
        {
            source.Stop();
        }

        public void Pause()
        {
            source.Pause();
        }

        public void UnPause()
        {
            source.UnPause();
        }

        public void DestroySong()
        {
            AudioManager.allSongs.Remove(this);
            //Destroy(source.gameObject);
        }

        IEnumerator TransitionNewMusic(AudioClip newClip,OnComplete onComplete = null)
        {
            float currentTime = 0;
            float start = source.volume;

            while (currentTime < 0.5f)
            {
                currentTime += Time.deltaTime;
                float newVol = Mathf.Lerp(start, 0f, 2f*Time.deltaTime);
                AudioManager.instance.masterMixer.SetFloat("MusicVolume", Mathf.Log10(newVol) * 20);
                yield return null;
            }
            yield break;
        }
    }
}
