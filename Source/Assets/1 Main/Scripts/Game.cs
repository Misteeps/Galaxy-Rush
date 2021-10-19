using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


namespace GalaxyRush
{
    public class Game : MonoBehaviour
    {
        public void Start()
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;

            Global.menu = null;
            Global.game = this;
            Global.loader = GameObject.FindWithTag("Scene Loader").GetComponent<SceneLoader>();
            Global.loader.gameObject.SetActive(false);

            Global.Initialize();
            Transition.Initialize();
            Settings.Initialize();
            Input.Initialize();
        }

        public void Update()
        {
            Transition.IncrementObjects(Time.deltaTime);
            Transition.IncrementTweens(Time.deltaTime);
        }
    }
}