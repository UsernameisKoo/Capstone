using UnityEngine;

public class PokemonIconAnimator : MonoBehaviour
{
    [SerializeField] PokemonBase pokemon;

    SpriteRenderer sr;

    int currentFrame;
    float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (pokemon == null || sr == null)
            return;

        // 프레임 애니메이션이 있으면 첫 프레임 사용
        if (pokemon.IconFrames != null &&
            pokemon.IconFrames.Length > 0)
        {
            sr.sprite = pokemon.IconFrames[0];
        }
        // 없으면 기본 아이콘 사용
        else
        {
            sr.sprite = pokemon.Icon;
        }
    }

    void Update()
    {
        // 프레임이 없으면 애니메이션 안 함
        if (pokemon == null ||
            pokemon.IconFrames == null ||
            pokemon.IconFrames.Length == 0)
            return;

        timer += Time.deltaTime;

        if (timer >= 1f / pokemon.IconFrameRate)
        {
            timer = 0f;

            currentFrame =
                (currentFrame + 1) %
                pokemon.IconFrames.Length;

            sr.sprite =
                pokemon.IconFrames[currentFrame];
        }
    }

    public void SetPokemon(PokemonBase newPokemon)
    {
        pokemon = newPokemon;

        currentFrame = 0;
        timer = 0f;

        if (pokemon.IconFrames != null &&
            pokemon.IconFrames.Length > 0)
        {
            sr.sprite = pokemon.IconFrames[0];
        }
        else
        {
            sr.sprite = pokemon.Icon;
        }
    }
}