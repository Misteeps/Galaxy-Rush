using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace GalaxyRush
{
    public class Target : MonoBehaviour
    {
        public Obstacle obstacle;
        public float resetTime;
        public Color normalBase = new Color(0, 0, 0, 0.4f);
        public Color normalColor = new Color(1, 0, 0, 1);
        public Color hitBase = new Color(0, 0, 0, 0);
        public Color hitColor = new Color(0, 0, 0, 0);

        [Header("Readonly")]
        public bool hit;
        public float timer;

        public new MeshRenderer renderer;
        public new Collider collider;
        public MaterialPropertyBlock mat;


        public void Initialize()
        {
            renderer = GetComponent<MeshRenderer>();
            collider = GetComponent<Collider>();
            mat = new MaterialPropertyBlock();
        }
        public void ResetValues()
        {
            hit = false;
            timer = 0;
            SetColors(normalBase, normalColor);

            collider.enabled = true;
            enabled = false;
        }

        public void Update()
        {
            timer += Time.deltaTime;
            if (timer >= resetTime)
                Hit(false);
        }

        public void Hit(bool value)
        {
            if (hit == value) return;
            hit = value;

            if (value)
            {
                obstacle?.TargetHit(1);
                SetColors(hitBase, hitColor);
                timer = 0;
                collider.enabled = false;

                if (resetTime != 0)
                    enabled = true;
            }
            else
            {
                obstacle?.TargetHit(-1);
                SetColors(normalBase, normalColor);
                timer = 0;
                collider.enabled = true;

                enabled = false;
            }
        }
        public void SetColors(Color _base, Color _color)
        {
            mat.SetColor("Base", _base);
            mat.SetColor("_Color", _color);

            renderer.SetPropertyBlock(mat, 1);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Projectile")
                Hit(true);
        }
    }
}