using UnityEditor.Experimental.Rendering.TestFramework;
using NUnit.Framework;
using System;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.HDPipeline.Tests
{
    public class CameraSettingsUtilitiesTests
    {
        Object m_ToClean;

        [Test]
        public void ApplySettingsThrowIfFrameSettingsIsNull()
        {
            var settings = new CameraSettings();
            var go = new GameObject();
            m_ToClean = go;
            var cam = go.AddComponent<Camera>();

            Assert.Throws<InvalidOperationException>(() => cam.ApplySettings(settings));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ApplySettings()
        {
            for (int i = 0; i < 10; ++i)
            {
                var perspectiveMatrix = Matrix4x4.Perspective(
                    RandomUtilities.RandomFloat(i, 2943.06587f) * 30.0f + 75.0f,
                    RandomUtilities.RandomFloat(i, 6402.79532f) * 0.5f + 1,
                    RandomUtilities.RandomFloat(i, 8328.97521f) * 10.0f + 10f,
                    RandomUtilities.RandomFloat(i, 6875.12374f) * 100.0f + 1000.0f
                );
                var worldToCameraMatrix = GeometryUtils.CalculateWorldToCameraMatrixRHS(
                    RandomUtilities.RandomVector3(i),
                    RandomUtilities.RandomQuaternion(i)
                );

                var settings = new CameraSettings
                {
                    bufferClearing = new CameraSettings.BufferClearing
                    {
                        backgroundColorHDR = RandomUtilities.RandomColor(i),
                        clearColorMode = RandomUtilities.RandomEnum<HDAdditionalCameraData.ClearColorMode>(i),
                        clearDepth = RandomUtilities.RandomBool(i)
                    },
                    culling = new CameraSettings.Culling
                    {
                        cullingMask = RandomUtilities.RandomInt(i),
                        useOcclusionCulling = RandomUtilities.RandomBool(i + 0.5f),
                    },
                    frameSettings = new FrameSettings(),
                    frustum = new CameraSettings.Frustum
                    {
                        aspect = RandomUtilities.RandomFloat(i, 6724.2745f) * 0.5f + 1,
                        nearClipPlane = RandomUtilities.RandomFloat(i, 7634.7235f) * 10.0f + 10f,
                        farClipPlane = RandomUtilities.RandomFloat(i, 1935.3234f) * 100.0f + 1000.0f,
                        fieldOfView = RandomUtilities.RandomFloat(i, 9364.2534f) * 30.0f + 75.0f,
                        mode = RandomUtilities.RandomEnum<CameraSettings.Frustum.Mode>(i * 2.5f),
                        projectionMatrix = perspectiveMatrix
                    },
                    volumes = new CameraSettings.Volumes
                    {
                        anchorOverride = null,
                        layerMask = RandomUtilities.RandomInt(i * 3.5f)
                    },
                    customRenderingSettings = RandomUtilities.RandomBool(i * 4.5f)
                };
                var position = new CameraPositionSettings
                {
                    mode = RandomUtilities.RandomEnum<CameraPositionSettings.Mode>(i),
                    position = RandomUtilities.RandomVector3(i * 5.5f),
                    rotation = RandomUtilities.RandomQuaternion(i * 6.5f),
                    worldToCameraMatrix = worldToCameraMatrix
                };

                var go = new GameObject("TestObject");
                m_ToClean = go;
                var cam = go.AddComponent<Camera>();

                cam.ApplySettings(settings);
                cam.ApplySettings(position);

                var add = cam.GetComponent<HDAdditionalCameraData>();
                Assert.NotNull(add);

                // Position
                switch (position.mode)
                {
                    case CameraPositionSettings.Mode.UseWorldToCameraMatrixField:
                        AssertUtilities.AssertAreEqual(position.worldToCameraMatrix, cam.worldToCameraMatrix);
                        break;
                    case CameraPositionSettings.Mode.ComputeWorldToCameraMatrix:
                        AssertUtilities.AssertAreEqual(position.position, cam.transform.position);
                        AssertUtilities.AssertAreEqual(position.rotation, cam.transform.rotation);
                        AssertUtilities.AssertAreEqual(position.ComputeWorldToCameraMatrix(), cam.worldToCameraMatrix);
                        break;
                }
                // Frustum
                switch (settings.frustum.mode)
                {
                    case CameraSettings.Frustum.Mode.UseProjectionMatrixField:
                        AssertUtilities.AssertAreEqual(settings.frustum.projectionMatrix, cam.projectionMatrix);
                        break;
                    case CameraSettings.Frustum.Mode.ComputeProjectionMatrix:
                        Assert.AreEqual(settings.frustum.nearClipPlane, cam.nearClipPlane);
                        Assert.AreEqual(settings.frustum.farClipPlane, cam.farClipPlane);
                        Assert.AreEqual(settings.frustum.fieldOfView, cam.fieldOfView);
                        Assert.AreEqual(settings.frustum.aspect, cam.aspect);
                        AssertUtilities.AssertAreEqual(settings.frustum.ComputeProjectionMatrix(), cam.projectionMatrix);
                        break;
                }
                // Culling
                Assert.AreEqual(settings.culling.useOcclusionCulling, cam.useOcclusionCulling);
                Assert.AreEqual(settings.culling.cullingMask, (LayerMask)cam.cullingMask);
                // Buffer clearing
                Assert.AreEqual(settings.bufferClearing.clearColorMode, add.clearColorMode);
                Assert.AreEqual(settings.bufferClearing.backgroundColorHDR, add.backgroundColorHDR);
                Assert.AreEqual(settings.bufferClearing.clearDepth, add.clearDepth);
                // Volumes
                Assert.AreEqual(settings.volumes.layerMask, add.volumeLayerMask);
                Assert.AreEqual(settings.volumes.anchorOverride, add.volumeAnchorOverride);
                // HD Specific
                Assert.AreEqual(settings.customRenderingSettings, add.customRenderingSettings);

                Object.DestroyImmediate(go);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (m_ToClean != null)
                CoreUtils.Destroy(m_ToClean);
        }
    }
}
