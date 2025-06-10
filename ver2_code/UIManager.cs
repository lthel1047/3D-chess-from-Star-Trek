using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public CameraController cameraController;

    public float zoomDistance = 1f;
    private float zoomDistanceMin = 3f;
    private float zoomDistanceMax = 15f;

    public Button zoomInBtn;
    public Button zoomOutBtn;

    public GameObject IntroParent;
    public GameObject InGameParent;

    [Header("Turn UI")]
    public GameObject whiteTurnSprite;
    public GameObject blackTurnSprite;

    [Header("AI Level")]
    public Button easyButton;
    public Button mediumButton;


    private BoardManager bm;

    private void Awake()
    {
        
    }

    private void Start()
    {
        bm = BoardManager.Instance;

        if (zoomInBtn != null)
            zoomInBtn.onClick.AddListener(OnZoomIn);

        if (zoomOutBtn != null)
            zoomOutBtn.onClick.AddListener(OnZoomOut);

        UpdateTurnUI(GameFlowManager.Player.White); // 초기값 설정

        easyButton.onClick.AddListener(() => SetAIDifficulty(0));
        mediumButton.onClick.AddListener(() => SetAIDifficulty(1));
    }

    private void OnZoomIn()
    {
        if (cameraController != null)
        {
            cameraController.distance = Mathf.Min(cameraController.distance + zoomDistance, zoomDistanceMax);
        }
    }

    private void OnZoomOut()
    {
        if (cameraController != null)
        {
            cameraController.distance = Mathf.Max(cameraController.distance - zoomDistance, zoomDistanceMin);
        }
    }

    public void UpdateTurnUI(GameFlowManager.Player currentPlayer)
    {
        if(currentPlayer == GameFlowManager.Player.White)
        {
            whiteTurnSprite.SetActive(true);
            blackTurnSprite.SetActive(false);
        }
        else
        {
            whiteTurnSprite.SetActive(false);
            blackTurnSprite.SetActive(true);
        }
    }

    private void SetAIDifficulty(int level)
    {
        if(IntroParent == null) return;
        IntroParent.SetActive(false);
        InGameParent.SetActive(true);

        bm.ResetBoard();

        FindObjectOfType<AIPlayer>()?.SetDifficulty(level);
    }
}
