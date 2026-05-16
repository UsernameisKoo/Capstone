using System.Collections;
using UnityEngine;

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

    [Header("전투 위치 설정")]
    public bool isEnemyUnit; 
    // 체크 ON  = 상대 포켓몬, frontSprite 사용
    // 체크 OFF = 내 포켓몬, backSprite 사용

    [Header("👉 직접 사용할 스프라이트")]
    public bool useCustomSprite;
    public Sprite customSprite;

    public void Setup(Pokemon pokemon, bool switchedIn = false)
    {
        Pokemon = pokemon;

        if (animatable == null)
        {
            Debug.LogError("Unit의 animatable이 비어 있습니다.");
            return;
        }

        Animator = animatable.GetComponent<Animator>();
        Renderer = animatable.GetComponent<SpriteRenderer>();
        RectTransform = animatable.GetComponent<RectTransform>();

        if (Renderer == null)
        {
            Debug.LogError("animatable 오브젝트에 SpriteRenderer가 없습니다.");
            return;
        }

        if (RectTransform != null)
            OriginalScale = RectTransform.localScale;

        if (switchPokeball != null)
            switchPokeball.enabled = false;

        if (switchedIn && RectTransform != null)
            RectTransform.localScale = new Vector3(0, 0, RectTransform.localScale.z);

        if (audioSource != null && pokemon != null && pokemon.Skeleton != null && pokemon.Skeleton.cry != null)
            audioSource.clip = pokemon.Skeleton.cry;

        // 지금은 Sprite 직접 교체 방식으로 사용하므로 Animator는 끔
        if (Animator != null)
            Animator.enabled = false;

        ApplyBattleSprite(pokemon);

        Renderer.enabled = true;
        Renderer.color = new Color(1f, 1f, 1f, 1f);
    }

    private void ApplyBattleSprite(Pokemon pokemon)
    {
        if (pokemon == null || pokemon.Skeleton == null)
        {
            Debug.LogWarning("Pokemon 또는 Pokemon.Skeleton이 없습니다.");
            return;
        }

        PokemonBase skeleton = pokemon.Skeleton;

        // 1. 직접 지정한 스프라이트가 있으면 우선 사용
        if (useCustomSprite && customSprite != null)
        {
            Renderer.sprite = customSprite;
            Debug.Log("Custom Sprite 적용됨: " + customSprite.name);
            return;
        }

        // 2. 상대 포켓몬이면 frontSprite 사용
        if (isEnemyUnit && skeleton.frontSprite != null)
        {
            Renderer.sprite = skeleton.frontSprite;
            Debug.Log("상대 포켓몬 Front Sprite 적용됨: " + skeleton.pokemonName);
            return;
        }

        // 3. 내 포켓몬이면 backSprite 사용
        if (!isEnemyUnit && skeleton.backSprite != null)
        {
            Renderer.sprite = skeleton.backSprite;
            Debug.Log("아군 포켓몬 Back Sprite 적용됨: " + skeleton.pokemonName);
            return;
        }

        // 4. backSprite가 없는데 아군이면 frontSprite라도 사용
        if (!isEnemyUnit && skeleton.frontSprite != null)
        {
            Renderer.sprite = skeleton.frontSprite;
            Debug.LogWarning("Back Sprite가 없어서 Front Sprite를 대신 사용함: " + skeleton.pokemonName);
            return;
        }

        // 5. frontSprite가 없는데 상대면 icon이라도 사용
        if (skeleton.icon != null)
        {
            Renderer.sprite = skeleton.icon;
            Debug.LogWarning("전투용 Sprite가 없어서 Icon을 대신 사용함: " + skeleton.pokemonName);
            return;
        }

        Debug.LogWarning("적용할 스프라이트가 없습니다: " + skeleton.pokemonName);
    }

    public void PlayCry()
    {
        if (audioSource == null) return;

        audioSource.pitch = 1f;
        audioSource.volume = 0.8f;
        audioSource.Play();
    }

    public void PlayEnterCry()
    {
        if (audioSource == null) return;

        audioSource.pitch = 1f;
        audioSource.volume = 0.8f;
        StartCoroutine(PlayCryWithDelay(1f));
    }

    private IEnumerator PlayCryWithDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        if (audioSource != null)
            audioSource.Play();
    }

    public void PlayFaintCry()
    {
        if (audioSource == null) return;

        audioSource.pitch = 0.8f;
        audioSource.volume = 0.8f;
        audioSource.Play();
    }

    public string Name
    {
        get
        {
            if (Pokemon == null || Pokemon.Skeleton == null)
                return "";

            return Pokemon.Skeleton.pokemonName;
        }
    }

    public Move[] Moves
    {
        get
        {
            if (Pokemon == null)
                return null;

            return Pokemon.Moves;
        }
    }
}