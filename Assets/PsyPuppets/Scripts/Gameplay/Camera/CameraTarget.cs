using PsyPuppets.Gameplay.Characters.Abilities;
using UnityEngine;

namespace PsyPuppets.Gameplay.Camera
{
    public class CameraTarget : MonoBehaviour, IAbility
    {
        public Bounds RenderBounds
        {
            get
            {
                var renderers = GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds bounds = renderers[0].bounds;
                    for (int i = 1, ni = renderers.Length; i < ni; i++)
                    {
                        bounds.Encapsulate(renderers[i].bounds);
                    }
                    return bounds;
                }
                else
                {
                    return new Bounds();
                }
            }
        }

        public bool Enabled { get => enabled; set => enabled = value; }

        void OnEnable()
        {
            if (UnityEngine.Camera.main &&
                UnityEngine.Camera.main.TryGetComponent<MultiTargetFollowCamera>(out var camera))
                camera.AddTarget(this);
            else
                Debug.Log("No main camera with multi target follow behaviour found!");
        }

        void OnDisable()
        {
            if (UnityEngine.Camera.main &&
                UnityEngine.Camera.main.TryGetComponent<MultiTargetFollowCamera>(out var camera))
                camera.RemoveTarget(this);
            // else
            //     Debug.Log("No main camera with multi target follow behaviour found!");
        }

    }
}