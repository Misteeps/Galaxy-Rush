using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace GalaxyRush
{
    public class Obstacle : MonoBehaviour
    {
        #region Object
        [Serializable]
        public class Object
        {
            public GameObject gameObject;
            [HideInInspector] public Vector3 position;
            [HideInInspector] public Vector3 rotation;
            [HideInInspector] public Vector3 scale;

            public void GetDefaults()
            {
                position = gameObject.transform.position;
                rotation = gameObject.transform.localEulerAngles;
                scale = gameObject.transform.localScale;
            }
            public void Reset()
            {
                SetPosition(position);
                SetRotation(rotation);
                SetScale(scale);
            }

            public void SetPosition(float? x, float? y, float? z)
            {
                if (x == null) x = gameObject.transform.position.x;
                if (y == null) y = gameObject.transform.position.y;
                if (z == null) z = gameObject.transform.position.z;

                SetPosition(new Vector3(x.Value, y.Value, z.Value));
            }
            public void SetPosition(Vector3 position) => gameObject.transform.position = position;

            public void SetRotation(float? x, float? y, float? z)
            {
                if (x == null) x = gameObject.transform.localEulerAngles.x;
                if (y == null) y = gameObject.transform.localEulerAngles.y;
                if (z == null) z = gameObject.transform.localEulerAngles.z;

                SetRotation(new Vector3(x.Value, y.Value, z.Value));
            }
            public void SetRotation(Vector3 rotation) => gameObject.transform.localEulerAngles = rotation;

            public void SetScale(float? x, float? y, float? z)
            {
                if (x == null) x = gameObject.transform.localScale.x;
                if (y == null) y = gameObject.transform.localScale.y;
                if (z == null) z = gameObject.transform.localScale.z;

                SetScale(new Vector3(x.Value, y.Value, z.Value));
            }
            public void SetScale(Vector3 scale) => gameObject.transform.localScale = scale;
        }
        #endregion Object
        #region Step
        [Serializable]
        public class Step
        {
            public float time;
            public TransitionAction[] actions;
        }

        [Serializable]
        public class TransitionAction
        {
            public int obsticle;
            public TransitionComponents component;
            public TransitionUnits unit;
            public EaseFunctions function;
            public EaseDirections direction;
            public float start;
            public float end;
            public float duration;

            public void Invoke(GameObject gameObject) => Transition.Add(gameObject, component, unit, function, direction, start, end, duration);
        }
        #endregion Step

        public int targets;

        public Object[] objects;
        public Step[] steps;

        public int activationStep;
        public TransitionAction[] activations;

        [Header("Readonly")]
        public int targetsHit;
        public int currentStep;
        public int nextStep;
        public float timer;


        public void Enable(bool value) => enabled = value && steps.Length != 0;
        public virtual void Initialize()
        {
            foreach (Object obj in objects)
                obj.GetDefaults();
        }
        public virtual void ResetValues()
        {
            targetsHit = 0;
            currentStep = 0;
            nextStep = 0;
            timer = 0;

            for (int i = 0; i < objects.Length; i++)
                objects[i].Reset();
        }
        public virtual void Update()
        {
            timer += Time.deltaTime;

            if (timer >= steps[nextStep].time)
                SetStep(nextStep, true);
        }

        public void TargetHit(int value)
        {
            targetsHit += value;
            if (targetsHit < 0) Debug.LogWarning("Targets Hit Less than 0");
            else if (targetsHit > targets) Debug.LogWarning("Targets Hit More than Max");

            if (targetsHit == targets)
            {
                Activate();
                Enable(false);
            }
            else
                Enable(true);
        }
        public virtual void Activate()
        {
            SetStep(activationStep, false);

            foreach (TransitionAction action in activations)
                action.Invoke(objects[action.obsticle].gameObject);
        }

        public void SetStep(int step, bool invokeActions)
        {
            currentStep = step;
            nextStep = (step + 1 == steps.Length) ? 0 : step + 1;
            timer = 0;

            if (invokeActions)
                foreach (TransitionAction action in steps[currentStep].actions)
                    action.Invoke(objects[action.obsticle].gameObject);
        }
    }
}