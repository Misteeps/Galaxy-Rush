using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


namespace Game
{
    public class Menu : MonoBehaviour
    {
        #region Foreground
        [Serializable]
        public class Foreground : UI.Group
        {
            public override void Update(GameObject hovered = null)
            {

            }
        }
        #endregion Foreground
        #region Main
        [Serializable]
        public class Main : UI.Group
        {
            public GameObject play;
            public GameObject chapters;
            public GameObject settings;
            public GameObject credits;

            public CanvasGroup button;
            public GameObject buttonTarget;

            public override void Update(GameObject hovered = null)
            {
                if (Input.click.Down && hovered != null)
                {
                    if (hovered == play) Debug.Log("Play");
                    if (hovered == chapters) Debug.Log("Chapters");
                    if (hovered == settings) Debug.Log("Settings");
                    if (hovered == credits) Debug.Log("Credits");
                }

                if (buttonTarget != hovered)
                    if (hovered == play || hovered == chapters || hovered == settings || hovered == credits)
                        MoveButton(hovered);
                    else
                        MoveButton(null);
            }

            public void MoveButton(GameObject target)
            {
                if (target == null)
                {
                    Transition.Add(button.gameObject, TransitionComponents.UIAlpha, TransitionUnits.A, EaseFunctions.Linear, EaseDirections.InOut, 1, 0, 0.5f);
                    buttonTarget = null;
                    return;
                }

                float start = (buttonTarget != null) ? buttonTarget.transform.position.y : 180;
                Transition.Add(button.gameObject, TransitionComponents.Position, TransitionUnits.Y, EaseFunctions.Elastic, EaseDirections.Out, start, target.transform.position.y, 0.2f);
                Transition.Add(button.gameObject, TransitionComponents.UIAlpha, TransitionUnits.A, EaseFunctions.Linear, EaseDirections.InOut, 0, 1, 0.2f);
                buttonTarget = target;
            }
        }
        #endregion Main


        public UI.Cursor cursor;
        public Foreground foreground;
        public Main main;

        public Transform bounds;
        public Transform view;


        public void Start()
        {
            Global.Initialize();
            Settings.Initialize();
            Input.Initialize();
            Transition.Initialize();

            Time.timeScale = 1;
        }

        public void Update()
        {
            Transition.IncrementObjects(Time.deltaTime);
            Transition.IncrementTweens(Time.deltaTime);

            cursor.Move(Input.mouseDelta, bounds.position);
            cursor.GetHovered();

            main.Update(cursor.hovered);
        }
    }
}