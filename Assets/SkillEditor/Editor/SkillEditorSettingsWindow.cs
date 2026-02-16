using UnityEngine;
using UnityEditor;
using System;

namespace SkillEditor.Editor
{
    public class SkillEditorSettingsWindow : EditorWindow
    {
        private SkillEditorState _state;
        private Action _onSettingsChanged;

        public static void Show(SkillEditorState state, Action onSettingsChanged)
        {
            var window = GetWindow<SkillEditorSettingsWindow>(Lan.SettingsPanelTitle);
            window._state = state;
            window._onSettingsChanged = onSettingsChanged;
            window.minSize = new Vector2(250, 150);
            window.Show();
        }

        private void OnGUI()
        {
            if (_state == null)
            {
                EditorGUILayout.HelpBox(Lan.SettingsWarning, MessageType.Warning);
                return;
            }

            EditorGUILayout.Space();
            GUILayout.Label(Lan.SettingsPrecisionLabel, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Frame Rate Selection
            EditorGUI.BeginChangeCheck();
            
            // Fixed values to match labels for now
            int[] frameRateValues = { 15, 30, 60 };
            string[] frameRateLabels = { "15 FPS", "30 FPS", "60 FPS" }; 
            
            _state.frameRate = EditorGUILayout.IntPopup(Lan.SettingsFrameRateLabel, _state.frameRate, frameRateLabels, frameRateValues);
            
            // Time Step Mode
            _state.timeStepMode = (TimeStepMode)EditorGUILayout.EnumPopup(Lan.SettingsTimeStepModeLabel, _state.timeStepMode);
            
            // Frame Snap Status (Read-only)
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle(Lan.SettingsFrameSnapLabel, _state.useFrameSnap);
            EditorGUI.EndDisabledGroup();
            
            // Magnet Snap Toggle
            _state.snapEnabled = EditorGUILayout.Toggle(Lan.SettingsSnapEnabledLabel, _state.snapEnabled);

            EditorGUILayout.Space();
            GUILayout.Label(Lan.SettingsSnapIntervalLabel + (_state.useFrameSnap ? $"{_state.SnapInterval:F4}s" : Lan.SettingsDynamicStep), EditorStyles.miniLabel);

            if (EditorGUI.EndChangeCheck())
            {
                _onSettingsChanged?.Invoke();
            }

            EditorGUILayout.Space();
            GUILayout.Label(Lan.PreviewSpeedMultiplier, EditorStyles.boldLabel);
            _state.previewSpeedMultiplier = EditorGUILayout.Slider(_state.previewSpeedMultiplier, 0.1f, 3f);

            // Language Selection
            EditorGUI.BeginChangeCheck();

            var languages = System.Linq.Enumerable.ToArray(Lan.AllLanguages.Keys);
            int currentIndex = System.Array.IndexOf(languages, _state.Language);
            if (currentIndex < 0) currentIndex = 0;

            int newIndex = EditorGUILayout.Popup(Lan.LanguageLabel, currentIndex, languages);

            if (EditorGUI.EndChangeCheck())
            {
                if (newIndex >= 0 && newIndex < languages.Length)
                {
                    _state.Language = languages[newIndex];
                    _onSettingsChanged?.Invoke();
                }
            }
        }
    }
}
