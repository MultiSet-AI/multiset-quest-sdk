/*
Copyright (c) 2026 MultiSet AI. All rights reserved.
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
        private SingleFrameLocalizationManager m_singleFrameLocalizationManager;
        private MapLocalizationManager m_mapLocalizationManager;
        private ObjectTrackingManager m_objectTrackingManager;

        private FrameCaptureManager m_frameCaptureManager;
        private QuestInputHandler m_questInputHandler;

        private void Awake()
        {
            m_singleFrameLocalizationManager = FindFirstObjectByType<SingleFrameLocalizationManager>();
            m_mapLocalizationManager = FindFirstObjectByType<MapLocalizationManager>();
            m_objectTrackingManager = FindFirstObjectByType<ObjectTrackingManager>();

            m_frameCaptureManager = FindFirstObjectByType<FrameCaptureManager>();
            m_questInputHandler = FindFirstObjectByType<QuestInputHandler>();

            if (m_singleFrameLocalizationManager != null)
            {
                m_singleFrameLocalizationManager.Initialize(m_frameCaptureManager);
            }

            if (m_mapLocalizationManager != null)
            {
                m_mapLocalizationManager.Initialize(m_frameCaptureManager);
            }

            if (m_objectTrackingManager != null)
            {
                m_objectTrackingManager.Initialize(m_frameCaptureManager);
            }

            if (m_frameCaptureManager == null)
                Debug.LogError("FrameCaptureManager is not found!");

            if (m_questInputHandler == null)
                Debug.LogError("QuestInputHandler is not found!");

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