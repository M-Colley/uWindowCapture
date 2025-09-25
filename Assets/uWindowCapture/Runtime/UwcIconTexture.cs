using UnityEngine;

namespace uWindowCapture
{

[RequireComponent(typeof(Renderer))]
public class UwcIconTexture : MonoBehaviour
{
    [SerializeField] UwcWindowTexture windowTexture_;
    Renderer renderer_;
    Material material_;
    public UwcWindowTexture windowTexture
    {
        get
        {
            return windowTexture_;
        }
        set
        {
            windowTexture_ = value;
            if (windowTexture_) {
                window = windowTexture_.window;
            }
        }
    }

    UwcWindow window_ = null;
    public UwcWindow window
    {
        get
        {
            return window_;
        }
        set
        {
            if (window_ == value) {
                return;
            }

            if (window_ != null) {
                window_.onIconCaptured.RemoveListener(OnIconCaptured);
            }

            window_ = value;

            if (window_ != null) {
                if (!window_.hasIconTexture) {
                    window_.onIconCaptured.AddListener(OnIconCaptured);
                    window_.RequestCaptureIcon();
                } else {
                    OnIconCaptured();
                }
            }
        }
    }

    bool isValid
    {
        get
        {
            return window != null;
        }
    }

    void Awake()
    {
        renderer_ = GetComponent<Renderer>();
        material_ = renderer_.material; // clone
    }

    void Update()
    {
        if (windowTexture != null) {
            if (window == null || window != windowTexture_.window) {
                window = windowTexture_.window;
            }
        }
    }

    void OnIconCaptured()
    {
        if (!isValid) return;

        material_.mainTexture = window.iconTexture;
        window.onIconCaptured.RemoveListener(OnIconCaptured);
    }

    void OnDestroy()
    {
        if (window_ != null) {
            window_.onIconCaptured.RemoveListener(OnIconCaptured);
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