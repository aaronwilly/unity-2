using UnityEngine;

/// <summary>
/// Capture mode: touch/drag line length vs threshold.
/// Capture chance = 30 + missingHP% - (enemyLevel * 2), clamped 5–95.
/// </summary>
public class CaptureManager : MonoBehaviour
{
    public const float LineLengthThreshold = 100f;

    private Vector2 _dragStart;
    private float _lastDragLength;
    private bool _isDragging;
    private BattleManager _battleManager;

    public void Initialize(BattleManager battleManager)
    {
        _battleManager = battleManager;
    }

    private void Update()
    {
        if (_battleManager == null || !_battleManager.CanCapture()) return;

        if (Input.GetMouseButtonDown(0))
        {
            _dragStart = Input.mousePosition;
            _isDragging = true;
        }

        if (_isDragging && Input.GetMouseButton(0))
        {
            _lastDragLength = Vector2.Distance(_dragStart, Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_lastDragLength >= LineLengthThreshold)
            {
                _battleManager.TryCapture();
            }
            _isDragging = false;
            _lastDragLength = 0f;
        }
    }

    /// <summary>
    /// Capture chance = 30 + missingHP% - (enemyLevel * 2), clamped 5–95.
    /// </summary>
    public float ComputeCaptureChance(Unit enemy)
    {
        if (enemy == null || enemy.MaxHP <= 0) return 5f;
        float missingHpPercent = (1f - enemy.HPRatio) * 100f;
        float chance = 30f + missingHpPercent - (enemy.Level * 2f);
        return Mathf.Clamp(chance, 5f, 95f);
    }
}
