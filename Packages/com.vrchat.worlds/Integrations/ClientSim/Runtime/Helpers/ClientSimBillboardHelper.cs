using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Rendering;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// ClientSim mirror of the client's billboarding. Per-object like the station and pickup helpers, but shares
    /// <see cref="VRCBillboardSolver"/> with the client's centralized pass so behaviour matches.
    /// </summary>
    [AddComponentMenu("")]
    // After PostLateUpdater's 31000, matching the client, so a billboard picks up repositioning done from
    // Udon's PostLateUpdate event in the same frame. Also after VRCVelocityTracker's 31001 so the two never race.
    [DefaultExecutionOrder(31002)]
    public class ClientSimBillboardHelper : ClientSimBehaviour
    {
        private VRCBillboardOptions _options;

        public static void Register(Transform target, VRCBillboardOptions options)
        {
            if (target == null)
            {
                return;
            }

            ClientSimBillboardHelper helper = target.gameObject.GetComponent<ClientSimBillboardHelper>();
            if (helper == null)
            {
                helper = target.gameObject.AddComponent<ClientSimBillboardHelper>();
            }

            helper._options = options;
            helper.enabled = true;
        }

        // Disabled rather than destroyed: VRCBillboard unregisters from OnDisable, and destroying a component
        // while its GameObject is being deactivated is not permitted.
        public static void Unregister(Transform target)
        {
            if (target == null)
            {
                return;
            }

            ClientSimBillboardHelper helper = target.gameObject.GetComponent<ClientSimBillboardHelper>();
            if (helper != null)
            {
                helper.enabled = false;
            }
        }

        private void LateUpdate()
        {
            VRCCameraSettings viewpoint = VRCCameraSettings.ScreenCamera;
            if (viewpoint == null)
            {
                return;
            }

            Quaternion destination = VRCBillboardSolver.Solve(transform.position, transform.rotation,
                viewpoint.Position, viewpoint.Forward, viewpoint.Up, _options);

            transform.rotation = Quaternion.Slerp(transform.rotation, destination,
                VRCBillboardSolver.SmoothingStep(_options.SmoothingFactor, Time.deltaTime));
        }
    }
}
