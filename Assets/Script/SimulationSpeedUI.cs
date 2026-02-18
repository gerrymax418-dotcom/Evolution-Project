using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimulationSpeedUI : MonoBehaviour
{
    [SerializeField]
    private Button button;
    [SerializeField]
    private TMP_InputField input;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.interactable = false;
        input.onValueChanged.AddListener((inputText) =>
        {
            button.interactable = float.TryParse(inputText,out float _);
        });
        button.onClick.AddListener(OnClick);
        
    }
    private void OnClick()
    {
        Time.timeScale = float.Parse(input.text); 
        input.text = string.Empty;
    }
}
