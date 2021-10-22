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
        public Transform[] planets;
        public Transform obstaclesGroup;
        public Transform targetsGroup;
        public Transform starsGroup;

        public int level;
        public int overheadStart;
        public Vector3[] checkpoints;

        [Header("Readonly")]
        public int shots;
        public int deaths;
        public float time;
        public int checkpoint;
        public int streak;
        public Vector3 checkpointPosition => checkpoints[checkpoint];
        public Obstacle[][] obstacles;
        public Target[][] targets;
        public Star[][] stars;

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

            #region Get Objects
            T[][] GetObjects<T>(Transform group) where T : MonoBehaviour
            {
                List<List<T>> list = new List<List<T>>(checkpoints.Length);
                for (int i = 0; i < checkpoints.Length; i++)
                    list.Add(new List<T>());

                for (int i = 0; i < group.childCount; i++)
                    if (group.GetChild(i).TryGetComponent(out T obj))
                    {
                        int checkpoint = CheckpointFromPosition(obj.transform.position.z);
                        list[checkpoint].Add(obj);
                        obj.enabled = false;
                        obj.Invoke("Initialize", 0);
                    }

                T[][] array = new T[checkpoints.Length][];
                for (int i = 0; i < checkpoints.Length; i++)
                    array[i] = list[i].ToArray();

                return array;
            }
            #endregion

            streak = 1;

            obstacles = GetObjects<Obstacle>(obstaclesGroup);
            targets = GetObjects<Target>(targetsGroup);
            stars = GetObjects<Star>(starsGroup);

            ActivateObstacles(0, true);
            ActivateStars(0, true);
        }
        public void Update()
        {
            Transition.IncrementObjects();
            Transition.IncrementTweens();

            foreach (Transform planet in planets)
                planet.Rotate(Vector3.up * Time.deltaTime * planet.localScale.x / 4000);

            foreground.Update();

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
                time += Time.unscaledDeltaTime;
                cursor.GetTargeted();

                int newCheckpoint = CheckpointFromPosition(Global.player.transform.position.z);
                if (newCheckpoint != checkpoint && Global.player.transform.position.y > checkpoints[newCheckpoint].y - 1)
                    SetCheckpoint(newCheckpoint);

                if (Input.escape.Down)
                    pause.Show();
            }
        }

        public void ActivateObstacles(int checkpoint, bool active)
        {
            foreach (Obstacle obsticle in obstacles[checkpoint])
                obsticle.Enable(active);
        }
        public void ActivateTargets(int checkpoint, bool active)
        {
            foreach (Target target in targets[checkpoint])
                target.enabled = active;
        }
        public void ActivateStars(int checkpoint, bool active)
        {
            foreach (Star star in stars[checkpoint])
                star.enabled = active;
        }

        public void ResetObstacles()
        {
            for (int i = 0; i < obstacles.Length; i++)
                foreach (Obstacle obstacle in obstacles[i])
                    obstacle.ResetValues();
        }
        public void ResetTargets()
        {
            for (int i = 0; i < targets.Length; i++)
                foreach (Target target in targets[i])
                    target.ResetValues();
        }

        public void SetCheckpoint(int checkpoint)
        {
            ActivateObstacles(this.checkpoint, false);
            ActivateStars(this.checkpoint, false);

            this.checkpoint = checkpoint;
            this.streak++;

            ActivateObstacles(this.checkpoint, true);
            ActivateStars(this.checkpoint, true);

            if (checkpoint == checkpoints.Length - 1)
            {
                Global.game.foreground.load = "Clear";
                foreground.Show(1);
            }
        }
        public int CheckpointFromPosition(float position)
        {
            int checkpoint = 0;
            for (int i = 0; i < checkpoints.Length; i++)
                if (checkpoints[i].z < position)
                    checkpoint = i;

            return checkpoint;
        }
    }
}