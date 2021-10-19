using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


namespace Game
{
    public class Menu : MonoBehaviour
    {
        public void Start()
        {
            Global.Initialize();
            Settings.Initialize();
            Input.Initialize();
            Transition.Initialize();

            Time.timeScale = 1;

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }


        private const float sideMenuYStart = -405;
        private const float sideMenuYEnd = -385;

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
        public class MainMenu : UI.Group
        {
            public GameObject play;
            public GameObject chapters;
            public GameObject settings;
            public GameObject credits;

            public GameObject effect;
            public GameObject effectTarget;

            public override void Update(GameObject hovered = null)
            {
                if (effectTarget != hovered)
                    if (hovered == play || hovered == chapters || hovered == settings || hovered == credits)
                        MoveButton(hovered);
                    else
                        MoveButton(null);
            }

            public void MoveButton(GameObject target)
            {
                if (target == null)
                {
                    Transition.Add(effect, TransitionComponents.Scale, TransitionUnits.X, EaseFunctions.Quartic, EaseDirections.Out, 1, 0, 0.25f);
                    effectTarget = null;
                    return;
                }

                Transition.Add(effect, TransitionComponents.LocalPosition, TransitionUnits.Y, EaseFunctions.Elastic, EaseDirections.Out, effect.transform.localPosition.y, target.transform.localPosition.y, 0.2f);
                Transition.Add(effect, TransitionComponents.Scale, TransitionUnits.X, EaseFunctions.Quartic, EaseDirections.Out, 0, 1, 0.2f);
                effectTarget = target;
            }
        }
        #endregion Main
        #region Chapters
        [Serializable]
        public class ChaptersMenu : UI.Group
        {
            public GameObject close;
            public bool active;

            public override void Update(GameObject hovered = null)
            {
                if (!active) return;

                if (Input.click.Down && hovered != null)
                {
                    if (hovered == close) Hide();
                }
            }

            public override void Show(float speed = 0.25F)
            {
                base.Show(speed);

                group.transform.localPosition = new Vector3(group.transform.localPosition.x, sideMenuYStart, group.transform.localPosition.z);
                Transition.Add(group.gameObject, TransitionComponents.LocalPosition, TransitionUnits.Y, EaseFunctions.Quadratic, EaseDirections.Out, sideMenuYStart, sideMenuYEnd, speed);

                active = true;
            }
            public override void Hide(float speed = 0.1F)
            {
                base.Hide(speed);

                active = false;
            }
        }
        #endregion Chapters
        #region Settings
        [Serializable]
        public class SettingsMenu : UI.Group
        {
            public GameObject close;
            public bool active;

            public override void Update(GameObject hovered = null)
            {
                if (!active) return;

                if (Input.click.Down && hovered != null)
                {
                    if (hovered == close) Hide();
                }
            }

            public override void Show(float speed = 0.25F)
            {
                base.Show(speed);

                group.transform.localPosition = new Vector3(group.transform.localPosition.x, sideMenuYStart, group.transform.localPosition.z);
                Transition.Add(group.gameObject, TransitionComponents.LocalPosition, TransitionUnits.Y, EaseFunctions.Quadratic, EaseDirections.Out, sideMenuYStart, sideMenuYEnd, speed);

                active = true;
            }
            public override void Hide(float speed = 0.1F)
            {
                base.Hide(speed);

                active = false;
            }
        }
        #endregion Settings
        #region Credits
        [Serializable]
        public class CreditsMenu : UI.Group
        {
            public GameObject close;
            public bool active;

            public override void Update(GameObject hovered = null)
            {
                if (!active) return;

                if (Input.click.Down && hovered != null)
                {
                    if (hovered == close) Hide();
                }
            }

            public override void Show(float speed = 0.25F)
            {
                base.Show(speed);

                group.transform.localPosition = new Vector3(group.transform.localPosition.x, sideMenuYStart, group.transform.localPosition.z);
                Transition.Add(group.gameObject, TransitionComponents.LocalPosition, TransitionUnits.Y, EaseFunctions.Quadratic, EaseDirections.Out, sideMenuYStart, sideMenuYEnd, speed);

                active = true;
            }
            public override void Hide(float speed = 0.1F)
            {
                base.Hide(speed);

                active = false;
            }
        }
        #endregion Credits


        public UI.Cursor cursor;
        public Foreground foreground;
        public MainMenu main;
        public ChaptersMenu chapters;
        public SettingsMenu settings;
        public CreditsMenu credits;

        public Transform bounds;
        public Transform view;

        public void Update()
        {
            Transition.IncrementObjects(Time.deltaTime);
            Transition.IncrementTweens(Time.deltaTime);

            cursor.Move(Input.mouseDelta, bounds.localPosition);
            cursor.GetHovered();

            main.Update(cursor.hovered);
            chapters.Update(cursor.hovered);
            settings.Update(cursor.hovered);
            credits.Update(cursor.hovered);

            if (Input.click.Down && cursor.hovered != null)
            {
                if (cursor.hovered == main.play) Debug.Log("Play");
                else if (cursor.hovered == main.chapters) { CloseSideMenus(); chapters.Show(); }
                else if (cursor.hovered == main.settings) { CloseSideMenus(); settings.Show(); }
                else if (cursor.hovered == main.credits) { CloseSideMenus(); credits.Show(); }
            }

            if (Input.escape.Down)
                CloseSideMenus();
        }

        public void CloseSideMenus()
        {
            chapters.Hide();
            settings.Hide();
            credits.Hide();
        }
    }
}