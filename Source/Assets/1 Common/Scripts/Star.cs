using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace GalaxyRush
{
    public class Star : MonoBehaviour
    {
        public void Initialize()
        {
        }
        public void Update()
        {
            transform.Rotate(Vector3.up * Time.deltaTime * 50);
        }

        public void Hit()
        {
            gameObject.SetActive(false);

            if (transform.localScale.x < 1)
                Global.game.AddScore(200, "Destroyed small star");
            else
                Global.game.AddScore(600, "Destroyed large star");
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Projectile")
                Hit();
        }
    }
}