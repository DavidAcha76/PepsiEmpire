// NPCView.cs
using TMPro;
using UnityEngine;

public class NPCView : MonoBehaviour
{
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text timerLabel;
    

    public void SetName(string n)
    {
        if (nameLabel) nameLabel.text = n;
    }

    public void ShowWaitTime(float secondsLeft, bool visible)
    {
        if (!timerLabel) return;
        if (!visible) { timerLabel.text = ""; return; }
        timerLabel.text = $"{Mathf.CeilToInt(secondsLeft)}s";
    }
}
