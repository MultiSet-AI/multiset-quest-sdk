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
        [SerializeField] private SingleFrameLocalizationManager localizationManager;
        [SerializeField] private FrameCaptureManager frameCaptureManager;

        [Header("Optional Components")]
        [SerializeField] private QuestInputHandler questInputHandler;

        private void Awake()
        {
            // Validate references
            if (localizationManager == null)
            {
                Debug.LogError("LocalizationManager is not assigned!");
                return;
            }

            if (frameCaptureManager == null)
            {
                Debug.LogError("FrameCaptureManager is not assigned!");
                return;
            }

            // Initialize the localization manager with the frame capture provider
            localizationManager.Initialize(frameCaptureManager);

            // Optional: Setup input handler if present
            if (questInputHandler != null)
            {
                Debug.Log("Quest input handler found and configured");
            }

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