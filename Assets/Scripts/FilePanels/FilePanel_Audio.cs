using Leap.Unity.Interaction;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class FilePanel_Audio : FilePanel
{
    public Text txtTotalTime;
    public Text txtCurrentTime;

    private InteractionButton btnPlay;
    private AudioSource audioSource;
    private AudioClip audioClip;
    
    public override void Initialize()
    {
        base.Initialize();
        audioSource = GetComponent<AudioSource>();
        btnPlay = GetComponentInChildren<InteractionButton>();

        btnPlay.OnContactEnd += ToggleAudio;
    }

    void Update()
    {
        if (audioSource == null)
        {
            return;
        }
        if (audioSource.isPlaying)
        {
            UpdateAudioTimes();
        }
    }

    private void UpdateAudioTimes()
    {
        txtCurrentTime.text = audioSource.time.ToString();
        txtTotalTime.text = audioClip.length.ToString();
    }

    private void SetAudioClip(AudioClip audioClip)
    {
        this.audioClip = audioClip;
        audioSource.clip = audioClip;
    }

    public void ToggleAudio()
    {
        if (audioClip == null)
        { return; }

        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.Play();
        }
    }

    public void LoadAudioFromPath(string name, string path, Vizlab.AUDIO_TYPE audioType)
    {
        switch (audioType)
        {
            case Vizlab.AUDIO_TYPE.MP3:
                //StartCoroutine(LoadAudioFile(name, path, AudioType.MPEG));
                break;
            case Vizlab.AUDIO_TYPE.WAV:
                StartCoroutine(LoadAudioFile(name, path, AudioType.WAV));
                break;
            default:
                Debug.Log("Unknown audio type!");
                break;
        }
    }
   
    IEnumerator LoadAudioFile(string name, string path, AudioType type)
    {
        Debug.Log("Start to load audio file " + name);
        Debug.Log("Path " + path);
        
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, type))
        {
            yield return www.SendWebRequest();
            
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Received audio");

                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                audioClip.name = name;
                SetAudioClip(audioClip);

                Debug.Log("Loaded: " + path);
            }
        }
    }
}
