using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Crea una UI mínima en runtime para controlar captura/procesado sin tocar la escena.
public static class AutoUIRuntime
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Build()
    {
        if (GameObject.Find("AutoUI") != null) return;

        var capture = UnityEngine.Object.FindObjectOfType<CaptureController>();
        var processing = UnityEngine.Object.FindObjectOfType<ProcessingController>();
        if (capture == null || processing == null) return;

        // Canvas + EventSystem
        var canvasGo = new GameObject("AutoUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        // Panel inferior
        var panel = CreateUIElement<RectTransform>("Panel", canvasGo.transform);
        var panelImg = panel.gameObject.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.35f);
        panel.anchorMin = new Vector2(0, 0);
        panel.anchorMax = new Vector2(1, 0);
        panel.pivot = new Vector2(0.5f, 0);
        panel.sizeDelta = new Vector2(0, 260);
        panel.anchoredPosition = new Vector2(0, 0);

        // Estado
        var status = CreateText(panel, "Listo. Pulsa Capturar");
        status.alignment = TextAnchor.MiddleLeft;
        var statusRt = status.rectTransform;
        statusRt.anchorMin = new Vector2(0, 0);
        statusRt.anchorMax = new Vector2(1, 0);
        statusRt.pivot = new Vector2(0.5f, 0);
        statusRt.sizeDelta = new Vector2(-40, 80);
        statusRt.anchoredPosition = new Vector2(20, 20);

        // Botones
        float btnWidth = 260;
        float btnHeight = 80;
        var btnCapture = CreateButton(panel, "Capturar", new Vector2(-360, 130), btnWidth, btnHeight);
        var btnStop = CreateButton(panel, "Detener", new Vector2(0, 130), btnWidth, btnHeight);
        var btnProcess = CreateButton(panel, "Procesar", new Vector2(360, 130), btnWidth, btnHeight);

        btnCapture.onClick.AddListener(() =>
        {
            capture.StartCapture();
            status.text = "Capturando... muévete alrededor del objeto";
        });

        btnStop.onClick.AddListener(() =>
        {
            capture.StopCapture();
            status.text = "Captura detenida. Pulsa Procesar si ya rodeaste el objeto";
        });

        btnProcess.onClick.AddListener(() =>
        {
            try
            {
                status.text = "Procesando...";
                int code = processing.RunReconstruction();
                status.text = code == 0 ? "Listo: STL generado" : $"Error {code}";
            }
            catch (DllNotFoundException)
            {
                status.text = "Falta plugin nativo para Android (phonetsl.so); no se puede procesar en este dispositivo.";
            }
            catch (Exception ex)
            {
                status.text = $"Error: {ex.Message}";
            }
        });
    }

    private static T CreateUIElement<T>(string name, Transform parent) where T : Component
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.transform.SetParent(parent, false);
        return go.AddComponent<T>();
    }

    private static Text CreateText(RectTransform parent, string content)
    {
        var txt = CreateUIElement<Text>("Text", parent);
        txt.text = content;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 28;
        txt.color = Color.white;
        return txt;
    }

    private static Button CreateButton(RectTransform parent, string label, Vector2 centerPos, float width, float height)
    {
        var btn = CreateUIElement<Button>("Button_" + label, parent);
        var rt = btn.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = centerPos;

        var img = btn.gameObject.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 1f, 0.8f);

        var txt = CreateText(rt, label);
        txt.alignment = TextAnchor.MiddleCenter;
        txt.rectTransform.anchorMin = Vector2.zero;
        txt.rectTransform.anchorMax = Vector2.one;
        txt.rectTransform.offsetMin = Vector2.zero;
        txt.rectTransform.offsetMax = Vector2.zero;

        var colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.6f, 1f, 0.9f);
        colors.pressedColor = new Color(0.15f, 0.45f, 0.9f, 1f);
        btn.colors = colors;

        return btn;
    }
}
