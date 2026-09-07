using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[System.Serializable]
public struct SongData
{
    public AudioClip clip;
    public string title;
    public string artist;
}

[RequireComponent(typeof(AudioSource))]
public class PlaylistManager : MonoBehaviour
{
    public static PlaylistManager Instance { get; private set; }

    [Header("Configuración de Lista")]
    private List<SongData> playlist = new List<SongData>();
    [SerializeField] private int indiceActual = 0;
    [SerializeField] private bool aleatorio = true;

    /*[Header("UI Toolkit Integration")]
    [Tooltip("Nombre del Label en el UXML donde se mostrará la canción")]
    [SerializeField] private string songInfoLabelName = "SongInfoLabel";*/

    //[Header("Configuración de Escenas")]
    [Tooltip("Nombre de la escena de partido donde NO debe sonar la música")]
    [SerializeField] private List<string> matchSceneName = new List<string>();

    private AudioSource audioSource;
    private bool isPausedByScene = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
    }

    private void Start()
    {
        // Carga dinámica desde Assets/Resources/Music/Menu
        LoadSongsFromResources();

        if (aleatorio)
        {
            MixPlaylist();
        }

        if (playlist.Count > 0 && !isPausedByScene)
        {
            PlaySong(0);
        }
    }

    private void Update()
    {
        if (!audioSource.isPlaying && playlist.Count > 0 && !isPausedByScene)
        {
            NextSong();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLoadingScene;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLoadingScene;
    }

    private void OnLoadingScene(Scene scene, LoadSceneMode modo)
    {
        if (matchSceneName.Contains(scene.name))
        {
            isPausedByScene = true;
            audioSource.Pause();
            return;
        }

        if (isPausedByScene)
        {
            isPausedByScene = false;
            NextSong();
        }
        else if (audioSource.isPlaying)
        {
            NextSong();
        }
        else
        {
            PlaySong(indiceActual);
        }
        
        StopAllCoroutines();
        //StartCoroutine(UpdateUIWithDelay());

    }

    public void PlaySong(int indice)
    {
        if (playlist.Count == 0) return;

        indiceActual = indice % playlist.Count;
        audioSource.clip = playlist[indiceActual].clip;
        audioSource.Play();

        //UpdateUIInCurrentScene();
    }

    public void NextSong()
    {
        if (playlist.Count == 0) return;

        int nextIndex = aleatorio ? Random.Range(0, playlist.Count) : (indiceActual + 1) % playlist.Count;
        PlaySong(nextIndex);
    }

    private void LoadSongsFromResources()
    {
        playlist.Clear();
        AudioClip[] loadedClips = Resources.LoadAll<AudioClip>("Music");

        foreach (var clip in loadedClips)
        {
            string artist = "Artista Desconocido";
            string title = clip.name;

            // Parsea el nombre con formato "Artista - Titulo"
            if (clip.name.Contains("-"))
            {
                string[] partes = clip.name.Split('-');
                artist = partes[0].Trim();
                title = partes[1].Trim();
            }

            playlist.Add(new SongData
            {
                clip = clip,
                title = title,
                artist = artist
            });
        }
    }

    private void MixPlaylist()
    {
        for (int i = 0; i < playlist.Count; i++)
        {
            SongData temp = playlist[i];
            int randomIndex = Random.Range(i, playlist.Count);
            playlist[i] = playlist[randomIndex];
            playlist[randomIndex] = temp;
        }
    }

    /*private System.Collections.IEnumerator UpdateUIWithDelay()
    {
        // Reintentar buscar el Label durante un máximo de 2 segundos hasta que el UIDocument esté cargado
        float timeout = 2f;
        float elapsed = 0f;
        Label songLabel = null;

        while (songLabel == null && elapsed < timeout)
        {
            yield return null; // Espera al siguiente frame

            UIDocument activeUIDoc = Object.FindFirstObjectByType<UIDocument>();

            if (activeUIDoc != null)
            {
                Debug.Log("UIDocument encontrado: " + activeUIDoc.gameObject.name);
            }

            if (activeUIDoc != null && activeUIDoc.rootVisualElement != null)
            {
                songLabel = activeUIDoc.rootVisualElement.Q<Label>(songInfoLabelName);

                Debug.Log(songLabel == null
                    ? "No encontré SongInfoLabel"
                    : "SongInfoLabel encontrado");
            }

            elapsed += Time.deltaTime;
        }

        // Si lo encontró, asigna el texto de la canción
        if (songLabel != null && playlist.Count > 0)
        {
            SongData currentSong = playlist[indiceActual];
            songLabel.text = $"{currentSong.artist} - {currentSong.title}";
        }
    }*/

    /*private void UpdateUIInCurrentScene()
    {
        UIDocument activeUIDoc = Object.FindFirstObjectByType<UIDocument>();
        if (activeUIDoc == null) return;

        Label songLabel = activeUIDoc.rootVisualElement.Q<Label>(songInfoLabelName);
        if (songLabel != null && playlist.Count > 0)
        {
            SongData currentSong = playlist[indiceActual];
            // Muestra: Artista - Título
            songLabel.text = $"{currentSong.artist} - {currentSong.title}";
        }
    }*/
}
