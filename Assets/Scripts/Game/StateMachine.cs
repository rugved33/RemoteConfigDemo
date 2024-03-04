using UnityEngine;
using System;

public class StateMachine
{
    private int _currentState = -1;
    private int _nextState = -1;
    private int _lastState = -1;

    private bool _debug = false;

    public delegate void Function();

    private Function[] _onEnterFunc;
    private Function[] _onExitFunc;
    private Function[] _onUpdateFunc;
    private Function[] _onFixedUpdateFunc;

    private float _timer = 0;
    private float _fixedTime = 0;


    public StateMachine(int nStates, bool debug = false)
    {
        _onEnterFunc = new Function[nStates];
        _onExitFunc = new Function[nStates];
        _onUpdateFunc = new Function[nStates];
        _onFixedUpdateFunc = new Function[nStates];

        _debug = debug;
    }

    public void RegisterState(Enum State, Function OnEnter = null, Function OnUpdate = null, Function OnExit = null, Function OnFixedUpdate = null)
    {
        _onEnterFunc[Convert.ToInt32(State)] = OnEnter;
        _onExitFunc[Convert.ToInt32(State)] = OnExit;
        _onUpdateFunc[Convert.ToInt32(State)] = OnUpdate;
        _onFixedUpdateFunc[Convert.ToInt32(State)] = OnFixedUpdate;
    }

    public void SetState(Enum state)
    {
        int newState = Convert.ToInt32(state);
        _nextState = newState;
        if (_debug)
        {
            Debug.Log("SET From " + _currentState + " to " + _nextState);
        }
    }

    public bool IsState(Enum state)
    {
        return _currentState == Convert.ToInt32(state);
    }
    public int GetState()
    {
        return _currentState;
    }
    public int GetLastState()
    {
        return _lastState;
    }
    public float GetStateTime()
    {
        return _timer;
    }
    public float GetStateTimeFixed()
    {
        return _fixedTime;
    }

    public void Update()
    {
        _timer += Time.deltaTime;
        if (_currentState != _nextState)
        {
        if (_currentState >= 0)
        {
            Execute(_onExitFunc[_currentState]); //OnExit
        }
        if (_debug) Debug.Log("From " + _currentState + " to " + _nextState);
        _lastState = _currentState;
        _currentState = _nextState;
        Execute(_onEnterFunc[_currentState]); //OnEnter
        _timer = 0;
        _fixedTime = 0;
        }
        Execute(_onUpdateFunc[_currentState]); //OnUpdate
    }

    public void FixedUpdate()
    {
        if (_currentState >= 0)
        {
        _fixedTime += Time.fixedDeltaTime;
        Execute(_onFixedUpdateFunc[_currentState]); //OnFixedUpdate
        }
    }

    private void Execute(Function func)
    {
        if (func != null)
        {
        func();
        }
    }
}

