using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace uWindowCapture
{

[RequireComponent(typeof(UwcWindowTexture))]
public class UwcWindowTextureChildrenManager : MonoBehaviour
{
    class ChildWindowEntry
    {
        public UwcWindow window;
        public UwcWindowTexture texture;
        public UnityAction markDirty;
    }

    UwcWindowTexture windowTexture_;
    Dictionary<int, ChildWindowEntry> children_ = new Dictionary<int, ChildWindowEntry>();
    HashSet<int> dirtyChildren_ = new HashSet<int>();
    List<int> dirtyChildrenBuffer_ = new List<int>();
    bool parentMetricsDirty_ = true;
    UnityAction parentCapturedListener_;
    UnityAction parentSizeChangedListener_;

    void Awake()
    {
        windowTexture_ = GetComponent<UwcWindowTexture>();
        windowTexture_.onWindowChanged.AddListener(OnWindowChanged);
        OnWindowChanged(windowTexture_.window, null);
    }

    void Update()
    {
        UpdateChildren();
    }

    UwcWindowTexture InstantiateChild()
    {
        var prefab = windowTexture_.childWindowPrefab;
        if (!prefab) return null;

        var childTexture = Instantiate(prefab, transform);
        return childTexture.GetComponent<UwcWindowTexture>();
    }

    void OnWindowChanged(UwcWindow newWindow, UwcWindow oldWindow)
    {
        if (newWindow == oldWindow) return;

        if (oldWindow != null) {
            oldWindow.onChildAdded.RemoveListener(OnChildAdded);
            oldWindow.onChildRemoved.RemoveListener(OnChildRemoved);

            if (parentCapturedListener_ != null) {
                oldWindow.onCaptured.RemoveListener(parentCapturedListener_);
            }
            if (parentSizeChangedListener_ != null) {
                oldWindow.onSizeChanged.RemoveListener(parentSizeChangedListener_);
            }
            parentCapturedListener_ = null;
            parentSizeChangedListener_ = null;

            foreach (var kv in children_) {
                var entry = kv.Value;
                if (entry.window != null && entry.markDirty != null) {
                    entry.window.onCaptured.RemoveListener(entry.markDirty);
                    entry.window.onSizeChanged.RemoveListener(entry.markDirty);
                }

                var windowTexture = entry.texture;
                if (windowTexture) {
                    Destroy(windowTexture.gameObject);
                }
            }

            children_.Clear();
            dirtyChildren_.Clear();
        }

        if (newWindow != null) {
            newWindow.onChildAdded.AddListener(OnChildAdded);
            newWindow.onChildRemoved.AddListener(OnChildRemoved);

            parentCapturedListener_ = MarkParentDirty;
            parentSizeChangedListener_ = MarkParentDirty;
            newWindow.onCaptured.AddListener(parentCapturedListener_);
            newWindow.onSizeChanged.AddListener(parentSizeChangedListener_);
            MarkParentDirty();

            foreach (var pair in UwcManager.windows) {
                var window = pair.Value;
                if (
                    !window.isAltTabWindow &&
                    window.isChild &&
                    window.parentWindow != null &&
                    window.parentWindow.id == newWindow.id) {
                    OnChildAdded(window);
                }
            }
        }
    }

    void OnChildAdded(UwcWindow window)
    {
        var childWindowTexture = InstantiateChild();
        if (!childWindowTexture) {
            Debug.LogError("childPrefab is not set or does not have UwcWindowTexture.");
            return;
        }
        childWindowTexture.window = window;
        childWindowTexture.parent = windowTexture_;
        childWindowTexture.manager = windowTexture_.manager;
        childWindowTexture.type = WindowTextureType.Child;
        childWindowTexture.captureFrameRate = windowTexture_.captureFrameRate;
        childWindowTexture.captureRequestTiming = windowTexture_.captureRequestTiming;
        childWindowTexture.drawCursor = windowTexture_.drawCursor;

        var entry = new ChildWindowEntry {
            window = window,
            texture = childWindowTexture,
        };
        UnityAction markDirty = () => MarkChildDirty(window.id);
        entry.markDirty = markDirty;
        window.onCaptured.AddListener(markDirty);
        window.onSizeChanged.AddListener(markDirty);

        children_[window.id] = entry;
        MarkChildDirty(window.id);
    }

    void OnChildRemoved(UwcWindow window)
    {
        OnChildRemoved(window.id);
    }

    void OnChildRemoved(int id)
    {
        ChildWindowEntry entry;
        children_.TryGetValue(id, out entry);
        if (entry != null) {
            if (entry.window != null && entry.markDirty != null) {
                entry.window.onCaptured.RemoveListener(entry.markDirty);
                entry.window.onSizeChanged.RemoveListener(entry.markDirty);
            }

            if (entry.texture) {
                Destroy(entry.texture.gameObject);
            }

            children_.Remove(id);
            dirtyChildren_.Remove(id);
        }
    }

    void MoveAndScaleChildWindow(UwcWindowTexture child)
    {
        var window = child.window;
        var parent = window.parentWindow;

        var px = parent.x;
        var py = parent.y;
        var pw = parent.width;
        var ph = parent.height;
        var parentZ = parent.zOrder;

        var cx = window.x;
        var cy = window.y;
        var cw = window.width;
        var ch = window.height;
        var childZ = window.zOrder;

        var dz = windowTexture_.childWindowZDistance;
        var desktopX = (cw - pw) * 0.5f + (cx - px);
        var desktopY = (ch - ph) * 0.5f + (cy - py);
        var localX = pw != 0 ? desktopX / pw : 0f;
        var localY = ph != 0 ? -desktopY / ph : 0f;

        var scaleZ = transform.localScale.z;
        var localZ = scaleZ != 0f ? dz * (childZ - parentZ) / scaleZ : 0f;
        child.transform.localPosition = new Vector3(localX, localY, localZ);

        var widthRatio = pw != 0 ? (float)cw / pw : 0f;
        var heightRatio = ph != 0 ? (float)ch / ph : 0f;
        child.transform.localScale = new Vector3(widthRatio, heightRatio, 1f);
    }

    void UpdateChildren()
    {
        if (parentMetricsDirty_) {
            MarkAllChildrenDirty();
            parentMetricsDirty_ = false;
        }

        if (dirtyChildren_.Count == 0) {
            return;
        }

        dirtyChildrenBuffer_.Clear();
        dirtyChildrenBuffer_.AddRange(dirtyChildren_);
        dirtyChildren_.Clear();

        for (int i = 0; i < dirtyChildrenBuffer_.Count; ++i) {
            var id = dirtyChildrenBuffer_[i];
            ChildWindowEntry entry;
            if (!children_.TryGetValue(id, out entry) || entry == null) {
                continue;
            }

            var child = entry.texture;
            if (child) {
                MoveAndScaleChildWindow(child);
            }
        }
    }

    void MarkChildDirty(int id)
    {
        dirtyChildren_.Add(id);
    }

    void MarkAllChildrenDirty()
    {
        foreach (var key in children_.Keys) {
            dirtyChildren_.Add(key);
        }
    }

    void MarkParentDirty()
    {
        parentMetricsDirty_ = true;
    }
}

}