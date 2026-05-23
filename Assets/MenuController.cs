using System.Collections;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public AudioClip bgmClip;
    public AudioClip pressSpaceSfx;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private bool isStarted = false;

    void Start()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.clip = bgmClip;
        bgmSource.loop = true;
        bgmSource.Play();

        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (!isStarted && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(PlayStartSound());
        }
    }

    IEnumerator PlayStartSound()
    {
        isStarted = true;

        bgmSource.Stop();
        sfxSource.PlayOneShot(pressSpaceSfx);

        yield return new WaitForSeconds(pressSpaceSfx.length);

        // 여기에는 원래 네가 구현한 다음 씬 이동 코드 넣기
    }
}