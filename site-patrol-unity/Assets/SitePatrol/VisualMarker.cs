using AprilTag;
using UnityEngine;
using UnityEngine.Serialization;

namespace SitePatrol
{
    public class VisualMarker : MonoBehaviour
    {
        public int id;

        public TagPose GetInfo()
        {
            var rotation = transform.rotation;
            rotation *= Quaternion.Euler(90, 0, 0);
            return new TagPose(id, transform.position, rotation);
        }
    }
}