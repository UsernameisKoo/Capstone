using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public GameObject animatable;
    public SpriteRenderer switchPokeball;
    public AudioSource audioSource;

    public Animator Animator { get; set; }
    public SpriteRenderer Renderer { get; set; }
    public RectTransform RectTransform { get; set; }
    public Vector3 OriginalScale { get; set; }
    public Pokemon Pokemon { get; set; }

    [Header("👉 직접 사용할 스프라이트")]
    public Sprite customSprite;   // 🔥 이거 추가

    public void Setup(Pokemon pokemon, bool switchedIn = false)
    {
        Pokemon = pokemon;

        Animator = animatable.GetComponent<Animator>();
        Renderer = animatable.GetComponent<SpriteRenderer>();
        RectTransform = animatable.GetComponent<RectTransform>();
        OriginalScale = RectTransform.localScale;

        switchPokeball.enabled = false;

        if (switchedIn)
            RectTransform.localScale = new Vector3(0, 0, RectTransform.localScale.z);

        audioSource.clip = pokemon.Skeleton.cry;

        // ❗ Animator 끄기 (덮어쓰기 방지)
        if (Animator != null)
        {
            Animator.enabled = false;
        }

        // 🔥 Sprite 직접 적용
        if (customSprite != null)
        {
            Renderer.sprite = customSprite;
        }

        Renderer.color = new Color(Renderer.color.r, Renderer.color.g, Renderer.color.b, 1f);
    }

    public void PlayCry()
    {
        audioSource.pitch = 1f;
        audioSource.volume = 0.8f;
        audioSource.Play();
    }

    public void PlayEnterCry()
    {
        audioSource.pitch = 1f;
        audioSource.volume = 0.8f;
        StartCoroutine(PlayCryWithDelay(1f));
    }

    private IEnumerator PlayCryWithDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        audioSource.Play();
    }

    public void PlayFaintCry()
    {
        audioSource.pitch = 0.8f;
        audioSource.volume = 0.8f;
        audioSource.Play();
    }

    public string Name { get { return Pokemon.Skeleton.pokemonName; } }
    public Move[] Moves { get { return Pokemon.Moves; } }
}