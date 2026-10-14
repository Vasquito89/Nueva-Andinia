using UnityEngine;

public class UIMenuAudioManager : MonoBehaviour
{

    public static UIMenuAudioManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioClip okClip;
    [SerializeField] private AudioClip errClip;
    [SerializeField] private AudioClip openPanelClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("Se instanceo UIMenuAudioManager");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayClick()
    {
        if (uiSource != null && okClip != null)
        {
            uiSource.PlayOneShot(okClip);
        }
    }

    public void PlayPanelOpen()
    {
        if (uiSource != null && openPanelClip != null)
        {
            uiSource.PlayOneShot(openPanelClip);
        }
    }

    public void ErrorPlay()
    {
        if (uiSource != null && errClip != null)
        {
            uiSource.PlayOneShot(errClip);
        }
    }

    public void TogglePauseMenu(bool isPaused)
    {
        if (isPaused)
        {
            PlayPanelOpen();
        }
        else
        {
            PlayClick();
        }
    }
}