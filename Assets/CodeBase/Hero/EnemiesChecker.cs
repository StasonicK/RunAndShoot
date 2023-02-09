using System;
using System.Collections.Generic;
using CodeBase.Enemy;
using CodeBase.StaticData.Weapon;
using CodeBase.UI.Elements.Hud;
using UnityEngine;

namespace CodeBase.Hero
{
    public class EnemiesChecker : MonoBehaviour
    {
        [SerializeField] private LayerMask _enemyLayerMask;
        [SerializeField] private LayerMask _visibleObstaclesLayerMask;

        private HeroRotating _heroRotating;
        private HeroWeaponSelection _heroWeaponSelection;

        private int _enemiesHitsCount = 10;
        private float _sphereDistance = 0f;
        private float _distanceToEnemy = 0f;
        private float _sphereRadius = 1f;
        private float _checkEnemiesDelay = 0.1f;
        private float _fixedDeltaTimeCounter = 0f;
        private List<EnemyHealth> _enemies = new List<EnemyHealth>();
        private EnemyHealth _targetEnemy = null;

        public event Action<EnemyHealth> FoundClosestEnemy;
        public event Action EnemyNotFound;
        public event Action EnemyVisibilityChecked;

        private void Awake()
        {
            _heroWeaponSelection = GetComponent<HeroWeaponSelection>();
            _heroRotating = GetComponent<HeroRotating>();
            _heroWeaponSelection.WeaponSelected += SetWeaponAimRange;
            _heroRotating.ShootDirection += CheckEnemyVisibility;
        }

        private void SetWeaponAimRange(WeaponStaticData weaponStaticData, Transform transform) =>
            _sphereRadius = weaponStaticData.AimRange;

        private void FixedUpdate()
        {
            UpFixedTime();

            if (CheckFixedTimeCounter())
                CheckEnemiesAround();
        }

        private void UpFixedTime() =>
            _fixedDeltaTimeCounter += Time.fixedDeltaTime;

        private bool CheckFixedTimeCounter() =>
            _fixedDeltaTimeCounter >= _checkEnemiesDelay;

        private void CheckEnemiesAround()
        {
            _fixedDeltaTimeCounter = 0f;
            _enemies.Clear();
            RaycastHit[] enemiesHits = new RaycastHit[_enemiesHitsCount];
            int enemiesHitsCount = GetEnemiesHits(enemiesHits);

            CheckEnemiesHits(enemiesHitsCount, enemiesHits);
        }

        private int GetEnemiesHits(RaycastHit[] enemiesHits) =>
            Physics.SphereCastNonAlloc(transform.position, _sphereRadius, transform.forward, enemiesHits, _sphereDistance, _enemyLayerMask,
                QueryTriggerInteraction.UseGlobal);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Debug.DrawLine(transform.position, transform.position + Vector3.forward * _distanceToEnemy);
            Gizmos.DrawWireSphere(transform.position + Vector3.forward * _sphereDistance, _sphereRadius);
        }

        private void CheckEnemiesHits(int enemiesHitsCount, RaycastHit[] enemiesHits)
        {
            if (enemiesHitsCount > 0)
            {
                for (int i = 0; i < enemiesHitsCount; i++)
                {
                    EnemyHealth enemyHealth = enemiesHits[i].transform.gameObject.GetComponent<EnemyHealth>();

                    if (enemyHealth.Current > 0)
                        _enemies.Add(enemyHealth);
                }

                if (_enemies.Count > 0)
                    FindClosestEnemy(_enemies);
                else
                {
                    Debug.Log("Not Found");
                    EnemyNotFound?.Invoke();
                }
            }
        }

        private void FindClosestEnemy(List<EnemyHealth> visibleEnemies)
        {
            EnemyHealth closestEnemy = visibleEnemies[0];

            GetClosestEnemy(visibleEnemies, ref closestEnemy);

            if (closestEnemy != null)
            {
                if (_targetEnemy != closestEnemy || _targetEnemy == null)
                {
                    _targetEnemy = closestEnemy;

                    TurnOffAnotherTargets(visibleEnemies);
                    TurnOnTarget();

                    Debug.Log($"closestEnemy name {closestEnemy.transform.gameObject.name}");
                    FoundClosestEnemy?.Invoke(closestEnemy);
                }
            }
            else
            {
                Debug.Log("Not Found");
                EnemyNotFound?.Invoke();
            }
        }

        private void GetClosestEnemy(List<EnemyHealth> visibleEnemies, ref EnemyHealth closestEnemy)
        {
            float minDistance = 500f;

            if (visibleEnemies.Count > 1)
            {
                foreach (EnemyHealth enemy in visibleEnemies)
                {
                    float distanceToEnemy = Vector3.Distance(enemy.transform.position, transform.position);

                    if (distanceToEnemy < minDistance)
                    {
                        minDistance = distanceToEnemy;
                        closestEnemy = enemy;
                    }
                }
            }
        }

        private void TurnOffAnotherTargets(List<EnemyHealth> visibleEnemies)
        {
            foreach (EnemyHealth enemy in visibleEnemies)
            {
                if (_targetEnemy != enemy)
                    enemy.transform.GetComponentInChildren<Target>().Hide();
            }
        }

        private void TurnOnTarget() =>
            _targetEnemy.transform.GetComponentInChildren<Target>().Show();

        private void CheckEnemyVisibility(Vector3 enemyPosition)
        {
            Vector3 direction = (enemyPosition - transform.position).normalized;
            _distanceToEnemy = Vector3.Distance(enemyPosition, transform.position);
            RaycastHit[] raycastHits = Physics.RaycastAll(transform.position, direction, _distanceToEnemy, _visibleObstaclesLayerMask,
                QueryTriggerInteraction.UseGlobal);

            if (raycastHits.Length == 0)
            {
                Debug.Log("EnemyVisibilityChecked");
                EnemyVisibilityChecked?.Invoke();
            }
            else
            {
                Debug.Log("EnemyNotFound");
                EnemyNotFound?.Invoke();
            }
        }
    }
}