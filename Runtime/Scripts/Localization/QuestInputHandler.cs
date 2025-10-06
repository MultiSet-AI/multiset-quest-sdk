/*
Copyright (c) 2025 MultiSet AI. All rights reserved.
Licensed under the MultiSet License. You may not use this file except in compliance with the License. and you can’t re-distribute this file without a prior notice
For license details, visit www.multiset.ai.
Redistribution in source or binary forms must retain this notice.
*/

using UnityEngine;

namespace MultiSet
{
    /// Handles Meta Quest specific input and triggers localization
    /// This script stays in Unity and manages platform-specific input
    public class QuestInputHandler : MonoBehaviour
    {
        [Header("Input Configuration")]
        [SerializeField] private OVRInput.RawButton m_actionButton = OVRInput.RawButton.A;

        private SingleFrameLocalizationManager m_singleFrameLocalizationManager;
        private MapLocalizationManager m_mapLocalizationManager;

        private ModelsetTrackingManager m_modelsetTrackingManager;

        private void Awake()
        {
            m_mapLocalizationManager = FindFirstObjectByType<MapLocalizationManager>();
            m_singleFrameLocalizationManager = FindFirstObjectByType<SingleFrameLocalizationManager>();
            m_modelsetTrackingManager = FindFirstObjectByType<ModelsetTrackingManager>();
        }

        private void Update()
        {
            // Check for Quest controller input
            if (OVRInput.GetDown(m_actionButton))
            {
                TriggerLocalization();
            }
        }

        private void TriggerLocalization()
        {
            if (m_mapLocalizationManager != null)
            {
                m_mapLocalizationManager.LocalizeFrame();
            }

            if (m_singleFrameLocalizationManager != null)
            {
                m_singleFrameLocalizationManager.LocalizeFrame();
            }

            if (m_modelsetTrackingManager != null)
            {
                m_modelsetTrackingManager.StartFrameTracking();
            }
        }

        /// Alternative method to trigger localization programmatically
        public void RequestLocalization()
        {
            TriggerLocalization();
        }

        /// Change the action button at runtime if needed
        public void SetActionButton(OVRInput.RawButton button)
        {
            m_actionButton = button;
        }
    }
}