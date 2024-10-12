using System;
using DefaultNamespace.CoreMechanicObjects;
using TMPro;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerController : Entity
{
    [Header("Главные настройки:")]
    // Настройки персонажа
    [SerializeField] private PlayerControllVars playerControllVars;
    // Количество монет
    private int Coins;

    public void AddCoin(int x = 1)
    {
        if (x > 0) Coins += x;
    }
    //Формула ускорения персонажа при разгоне (П.Ж ноль - Минимальная скорость, один - максимальная)
    [SerializeField] private AnimationCurve accelerateCurve;
    //Скорость(Множетель) разгона до максимальной сокрость
    [SerializeField, Range(0.1f, 3f)] private float accelerateSpeed = 100;
    [SerializeField] private Transform GroundPos;
    [SerializeField] private Camera CameraMain;
    [SerializeField] private TextMeshProUGUI CoinText;
    
    //Таймер для ускорения(Разгон до максимальной скорости)
    private float accelerateTime;
    //Передвижение
    private Vector3 movement;
    // RigidBody для физики персонажа
    private Rigidbody rb;

    private bool IsInWater;

    public delegate void OnMovementEvent();
    public delegate void OnJumpEvent();
    public delegate void OnHighJumpEvent();
    public delegate void OnSwimEvent();
    public delegate void OnClimbEvent();
    public delegate void OnFlyEvent();
    public delegate void OnCoinGetEvent(GameObject coin);

    public OnMovementEvent OnMovement;
    public OnJumpEvent OnJump;
    public OnHighJumpEvent OnHighJump;
    public OnSwimEvent OnSwim;
    public OnClimbEvent OnClimb;
    public OnFlyEvent OnFly;
    public OnCoinGetEvent OnCoinGet;


    // Состояния (P.S Почти рудимент не смог вписать в код)
    private enum State
    {
        Walking,
        Jumping,
        Swimming,
        Flying,
        HighJumping
    }

    //Текущие Состояние (P.S Почти рудимент не смог вписать в код)
    private State currentState;

    private void OnEnable()
    {
        OnCoinGet += CoinsUpdate;
    }

    private void OnDisable()
    {
        OnCoinGet -= CoinsUpdate;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        CameraMain = Camera.main;
        currentState = State.Walking;

        CoinsUpdate();
    }
    private void CoinsUpdate()
    {
        CoinText.text = $"Текст: {Coins}";
    }

    private void CoinsUpdate(GameObject coin)
    {
        CoinsUpdate();
        //coin. //Вызов партиклов
    }

    void Update()
    {
        WalkingUpdate();
        JumpingUpdate();
        SwimmingUpdate();
        FlyingUpdate();
        CoinsUpdate();
        //Его ищите в JumpingUpdate()
        //HighJumpingUpdate();
    }
    
    /// <summary>
    /// Тик ходьбы
    /// </summary>
    void WalkingUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        
        if (horizontalInput == 0)
        {
            accelerateTime = 0;
        }
        accelerateTime += Time.deltaTime;
        horizontalInput *= playerControllVars.maxSpeed * (accelerateCurve.Evaluate(accelerateTime) * accelerateSpeed);

        movement = new Vector3(horizontalInput, 0, 0);

        OnMovement?.Invoke();
        // Избегаем стен
        if (!IsWallInFront(movement))
        {
            // Двигаем персонажа по вектору с учётом колизии
            rb.MovePosition(transform.position + movement * Time.deltaTime);
            
            Vector3 targetPosition = transform.position + new Vector3(0, 0, -10); // задаем позицию камеры
            //CameraMain.transform.position =  Vector3.SmoothDamp(CameraMain.transform.position, targetPosition, ref velocity, 0.1f);
            CameraMain.transform.position = Vector3.Lerp(CameraMain.transform.position, targetPosition, 0.1f); // плавно перемещаем камеру
        }
    }

    /// <summary>
    /// Есть ли твёрдое перед игроком (P.S это для того чтобы сквозь стены, как через желе не ходить)
    /// </summary>
    bool IsWallInFront(Vector3 movement)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, movement, out hit, 0.1f))
        {
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Тик Прыжка
    /// </summary>
    void JumpingUpdate()
    {
        if (!playerControllVars.isJump)
        {
            return;
        }
        
        if (!playerControllVars.isHighJump)
        {
            HighJumpingUpdate();
            return;
        }
        
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            OnJump?.Invoke();
            rb.AddForce(Vector3.up * playerControllVars.jumpPower, ForceMode.Impulse);
            currentState = State.Jumping;
        }
    }

    /// <summary>
    /// Тик Плаванья
    /// </summary>
    void SwimmingUpdate()
    {
        if (!playerControllVars.isSwim)
        {
            rb.drag = 0f;
            return;
        }

        if (IsInWater)//Input.GetButtonDown("Swim")
        {
            if (Input.GetButtonDown("Jump"))
            {
                rb.velocity = new Vector3(0, Math.Min(rb.velocity.y+2, rb.velocity.y+playerControllVars.swimSpeed), 0);
            }

            //rb.velocity = new Vector3(0, playerControllVars.swimSpeed, 0);
            rb.drag = 0.1f;
            currentState = State.Swimming;
            
            OnSwim?.Invoke();
        }
        
    }

    /// <summary>
    /// Тик Полёта
    /// </summary>
    void FlyingUpdate()
    {
        if (!playerControllVars.isFly && IsGrounded())
        {
            return;
        }

        if (Input.GetButtonDown("Jump") && HasFlyTime())
        {
            OnFly?.Invoke();
            rb.velocity = new Vector3(0, 2, 0); //playerControllVars.flySpeed
            currentState = State.Flying;
        }
        else if (playerControllVars.flyTime == 0)
        {
            playerControllVars.flyTime = playerControllVars.flyMaxTime;
        }
    }

    /// <summary>
    /// Тик Высокого прыжка
    /// </summary>
    void HighJumpingUpdate()
    {
        /*if (!playerControllVars.isHighJump)
        {
            return;
        }*/

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            OnHighJump?.Invoke();
            rb.AddForce(Vector3.up * playerControllVars.jumpPower * 2, ForceMode.Impulse);
            currentState = State.HighJumping;
        }
    }

    void ClimingUpdate()
    {
        if (IsWallInFront(movement))
        {
            OnClimb?.Invoke();
            rb.velocity = new Vector3(0, playerControllVars.climbSpeed, 0);
        }
    }

    /// <summary>
    /// Проверка земли
    /// </summary>
    bool IsGrounded()
    {
        return Physics.Raycast(GroundPos.position, Vector3.down, 0.1f);
    }

    /// <summary>
    /// Для теста прыжка (TODO: Удалить)
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(GroundPos.position, transform.position + Vector3.down*0.1f);
    }

    /// <summary>
    /// Поверяет в в воде ли игрок
    /// </summary>
    /// <returns></returns>
    /*bool IsInWater()
    {
        return IsTriggering("Water", true);
    }*/

    
    /*bool coinCollected = false;
    
    /// <summary>
    /// Поверяет в монетке ли игрок
    /// </summary>
    /// <returns></returns>
    void CoinsUpdate()
    {
        if (!coinCollected)
        {
            IsTriggering("Coin", true, (hitCollider) =>
            {
                Debug.Log("Тронули Coin");
                // Удаляем монетку
                Destroy(hitCollider.gameObject);
                // Увеличиваем количество монет
                Coins++;
                CoinText.text = $"Текст: {Coins}";
                OnCoinGet?.Invoke();
                coinCollected = true;
            });
        }
    }*/


    /// <summary>
    /// </summary>
    /// <returns>осталось ли время для полёта</returns>
    bool HasFlyTime()
    {
        return playerControllVars.flyTime > 0;
    }


    /// <summary>
    /// </summary>
    /// <param name="tag">Тег обьекта с которым мы столкнулись</param>
    /// <param name="isTrigger">Состояние(триггер, колизия) обьекта с которым мы столкнулись</param>
    /// <returns>столкновение обьекта по тегу tag с виртуальной сферой</returns>
    /*bool IsTriggering(string tag, bool isTrigger = false, Action<Collider> action = null)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (var hitCollider in hitColliders)
        {
            //Debug.Log(hitCollider.tag);
            if (hitCollider.CompareTag(tag) && hitCollider.isTrigger == isTrigger)
            {
                if (action != null)
                {
                    action(hitCollider);
                }
                //Debug.Log(hitCollider.tag);
                return true;
            }
        }
        return false;
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<ITriggerZone>() is not null) other.GetComponent<ITriggerZone>().OnTouch(this);
        if (other.CompareTag("Water"))
        {
            IsInWater = true;
        }
    }
}