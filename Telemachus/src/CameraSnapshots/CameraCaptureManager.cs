using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Telemachus.CameraSnapshots
{
    class CameraCaptureManager : MonoBehaviour
    {
        #region Singleton management
        public static GameObject instance;

        private CameraCaptureManager() { }

        public static GameObject Instance
        {
            get
            {
                instance ??= GameObject.Find("CameraCaptureManager")
                    ?? new GameObject("CameraCaptureManager", typeof(CameraCaptureManager));
                return instance;
            }
        }

        public static CameraCaptureManager classedInstance
        {
            get
            {
                return (CameraCaptureManager)Instance.GetComponent(typeof(CameraCaptureManager));
            }
        }
        #endregion

        private CurrentFlightCameraCapture cameraCaptureTest = null;
        private VesselViewCapture vesselViewCapture = null;
        public Dictionary<string, CameraCapture> cameras = new();
        public Dictionary<Guid, List<string>> vesselCameraMappings = new();


        public void addToVesselCameraMappings(Vessel vessel, string cameraName)
        {
            if (!vesselCameraMappings.TryGetValue(vessel.id, out List<string> vesselList))
            {
                vesselList = new List<string>();
                vesselCameraMappings[vessel.id] = vesselList;
            }

            if (!vesselList.Contains(cameraName))
            {
                vesselList.Add(cameraName);
            }
        }

        protected void OnEnable()
        {
            GameEvents.onFlightReady.Add(addFlightCamera);
            GameEvents.onGameSceneLoadRequested.Add(removeFlightCameraIfNotFlight);
        }

        /// <summary>
        /// Ensures the flight camera exists. Called lazily on first camera
        /// request in case onFlightReady fired before the manager was created.
        /// </summary>
        public void EnsureFlightCamera()
        {
            if (HighLogic.LoadedSceneIsFlight && cameraCaptureTest == null)
            {
                addFlightCamera();
            }
        }

        private void removeFlightCameraIfNotFlight(GameScenes data)
        {
            if (data != GameScenes.FLIGHT)
            {
                if (cameraCaptureTest)
                {
                    removeCamera(cameraCaptureTest.cameraManagerName());
                    Destroy(cameraCaptureTest.gameObject);
                    cameraCaptureTest = null;
                }
                if (vesselViewCapture)
                {
                    removeCamera(vesselViewCapture.cameraManagerName());
                    Destroy(vesselViewCapture.gameObject);
                    vesselViewCapture = null;
                }
            }
        }

        private void addFlightCamera()
        {
            GameObject obj = new GameObject("CurrentFlightCameraCapture", typeof(CurrentFlightCameraCapture));
            this.cameraCaptureTest = (CurrentFlightCameraCapture)obj.GetComponent(typeof(CurrentFlightCameraCapture));
            addCameraCapture(cameraCaptureTest);
            addVesselViewCamera();
        }

        private void addVesselViewCamera()
        {
            if (vesselViewCapture != null) return;
            var obj = new GameObject("VesselViewCapture", typeof(VesselViewCapture));
            vesselViewCapture = obj.GetComponent<VesselViewCapture>();
            addCameraCapture(vesselViewCapture);
        }

        public bool isRemoveCameraFromManager(Vessel vessel, string name)
        {
            //PluginLogger.debug("CHECKING FOR: " + name + " IN : " + vessel.id);
            if (!vesselCameraMappings.ContainsKey(vessel.id))
            {
                return true;
            }

            //PluginLogger.debug("FOUND KEY: " + vessel.id);


            if (!vesselCameraMappings[vessel.id].Contains(name))
            {
                //PluginLogger.debug("MISSING: " + name + " IN : " + vessel.id);
                return true;
            }

            return false;
        }

        public void addCamera(RasterPropMonitorCamera camera)
        {
            if (camera == null)
            {
                return;
            }

            GameObject container = new GameObject("RasterPropMonitorCameraCapture:" + camera.cameraName, typeof(RasterPropMonitorCameraCapture));
            RasterPropMonitorCameraCapture cameraCapture = (RasterPropMonitorCameraCapture)container.GetComponent(typeof(RasterPropMonitorCameraCapture));
            cameraCapture.rpmCamera = camera;

            string name = cameraCapture.cameraManagerName().ToLower();
            cameras[name] = cameraCapture;
            cameraCapture.renderOffsetFactor = cameras.Count;
            addToVesselCameraMappings(camera.vessel, camera.cameraName);
        }

        public void addCameraCapture(CameraCapture cameraCapture)
        {
            cameras[cameraCapture.cameraManagerName().ToLower()] = cameraCapture;
            cameraCapture.renderOffsetFactor = cameras.Count;
        }

        public void removeCamera(string name)
        {
            cameras.Remove(name.ToLower());
        }
    }
}
