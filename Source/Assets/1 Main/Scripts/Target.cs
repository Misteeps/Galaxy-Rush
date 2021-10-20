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

        public new MeshRenderer renderer;
        public MaterialPropertyBlock block;
        public Color normalBase = new Color(0, 0, 0, 0.4f);
        public Color normalColor = new Color(1, 0, 0, 1);
        public Color hitBase = new Color(0, 0, 0, 0);
        public Color hitColor = new Color(0, 0, 0, 0);


        public void Start()
        {
            enabled = false;
            renderer = GetComponent<MeshRenderer>();
            block = new MaterialPropertyBlock();
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
                obsticle.TargetHit(1);
                SetColors(hitBase, hitColor);
                timer = 0;

                if (resetTime != 0)
                    enabled = true;
            }
            else
            {
                obsticle.TargetHit(-1);
                SetColors(normalBase, normalColor);
                timer = 0;

                enabled = false;
            }
        }
        public void SetColors(Color _base, Color _color)
        {
            block.SetColor("Base", _base);
            block.SetColor("_Color", _color);

            renderer.SetPropertyBlock(block, 1);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Projectile")
                Hit(true);
        }
    }
}