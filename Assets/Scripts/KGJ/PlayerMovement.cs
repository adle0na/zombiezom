using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private static readonly int Speed = Animator.StringToHash("Speed");
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D _rb;
    private Animator _ani;
    private SpriteRenderer _sr;
    private float _hInput;
    private bool _isFacingRight = true;
    private bool _canMove = true;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _ani = GetComponent<Animator>();
        _sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        _ani.SetFloat(Speed, Mathf.Abs(_rb.linearVelocityX));
        if (!_canMove)
        {
            _hInput = 0f; // 이동 금지 시 입력값 초기화
            return;
        }

        _hInput = Input.GetAxisRaw("Horizontal");

        if (_hInput > 0)
            _isFacingRight = true;
        else if (_hInput < 0)
            _isFacingRight = false;
    }

    void FixedUpdate()
    {
        // 이동 처리
        _rb.linearVelocity = new Vector2(_hInput * moveSpeed, _rb.linearVelocity.y);

        // 방향 반영
        _sr.flipX = !_isFacingRight;
    }

    public void EnableMove(bool enable)
    {
        _canMove = enable;

        if (!enable)
        {
            _rb.linearVelocity = Vector2.zero; // 완전히 멈추기
            _hInput = 0f;
        }
    }
}