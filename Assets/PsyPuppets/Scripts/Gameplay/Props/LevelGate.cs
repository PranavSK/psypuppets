using UnityEngine;

namespace PsyPuppets.Gameplay.Props
{
    public class LevelGate : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                GameManager.Instance.LoadNextLevel();
            }
        }
    }
}