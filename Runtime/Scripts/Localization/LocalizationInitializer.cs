/*
Copyright (c) 2025 MultiSet AI. All rights reserved.
Licensed under the MultiSet License. You may not use this file except in compliance with the License. and you can’t re-distribute this file without a prior notice
For license details, visit www.multiset.ai.
Redistribution in source or binary forms must retain this notice.
*/

using UnityEngine;

namespace MultiSet
{
    /// This script stays in Unity and wires up the DLL components with Unity-specific implementations
    public class LocalizationInitializer : MonoBehaviour
    {
        [Header("Core Components")]
        private SingleFrameLocalizationManager localizationManager;
        private FrameCaptureManager frameCaptureManager;
        private QuestInputHandler questInputHandler;

        private void Awake()
        {
            localizationManager = FindFirstObjectByType<SingleFrameLocalizationManager>();
            frameCaptureManager = FindFirstObjectByType<FrameCaptureManager>();
            questInputHandler = FindFirstObjectByType<QuestInputHandler>();

            if (localizationManager == null)
                Debug.LogError("LocalizationManager is not found!");

            if (frameCaptureManager == null)
                Debug.LogError("FrameCaptureManager is not found!");

            if (questInputHandler == null)
                Debug.LogError("QuestInputHandler is not found!");

            // Initialize the localization manager with the frame capture provider
            localizationManager.Initialize(frameCaptureManager);

            Debug.Log("Localization system initialized successfully");
        }

        // Alternative: Initialize through code if components are created dynamically
        public void InitializeManually(SingleFrameLocalizationManager locManager, IFrameCaptureProvider captureProvider)
        {
            if (locManager != null && captureProvider != null)
            {
                locManager.Initialize(captureProvider);
            }
        }
    }
}