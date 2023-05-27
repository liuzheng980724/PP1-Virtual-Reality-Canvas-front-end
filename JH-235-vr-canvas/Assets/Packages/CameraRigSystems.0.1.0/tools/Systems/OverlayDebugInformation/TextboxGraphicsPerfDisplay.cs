using TMPro;
using UnityEngine;
using System.Text;
using UnityEngine.Profiling;

public class TextboxGraphicsPerfDisplay : MonoBehaviour
{
    [SerializeField] 
    TMP_Text displayText = null;

    int frameCount = 0;
    float dt = 0.0f;
    float fps = 0.0f;
    float updateRate = 2f;

    StringBuilder stringBuilder = new StringBuilder();

    private void OnValidate()
    {
        if (displayText == null)
            displayText = this.GetComponentInChildren<TMP_Text>();
    }

    private void Awake()
    {
        Debug.Assert(displayText != null);
    }

    private void Start()
    {
        displayText.text = string.Empty;
    }

    void OnEnable()
    {
        UpdateTextMesh();
    }

    void Update()
    {
        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1.0f / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1.0f / updateRate;
            UpdateTextMesh();
        }
    }

    void UpdateTextMesh()
    {
        stringBuilder.Clear();
        stringBuilder.Append("<color=yellow>Frame Rate: ");
        stringBuilder.Append((int)fps);
        stringBuilder.Append(" FPS</color>");
        stringBuilder.Append("<color=blue>\n");

        stringBuilder.Append("RAM: ");
        stringBuilder.Append(string.Format("{0:n2}", Profiler.GetMonoUsedSizeLong() / 1000));
        stringBuilder.Append(" / ");
        stringBuilder.Append(string.Format("{0:n2}", Profiler.GetMonoHeapSizeLong() / 1000));
        stringBuilder.Append(" mb</color>\n");

        displayText.text = stringBuilder.ToString();
    }
    
}
