using System;
using DG.Tweening;
using PsyPuppets.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PsyPuppets.Gameplay
{
    public class GameManager : Singleton<GameManager>
    {
        public static bool IsPaused = false;

        [SerializeField]
        private PlayerInput _playerInput;

        [SerializeField]
        private AudioSource _startAudioPlayer;

        [SerializeField]
        private AudioSource _loopAudioPlayer;

        private int _mainSceneBuildIndex;
        private InputAction _pauseAction;

        public PlayerInput PlayerInput => _playerInput;
        public int Level { get; set; } = 0;
        public System.Action OnPaused;
        public System.Action OnResumed;

        private void Start()
        {
            _mainSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
            _pauseAction = GameManager.Instance.PlayerInput.actions["Pause"];
            if (_pauseAction == null)
                Debug.LogError("No valid action named Pause in " + GameManager.Instance.PlayerInput.ToString());

            _pauseAction.performed += TogglePause;

            SceneManager.LoadScene(++_mainSceneBuildIndex);
            PlayMenuClip();
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void LoadNextLevel()
        {
            Level += 1;
            SceneManager.LoadScene(_mainSceneBuildIndex + Level);
            PlayLoopClip();
        }

        public void Resume()
        {
            GameManager.IsPaused = false;
            Time.timeScale = 1.0f;
            OnResumed();
        }

        public void Pause()
        {
            GameManager.IsPaused = true;
            Time.timeScale = 0.0f;
            OnPaused();

        }

        public void PlayMenuClip()
        {
            if (_loopAudioPlayer.isPlaying)
            {
                var tween = _loopAudioPlayer.DOFade(0.0f, 2.5f);
                tween.OnComplete(() => { _loopAudioPlayer.Stop(); });
            }

            if (!_startAudioPlayer.isPlaying)
            {
                _startAudioPlayer.Play();
                _startAudioPlayer.DOFade(1.0f, 2.5f);
            }
        }

        public void PlayLoopClip()
        {
            if (_startAudioPlayer.isPlaying)
            {
                var tween = _startAudioPlayer.DOFade(0.0f, 2.5f);
                tween.OnComplete(() => { _startAudioPlayer.Stop(); });
            }
            if (!_loopAudioPlayer.isPlaying)
            {
                _loopAudioPlayer.Play();
                _loopAudioPlayer.DOFade(1.0f, 2.5f);
            }


        }

        private void TogglePause(InputAction.CallbackContext context)
        {
            if (GameManager.IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

    }
}