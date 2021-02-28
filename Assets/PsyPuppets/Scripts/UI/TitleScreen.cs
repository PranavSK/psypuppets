using System.Collections;
using System.Collections.Generic;
using PsyPuppets.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PsyPuppets.UI
{
    public class TitleScreen : MonoBehaviour
    {
        public void OnPlayClicked()
        {
            GameManager.Instance.LoadNextLevel();
        }
        public void OnQuitClicked() { }
    }
}