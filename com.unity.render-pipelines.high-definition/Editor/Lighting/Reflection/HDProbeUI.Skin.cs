using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    static partial class HDProbeUI
    {
        static readonly GUIContent k_ProxyVolumeContent = CoreEditorUtils.GetContent("Proxy Volume");
        internal static readonly GUIContent k_UseInfiniteProjectionContent = CoreEditorUtils.GetContent("Same As Influence Volume|If enabled, parallax correction will occure, causing reflections to appear to change based on the object's position within the probe's box, while still using a single probe as the source of the reflection. This works well for reflections on objects that are moving through enclosed spaces such as corridors and rooms. When disabled, the cubemap reflection will be treated as coming from infinitely far away. Note that this feature can be globally disabled from Graphics Settings -> Tier Settings");

        static readonly GUIContent k_WeightContent = CoreEditorUtils.GetContent("Weight|Blend weight applied on this reflection probe. This can be used for fading in or out a reflection probe.");
        static readonly GUIContent k_MultiplierContent = CoreEditorUtils.GetContent("Intensity Multiplier|Allows you to boost or dimmer the reflected cubemap. Values above 1 will make reflections brighter and values under 1 will make reflections darker. Using values different than 1 is not physically correct.");
        static readonly GUIContent k_LightLayersContent = CoreEditorUtils.GetContent("Light Layers|Specifies the current light layers that the light affect. Corresponding renderer with the same flags will be lit by this light.");
        
        const string k_MimapHelpBoxText = "No mipmaps in the cubemap, Smoothness value in Standard shader will be ignored.";
        const string k_NoProxyHelpBoxText = "Influence shape will be used as Projection shape too.";
        const string k_NoProxyInfiniteHelpBoxText = "Projection will be at infinite.";
        const string k_ProxyInfluenceShapeMismatchHelpBoxText = "Proxy volume and influence volume have different shapes, this is not supported.";

        internal static readonly GUIContent k_ProxySettingsHeader = CoreEditorUtils.GetContent("Projection Settings");
        internal static readonly GUIContent k_InfluenceVolumeHeader = CoreEditorUtils.GetContent("Influence Volume");
        internal static readonly GUIContent k_CaptureSettingsHeader = CoreEditorUtils.GetContent("Capture Settings");
        internal static readonly GUIContent k_CustomSettingsHeader = CoreEditorUtils.GetContent("Custom Settings");

        static readonly Dictionary<ToolBar, GUIContent> k_ToolbarContents = new Dictionary<ToolBar, GUIContent>
        {
            { ToolBar.InfluenceShape,  EditorGUIUtility.IconContent("EditCollider", "|Modify the base shape. (SHIFT+1)") },
            { ToolBar.Blend,  EditorGUIUtility.IconContent("PreMatCube", "|Modify the influence volume. (SHIFT+2)") },
            { ToolBar.NormalBlend,  EditorGUIUtility.IconContent("SceneViewOrtho", "|Modify the influence normal volume. (SHIFT+3)") },
            { ToolBar.CapturePosition,  EditorGUIUtility.IconContent("MoveTool", "|Change the capture position.") },
            { ToolBar.MirrorPosition,  EditorGUIUtility.IconContent("MoveTool", "|Change the mirror position.") },
            { ToolBar.MirrorRotation,  EditorGUIUtility.IconContent("RotateTool", "|Change the mirror rotation.") }
        };
    }
}
