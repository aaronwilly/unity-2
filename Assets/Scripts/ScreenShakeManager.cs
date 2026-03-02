using System.Collections;
using UnityEngine;

/// <summary>
/// Reusable screen shake: shakes a target Transform (camera or canvas root) for a duration with adjustable intensity.
/// Pure code, no Cinemachine, no inspector refs. Call SetTarget() from UIManager to shake canvas.
/// </summary>
public class ScreenShakeManager : MonoBehaviour
{
    public static ScreenShakeManager Instance { get; private set; }

    private Transform _target;
    private Vector3 _basePosition;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        _target = Camera.main != null ? Camera.main.transform : null;
        if (_target != null) _basePosition = _target.localPosition;
    }

    /// <summary>Set the transform to shake (e.g. canvas root). If null, uses Main Camera.</summary>
    public void SetTarget(Transform target)
    {
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _target = target;
        if (_target != null) _basePosition = _target.localPosition;
        else if (Camera.main != null) { _target = Camera.main.transform; _basePosition = _target.localPosition; }
    }

    /// <summary>Shake for duration seconds with given intensity (e.g. 5–15 for UI canvas, 0.1–0.5 for camera).</summary>
    public void Shake(float duration = 0.3f, float intensity = 10f)
    {
        if (_target == null)
        {
            if (Camera.main != null) { _target = Camera.main.transform; _basePosition = _target.localPosition; }
            else return;
        }
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, intensity));
    }

    private IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float amp = intensity * (1f - t);
            _target.localPosition = _basePosition + (Vector3)(UnityEngine.Random.insideUnitCircle * amp);
            yield return null;
        }
        _target.localPosition = _basePosition;
        _shakeCoroutine = null;
    }
}
