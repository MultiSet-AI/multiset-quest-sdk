/*
Copyright (c) 2026 MultiSet AI. All rights reserved.
Licensed under the MultiSet License. You may not use this file except in compliance with the License. and you can’t re-distribute this file without a prior notice
For license details, visit www.multiset.ai.
Redistribution in source or binary forms must retain this notice.
*/

using UnityEngine;

namespace MultiSet
{
    public class ObjectTrackingDataHandler : MonoBehaviour
    {
        private ObjectTrackingManager objectTrackingManager;

        void Start()
        {
            objectTrackingManager = FindFirstObjectByType<ObjectTrackingManager>();
            // Subscribe to ObjectTrackingManager
            if (objectTrackingManager != null)
            {
                objectTrackingManager.OnTrackingWithResponse += OnTrackingResponse;
            }
        }

        void OnTrackingResponse(ObjectTrackingResponse response)
        {
            Debug.Log($"Object Tracking Response: {JsonUtility.ToJson(response)}");
            // Access response.position, response.rotation, response.confidence, response.objectCodes, etc.
        }

        void OnDestroy()
        {
            // Unsubscribe from ObjectTrackingManager
            if (objectTrackingManager != null)
            {
                objectTrackingManager.OnTrackingWithResponse -= OnTrackingResponse;
            }
        }
    }
}