using UnityEngine;

/// <summary>
/// CloakSway2D:
/// - Есть Pivot, который можно крутить ползунком (pivotRotationDeg).
/// - Верхний сегмент качается туда-сюда с паузами на концах.
/// - Движение по X даёт позу (смещение) и усиливает амплитуду.
/// - Хвост запаздывает за верхом (S-форма), низ болтается сильнее.
/// Источник скорости: IMovementSource (event). Если не найден — fallback по позиции.
/// </summary>
public sealed class CloakSway2D : MonoBehaviour
{
    [Header("Pivot (rotate whole cloak)")]
    [SerializeField] private Transform pivot;
    [SerializeField, Range(-180f, 180f)] private float pivotRotationDeg = 0f;

    [Header("Segments (top -> bottom)")]
    [SerializeField] private Transform[] segments;

    [Header("Angles (asymmetric clamp)")]
    [SerializeField, Range(0f, 90f)] private float maxAngleLeft = 45f;   // отриц. (влево)
    [SerializeField, Range(0f, 90f)] private float maxAngleRight = 15f;  // полож. (вправо)

    [Header("Top motion")]
    [SerializeField, Range(0f, 90f)] private float idleAmplitude = 15f;  // базовая амплитуда (град)
    [SerializeField, Min(0.01f)] private float cycleSeconds = 1.0f;      // длительность полного цикла
    [SerializeField, Range(0f, 0.45f)] private float endHold01 = 0.30f;  // пауза на концах (доля цикла)
    [SerializeField, Range(0.2f, 2.5f)] private float holdSharpness = 1.6f; // резкость входа/выхода из паузы

    [Header("React to movement")]
    [SerializeField, Min(0.01f)] private float maxSpeedForFull = 6f;
    [SerializeField, Range(0f, 1f)] private float poseFromMove = 0.50f;  // смещение позы от движения
    [SerializeField, Range(0f, 3f)] private float ampFromMove = 2.0f;    // усиление амплитуды от движения

    [Header("Top spring")]
    [SerializeField, Min(0f)] private float topSpring = 70f;
    [SerializeField, Min(0f)] private float topDamp = 10f;

    [Header("Tail lag (S-shape)")]
    [SerializeField, Range(0f, 1f)] private float tailFollow = 0.88f;
    [SerializeField, Range(0f, 1f)] private float tailLag = 0.85f;

    [Header("Tail extra swing")]
    [SerializeField, Range(0f, 2f)] private float tailExtraSwing = 1.0f;
    [SerializeField, Range(0f, 1f)] private float tailExtraToTop = 0.15f;
    [SerializeField, Min(0f)] private float tailDamp = 12f;

    [Header("Movement source (event)")]
    [SerializeField] private MonoBehaviour movementSourceBehaviour;
    private IMovementSource movementSource;

    private Vector2 currentVelocity;

    private float[] _ang;
    private float[] _angVel;
    private Quaternion[] _baseLocalRot;

    private float _phase;     // 0..1
    private Vector3 _prevPos; // fallback velocity
    private bool _hasSource;

    private void Awake()
    {
        int n = segments != null ? segments.Length : 0;
        _ang = new float[n];
        _angVel = new float[n];
        _baseLocalRot = new Quaternion[n];

        if (segments != null)
        {
            for (int i = 0; i < n; i++)
                _baseLocalRot[i] = segments[i] != null ? segments[i].localRotation : Quaternion.identity;
        }

        _prevPos = transform.position;
    }

    private void OnEnable()
    {
        movementSource = movementSourceBehaviour as IMovementSource;
        if (movementSource == null)
            movementSource = GetComponentInParent<IMovementSource>();

        _hasSource = (movementSource != null);

        if (_hasSource)
            movementSource.OnVelocityChanged += OnVelocityChanged;

        Debug.Log($"[Cloak] movementSource found = {_hasSource}");
    }

    private void OnDisable()
    {
        if (_hasSource)
            movementSource.OnVelocityChanged -= OnVelocityChanged;

        currentVelocity = Vector2.zero;
        _hasSource = false;
    }

    private void OnVelocityChanged(Vector2 v) => currentVelocity = v;

    private void LateUpdate()
    {
        // Pivot крутим в LateUpdate, чтобы не спорить с другими системами визуала
        if (pivot != null)
            pivot.localRotation = Quaternion.Euler(0f, 0f, pivotRotationDeg);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f || segments == null || segments.Length == 0) return;

        // если поменял segments в инспекторе во время Play
        if (_ang == null || _ang.Length != segments.Length)
        {
            _ang = new float[segments.Length];
            _angVel = new float[segments.Length];
            _baseLocalRot = new Quaternion[segments.Length];

            for (int i = 0; i < segments.Length; i++)
                _baseLocalRot[i] = segments[i] != null ? segments[i].localRotation : Quaternion.identity;
        }

        // скорость: event или fallback
        Vector2 vel = currentVelocity;
        if (!_hasSource)
        {
            Vector3 p = transform.position;
            vel = (Vector2)((p - _prevPos) / dt);
            _prevPos = p;
        }

        float moveX = Mathf.Clamp(vel.x / maxSpeedForFull, -1f, 1f);
        float moveAbs = Mathf.Abs(moveX);

        // поза от движения (вправо => плащ влево)
        float poseMax = (moveX >= 0f) ? maxAngleLeft : maxAngleRight;
        float pose = -moveX * poseMax * poseFromMove;

        // осциллятор с паузами
        _phase += dt / Mathf.Max(0.0001f, cycleSeconds);
        if (_phase >= 1f) _phase -= 1f;

        float osc = OscHold01(_phase, endHold01, holdSharpness); // -1..+1
        float amp = idleAmplitude * (1f + ampFromMove * moveAbs);

        float topTarget = pose + osc * amp;

        // TOP
        ApplySpringToSegment(0, topTarget, dt, topSpring, topDamp);

        // TAIL
        int last = segments.Length - 1;
        for (int i = 1; i < segments.Length; i++)
        {
            float t = (last <= 0) ? 1f : (float)i / last;
            float lag = Mathf.Lerp(0.05f, tailLag, t);

            float localSpring = Mathf.Lerp(topSpring * 0.85f, topSpring * 0.20f, lag);
            float localDamping = Mathf.Lerp(topDamp, tailDamp, lag);

            float prev = _ang[i - 1];

            float extraW = Mathf.Lerp(tailExtraToTop, 1f, t);
            float extra = (prev - _ang[i]) * tailExtraSwing * extraW;

            float segTarget = prev * tailFollow + topTarget * (1f - tailFollow) * 0.10f + extra;

            ApplySpringToSegment(i, segTarget, dt, localSpring, localDamping);
        }
    }

    private void ApplySpringToSegment(int i, float target, float dt, float spring, float damping)
    {
        float a = _ang[i];
        float v = _angVel[i];

        float force = (target - a) * spring;
        v += force * dt;
        v *= Mathf.Exp(-damping * dt);
        a += v * dt;

        a = Mathf.Clamp(a, -maxAngleLeft, maxAngleRight);

        _ang[i] = a;
        _angVel[i] = v;

        ApplyRotation(i, a);
    }

    private void ApplyRotation(int i, float a)
    {
        if (segments[i] == null) return;
        // ВАЖНО: pivotRotationDeg НЕ добавляем сюда — pivot уже повернут отдельно.
        segments[i].localRotation = _baseLocalRot[i] * Quaternion.Euler(0f, 0f, a);
    }

    private static float OscHold01(float phase01, float endHold01, float sharpness)
    {
        endHold01 = Mathf.Clamp(endHold01, 0f, 0.45f);
        sharpness = Mathf.Max(0.01f, sharpness);

        float hold = endHold01;
        float travel = Mathf.Max(0.0001f, 0.5f - hold);

        if (phase01 < hold) return 1f;

        if (phase01 < hold + travel)
        {
            float u = (phase01 - hold) / travel;
            u = PowEase(u, sharpness);
            return Mathf.Lerp(1f, -1f, u);
        }

        if (phase01 < hold + travel + hold) return -1f;

        {
            float start = hold + travel + hold;
            float u = (phase01 - start) / travel;
            u = PowEase(u, sharpness);
            return Mathf.Lerp(-1f, 1f, u);
        }
    }

    private static float PowEase(float u, float p)
    {
        u = Mathf.Clamp01(u);
        return Mathf.Pow(u, p);
    }
}