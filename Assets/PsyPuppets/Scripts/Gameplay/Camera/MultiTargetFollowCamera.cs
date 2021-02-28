using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PsyPuppets.Gameplay.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class MultiTargetFollowCamera : MonoBehaviour
    {
        [SerializeField]
        private Vector2 _rectPadding = new Vector2(5.0f, 5.0f);

        [SerializeField]
        [Tooltip("The minimum distance of the camera from the closest target.")]
        private float _minSpringArmDistance = 4.0f;

        [SerializeField]
        private float _moveSmoothWeight = 0.19f;

        // [SerializeField]
        // private bool _useRenderBounds = true;

        // [SerializeField, Range(1.0f, 360.0f)]
        // private float _rotationSpeed = 90.0f;

        private UnityEngine.Camera _camera;
        private List<CameraTarget> _followTargets;
        private Vector3 _focusPoint;
        private Vector3 _velocity = Vector3.zero;

        public int TargetCount => _followTargets.Count;

        private void Awake()
        {
            _followTargets = new List<CameraTarget>();
            _camera = GetComponent<UnityEngine.Camera>();
        }

        private void LateUpdate()
        {
            if (_followTargets.Count == 0)
                return;

            var targetPoint = CalculateFollowTargetPosition();
            // TODO: Hacky to prevent camera from sinking to ground. Need to investigate.
            targetPoint.y = Mathf.Max(12.0f, targetPoint.y);
            _focusPoint = targetPoint;

            //NOTE: Can we also do camera orbit?
            transform.position = Vector3.SmoothDamp(transform.position, _focusPoint, ref _velocity, _moveSmoothWeight);
        }

        private void OnValidate()
        {
            // Ensure scale is never anything other than Vector3.one
            if (transform.localScale != Vector3.one)
                transform.localScale = Vector3.one;
        }

        private Vector3 CalculateFollowTargetPosition()
        {
            // if (_followTargets.Count == 1)
            // {
            //     var followTarget = _followTargets[0];
            //     // Transform the target positions into camera local space.
            //     var posInCameraSpace = transform.InverseTransformPoint(followTarget.position);
            //     var pos = new Vector3
            //     (
            //         posInCameraSpace.x,
            //         posInCameraSpace.y,
            //         Mathf.Sign(posInCameraSpace.z) *
            //         (Mathf.Abs(posInCameraSpace.z) - _minSpringArmDistance)
            //     );

            //     return transform.TransformPoint(pos);
            // }

            float halfVerticalFovRad = (_camera.fieldOfView * Mathf.Deg2Rad) / 2.0f;
            float halfHorizontalFovRad = Mathf.Atan(Mathf.Tan(halfVerticalFovRad) * _camera.aspect);
            float tanTheta = Mathf.Tan(halfVerticalFovRad);
            float tanGamma = Mathf.Tan(halfHorizontalFovRad);

            Bounds bounds = new Bounds();

            _followTargets.ForEach((target) =>
            {
                if (bounds.size == Vector3.zero)
                {
                    bounds = target.RenderBounds;
                }
                else
                {
                    bounds.Encapsulate(target.RenderBounds);
                }
            });

            var followPositions = new Vector3[8];

            followPositions[0] = bounds.min;
            followPositions[1] = bounds.max;
            followPositions[2] = new Vector3(followPositions[0].x, followPositions[0].y, followPositions[1].z);
            followPositions[3] = new Vector3(followPositions[0].x, followPositions[1].y, followPositions[0].z);
            followPositions[4] = new Vector3(followPositions[1].x, followPositions[0].y, followPositions[0].z);
            followPositions[5] = new Vector3(followPositions[0].x, followPositions[1].y, followPositions[1].z);
            followPositions[6] = new Vector3(followPositions[1].x, followPositions[0].y, followPositions[1].z);
            followPositions[7] = new Vector3(followPositions[1].x, followPositions[1].y, followPositions[0].z);

            // First find the max z distance to get the projectionPlane
            var projectionPlaneDepth = followPositions.Select((target) => transform.InverseTransformPoint(target).z).Max();

            // We want max values of rect.xMax and rect.yMax
            // Set the initial values to -inf.
            var xProjRectMax = -Mathf.Infinity;
            var yProjRectMax = -Mathf.Infinity;
            // We want min values of xProjRectMin and yProjRectMin
            // Set the initial values to inf.
            var xProjRectMin = Mathf.Infinity;
            var yProjRectMin = Mathf.Infinity;
            foreach (var worldPos in followPositions)
            {
                // Transform the target positions into camera local space.
                var posInCameraSpace = transform.InverseTransformPoint(worldPos);
                // Calculate the 'shadow' cast by the pos on to the projectPlane
                // using the vertical and horizontal fovs.

                var distanceToProjectionPlane = Mathf.Abs(projectionPlaneDepth - posInCameraSpace.z);
                // Vertical projections
                var vProjectionHalfSpan = tanTheta * distanceToProjectionPlane;
                var vProjMax = posInCameraSpace.y + vProjectionHalfSpan;
                var vProjMin = posInCameraSpace.y - vProjectionHalfSpan;
                // Horizontal projections
                var hProjectionHalfSpan = tanGamma * distanceToProjectionPlane;
                var hProjMax = posInCameraSpace.x + hProjectionHalfSpan;
                var hProjMin = posInCameraSpace.x - hProjectionHalfSpan;

                // Update the projection rect to include the max area under the collective 'shadows'
                // ie. the outermost projections.
                if (vProjMax > yProjRectMax) yProjRectMax = vProjMax;
                if (vProjMin < yProjRectMin) yProjRectMin = vProjMin;
                if (hProjMax > xProjRectMax) xProjRectMax = hProjMax;
                if (hProjMin < xProjRectMin) xProjRectMin = hProjMin;
                // Debug.Log(
                //     "Rect: " + yProjRectMax + ", " + yProjRectMin + ", " + xProjRectMax + ", " + xProjRectMin +
                //     "\nvProj\t" + vProjMax + ", " + vProjMin + "\thProj\t" + hProjMax + ", " + hProjMin
                // );
            }

            // Add the rect padding
            yProjRectMax += _rectPadding.y;
            yProjRectMin -= _rectPadding.y;
            xProjRectMax += _rectPadding.x;
            xProjRectMin -= _rectPadding.x;

            var desiredDepth = Mathf.Max(
                // Calculate from center, hence divide by 2.
                (yProjRectMax - yProjRectMin) / (2.0f * tanTheta),
                (xProjRectMax - xProjRectMin) / (2.0f * tanGamma)
            );

            desiredDepth = Mathf.Max(_minSpringArmDistance, desiredDepth);

            // Calculate the new camera position in the local space.
            // The xy position is the center of the projected rect.
            var newPosition = new Vector3(
                (xProjRectMax + xProjRectMin) / 2.0f,
                (yProjRectMax + yProjRectMin) / 2.0f,
                // This is the required offset of the camera position
                // to make the distance to the projection plane be equal
                // to the desired depth.
                Mathf.Sign(projectionPlaneDepth) *
                (Mathf.Abs(projectionPlaneDepth) - desiredDepth)
            );

            // Calculate and return the new position in global space.
            return transform.TransformPoint(newPosition);
        }

        public void AddTarget(Camera.CameraTarget target)
        {
            _followTargets.Add(target);
        }

        public void RemoveTarget(Camera.CameraTarget target)
        {
            _followTargets.Remove(target);
        }
    }
}