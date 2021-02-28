using DG.Tweening;
using UnityEngine;


namespace PsyPuppets.Gameplay.Props
{
    public class WirelessReciever : MonoBehaviour
    {
        [SerializeField]
        private float _endY;
        
        [SerializeField]
        private float _duration;

        public Tween TriggeredTween { get; private set; }

        public void Trigger()
        {
            TriggeredTween = transform.DOLocalMoveY(_endY, _duration);
        }
    }
}