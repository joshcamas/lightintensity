using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ardenfall
{
    public class LightIntensity : MonoBehaviour
    {
        private List<System.Func<float,float>> IntensityFunctions = new List<System.Func<float, float>>();
        private List<System.Func<float, float>> ShadowDistanceFunctions = new List<System.Func<float, float>>();
        private List<System.Func<bool>> EnabledFunctions = new List<System.Func<bool>>();
        private Light _light;
        private float initialIntensity;

        private float[] distanceLayers;

        public void RegisterIntensityFunction(System.Func<float, float> intensityFunction)
        {
            IntensityFunctions.Add(intensityFunction);
        }

        public void UnregisterIntensityFunction(System.Func<float, float> intensityFunction)
        {
            IntensityFunctions.Remove(intensityFunction);
        }

        public void RegisterShadowDistanceFunctions(System.Func<float, float> distanceFunctions)
        {
            ShadowDistanceFunctions.Add(distanceFunctions);
        }

        public void UnregisterShadowDistanceFunctions(System.Func<float, float> distanceFunctions)
        {
            ShadowDistanceFunctions.Remove(distanceFunctions);
        }

        public void RegisterEnabledFunction(System.Func<bool> enabledFunction)
        {
            EnabledFunctions.Add(enabledFunction);
        }

        public void UnregisterEnabledFunction(System.Func<bool> enabledFunction)
        {
            EnabledFunctions.Remove(enabledFunction);
        }

        private void Awake()
        {
            _light = GetComponent<Light>();
            initialIntensity = _light.intensity;

            distanceLayers = new float[32];

        }

        private void Update()
        {
            if (_light == null)
                return;

            //Enable Check
            bool lightEnabled = true;

            foreach (var funct in EnabledFunctions)
            {
                if (funct != null)
                    lightEnabled = funct.Invoke();

                if (!lightEnabled)
                    break;
            }

            _light.enabled = lightEnabled;

            if (!lightEnabled)
                return;

            float shadowDistance = 0;

            foreach(var funct in ShadowDistanceFunctions)
            {
                float val = funct.Invoke(shadowDistance);

                if (val != -1)
                    shadowDistance = val;
            }
            
            //TODO: Only run this if shadow distance has changed
            if (shadowDistance == 0)
                _light.layerShadowCullDistances = null;
            else
            {
                for (int i = 0; i < 32; i++)
                    distanceLayers[i] = shadowDistance;

                _light.layerShadowCullDistances = distanceLayers;
            }

            //Intensity Check
            float intensity = initialIntensity;

            foreach(var funct in IntensityFunctions)
            {
                if(funct != null)
                    intensity = funct.Invoke(intensity);
            }

            _light.intensity = intensity;

            if (_light.intensity <= 0)
                _light.enabled = false;
            else
                _light.enabled = true;
        }
    }
}
