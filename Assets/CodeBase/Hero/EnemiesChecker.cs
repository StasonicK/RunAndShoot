using System;
using System.Collections.Generic;
using CodeBase.Enemy;
using CodeBase.Logic;
using CodeBase.StaticData.ProjectileTrace;
using CodeBase.StaticData.Weapon;
using CodeBase.UI.Elements.Hud;
using UnityEngine;

namespace CodeBase.Hero
{
    public class EnemiesChecker : MonoBehaviour
    {
        [SerializeField] private LayerMask _enemyLayerMask;
        [SerializeField] private LayerMask _visibleObstaclesLayerMask;
        [SerializeField] private HeroWeaponSelection _heroWeaponSelection;

        private int _enemiesHitsCount = 10;
        private float _sphereDistance = 0f;
        private float _distanceToEnemy = 0f;
        private float _aimRange = 1f;
        private float _checkEnemiesDelay = 0.2f;
        private float _checkEnemiesTimer = 0f;
        private List<EnemyHealth> _enemies = new List<EnemyHealth>();
        private string _targetEnemyId = null;
        private EnemyHealth _targetEnemy = null;
        private Vector3 _targetPosition;
        private string _enemyId = null;
        private bool _enemyNotFound = false;

        public event Action<GameObject> FoundClosestEnemy;
        public event Action EnemyNotFound;

        private void Awake()
        {
            _heroWeaponSelection.WeaponSelected += SetWeaponAimRange;
        }

        private void SetWeaponAimRange(GameObject weaponPrefab, HeroWeaponStaticData heroWeaponStaticData,
            ProjectileTraceStaticData projectileTraceStaticData) =>
            _aimRange = heroWeaponStaticData.AimRange;

        private void FixedUpdate()
        {
            UpFixedTime();

            if (IsCheckEnemiesTimerReached())
                CheckEnemiesAround();
        }

        private void UpFixedTime() =>
            _checkEnemiesTimer += Time.fixedDeltaTime;

        private bool IsCheckEnemiesTimerReached() =>
            _checkEnemiesTimer >= _checkEnemiesDelay;

        private void CheckEnemiesAround()
        {
            _checkEnemiesTimer = 0f;
            _enemies.Clear();
            RaycastHit[] enemiesHits = new RaycastHit[_enemiesHitsCount];
            int enemiesHitsCount = GetEnemiesHits(enemiesHits);

            CheckEnemiesHits(enemiesHitsCount, enemiesHits);
        }

        private int GetEnemiesHits(RaycastHit[] enemiesHits) =>
            Physics.SphereCastNonAlloc(transform.position, _aimRange, transform.forward, enemiesHits, _sphereDistance, _enemyLayerMask,
                QueryTriggerInteraction.UseGlobal);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Debug.DrawLine(transform.position, transform.position + Vector3.forward * _distanceToEnemy);
            Gizmos.DrawWireSphere(transform.position + Vector3.forward * _sphereDistance, _aimRange);
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
                {
                    _enemyNotFound = false;
                    FindClosestEnemy(_enemies);
                }
                else
                {
                    // Debug.Log($"CheckEnemiesHits NotFound");
                    NotFound();
                }
            }
            else
            {
                // Debug.Log($"CheckEnemiesHits NotFound");
                NotFound();
            }
        }

        private void FindClosestEnemy(List<EnemyHealth> visibleEnemies)
        {
            EnemyHealth closestEnemy = GetClosestEnemy(visibleEnemies);

            if (closestEnemy != null)
            {
                string id = closestEnemy.gameObject.GetComponent<UniqueId>().Id;

                if (_targetEnemyId != id || _targetEnemyId == null)
                {
                    _targetEnemyId = id;
                    _targetEnemy = closestEnemy;
                    _targetPosition = new Vector3(closestEnemy.transform.position.x, closestEnemy.transform.position.y, closestEnemy.transform.position.z);
                    CheckEnemyVisibility(closestEnemy);
                }

                // if (_targetEnemyId == id && _targetPosition != closestEnemy.transform.position)
                //     CheckEnemyVisibility(closestEnemy);
            }
            else
            {
                // Debug.Log($"FindClosestEnemy NotFound");
                NotFound();
            }
        }

        private void NotFound()
        {
            if (_enemyNotFound == false)
            {
                // Debug.Log("NotLookAtTarget");
                _enemyNotFound = true;
                _targetEnemyId = null;
                _targetEnemy = null;
                EnemyNotFound?.Invoke();
            }
        }

        private EnemyHealth GetClosestEnemy(List<EnemyHealth> visibleEnemies)
        {
            float minDistance = _aimRange;
            EnemyHealth closestEnemy = null;

            foreach (EnemyHealth enemy in visibleEnemies)
            {
                _distanceToEnemy = Vector3.Distance(enemy.transform.position, transform.position);

                if (_distanceToEnemy < minDistance)
                {
                    minDistance = _distanceToEnemy;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }

        private void CheckEnemyVisibility(EnemyHealth enemy)
        {
            Vector3 direction = (enemy.gameObject.transform.position - transform.position).normalized;
            RaycastHit[] raycastHits = Physics.RaycastAll(transform.position, direction, _distanceToEnemy, _visibleObstaclesLayerMask,
                QueryTriggerInteraction.UseGlobal);

            if (raycastHits.Length == 0)
            {
                TurnOffAnotherTargets(_enemies);
                TurnOnTarget();
                FoundClosestEnemy?.Invoke(enemy.gameObject);
            }
            else
            {
                // Debug.Log($"CheckEnemyVisibility NotFound");
                NotFound();
            }
        }

        private void TurnOffAnotherTargets(List<EnemyHealth> visibleEnemies)
        {
            foreach (EnemyHealth enemy in visibleEnemies)
                if (_targetEnemyId != enemy.gameObject.GetComponent<UniqueId>().Id)
                    enemy.transform.GetComponentInChildren<TargetMovement>().Hide();
        }

        private void TurnOnTarget() =>
            _targetEnemy.transform.GetComponentInChildren<TargetMovement>().Show();
    }
}