using System;
using UnityEngine.Serialization;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [DisallowMultipleComponent, ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public partial class HDAdditionalCameraData : MonoBehaviour, ISerializationCallbackReceiver, IDebugData
    {
        public enum FlipYMode
        {
            Automatic,
            ForceFlipY
        }

        // The light culling use standard projection matrices (non-oblique)
        // If the user overrides the projection matrix with an oblique one
        // He must also provide a callback to get the equivalent non oblique for the culling
        public delegate Matrix4x4 NonObliqueProjectionGetter(Camera camera);

        Camera m_camera;

        public enum ClearColorMode
        {
            Sky,
            BackgroundColor,
            None
        };

        public ClearColorMode clearColorMode = ClearColorMode.Sky;
        [ColorUsage(true, true)]
        public Color backgroundColorHDR = new Color(0.025f, 0.07f, 0.19f, 0.0f);
        public bool clearDepth = true;
        
        [Tooltip("Layer Mask used for the volume interpolation for this camera.")]
        public LayerMask volumeLayerMask = -1;
        [Tooltip("Transform used for the volume interpolation for this camera.")]
        public Transform volumeAnchorOverride;

        // Physical parameters
        public float aperture = 8f;
        public float shutterSpeed = 1f / 200f;
        public float iso = 400f;
        public FlipYMode flipYMode;
        
        [Tooltip("This will skip rendering settings to directly rendering in fullscreen (for instance: Useful for video)")]
        public bool fullscreenPassthrough = false;

        [Tooltip("This will allows to override default settings for this renderer")]
        public bool customRenderingSettings = false;

        // Event used to override HDRP rendering for this particular camera.
        public event Action<ScriptableRenderContext, HDCamera> customRender;
        public bool hasCustomRender { get { return customRender != null; } }
        
        // To be able to turn on/off FrameSettings properties at runtime for debugging purpose without affecting the original one
        // we create a runtime copy (m_ActiveFrameSettings that is used, and any parametrization is done on serialized frameSettings)
        [SerializeField]
        [FormerlySerializedAs("serializedFrameSettings")]
        FrameSettings m_FrameSettings = new FrameSettings(); // Serialize frameSettings

        // Not serialized, visible only in the debug windows
        FrameSettings m_FrameSettingsRuntime = new FrameSettings();

        bool m_frameSettingsIsDirty = true;

        // Use for debug windows
        // When camera name change we need to update the name in DebugWindows.
        // This is the purpose of this class
        bool m_IsDebugRegistered = false;
        string m_CameraRegisterName;

        public bool IsDebugRegistred()
        {
            return m_IsDebugRegistered;
        }

        // When we are a preview, there is no way inside Unity to make a distinction between camera preview and material preview.
        // This property allow to say that we are an editor camera preview when the type is preview.
        public bool isEditorCameraPreview { get; set; }

        // This is use to copy data into camera for the Reset() workflow in camera editor
        public void CopyTo(HDAdditionalCameraData data)
        {
            data.clearColorMode = clearColorMode;
            data.backgroundColorHDR = backgroundColorHDR;
            data.clearDepth = clearDepth;
            data.customRenderingSettings = customRenderingSettings;
            data.volumeLayerMask = volumeLayerMask;
            data.volumeAnchorOverride = volumeAnchorOverride;
            data.aperture = aperture;
            data.shutterSpeed = shutterSpeed;
            data.iso = iso;

            m_FrameSettings.CopyTo(data.m_FrameSettings);
            m_FrameSettingsRuntime.CopyTo(data.m_FrameSettingsRuntime);
            data.m_frameSettingsIsDirty = true; // Let's be sure it is dirty for update

            // We must not copy the following
            //data.m_IsDebugRegistered = m_IsDebugRegistered;
            //data.m_CameraRegisterName = m_CameraRegisterName;
            //data.isEditorCameraPreview = isEditorCameraPreview;
        }

        public void SetPersistentFrameSettings(FrameSettings settings)
        {
            m_FrameSettings = settings;
            m_frameSettingsIsDirty = true;
        }

        // This is the function use outside to access FrameSettings. It return the current state of FrameSettings for the camera
        // taking into account the customization via the debug menu
        public FrameSettings GetFrameSettings()
        {
            return m_FrameSettingsRuntime;
        }

        // This allows to read serialized value in readonly mode
        internal T ReadSerializedFrameSettings<T>(Func<FrameSettings, T> reader) where T : struct => reader(m_FrameSettings);

        // IDebugData interface required to reset DebugMenu's FrameSettings
        Action IDebugData.GetReset() => () => m_FrameSettings.CopyTo(m_FrameSettingsRuntime);

        // This function is call at the beginning of camera loop in HDRenderPipeline.Render()
        // It allow to correctly init the m_FrameSettingsRuntime to use.
        // If the camera use defaultFrameSettings it must be copied in m_FrameSettingsRuntime
        // otherwise it is the serialized m_FrameSettings that are used
        // This is required so each camera have its own debug settings even if they all use the RenderingPath.Default path
        // and important at Runtime as Default Camera from Scene Preview doesn't exist
        // assetFrameSettingsIsDirty is the current dirty frame settings state of HDRenderPipelineAsset
        // if it is dirty and camera use RenderingPath.Default, we need to update it
        // defaultFrameSettings are the settings store in the HDRenderPipelineAsset
        public void UpdateDirtyFrameSettings(bool assetFrameSettingsIsDirty, FrameSettings defaultFrameSettings)
        {
            if (m_frameSettingsIsDirty || assetFrameSettingsIsDirty)
            {
                // We do a copy of the settings to those effectively used
                defaultFrameSettings.CopyTo(m_FrameSettingsRuntime);

                if (!fullscreenPassthrough && customRenderingSettings)
                    m_FrameSettings.ApplyOverrideOn(m_FrameSettingsRuntime);

                m_frameSettingsIsDirty = false;
            }
        }

        // For custom projection matrices
        // Set the proper getter
        public NonObliqueProjectionGetter nonObliqueProjectionGetter = GeometryUtils.CalculateProjectionMatrix;

        public Matrix4x4 GetNonObliqueProjection(Camera camera)
        {
            return nonObliqueProjectionGetter(camera);
        }

        void RegisterDebug()
        {
            if (!m_IsDebugRegistered)
            {
                // Note that we register m_FrameSettingsRuntime, so manipulating it in the Debug windows
                // doesn't affect the serialized version
                if (m_camera.cameraType != CameraType.Preview && m_camera.cameraType != CameraType.Reflection)
                {
                    DebugDisplaySettings.RegisterCamera(m_camera, this);
                }
                m_CameraRegisterName = m_camera.name;
                m_IsDebugRegistered = true;
            }
        }

        void UnRegisterDebug()
        {
            if (m_camera == null)
                return;

            if (m_IsDebugRegistered)
            {
                if (m_camera.cameraType != CameraType.Preview && m_camera.cameraType != CameraType.Reflection)
                {
                    DebugDisplaySettings.UnRegisterCamera(m_camera, this);
                }
                m_IsDebugRegistered = false;
            }
        }

        void OnEnable()
        {
            // Be sure legacy HDR option is disable on camera as it cause banding in SceneView. Yes, it is a contradiction, but well, Unity...
            // When HDR option is enabled, Unity render in FP16 then convert to 8bit with a stretch copy (this cause banding as it should be convert to sRGB (or other color appropriate color space)), then do a final shader with sRGB conversion
            // When LDR, unity render in 8bitSRGB, then do a final shader with sRGB conversion
            // What should be done is just in our Post process we convert to sRGB and store in a linear 10bit, but require C++ change...
            m_camera = GetComponent<Camera>();
            if (m_camera == null)
                return;

            m_camera.allowMSAA = false; // We don't use this option in HD (it is legacy MSAA) and it produce a warning in the inspector UI if we let it
            m_camera.allowHDR = false;

            //  Tag as dirty so frameSettings are correctly initialize at next HDRenderPipeline.Render() call
            m_frameSettingsIsDirty = true;

            RegisterDebug();
        }

        void Update()
        {
            // We need to detect name change in the editor and update debug windows accordingly
#if UNITY_EDITOR
            // Caution: Object.name generate 48B of garbage at each frame here !
            if (m_camera.name != m_CameraRegisterName)
            {
                UnRegisterDebug();
                RegisterDebug();
            }
#endif
        }

        void OnDisable()
        {
            UnRegisterDebug();
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            // This is call on load or when this settings are change.
            // When FrameSettings are manipulated or RenderPath change we reset them to reflect the change, discarding all the Debug Windows change.
            // Tag as dirty so frameSettings are correctly initialize at next HDRenderPipeline.Render() call
            m_frameSettingsIsDirty = true;
        }

        // This is called at the creation of the HD Additional Camera Data, to convert the legacy camera settings to HD
        public static void InitDefaultHDAdditionalCameraData(HDAdditionalCameraData cameraData)
        {
            var camera = cameraData.gameObject.GetComponent<Camera>();

            cameraData.clearDepth = camera.clearFlags != CameraClearFlags.Nothing;

            if (camera.clearFlags == CameraClearFlags.Skybox)
                cameraData.clearColorMode = ClearColorMode.Sky;
            else if (camera.clearFlags == CameraClearFlags.SolidColor)
                cameraData.clearColorMode = ClearColorMode.BackgroundColor;
            else     // None
                cameraData.clearColorMode = ClearColorMode.None;
        }

        public void ExecuteCustomRender(ScriptableRenderContext renderContext, HDCamera hdCamera)
        {
            if (customRender != null)
            {
                customRender(renderContext, hdCamera);
            }
        }
    }
}
