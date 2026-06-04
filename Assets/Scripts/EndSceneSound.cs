using UnityEngine;

public class EndSceneSound : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private AudioClip endSound;
    [SerializeField] private float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void Start()
    {
        if (endSound != null)
        {
            audioSource.PlayOneShot(endSound, volume);
        }
    }
}