using UnityEngine;

namespace uWindowCapture
{

[RequireComponent(typeof(Renderer))]
public class UwcCursorTexture : MonoBehaviour
{
    Renderer renderer_;
    Material material_;
    UwcCursor cursor_;

    void Awake()
    {
        renderer_ = GetComponent<Renderer>();
        material_ = renderer_.material; // clone
        cursor_ = UwcManager.cursor;
        if (cursor_ != null) {
            cursor_.onTextureChanged.AddListener(OnTextureChanged);
        }
    }

    void Update()
    {
        if (cursor_ == null) {
            return;
        }

        cursor_.CreateTextureIfNeeded();
        cursor_.RequestCapture();
    }

    void OnTextureChanged()
    {
        if (cursor_ == null) {
            return;
        }

        material_.mainTexture = cursor_.texture;
    }

    void OnDestroy()
    {
        if (cursor_ != null) {
            cursor_.onTextureChanged.RemoveListener(OnTextureChanged);
        }

        if (material_) {
            if (Application.isPlaying) {
                Destroy(material_);
            } else {
                DestroyImmediate(material_);
            }
            material_ = null;
        }
    }
}

}