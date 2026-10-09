using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ArrowBuster
{
    /// <summary>Code-built uGUI helpers (D-012): every screen is assembled from these few primitives.</summary>
    public static class UiFactory
    {
        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5; // UI
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static RectTransform Stretch(RectTransform rect, float inset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
            return rect;
        }

        public static RectTransform Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public static Image Image(Transform parent, string name, Sprite sprite, Color color, bool raycast = false)
        {
            RectTransform rect = Rect(parent, name);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycast;
            if (sprite != null && sprite.border.sqrMagnitude > 0f) image.type = UnityEngine.UI.Image.Type.Sliced;
            return image;
        }

        public static TextMeshProUGUI Label(Transform parent, string name, string text, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.Center, FontStyles style = FontStyles.Bold)
        {
            RectTransform rect = Rect(parent, name);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset font = UiTheme.Font;
            if (font != null) label.font = font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = align;
            label.fontStyle = style;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Overflow;
            return label;
        }

        /// <summary>Rounded button with a label, a press punch and a UI tap sound.</summary>
        public static Button Button(Transform parent, string name, string text, Color fill, Color textColor, Vector2 size,
            UnityAction onClick, float fontSize = 54f)
        {
            Image background = Image(parent, name, ProceduralTextures.Pill, fill, raycast: true);
            background.rectTransform.sizeDelta = size;
            Image shadow = Image(background.transform, "Shadow", ProceduralTextures.Pill, new Color(0f, 0f, 0f, 0.18f));
            Stretch(shadow.rectTransform);
            shadow.rectTransform.offsetMin = new Vector2(0f, -10f);
            shadow.rectTransform.offsetMax = new Vector2(0f, -10f);
            shadow.transform.SetAsFirstSibling();
            if (text != null)
            {
                TextMeshProUGUI label = Label(background.transform, "Label", text, fontSize, textColor);
                Stretch(label.rectTransform, 8f);
            }
            var button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            colors.selectedColor = Color.white;
            button.colors = colors;
            button.onClick.AddListener(() =>
            {
                UiTween.Punch(background.transform, 0.1f, 0.18f);
                Services.Audio.Play(SfxId.UiTap, 0.7f);
            });
            if (onClick != null) button.onClick.AddListener(onClick);
            return button;
        }

        /// <summary>Round icon button (pause, restart).</summary>
        public static Button IconButton(Transform parent, string name, Sprite icon, Color fill, Color iconColor, float size, UnityAction onClick)
        {
            Button button = Button(parent, name, null, fill, Color.white, new Vector2(size, size), onClick);
            button.GetComponent<Image>().sprite = ProceduralTextures.Circle;
            button.GetComponent<Image>().type = UnityEngine.UI.Image.Type.Simple;
            Transform shadow = button.transform.Find("Shadow");
            if (shadow != null) shadow.GetComponent<Image>().sprite = ProceduralTextures.Circle;
            Image glyph = Image(button.transform, "Icon", icon, iconColor);
            Stretch(glyph.rectTransform, size * 0.22f);
            return button;
        }

        public static CanvasGroup Group(GameObject go)
        {
            var group = go.GetComponent<CanvasGroup>();
            return group != null ? group : go.AddComponent<CanvasGroup>();
        }

        public static void SetVisible(CanvasGroup group, bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
            group.gameObject.SetActive(visible);
        }
    }
}
