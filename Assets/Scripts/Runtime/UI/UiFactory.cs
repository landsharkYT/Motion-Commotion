using System;
using UnityEngine;
using UnityEngine.UI;

namespace MotionCommotion
{
    public static class UiFactory
    {
        public static RectTransform Rect(string name, Transform parent)
        { var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); return (RectTransform)go.transform; }
        public static Text Text(string name, Transform parent, Font font, string text, int size = 15)
        {
            var rect = Rect(name, parent); var label = rect.gameObject.AddComponent<Text>();
            label.font = font; label.text = text; label.fontSize = size; label.color = new Color(.12f,.14f,.16f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false; return label;
        }
        public static void Stretch(RectTransform rect)
        { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
        public static void Height(GameObject go, float height)
        { var layout = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>(); layout.minHeight = height; layout.preferredHeight = height; }
        public static VerticalLayoutGroup Vertical(RectTransform rect, int padding = 8, float spacing = 6)
        {
            var group = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(padding,padding,padding,padding); group.spacing = spacing;
            group.childControlWidth = true; group.childForceExpandWidth = true;
            group.childControlHeight = true; group.childForceExpandHeight = false; return group;
        }
        public static HorizontalLayoutGroup Horizontal(RectTransform rect, float spacing = 6)
        {
            var group = rect.gameObject.AddComponent<HorizontalLayoutGroup>(); group.spacing = spacing;
            group.childControlHeight = true; group.childForceExpandHeight = true;
            group.childControlWidth = true; group.childForceExpandWidth = false; return group;
        }
        public static Button Button(string name, Transform parent, Font font, string text, Action action)
        {
            var rect = Rect(name, parent); var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.89f,.9f,.92f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var label = Text(name + "Label", rect, font, text); label.alignment = TextAnchor.MiddleCenter; Stretch(label.rectTransform);
            Height(rect.gameObject, 32); if (action != null) button.onClick.AddListener(() => action()); return button;
        }
        public static InputField Input(string name, Transform parent, Font font, string value, Action<string> change)
        {
            var rect = Rect(name, parent); var image = rect.gameObject.AddComponent<Image>(); image.color = Color.white;
            var field = rect.gameObject.AddComponent<InputField>(); field.targetGraphic = image;
            var label = Text("Value", rect, font, value); label.alignment = TextAnchor.MiddleLeft;
            Stretch(label.rectTransform); label.rectTransform.offsetMin = new Vector2(6,0); label.rectTransform.offsetMax = new Vector2(-6,0);
            field.textComponent = label; field.contentType = InputField.ContentType.Standard; field.text = value;
            field.onValueChanged.AddListener(text => change(text)); Height(rect.gameObject, 30);
            var le = rect.gameObject.GetComponent<LayoutElement>(); le.flexibleWidth = 1; le.minWidth = 55;
            return field;
        }
        public static void Clear(Transform parent)
        {
            for (int i = parent.childCount-1; i >= 0; i--)
            { var child = parent.GetChild(i); child.gameObject.SetActive(false); child.SetParent(null); UnityEngine.Object.Destroy(child.gameObject); }
        }
    }
}
