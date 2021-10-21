using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


namespace GalaxyRush
{
    public class Game : MonoBehaviour
    {
        public UI.Cursor cursor;
        public Menu.Foreground foreground;
        public Menu.PauseMenu pause;
        public Menu.SettingsMenu settings;

        public Transform bounds;
        public Transform obsticlesGroup;

        public int level;
        public int overheadStart;
        public int[] checkpoints;

        [Header("Readonly")]
        public int shots;
        public int deaths;
        public float time;
        public int checkpoint;
        public Obsticle[][] obsticles;

        public bool settingsLocked;


        public void Start()
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;

            Global.menu = null;
            Global.game = this;
            Global.loader = GameObject.FindWithTag("Scene Loader").GetComponent<SceneLoader>();
            Global.camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            Global.player = GameObject.FindWithTag("Player").GetComponent<Player>();

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

            List<List<Obsticle>> obsticlesList = new List<List<Obsticle>>(checkpoints.Length);
            for (int i = 0; i < checkpoints.Length; i++)
                obsticlesList.Add(new List<Obsticle>());

            for (int i = 0; i < obsticlesGroup.childCount; i++)
                if (obsticlesGroup.GetChild(i).TryGetComponent(out Obsticle obsticle))
                {
                    int checkpoint = CheckpointFromPosition(obsticle.transform.position.z);
                    obsticlesList[checkpoint].Add(obsticle);
                    obsticle.Enable(false);
                }

            obsticles = new Obsticle[checkpoints.Length][];
            for (int i = 0; i < checkpoints.Length; i++)
                obsticles[i] = obsticlesList[i].ToArray();

            ActivateObsticles(0, true);
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

        public void ActivateObsticles(int checkpoint, bool active)
        {
            foreach (Obsticle obsticle in obsticles[checkpoint])
                obsticle.Enable(active);
        }

        public int CheckpointFromPosition(float position)
        {
            int checkpoint = 0;
            for (int i = 0; i < checkpoints.Length; i++)
                if (checkpoints[i] < position)
                    checkpoint = i;

            return checkpoint;
        }
    }
}