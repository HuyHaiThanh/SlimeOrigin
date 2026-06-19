using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Gọi Claude API sinh 1–2 câu độc thoại của Slime sau khi absorb (mục 5).
/// Dùng UnityWebRequest + coroutine (không dùng SDK NuGet trong Unity).
/// Key đọc từ Resources/config.json (gitignore). Thiếu key → degrade dùng câu fallback offline.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    private const string Endpoint = "https://api.anthropic.com/v1/messages";
    private const string Model = "claude-sonnet-4-6";   // theo CLAUDE.md mục 5.1 (flavor text, rẻ + nhanh)

    private const string SystemPrompt =
        "Bạn là một Slime mới thức tỉnh trong thế giới giả tưởng. " +
        "Bạn vừa absorb một sinh vật và học được element mới. " +
        "Hãy nói 1–2 câu ngắn, giọng tò mò và hơi ngây thơ, " +
        "phản ánh đúng element vừa học (Pyro = nóng bỏng/tự tin, " +
        "Hydro = bình tĩnh/linh hoạt, Cryo = lạnh lùng/chậm rãi...). " +
        "Không giải thích — chỉ nói như nhân vật đang cảm nhận. " +
        "Trả lời bằng tiếng Việt, tối đa 2 câu.";

    private string apiKey;

    void Awake()
    {
        LoadApiKey();
    }

    private void LoadApiKey()
    {
        var cfg = Resources.Load<TextAsset>("config");
        if (cfg == null) { apiKey = null; return; }
        try
        {
            var data = JsonUtility.FromJson<ConfigData>(cfg.text);
            apiKey = data != null ? data.anthropicApiKey : null;
        }
        catch { apiKey = null; }
    }

    /// <summary>Xin dialogue cho lần absorb. Luôn trả kết quả qua onResult (API hoặc fallback).</summary>
    public void RequestDialogue(string enemyName, ElementType element, Action<string> onResult)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            onResult?.Invoke(GetFallbackLine(element));
            return;
        }
        StartCoroutine(RequestRoutine(enemyName, element, onResult));
    }

    private IEnumerator RequestRoutine(string enemyName, ElementType element, Action<string> onResult)
    {
        var req = new AnthropicRequest
        {
            model = Model,
            max_tokens = 200,
            system = SystemPrompt,
            messages = new[]
            {
                new Msg { role = "user", content = "Tôi vừa absorb " + enemyName + " và học được element " + element + "." }
            }
        };
        string json = JsonUtility.ToJson(req);

        using (var web = new UnityWebRequest(Endpoint, "POST"))
        {
            web.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            web.downloadHandler = new DownloadHandlerBuffer();
            web.SetRequestHeader("content-type", "application/json");
            web.SetRequestHeader("x-api-key", apiKey);
            web.SetRequestHeader("anthropic-version", "2023-06-01");

            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[DialogueManager] API lỗi: " + web.error + " → dùng fallback");
                onResult?.Invoke(GetFallbackLine(element));
                yield break;
            }

            string text = ParseText(web.downloadHandler.text);
            onResult?.Invoke(string.IsNullOrEmpty(text) ? GetFallbackLine(element) : text.Trim());
        }
    }

    /// <summary>Tách content[0].text từ JSON response của Claude.</summary>
    private string ParseText(string responseJson)
    {
        try
        {
            var resp = JsonUtility.FromJson<AnthropicResponse>(responseJson);
            if (resp != null && resp.content != null && resp.content.Length > 0)
                return resp.content[0].text;
        }
        catch (Exception e) { Debug.LogWarning("[DialogueManager] parse lỗi: " + e.Message); }
        return null;
    }

    /// <summary>Câu thoại offline khi không có key / API lỗi — game vẫn chạy mượt.</summary>
    private string GetFallbackLine(ElementType element)
    {
        switch (element)
        {
            case ElementType.Pyro:  return "Nóng quá... nhưng mình thấy mạnh mẽ hẳn lên!";
            case ElementType.Hydro: return "Mát lạnh và êm dịu. Mình có thể chảy đi bất cứ đâu.";
            case ElementType.Cryo:  return "Lạnh thật... mọi thứ như chậm lại quanh mình.";
            case ElementType.Dendro:return "Mình cảm nhận được sự sống đang nảy mầm bên trong.";
            default:                return "Mình vừa học được điều gì đó mới mẻ...";
        }
    }

    // ===== DTO cho JsonUtility =====
    [Serializable] private class ConfigData { public string anthropicApiKey; }
    [Serializable] private class AnthropicRequest { public string model; public int max_tokens; public string system; public Msg[] messages; }
    [Serializable] private class Msg { public string role; public string content; }
    [Serializable] private class AnthropicResponse { public ContentBlock[] content; }
    [Serializable] private class ContentBlock { public string type; public string text; }
}
