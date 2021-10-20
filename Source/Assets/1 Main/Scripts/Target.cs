using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace GalaxyRush
{
    public class Target : MonoBehaviour
    {
        public Obsticle obsticle;
        public bool hit;
        public float resetTime;
        public float timer;


        public void Start()
        {
            enabled = false;
        }

        public void Hit(bool value)
        {
            if (hit == value) return;
            hit = value;

            if (value)
            {
                obsticle.TargetHit(1);
                // Change Color via Material Property Block
                timer = 0;

                if (resetTime != 0)
                    enabled = true;
            }
            else
            {
                obsticle.TargetHit(-1);
                // Change Color via Material Property Block
                timer = 0;

                enabled = false;
            }
        }
        public void Update()
        {
            timer += Time.deltaTime;
            if (timer >= resetTime)
                Hit(false);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Projectile")
                Hit(true);
        }
    }
}