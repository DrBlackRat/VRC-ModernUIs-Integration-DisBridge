using UnityEditor;
using UnityEngine.UIElements;
using VRCBillboard = VRC.SDK3.Components.VRCBillboard;

namespace VRC.SDK3.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VRCBillboard))]
    public class VRCBillboardEditor : VRCInspectorBase
    {
        private const string SHOW_OVERRIDES_KEY = "VRC.SDK3.Components.VRCBillboard.ShowOverrides";

        private SerializedProperty propFacing;
        private SerializedProperty propAllowTilt;
        private SerializedProperty propSmoothingFactor;
        private SerializedProperty propUpFrom;
        private SerializedProperty propMatchViewpointRotation;


        private void OnEnable()
        {
            propFacing = serializedObject.FindProperty("_facing");
            propAllowTilt = serializedObject.FindProperty("_allowTilt");
            propSmoothingFactor = serializedObject.FindProperty("_smoothingFactor");
            propUpFrom = serializedObject.FindProperty("_upFrom");
            propMatchViewpointRotation = serializedObject.FindProperty("_matchViewpointRotation");
        }

        public override void BuildInspectorGUI()
        {
            base.BuildInspectorGUI();

            AddFieldTooltip(propFacing,
                "Which of this object's axes points at the player. Back (-Z) for UI, text and sprites. Forward (+Z) for models with a built-in front.");
            AddFieldTooltip(propAllowTilt,
                "Tilts the object up and down to face players above or below it. Off keeps it upright, turning left and right only.");
            AddFieldTooltip(propSmoothingFactor,
                "How long the turn takes, in seconds. 0 snaps instantly. The object covers about 63% of the remaining angle every Smoothing Factor seconds, so higher values look smoother and floatier.");

            Foldout foldout = AddKeyedFoldout("Overrides", SHOW_OVERRIDES_KEY);
            foldout.Add(AddFieldTooltip(propUpFrom,
                "What this object treats as up. Empty means world up. Set a vehicle or station here so content stays upright for players riding it."));
            foldout.Add(AddFieldTooltip(propMatchViewpointRotation,
                "Copies the player's view rotation instead of aiming at them, so the object stays flat to the screen."));
        }
    }
}
