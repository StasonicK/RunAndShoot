using System;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyAnimator : MonoBehaviour, IAnimationStateReader
    {
        private readonly int _attackTriggerHash = Animator.StringToHash("Attack");
        private readonly int _isMovingTriggerHash = Animator.StringToHash("IsMoving");

        private readonly int _idleStateHash = Animator.StringToHash("Idle");
        private readonly int _attackStateHash = Animator.StringToHash("Attack");
        private readonly int _walkingStateHash = Animator.StringToHash("Move");

        private Animator _animator;

        public event Action<AnimatorState> StateEntered;
        public event Action<AnimatorState> StateExited;

        public AnimatorState State { get; private set; }

        private void Awake() =>
            _animator = GetComponent<Animator>();

        public void Move() =>
            _animator.SetBool(_isMovingTriggerHash, true);

        public void StopMoving() =>
            _animator.SetBool(_isMovingTriggerHash, false);

        public void PlayAttack() =>
            _animator.SetTrigger(_attackTriggerHash);

        public void EnteredState(int stateHash)
        {
            State = StateFor(stateHash);
            StateEntered?.Invoke(State);
        }

        public void ExitedState(int stateHash) =>
            StateExited?.Invoke(StateFor(stateHash));

        private AnimatorState StateFor(int stateHash)
        {
            AnimatorState state;

            if (stateHash == _idleStateHash)
                state = AnimatorState.Idle;
            else if (stateHash == _attackStateHash)
                state = AnimatorState.Attack;
            else if (stateHash == _walkingStateHash)
                state = AnimatorState.Walking;
            else
                state = AnimatorState.Unknown;

            return state;
        }
    }
}