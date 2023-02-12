﻿using UnityEngine;

namespace CodeBase.Projectiles
{
    public class BulletMovement : ProjectileMovement
    {
        private float _speed;
        private bool _move = false;

        private void Update()
        {
            if (_move) 
                transform.position += transform.forward * _speed * Time.deltaTime;
        }

        public void Construct(float speed, Transform parent)
        {
            _speed = speed * 1f;
            base.Construct(parent);
        }

        public override void Launch() =>
            _move = true;

        public override void Stop() =>
            _move = false;
    }
}