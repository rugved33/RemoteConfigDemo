using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class SwordsMan : CharacterBehaviour
{
    private enum NPCState
    {
        IDLE,
        CHASE,
        ATTACK,
        DIE,
        WAIT,
        SIZE,
    }

    private const string ANIM_HUMAN_STRIKE_NAME = "HumanStrike2";
    private const string ANIM_DEATH1 = "DEATH1";
    private const string ANIM_RUN = "RUN";
    private const string ANIM_IDLE = "IDLE";

    [SerializeField] private GameObject _chassis;
    [SerializeField] private AnimatorHelper _animatorHelper;
    [SerializeField] private string _state;
    private Quaternion _defaultLocalRotation;
    private StateMachine _stateMachine;
    private NavMeshAgent _navmesh;
    private float _timer;
    private float _waitTime = 1;


    public override void Awake()
    {
        base.Awake();
        _navmesh = GetComponent<NavMeshAgent>();
        _navmesh.speed = _settings.MovementSpeed;
        _defaultLocalRotation = _chassis.transform.localRotation;
        OnAttacked += SetToWaitState;
        BootStates();
    }

    private void BootStates()
    {
        _stateMachine = new StateMachine((int)NPCState.SIZE, false);
        _stateMachine.RegisterState(NPCState.IDLE, OnIdleEnter, OnIdleUpdate, null);
        _stateMachine.RegisterState(NPCState.CHASE, OnChaseEnter, OnChaseUpdate, null);
        _stateMachine.RegisterState(NPCState.ATTACK, OnAttackEnter, OnAttackUpdate, null);
        _stateMachine.RegisterState(NPCState.DIE, OnDieEnter, OnDieUpdate);
        _stateMachine.RegisterState(NPCState.WAIT, OnWaitEnter, OnWaitUpdate, OnWaitExit);
        _stateMachine.SetState(NPCState.IDLE);
    }

    private void FixedUpdate()
    {
        if (_stateMachine != null)
        {
            _stateMachine.Update();
            _state = "" + (NPCState)_stateMachine.GetState();
        }
        ProcessWalkingAnimation();
    }

    private void ProcessWalkingAnimation()
    {
        if (IsAlive)
        {
            if (_currentTarget == null || !_currentTarget.IsAlive)
            {
                _stateMachine.SetState(NPCState.IDLE);
            }
            if (_stateMachine.GetState() != (int)NPCState.ATTACK)
            {
                if (_navmesh.velocity.magnitude > 0.5f)
                {
                    var name = _animatorHelper.GetCurrentStateName();

                    if (name != ANIM_RUN)
                        _animatorHelper.Play(ANIM_RUN);
                }
                else
                {
                    var name = _animatorHelper.GetCurrentStateName();

                    if (name != ANIM_IDLE)
                        _animatorHelper.Play(ANIM_IDLE);
                }
            }
        }
    }

    #region  STATES

    private void OnIdleEnter()
    {
        ResetChassisRotation();
        StopCoroutine(Co_Attack());
        //_navmesh.destination = _originalPosition;
    }

    private void OnIdleUpdate()
    {
        FindEnemyTarget();
        if (_currentTarget != null)
        {
            _stateMachine.SetState(NPCState.CHASE);
        }
    }

    private void OnChaseEnter()
    {
        ResetChassisRotation();
    }

    private void OnChaseUpdate()
    {
        FindEnemyTarget();

        if (_currentTarget != null)
        {
            if (_navmesh != null)
            {
                if (_navmesh.enabled)
                    _navmesh.destination = _currentTarget.transform.position;
            }
        }

        if (IsEnemyInRange())
        {
            _stateMachine.SetState(NPCState.ATTACK);
        }
    }

    private void OnAttackEnter()
    {

    }

    private void OnAttackUpdate()
    {
        var name = _animatorHelper.GetCurrentStateName();

        if (name != ANIM_HUMAN_STRIKE_NAME)
            _animatorHelper.Play(ANIM_HUMAN_STRIKE_NAME, OnAttacked);

        FindEnemyTarget();
        if (IsEnemyInRange())
        {
            RotateTowardsTarget();
        }
        else
        {
            _stateMachine.SetState(NPCState.CHASE);
        }
    }
    private void OnWaitEnter()
    {
        if (_navmesh.enabled) { _navmesh.destination = transform.position; }
        StopCoroutine(Co_Attack());
        if (name != ANIM_IDLE)
            _animatorHelper.Play(ANIM_IDLE, null, 0, false, 5);
    }
    private void OnWaitUpdate()
    {
        _timer += Time.deltaTime;
        if (_timer > _waitTime)
        {
            _timer = 0;
            _stateMachine.SetState(NPCState.ATTACK);
        }
        if (IsOutOfSight())
        {
            _currentTarget = null;
            _stateMachine.SetState(NPCState.IDLE);
        }
    }
    private void OnWaitExit()
    {

    }

    private void OnDieEnter()
    {
        var name = _animatorHelper.GetCurrentStateName();

        if (name != ANIM_DEATH1)
            _animatorHelper.Play(ANIM_DEATH1);

        IsAlive = false;
        _currentTarget = null;
    }

    private void OnDieUpdate()
    {
        var name = _animatorHelper.GetCurrentStateName();

        if (name != ANIM_DEATH1)
            _animatorHelper.Play(ANIM_DEATH1);
    }


    #endregion

    public override void Kill()
    {
        base.Kill();
        _stateMachine.SetState(NPCState.DIE);
        DisableComponents();
    }

    private void RotateTowardsTarget()
    {
        if (_currentTarget != null)
        {
            Vector3 targetDirection = _currentTarget.transform.position - transform.position;
            float step = RotateSpeed * Time.deltaTime;
            Vector3 lookDirection = Vector3.RotateTowards(_chassis.transform.forward, targetDirection, step, 0.0f);
            lookDirection.y = 0;
            _chassis.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private void ResetChassisRotation()
    {
        float step = RotateSpeed * Time.deltaTime;
        _chassis.transform.localRotation = Quaternion.Lerp(_chassis.transform.localRotation, _defaultLocalRotation, step);
    }

    private void SetToWaitState()
    {
        if (_animatorHelper.GetCurrentStateName() == ANIM_HUMAN_STRIKE_NAME)
        {
            AttackCurrentTarget();
            _stateMachine.SetState(NPCState.WAIT);
        }
    }
    public override void RegisterAttacker(CharacterBehaviour characterBehaviour)
    {
        if (_currentTarget == null)
            _currentTarget = characterBehaviour;
    }
}
