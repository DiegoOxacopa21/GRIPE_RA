using TMPro;
using UnityEngine;

public class UIDebugger : MonoBehaviour
{
    public static UIDebugger Instance;

    [SerializeField] private TextMeshProUGUI debugText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void Log(string message)
    {
        if (debugText != null)
        {
            debugText.text = message;
        }
    }
}
