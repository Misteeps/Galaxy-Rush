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
        public Menu.ResultsMenu results;

        public Transform bounds;
        public Transform[] planets;
        public Transform obstaclesGroup;
        public Transform targetsGroup;
        public Transform starsGroup;

        public TMPro.TextMeshProUGUI uiTime;
        public TMPro.TextMeshProUGUI uiScore;
        public TMPro.TextMeshProUGUI uiMultiplier;
        public TMPro.TextMeshProUGUI uiAddScore;

        public int level;
        public float expectedTime;
        public Vector3[] checkpoints;

        [Header("Readonly")]
        public float time;
        public int deaths;
        public int shots;
        public int checkpoint;
        public int multiplier;
        public int score;
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
            Global.cinemachine = Global.camera.GetComponent<Cinemachine.CinemachineVirtualCamera>();
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

            multiplier = 1;

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

            if (results.active)
            {
                cursor.Move(Input.mouseDelta, bounds.localPosition);
                cursor.GetHovered();

                results.Update();
            }
            else if (pause.active || settings.active)
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
                this.time += Time.unscaledDeltaTime;
                TimeSpan time = TimeSpan.FromSeconds(this.time);
                uiTime.text = $"{time:mm\\:ss}";

                cursor.GetTargeted();

                int newCheckpoint = CheckpointFromPosition(Global.player.transform.position.z);
                if (newCheckpoint > checkpoint && Global.player.transform.position.y > checkpoints[newCheckpoint].y - 1)
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

            ActivateObstacles(this.checkpoint, true);
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

            AddScore(1000, "Reached Checkpoint");

            this.checkpoint = checkpoint;
            this.multiplier++;

            uiMultiplier.text = $"<size=9>x</size>{multiplier}";

            ActivateObstacles(this.checkpoint, true);
            ActivateStars(this.checkpoint, true);

            if (checkpoint == checkpoints.Length - 1)
            {
                multiplier = 1;
                AddScore(Mathf.RoundToInt(Mathf.Lerp(checkpoints.Length * 1500, 0, Mathf.InverseLerp(0, expectedTime, Global.game.time))), "Time Bonus");

                results.nextLevel = level + 1;
                results.Show();
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

        public void AddScore(int amount, string reason)
        {
            score += amount * multiplier;

            uiAddScore.alpha = 1;
            uiAddScore.rectTransform.anchoredPosition = new Vector2(-50, -60);
            uiAddScore.text = $"+{amount}\n<size=9>{reason}</size>";

            Transition.Add((v) =>
            {
                uiAddScore.rectTransform.anchoredPosition = new Vector2(-50, Mathf.Lerp(-60, -40, v));
                uiAddScore.alpha = Mathf.Lerp(1, 0, v);
                if (v > 0.9f) uiScore.text = $"{score}";
            }, EaseFunctions.Exponential, EaseDirections.In, 0, 1, 2);
        }
    }
}