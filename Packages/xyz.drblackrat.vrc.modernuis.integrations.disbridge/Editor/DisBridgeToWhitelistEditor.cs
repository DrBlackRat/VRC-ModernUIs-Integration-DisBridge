using UnityEngine.UIElements;
using UnityEditor;

namespace DrBlackRat.VRC.ModernUIs.Integrations.Editor
{
    [CustomEditor(typeof(DisBridgeToWhitelistBridge))]
    public class DisBridgeToWhitelistEditor : UnityEditor.Editor

    {
    public VisualTreeAsset visualTree;

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        // UXML
        visualTree.CloneTree(root);

        return root;
    }
    }
}
