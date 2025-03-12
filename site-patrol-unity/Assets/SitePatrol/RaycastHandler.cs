using UnityEngine;

namespace SitePatrol
{
    public class RaycastHandler : MonoBehaviour
    {
        public Camera mainCamera;
        public LayerMask layerMask;
        public float maxDistance = 100f;
        public CoordinateMatcher coordinateMatcher;

        public Vector3? GetScreenCenterInBim()
        {
            // Return the raycast result of the center of the screen
            var ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask))
            {
                // 变换到BIM坐标系
                if (!coordinateMatcher.ready) return hit.point;
                var bimPosition = coordinateMatcher.modelRoot.transform.InverseTransformPoint(hit.point);
                return bimPosition;
            }

            return null;
        }
    }
}