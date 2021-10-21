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
        public Transform obstaclesGroup;

        public int level;
        public int overheadStart;
        public int[] checkpoints;

        [Header("Readonly")]
        public int shots;
        public int deaths;
        public float time;
        public int checkpoint;
        public Obstacle[][] obstacles;

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

            List<List<Obstacle>> obstaclesList = new List<List<Obstacle>>(checkpoints.Length);
            for (int i = 0; i < checkpoints.Length; i++)
                obstaclesList.Add(new List<Obstacle>());

            for (int i = 0; i < obstaclesGroup.childCount; i++)
                if (obstaclesGroup.GetChild(i).TryGetComponent(out Obstacle obsticle))
                {
                    int checkpoint = CheckpointFromPosition(obsticle.transform.position.z);
                    obstaclesList[checkpoint].Add(obsticle);
                    obsticle.Enable(false);
                }

            obstacles = new Obstacle[checkpoints.Length][];
            for (int i = 0; i < checkpoints.Length; i++)
                obstacles[i] = obstaclesList[i].ToArray();

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
            foreach (Obstacle obsticle in obstacles[checkpoint])
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