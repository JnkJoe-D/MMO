using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Logic.Character;
using Game.Logic.Action.Combo;

namespace Game.Logic.DebugTools
{
    public class CharacterDebugHUD : MonoBehaviour
    {
        [SerializeField] private CharacterEntity targetEntity;
        
        private GUIStyle boxStyle;
        private GUIStyle labelStyle;
        private GUIStyle titleStyle;
        private GUIStyle historyStyle;
        
        private Texture2D backgroundTexture;

        private void Start()
        {
            if (targetEntity == null)
            {
                targetEntity = GetComponent<CharacterEntity>();
                if (targetEntity == null) targetEntity = GameObject.FindWithTag("Player")?.GetComponent<CharacterEntity>();
            }
        }

        private void InitStyles()
        {
            if (boxStyle != null) return;

            // 创建圆角背景贴图 (动态生成)
            backgroundTexture = CreateRoundedTex(128, 128, 15, new Color(0.12f, 0.12f, 0.12f, 0.85f));

            boxStyle = new GUIStyle();
            boxStyle.normal.background = backgroundTexture;
            boxStyle.padding = new RectOffset(15, 15, 15, 15);

            labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 14;
            labelStyle.margin = new RectOffset(0, 0, 2, 2);

            titleStyle = new GUIStyle(labelStyle);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(0.4f, 0.8f, 1f); // 亮蓝色
            titleStyle.fontSize = 16;

            historyStyle = new GUIStyle(labelStyle);
            historyStyle.fontSize = 13;
            historyStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        }

        private void OnGUI()
        {
            if (targetEntity == null) return;

            InitStyles();

            float width = 320;
            float height = 450;
            float margin = 20;
            Rect rect = new Rect(Screen.width - width - margin, margin, width, height);

            GUILayout.BeginArea(rect, boxStyle);
            {
                GUILayout.Label("CHARACTER DEBUG HUD", titleStyle);
                GUILayout.Space(10);

                // 1. 状态机信息
                var machine = targetEntity.Machine;
                if (machine != null)
                {
                    DrawInfo("Current State", machine.CurrentState?.GetType().Name ?? "None");
                    DrawInfo("Previous State", machine.PreviousState?.GetType().Name ?? "None", new Color(0.7f, 0.7f, 0.7f));
                }

                GUILayout.Space(15);
                GUILayout.Label("COMMAND BUFFER", titleStyle);
                if (targetEntity.CommandBuffer != null)
                {
                    var cmds = targetEntity.CommandBuffer.GetUnconsumedCommands();
                    if (!cmds.Any())
                    {
                        GUILayout.Label("  (Empty)", historyStyle);
                    }
                    else
                    {
                        foreach (var cmd in cmds)
                        {
                            DrawInfo($"> {cmd.InputType}", $"{(Time.time - cmd.Timestamp):F2}s ago", Color.yellow);
                        }
                    }
                }

                GUILayout.Space(15);
                GUILayout.Label("EXECUTION HISTORY (Latest 10)", titleStyle);
                if (targetEntity.ComboController != null)
                {
                    var history = targetEntity.ComboController.ExecutionHistory;
                    if (history.Count == 0)
                    {
                        GUILayout.Label("  (No records)", historyStyle);
                    }
                    else
                    {
                        for (int i = 0; i < history.Count; i++)
                        {
                            var record = history[i];
                            // 格式： [Time] Input -> Action
                            string timeStr = record.Timestamp.ToString("F1");
                            GUILayout.Label($"<color=#aaaaaa>[{timeStr}]</color> {record.Input} <color=#66ccff>→</color> {record.ActionId}", historyStyle);
                        }
                    }
                }
            }
            GUILayout.EndArea();
        }

        private void DrawInfo(string label, string value, Color? valueColor = null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}:", labelStyle, GUILayout.Width(110));
            Color oldColor = GUI.color;
            if (valueColor.HasValue) GUI.color = valueColor.Value;
            GUILayout.Label(value, labelStyle);
            GUI.color = oldColor;
            GUILayout.EndHorizontal();
        }

        private Texture2D CreateRoundedTex(int width, int height, int radius, Color color)
        {
            Texture2D tex = new Texture2D(width, height);
            Color[] cols = new Color[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (IsInsideRoundedRect(x, y, width, height, radius))
                        cols[y * width + x] = color;
                    else
                        cols[y * width + x] = Color.clear;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            return tex;
        }

        private bool IsInsideRoundedRect(int x, int y, int w, int h, int r)
        {
            // 检查四个圆角
            if (x < r && y < r) return Vector2.Distance(new Vector2(x, y), new Vector2(r, r)) <= r;
            if (x > w - r && y < r) return Vector2.Distance(new Vector2(x, y), new Vector2(w - r, r)) <= r;
            if (x < r && y > h - r) return Vector2.Distance(new Vector2(x, y), new Vector2(r, h - r)) <= r;
            if (x > w - r && y > h - r) return Vector2.Distance(new Vector2(x, y), new Vector2(w - r, h - r)) <= r;
            return true;
        }
    }
}
