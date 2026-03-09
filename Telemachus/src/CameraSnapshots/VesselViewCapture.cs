using System;
using System.Reflection;
using UnityEngine;

namespace Telemachus.CameraSnapshots
{
    /// <summary>
    /// Captures vessel diagram images from VesselView mod (if installed).
    /// VesselView renders a 2D schematic of the vessel (wireframe, mesh, bounding boxes)
    /// with color-coding for fuel, stages, temperature, etc.
    ///
    /// All access is via reflection — VesselView is a soft dependency.
    /// </summary>
    class VesselViewCapture : CameraCapture
    {
        // Reflected VesselViewer instance and methods
        object viewer;
        MethodInfo drawCallMethod;
        MethodInfo nilOffsetMethod;
        object settings;
        FieldInfo screenVisibleField;

        bool initialized;
        bool available;

        const int RENDER_SIZE = 512;

        public override string cameraManagerName() => "vesselview";
        public override string cameraType() => "VesselView";

        /// <summary>
        /// Try to create a VesselViewer instance via reflection.
        /// Returns false if VesselView mod is not installed.
        /// </summary>
        bool Initialize()
        {
            if (initialized) return available;
            initialized = true;

            try
            {
                Type viewerType = null;
                foreach (var asm in AssemblyLoader.loadedAssemblies)
                {
                    viewerType = asm.assembly.GetType("VesselView.VesselViewer", false);
                    if (viewerType != null) break;
                }

                if (viewerType == null)
                {
                    PluginLogger.debug("VesselView not found");
                    return false;
                }

                viewer = Activator.CreateInstance(viewerType);
                drawCallMethod = viewerType.GetMethod("drawCall",
                    BindingFlags.Public | BindingFlags.Instance,
                    null, new[] { typeof(RenderTexture) }, null);
                nilOffsetMethod = viewerType.GetMethod("nilOffset",
                    BindingFlags.Public | BindingFlags.Instance,
                    null, new[] { typeof(int), typeof(int) }, null);

                if (drawCallMethod == null)
                {
                    PluginLogger.debug("VesselView: drawCall method not found");
                    return false;
                }

                // Get settings and mark screen as visible so it actually renders
                var settingsField = viewerType.GetField("basicSettings",
                    BindingFlags.Public | BindingFlags.Instance);
                if (settingsField != null)
                {
                    settings = settingsField.GetValue(viewer);
                    var settingsType = settings.GetType();
                    screenVisibleField = settingsType.GetField("screenVisible",
                        BindingFlags.Public | BindingFlags.Instance);

                    // Set latency to OFF so it renders every frame we ask
                    var latencyField = settingsType.GetField("latency",
                        BindingFlags.Public | BindingFlags.Instance);
                    latencyField?.SetValue(settings, 0); // LATENCY.OFF = 0

                    // Configure for clean schematic output
                    var autoCenter = settingsType.GetField("autoCenter",
                        BindingFlags.Public | BindingFlags.Instance);
                    autoCenter?.SetValue(settings, true);
                }

                // Center the drawing
                nilOffsetMethod?.Invoke(viewer, new object[] { RENDER_SIZE, RENDER_SIZE });

                available = true;
                PluginLogger.debug("VesselView initialized successfully");
            }
            catch (Exception e)
            {
                PluginLogger.debug("VesselView init failed: " + e.Message);
                available = false;
            }

            return available;
        }

        protected override void LateUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight || mutex) return;
            if (!Initialize()) return;

            mutex = true;
            StartCoroutine(RenderVesselView());
        }

        System.Collections.IEnumerator RenderVesselView()
        {
            yield return new WaitForEndOfFrame();

            try
            {
                if (overviewTexture == null)
                    overviewTexture = new RenderTexture(RENDER_SIZE, RENDER_SIZE, 24);

                // Make sure VesselView thinks it's visible
                screenVisibleField?.SetValue(settings, true);

                // Ask VesselView to draw onto our RenderTexture
                drawCallMethod.Invoke(viewer, new object[] { overviewTexture });

                // Read back to CPU and encode
                Texture2D texture = getTexture2DFromRenderTexture();
                imageBytes = texture.EncodeToJPG(85);
                didRender = true;
                Destroy(texture);
            }
            catch (Exception e)
            {
                PluginLogger.debug("VesselView render error: " + e.Message);
            }

            // Throttle to ~1 FPS like other camera captures
            yield return new WaitForSeconds(1.0f + (.3f * renderOffsetFactor));
            mutex = false;
        }

        // We don't duplicate game cameras — VesselView does its own GL rendering
        public override void repositionCamera() { }
    }
}
