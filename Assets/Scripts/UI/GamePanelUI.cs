using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel overlay đa dụng cho GameManager: hiện 1 đoạn text + tối đa 2 nút động.
/// Dùng cho intro màn, hỏi Absorb, dialogue, Game Over, hoàn thành Chapter.
/// </summary>
public class GamePanelUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button button1;
    [SerializeField] private TMP_Text button1Label;
    [SerializeField] private Button button2;
    [SerializeField] private TMP_Text button2Label;

    void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    /// <summary>Hiện panel với message + 1-2 nút. Truyền b2=null để chỉ 1 nút; b1="" để không nút (màn chờ).</summary>
    public void ShowMessage(string msg, string b1, Action onB1, string b2 = null, Action onB2 = null)
    {
        if (root != null) root.SetActive(true);
        if (messageText != null) messageText.text = msg;
        Configure(button1, button1Label, b1, onB1);
        Configure(button2, button2Label, b2, onB2);
    }

    private void Configure(Button btn, TMP_Text label, string text, Action action)
    {
        if (btn == null) return;
        if (string.IsNullOrEmpty(text)) { btn.gameObject.SetActive(false); return; }
        btn.gameObject.SetActive(true);
        if (label != null) label.text = text;
        btn.onClick.RemoveAllListeners();
        if (action != null) btn.onClick.AddListener(() => action());
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }
}
