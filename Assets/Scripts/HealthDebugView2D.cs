using System.Collections;
using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary>
    /// ДЕБАГ-визуал для HealthModule:
    /// - HP bar над головой
    /// - Урон: мигание + лёгкий дёрг
    /// - Смерть: смена спрайта (+ отключение Animator)
    ///
    /// Не для релиза. Подписывается ТОЛЬКО на события HealthModule.
    /// </summary>
    public sealed class HealthDebugView2D : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private HealthModule _health;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Animator _childAnimator;
        [SerializeField] private Animator _animator; // опционально
        [SerializeField] private Transform _visualRoot; // что трясти

        [Header("HP Bar")]
        [SerializeField] private Vector3 _barOffset = new(0f, 1.2f, 0f);
        [SerializeField] private Vector2 _barSize = new(60f, 8f);
        [SerializeField] private float _barPadding = 0.15f; // расстояние НАД спрайтом (в world units)

        [Header("HP Bar Colors")]
        [SerializeField] private Color _barFillColor = new(0.35f, 0.7f, 0.35f, 0.9f);
        [SerializeField] private Color _barBgColor = new(0f, 0f, 0f, 0.55f);

        [Header("HP Bar Outline")]
        [SerializeField] private Color _outlineColor = new(0f, 0f, 0f, 0.8f);
        [SerializeField, Range(1f, 4f)] private float _outlinePx = 2f;

        [Header("HP Bar Shape")]
        [SerializeField, Range(0f, 8f)]
        private float _cornerRadius = 4f;

        [SerializeField, Range(0f, 3f)]
        private float _innerPadding = 1f;

        [Header("Damage Flash")]
        [SerializeField] private int _flashCount = 2;
        [SerializeField] private float _flashTime = 0.05f;

        [Header("Damage Shake")]
        [SerializeField] private float _shakeDuration = 0.08f;
        [SerializeField] private float _shakeDistance = 0.06f;

        [Header("Death")]
        [SerializeField] private Sprite _deadSprite;

        // internal
        private Camera _cam;
        private Texture2D _whiteTex;
        private Color _baseColor;
        private Vector3 _baseLocalPos;
        private Coroutine _feedbackRoutine;

        private Texture2D _capTex;
        private void Awake()
        {
            if (_health == null) _health = GetComponentInParent<HealthModule>();
            if (_renderer == null) _renderer = GetComponentInChildren<SpriteRenderer>();
            if (_visualRoot == null) _visualRoot = _renderer != null ? _renderer.transform : transform;

            _animator ??= GetComponentInChildren<Animator>();
            _cam = Camera.main;
            if (_childAnimator == null)
                _childAnimator = GetComponentInChildren<Animator>(true);

            // 1x1 белая
            _whiteTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _whiteTex.SetPixel(0, 0, Color.white);
            _whiteTex.Apply();

            // круглая "кап" текстура (альфа-круг)
            _capTex = BuildCircleTex(32);

            if (_renderer != null) _baseColor = _renderer.color;
            _baseLocalPos = _visualRoot.localPosition;
        }
        private Texture2D BuildCircleTex(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float r = (size - 1) * 0.5f;
            Vector2 c = new Vector2(r, r);

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    float a = d <= r ? 1f : 0f; // жёсткий круг (для дебага ок)
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }

            tex.Apply();
            return tex;
        }
        private void OnEnable()
        {
            if (_health == null) return;

            _health.OnDamaged += OnDamaged;
            _health.OnZeroHp += OnZeroHp;
        }

        private void OnDisable()
        {
            if (_health == null) return;

            _health.OnDamaged -= OnDamaged;
            _health.OnZeroHp -= OnZeroHp;
        }

        // =======================
        // DAMAGE FEEDBACK
        // =======================

        private void OnDamaged(DamageInfo info, float newHp)
        {
            if (_feedbackRoutine != null)
                StopCoroutine(_feedbackRoutine);

            _feedbackRoutine = StartCoroutine(DamageFeedback());
        }

        private IEnumerator DamageFeedback()
        {
            bool animWasEnabled = false;
            if (_childAnimator != null)
            {
                animWasEnabled = _childAnimator.enabled;
                _childAnimator.enabled = false;
            }

            // === FLASH ===
            for (int i = 0; i < _flashCount; i++)
            {
                _renderer.color = Color.white;
                yield return new WaitForSeconds(_flashTime);

                _renderer.color = _baseColor;
                yield return new WaitForSeconds(_flashTime);
            }

            // === RESTORE ===
            if (_childAnimator != null)
                _childAnimator.enabled = animWasEnabled;

            // SHAKE
            float t = 0f;
            while (t < _shakeDuration)
            {
                t += Time.deltaTime;
                Vector2 rnd = Random.insideUnitCircle * _shakeDistance;
                _visualRoot.localPosition = _baseLocalPos + new Vector3(rnd.x, rnd.y, 0f);
                yield return null;
            }

            _visualRoot.localPosition = _baseLocalPos;
            _feedbackRoutine = null;
        }

        // =======================
        // DEATH
        // =======================

        private void OnZeroHp(DamageInfo info)
        {
            if (_animator != null)
                _animator.enabled = false;

            if (_renderer != null && _deadSprite != null)
                _renderer.sprite = _deadSprite;
        }

        // =======================
        // HP BAR
        // =======================

        private void OnGUI()
        {
            if (_health == null || _cam == null) return;
            if (!_health.IsAlive) return;

            // позиция: центр X, верх спрайта + отступ
            Vector3 world;
            if (_renderer != null)
            {
                var b = _renderer.bounds;
                world = new Vector3(b.center.x, b.max.y + _barPadding, b.center.z);
            }
            else
            {
                world = transform.position + Vector3.up;
            }

            Vector3 screen = _cam.WorldToScreenPoint(world);
            if (screen.z <= 0f) return;

            float pct = Mathf.Clamp01(_health.CurrentHp / _health.MaxHp);

            float x = screen.x - _barSize.x * 0.5f;
            float y = (Screen.height - screen.y) - _barSize.y * 0.5f;

            float o = _outlinePx;

            // =========================
            // OUTLINE (самый нижний слой)
            // =========================
            GUI.color = _outlineColor;
            DrawCapsuleBar(
                x - o,
                y - o,
                _barSize.x + o * 2f,
                _barSize.y + o * 2f
            );

            // =========================
            // BACKGROUND
            // =========================
            GUI.color = _barBgColor;
            DrawCapsuleBar(x, y, _barSize.x, _barSize.y);

            // =========================
            // FILL
            // =========================
            float fillW = _barSize.x * pct;
            GUI.color = _barFillColor;
            DrawCapsuleBar(x, y, fillW, _barSize.y);

            GUI.color = Color.white;
        }



        private void DrawCapsuleBar(float x, float y, float w, float h)
        {
            if (w <= 0f || h <= 0f) return;

            float r = h * 0.5f;

            // если ширина меньше диаметра — рисуем просто круг, сжимаем по ширине
            if (w <= h)
            {
                GUI.DrawTexture(new Rect(x, y, w, h), _capTex);
                return;
            }

            // левый кап (круг)
            GUI.DrawTexture(new Rect(x, y, h, h), _capTex);

            // центр (прямоугольник)
            GUI.DrawTexture(new Rect(x + r, y, w - h, h), _whiteTex);

            // правый кап (круг)
            GUI.DrawTexture(new Rect(x + w - h, y, h, h), _capTex);
        }





    }
}
