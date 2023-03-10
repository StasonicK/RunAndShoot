using System.Collections;
using CodeBase.Logic;
using CodeBase.UI.Elements.Hud;
using UnityEngine;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyDeath : MonoBehaviour, IDeath
    {
        private IHealth _health;

        private const float UpForce = 500f;

        private Rigidbody _rigidbody;
        private float _deathDelay = 5f;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _health = GetComponent<IHealth>();
            _health.Died += ForceUp;
        }

        private void ForceUp()
        {
            transform.GetComponentInChildren<TargetMovement>().Hide();
            _rigidbody.AddForce(Vector3.up * UpForce, ForceMode.Force);
            StartCoroutine(CoroutineDestroyTimer());
        }

        public void Die()
        {
            _health.TakeDamage(100);
            // GetComponentInChildren<BoxCollider>().enabled = false;
        }

        private IEnumerator CoroutineDestroyTimer()
        {
            yield return new WaitForSeconds(_deathDelay);
            Destroy(gameObject);
        }
    }
}