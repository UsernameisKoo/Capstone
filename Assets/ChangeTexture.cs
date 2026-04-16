using UnityEngine;
using UnityEngine.UI;

public class ChangeTexture : MonoBehaviour
{
    public RawImage rawImage;
    public Texture newTexture;

    void Start()
    {
        rawImage.texture = newTexture;
    }
}