using System.Collections;
using System;
using System.Collections.Generic;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorHelper : MonoBehaviour
{
    [System.Serializable]
    public class AnimationHelperState
    {
        public string animationName;

        [System.NonSerialized]
        public int animationNameHash = -1;

        public int GetAnimNameHash()
        {
            if (animationNameHash == -1)
            {
                animationNameHash = Animator.StringToHash(animationName);
            }

            return animationNameHash;
        }

        public override string ToString()
        {
            return animationName;
        }
    }

    [System.Serializable]
    private class LayerData
    {
        public AnimationHelperState activeAnimationState;
        public System.Action animationCompleteCallback = null;
        public float currentlClipTime = 0f;
        public float startTime = 0f;
        public AnimationHelperState nextAnimationState;
        public bool animationFinished = false;
        public bool animationStarted = false;


        public LayerData(AnimationHelperState state, System.Action callback, float startTime)
        {
            this.nextAnimationState = state;
            this.animationCompleteCallback = callback;
            this.startTime = startTime;
            this.animationFinished = false;
            this.animationStarted = false;
        }

        public void SetupNewActiveState(AnimationHelperState newActive)
        {
            this.currentlClipTime = this.startTime;
            this.activeAnimationState = newActive;
            this.nextAnimationState = null;
            this.animationFinished = false;
            this.animationStarted = true;
        }
    }

    [SerializeField] private bool _continueAnimationOnEnable = true;
    [SerializeField] private bool _turnOnAnimatorOnEnable = true;

    [SerializeField] private Animator animator;
    [SerializeField, ReadOnly] private List<AnimationHelperState> _animStates = new List<AnimationHelperState>();
    private Dictionary<int, LayerData> _animatorLayerDatas = new Dictionary<int, LayerData>();
    private List<int> _activeKeys = new List<int>();

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnEnable()
    {

        // automatically switch the animator on incase it accidentally gets turned off in the editor and then checked in.
        if (animator != null)
        {
            animator.enabled = _turnOnAnimatorOnEnable;

            foreach (int layer in _animatorLayerDatas.Keys)
            {
                AnimationHelperState activeState = _animatorLayerDatas[layer].activeAnimationState
                                                   ?? _animatorLayerDatas[layer].nextAnimationState;
                if (activeState != null && _continueAnimationOnEnable)
                {
                    animator.Play(activeState.animationName, layer, _animatorLayerDatas[layer].currentlClipTime);

                }
            }
        }
        animator.enabled = true;
    }

    private void OnApplicationFocus()
    {
        if (animator != null)
        {
            animator.enabled = true;
        }
    }

    private void OnDisable()
    {
        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    public List<AnimationHelperState> GetAnimationList()
    {
        return _animStates;
    }

    public void SetStates(List<string> allStates)
    {
        _animStates.Clear();
        if (allStates == null)
        {
            return;
        }

        for (int i = 0; i < allStates.Count; i++)
        {
            AnimationHelperState state = new AnimationHelperState();
            state.animationName = allStates[i];
            state.GetAnimNameHash();

            _animStates.Add(state);
        }
    }

    public IList<AnimationHelperState> AnimStates => _animStates;

    public void SetAnimator(Animator animator)
    {
        this.animator = animator;
    }

    public Animator GetAnimator()
    {
        return animator;
    }

    public void ChangeAnimaState(int index, string animationName)
    {
        AnimationHelperState animState = _animStates[index];
        animState.animationName = animationName;
        animState.animationNameHash = -1;
    }

    private void PlayAnimationInternal(AnimationHelperState state, Action onFinished = null, float startTime = 0, float crossfade = 0, int layer = 0)
    {
        if (state == null)
        {
            if (onFinished != null)
            {
                onFinished();
            }

            return;
        }

        LayerData newData = new LayerData(state, onFinished, startTime);
        newData.currentlClipTime = 0;

        Animator anim = GetAnimator();
        AnimatorStateInfo animInfo = anim.GetCurrentAnimatorStateInfo(layer);

        if (_animatorLayerDatas.ContainsKey(layer))
        {
            _animatorLayerDatas[layer] = newData;
        }
        else
        {
            _animatorLayerDatas.Add(layer, newData);
        }

        if (!_activeKeys.Contains(layer))
        {
            _activeKeys.Add(layer);
        }

        if (crossfade > 0.0f)
        {
            animator.CrossFade(state.animationName, crossfade, layer, 0f);
        }
        else
        {
            animator.Play(state.animationName, layer, startTime);
        }
    }

    private void Update()
    {
        if (ShouldUpdateAnimState())
        {
            UpdateAnimState();
        }
    }

    private bool ShouldUpdateAnimState()
    {
        return animator != null;
    }

    public void Play(string animationName, Action onFinished = null, float startTime = 0f, bool checkIsPlayingFirst = false, float crossFade = 0f, int layer = 0)
    {
        if (!animator.enabled)
        {

        }
        else
        {
            //Shoud we check if the next animation to play is ALREADY playing?
            if (checkIsPlayingFirst)
            {
                Animator anim = GetAnimator();
                AnimatorStateInfo animInfo = anim.GetCurrentAnimatorStateInfo(layer);
                bool isCurentAnimPlating = animInfo.IsName(animationName);

                if (isCurentAnimPlating)
                {

                    return;
                }
            }

            if (gameObject.activeSelf && gameObject.activeInHierarchy)
            {
                AnimationHelperState animState = GetAnimStateFromAnimationName(animationName);

                if (animState != null)
                {

                    if (animator.HasState(layer, Animator.StringToHash(animationName)))
                    {
                        PlayAnimationInternal(animState, onFinished, startTime, crossFade, layer);
                    }
                    else
                    {

                    }
                }
            }
            else
            {

            }
        }
    }

    private void AnimationFinished(int layer)
    {
        if (_animatorLayerDatas.ContainsKey(layer) && _activeKeys.Contains(layer))
        {
            _activeKeys.Remove(layer);

            System.Action action = _animatorLayerDatas[layer].animationCompleteCallback;
            if (action != null)
            {
                _animatorLayerDatas[layer].animationCompleteCallback = null;
                action();
            }
        }
    }

    public bool HasAnimationStateWithName(string stateName)
    {
        return GetAnimStateFromAnimationName(stateName) != null;
    }

    private AnimationHelperState GetAnimStateFromAnimationName(string animationName)
    {
        foreach (AnimationHelperState animState in _animStates)
        {
            if (animState.animationName == animationName)
            {
                return animState;
            }
        }

        return null;
    }

    private AnimationHelperState GetAnimStateFromAnimationNameHash(int animationNameHash)
    {
        foreach (AnimationHelperState animState in _animStates)
        {
            if (animState.GetAnimNameHash() == animationNameHash)
            {
                return animState;
            }
        }

        return null;
    }

    private void UpdateAnimState()
    {
        int numKeys = _activeKeys.Count;
        for (int i = 0; i < numKeys; i++)
        {
            if (i >= _activeKeys.Count)
            {
                continue;
            }

            int layer = _activeKeys[i];
            AnimatorStateInfo layerStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
            _animatorLayerDatas[layer].currentlClipTime = layerStateInfo.normalizedTime;

            AnimationHelperState nextState = _animatorLayerDatas[layer].nextAnimationState;

            if (nextState != null)
            {
                if (layerStateInfo.shortNameHash == nextState.GetAnimNameHash())
                {
                    _animatorLayerDatas[layer].SetupNewActiveState(nextState);
                }
            }
            else
            {
                bool hasFinished = UpdateAnimFinished(layerStateInfo, layer);

                if (!hasFinished)
                {
                    UpdateAnimStarted(layerStateInfo, layer);
                }
            }

        }
    }

    private void UpdateAnimStarted(AnimatorStateInfo stateInfo, int layer)
    {
        int animStateShortNameHash = stateInfo.shortNameHash;

        AnimationHelperState activeStateOnLayer = _animatorLayerDatas[layer].activeAnimationState;
        int currentAnimStateShortNameHash = activeStateOnLayer != null ? activeStateOnLayer.GetAnimNameHash() : -1;

        if (animStateShortNameHash != currentAnimStateShortNameHash)
        {
            AnimationHelperState helperState = GetAnimStateFromAnimationNameHash(animStateShortNameHash);
            if (helperState != null)
            {
                _animatorLayerDatas[layer].SetupNewActiveState(helperState);
            }
        }
    }

    private bool UpdateAnimFinished(AnimatorStateInfo stateInfo, int layer)
    {
        int animStateShortNameHash = stateInfo.shortNameHash;
        AnimationHelperState activeStateOnLayer = _animatorLayerDatas[layer].activeAnimationState;
        int currentAnimStateShortNameHash = activeStateOnLayer != null ? activeStateOnLayer.GetAnimNameHash() : -1;

        if (!_animatorLayerDatas[layer].animationFinished)
        {
            AnimatorClipInfo[] animClipInfos = animator.GetCurrentAnimatorClipInfo(layer);
            bool animClipHasFrames = animClipInfos.Length > 0 && animClipInfos[0].clip.length <= Mathf.Epsilon;

            if (stateInfo.normalizedTime >= 1f || animStateShortNameHash != currentAnimStateShortNameHash || animClipHasFrames)
            {
                _animatorLayerDatas[layer].animationFinished = true;
                AnimationFinished(layer);
                return true;
            }
        }

        return false;
    }

    public AnimatorControllerParameter[] GetAnimatorParameters()
    {
#if UNITY_EDITOR
        if (animator == null)
        {
            return null;
        }

        UnityEditor.Animations.AnimatorController editorAnimatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
        if (editorAnimatorController != null)
        {
            return editorAnimatorController.parameters;
        }
#endif
        return null;
    }

    //Helper functions
    public string GetCurrentStateName(int layer = 0)
    {
        if (_animatorLayerDatas.ContainsKey(layer))
        {
            AnimationHelperState activeState = _animatorLayerDatas[layer].activeAnimationState;
            if (activeState != null)
            {
                return activeState.animationName;
            }
        }

        return "";
    }

    public float GetCurrentStateNormalisedTime(int layer = 0)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
        return stateInfo.normalizedTime;
    }

    public void SetAnimatorSpeed(float speed)
    {
        animator.speed = speed;
    }

    public void StopAnimator()
    {
        animator.enabled = false;
    }

    //Params
    #region  Paramaters
    public void SetBool(string key, bool value)
    {
        if (animator == null)
        {
            return;
        }
        animator.SetBool(key, value);
    }

    public void SetInt(string key, int value)
    {
        if (animator == null)
        {
            return;
        }
        animator.SetInteger(key, value);
    }

    public void SetFloat(string key, float value)
    {
        if (animator == null)
        {
            return;
        }
        animator.SetFloat(key, value);
    }

    public void SetTrigger(string key)
    {
        if (animator == null)
        {
            return;
        }
        animator.SetTrigger(key);
    }
    #endregion
}
