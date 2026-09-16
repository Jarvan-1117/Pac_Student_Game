using UnityEngine;

public class MusicSequenceController : MonoBehaviour
{
    [SerializeField] private AudioClip normalMusic;

    private AudioSource musicSource;
    private float elapsedTime;
    private bool switchedToNormal;

    void Start()
    {
        musicSource = GetComponent<AudioSource>();

    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        bool introFinished = !musicSource.isPlaying;
        bool reachedTimeLimit = elapsedTime >= 3f;

        if (!switchedToNormal && (introFinished || reachedTimeLimit))
        {
            musicSource.clip = normalMusic;
            musicSource.loop = true;
            musicSource.Play();

            switchedToNormal = true;
        }

    }
}