using UnityEngine;

public class LaptopInteract : MonoBehaviour
{
    [SerializeField] Transform laptopScreen;
    [SerializeField] float zoomSpeed = 2f;
    [SerializeField] GameObject laptopCanvas;

    CameraController cameraController;
    bool playerNearby = false;
    bool isZoomed = false;

    void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        laptopCanvas.SetActive(false);
    }

    void Update()
    {
        // 근처에서 E / Space / Enter / RightArrow 누르면 토글
        if (playerNearby && IsZoomInputPressed())
        {
            ToggleZoom();
        }

        // 근처에 있고, 우클릭으로 이 노트북 오브젝트를 클릭하면 줌인
        if (playerNearby && Input.GetMouseButtonDown(1))
        {
            TryZoomInByRightClick();
        }
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

    void TryZoomInByRightClick()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // 클릭한 오브젝트가 이 노트북 자신이거나 자식 오브젝트면 줌인
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                if (!isZoomed)
                {
                    ZoomIn();
                }
            }
        }
    }

    void ZoomIn()
    {
        isZoomed = true;
        laptopCanvas.SetActive(true);
        Debug.Log("ZoomIn 실행!");
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
            Debug.Log("노트북 근처!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (isZoomed) ZoomOut();
        }
    }
}