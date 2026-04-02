using UnityEngine;

public class CameraSingleton : MonoBehaviour
{
    private static CameraSingleton instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
