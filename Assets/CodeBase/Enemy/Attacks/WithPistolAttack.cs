using CodeBase.Weapons;
using UnityEngine;

namespace CodeBase.Enemy.Attacks
{
    public class WithPistolAttack : Attack
    {
        private EnemyWeaponAppearance _enemyWeaponAppearance;

        private void Awake() =>
            _enemyWeaponAppearance = GetComponentInChildren<EnemyWeaponAppearance>();

        public void Construct(Transform heroTransform, float attackCooldown) => base.Construct(heroTransform, attackCooldown);

        protected override void OnAttack()
        {
            _enemyWeaponAppearance.Shoot(1, 1);
            Debug.Log($"{gameObject.name} shoot Hero");
        }
    }
}