using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;


namespace GalaxyRush
{
    public class SceneLoader : MonoBehaviour
    {
        public AsyncOperation operation;
        public Transform bar;

        public void Load(string scene)
        {
            foreach (GameObject gameObject in SceneManager.GetActiveScene().GetRootGameObjects())
                gameObject.SetActive(false);

            this.gameObject.SetActive(true);
            this.bar = transform.GetChild(0);

            Progress(0);

            operation = SceneManager.LoadSceneAsync(scene);
        }

        public void Update()
        {
            Progress(operation.progress);
        }

        public void Progress(float value)
        {
            bar.localScale = new Vector3(Mathf.Lerp(0, 100, value), 4, 1);
            bar.localPosition = new Vector3(Mathf.Lerp(-8, 0, value), 0, 0);
        }
    }
}