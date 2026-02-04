/*
Copyright (c) 2026 MultiSet AI. All rights reserved.
Licensed under the MultiSet License. You may not use this file except in compliance with the License. and you can’t re-distribute this file without a prior notice
For license details, visit www.multiset.ai.
Redistribution in source or binary forms must retain this notice.
*/

using UnityEngine;

namespace MultiSet
{
    public class LocalizationDataHandler : MonoBehaviour
    {
        private SingleFrameLocalizationManager singleFrameLocalizationManager;
        private MapLocalizationManager mapLocalizationManager;

        void Start()
        {
            singleFrameLocalizationManager = FindFirstObjectByType<SingleFrameLocalizationManager>();
            mapLocalizationManager = FindFirstObjectByType<MapLocalizationManager>();

            // Subscribe to SingleFrameLocalizationManager
            if (singleFrameLocalizationManager != null)
            {
                singleFrameLocalizationManager.OnLocalizationWithResponse += OnSingleFrameLocalizationResponse;
            }

            // Subscribe to MapLocalizationManager
            if (mapLocalizationManager != null)
            {
                mapLocalizationManager.OnLocalizationWithResponse += OnMultiFrameLocalizationResponse;
            }
        }

        void OnSingleFrameLocalizationResponse(LocalizationSuccessResponse response)
        {
            Debug.Log($"SingleFrame Localization Response: {JsonUtility.ToJson(response)}");
            // Access response.position, response.rotation, response.confidence, response.mapCodes, etc.
        }

        void OnMultiFrameLocalizationResponse(LocalizationResponseMultiFrame response)
        {
            Debug.Log($"MultiFrame Localization Response: {JsonUtility.ToJson(response)}");
            // Access response.estimatedPose, response.trackingPose, response.confidence, response.mapCodes, etc.
        }

        void OnDestroy()
        {
            // Unsubscribe from SingleFrameLocalizationManager
            if (singleFrameLocalizationManager != null)
            {
                singleFrameLocalizationManager.OnLocalizationWithResponse -= OnSingleFrameLocalizationResponse;
            }

            // Unsubscribe from MapLocalizationManager
            if (mapLocalizationManager != null)
            {
                mapLocalizationManager.OnLocalizationWithResponse -= OnMultiFrameLocalizationResponse;
            }
        }
    }
}