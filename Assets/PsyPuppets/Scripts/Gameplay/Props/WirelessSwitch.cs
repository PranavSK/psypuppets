using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;


namespace PsyPuppets.Gameplay.Props
{
    public class WirelessSwitch : MonoBehaviour
    {
        [SerializeField]
        private WirelessReciever _reciever;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Drone")
            {
                _reciever.gameObject.AddComponent<Camera.CameraTarget>();
                _reciever.Trigger();
                StartCoroutine(WaitForComplete());
            }
        }

        private IEnumerator WaitForComplete()
        {
            if (_reciever.TriggeredTween != null)
                yield return _reciever.TriggeredTween.WaitForCompletion();

            gameObject.SetActive(false);
            Destroy(_reciever.gameObject.GetComponent<Camera.CameraTarget>());
        }
    }
}