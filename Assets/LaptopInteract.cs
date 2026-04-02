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
        laptopCanvas.SetActive(false); // 처음엔 꺼두기
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
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
    }

    void ZoomIn()
    {
        isZoomed = true; // 추가!
        laptopCanvas.SetActive(true);
        Debug.Log("ZoomIn 실행!");
    }

    void ZoomOut()
    {
        isZoomed = false; // 추가!
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