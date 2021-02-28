using PsyPuppets.Gameplay;
using UnityEngine;

namespace PsyPuppets.UI
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField]
        private GameObject _pauseMenuView;
        private void OnEnable()
        {
            GameManager.Instance.OnPaused += OnPaused;
            GameManager.Instance.OnResumed += OnResumed;
        }

        private void OnResumed()
        {
            _pauseMenuView.SetActive(false);
        }

        private void OnPaused()
        {
            _pauseMenuView.SetActive(true);
        }

        private void OnDisable()
        {
            GameManager.Instance.OnPaused -= OnPaused;
            GameManager.Instance.OnResumed -= OnResumed;
        }

        public void OnResumeButtonClicked()
        {
            GameManager.Instance.Resume();
        }

        public void OnQuitButtonClicked()
        {
            GameManager.Instance.Quit();
        }
    }
}
