using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


namespace GalaxyRush
{
    public class Game : MonoBehaviour
    {
        public Menu.Cursor cursor;
        public Menu.Foreground foreground;
        public Menu.PauseMenu pause;
        public Menu.SettingsMenu settings;

        public Transform bounds;

        public int level;
        public int shots;
        public float time;

        public bool settingsLocked;


        public void Start()
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;

            Global.menu = null;
            Global.game = this;
            Global.loader = GameObject.FindWithTag("Scene Loader").GetComponent<SceneLoader>();
            Global.camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

            Global.loader.gameObject.SetActive(false);

            Global.Initialize();
            Transition.Initialize();
            Settings.Initialize();
            Input.Initialize();

            Initialize();
        }
        public void Initialize()
        {
            cursor.Initialize();
            foreground.Initialize();
            pause.Initialize();
            settings.Initialize();
        }
        public void Update()
        {
            Transition.IncrementObjects();
            Transition.IncrementTweens();

            if (pause.active || settings.active)
            {
                Physics2D.Simulate(Time.unscaledDeltaTime);

                cursor.Move(Input.mouseDelta, bounds.localPosition);
                cursor.GetHovered();

                pause.Update();
                settings.Update();

                if (settingsLocked)
                    return;

                if (Input.escape.Down)
                    if (settings.active) settings.Hide();
                    else pause.Hide();
            }
            else
            {
                cursor.GetTargeted();

                if (Input.escape.Down)
                    pause.Show();
            }
        }
    }
}