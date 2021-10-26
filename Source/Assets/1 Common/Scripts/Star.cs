using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace GalaxyRush
{
    public class Star : MonoBehaviour
    {
        public float scale;

        public void Initialize()
        {
        }
        public void Update()
        {
            transform.Rotate(Vector3.up * Time.deltaTime * 50);
        }

        public void Hit()
        {
            Global.game.PlaySound("Star Break", transform.position);

            Transition.Add(gameObject, TransitionComponents.Scale, TransitionUnits.X, EaseFunctions.Bounce, EaseDirections.In, 1, 0, 0.3f, true);
            Transition.Add(gameObject, TransitionComponents.Scale, TransitionUnits.Y, EaseFunctions.Bounce, EaseDirections.In, 1, 0, 0.3f, true);
            Transition.Add(gameObject, TransitionComponents.Scale, TransitionUnits.Z, EaseFunctions.Bounce, EaseDirections.In, 1, 0, 0.3f, true);

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