using UnityEngine;

public class LaptopInteract : MonoBehaviour
{
    [SerializeField] Transform laptopScreen;
    [SerializeField] float zoomSpeed = 2f;
    [SerializeField] GameObject laptopCanvas;
    [SerializeField] GameObject arrowIndicator;
    [SerializeField] AudioClip openSfx;

    AudioSource sfxSource;

    CameraController cameraController;
    bool playerNearby = false;
    bool isZoomed = false;
    bool isStoryLocked = false;

    void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        laptopCanvas.SetActive(false);
        if (arrowIndicator != null)
            arrowIndicator.SetActive(false);
    }

    void Update()
    {
        // 근처에서 E / Space / Enter / RightArrow 누르면 토글
        if (playerNearby && IsZoomInputPressed())
        {
            ToggleZoom();
        }
        // 근처에 있고, 우클릭으로 이 노트북 오브젝트를 클릭하면 줌인
        if (playerNearby && Input.GetMouseButtonDown(0))
        {
            TryZoomInByLeftClick();
        }
        if (isStoryLocked) return;
    }
    bool IsZoomInputPressed()
    {
        return Input.GetKeyDown(KeyCode.E)
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.Return)      // 엔터
            || Input.GetKeyDown(KeyCode.KeypadEnter) // 키패드 엔터
            || Input.GetKeyDown(KeyCode.RightArrow);
    }

    void ToggleZoom()
    {
        if (!isZoomed)
        {
            ZoomIn();
        }
        else
        {
            ZoomOut();
        }
    }

    void TryZoomInByLeftClick()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.Log("카메라 없음");
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("맞은 오브젝트: " + hit.transform.name);

            LaptopInteract clickedLaptop = hit.transform.GetComponentInParent<LaptopInteract>();

            if (clickedLaptop == this)
            {
                Debug.Log("노트북 클릭 성공!");
                if (!isZoomed)
                {
                    ZoomIn();
                }
            }
        }
        else
        {
            Debug.Log("Raycast 아무것도 안 맞음");
        }
    }

    void ZoomIn()
    {
        isZoomed = true;

        // 효과음 재생
        if (openSfx != null)
        {
            sfxSource.PlayOneShot(openSfx);
        }

        laptopCanvas.SetActive(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("ZoomIn 실행!");

        LaptopStorySequence sequence = GetComponent<LaptopStorySequence>();
        if (sequence != null && !sequence.IsPlaying())
        {
            sequence.StartSequence();
        }
    }

    void ZoomOut()
    {
        isZoomed = false;
        laptopCanvas.SetActive(false);
        Debug.Log("ZoomOut 실행!");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;

            if (arrowIndicator != null)
                arrowIndicator.SetActive(true);

            Debug.Log("노트북 근처!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (arrowIndicator != null)
                arrowIndicator.SetActive(false);

            if (isZoomed) ZoomOut();
        }
    }

    public void SetStoryLock(bool locked)
    {
        isStoryLocked = locked;
    }

}