#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AlienSurvivors.EditorTools
{
    public static class LevelUpUISetup
    {
        [MenuItem("Tools/Survivors/Setup Sci-Fi Level Up UI")]
        public static void SetupSciFiUI()
        {
            UIController ui = Object.FindFirstObjectByType<UIController>();
            if (ui == null)
            {
                EditorUtility.DisplayDialog("Error", "UIController not found in current scene!", "OK");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Setup Sci-Fi UI");

            if (ui.levelUpPanel != null)
            {
                Undo.RecordObject(ui.levelUpPanel, "Setup LevelUp Panel");

                // 1. Dark Sci-Fi background color (#080D1A)
                Image panelImg = ui.levelUpPanel.GetComponent<Image>();
                if (panelImg != null)
                {
                    Undo.RecordObject(panelImg, "Panel Background Color");
                    panelImg.color = new Color(0.031f, 0.051f, 0.102f, 0.94f);
                }

                // 2. Hide 4 bottom shop buttons
                if (ui.moveSpeedUpgradeDisplay != null)
                {
                    Undo.RecordObject(ui.moveSpeedUpgradeDisplay.gameObject, "Hide Move Speed");
                    ui.moveSpeedUpgradeDisplay.gameObject.SetActive(false);
                }
                if (ui.healthUpgradeDisplay != null)
                {
                    Undo.RecordObject(ui.healthUpgradeDisplay.gameObject, "Hide Health");
                    ui.healthUpgradeDisplay.gameObject.SetActive(false);
                }
                if (ui.pickupRangeUpgradeDisplay != null)
                {
                    Undo.RecordObject(ui.pickupRangeUpgradeDisplay.gameObject, "Hide Pickup Range");
                    ui.pickupRangeUpgradeDisplay.gameObject.SetActive(false);
                }
                if (ui.maxWeaponsUpgradeDisplay != null)
                {
                    Undo.RecordObject(ui.maxWeaponsUpgradeDisplay.gameObject, "Hide Max Weapons");
                    ui.maxWeaponsUpgradeDisplay.gameObject.SetActive(false);
                }

                // 3. Title Text Styling
                TMP_Text[] allTexts = ui.levelUpPanel.GetComponentsInChildren<TMP_Text>(true);
                foreach (var t in allTexts)
                {
                    if (t.name.Contains("Title") || t.text.Contains("Select An Upgrade") || t.text.Contains("SELECT AN UPGRADE"))
                    {
                        Undo.RecordObject(t, "Style Title Text");
                        t.text = "<color=#00FFA3>—</color>  <b>SELECT AN UPGRADE</b>  <color=#00FFA3>—</color>";
                        t.fontSize = 38;
                        t.alignment = TextAlignmentOptions.Center;
                        t.color = Color.white;
                    }
                    else if (t.text.Contains("Skip Level Up") || t.text.Contains("SKIP"))
                    {
                        Undo.RecordObject(t, "Style Skip Text");
                        t.text = "<color=#8EA5C8><b>SKIP >></b></color>";
                    }
                }

                // 4. Align 3 Level Up Buttons
                if (ui.levelUpButtons != null && ui.levelUpButtons.Length >= 3)
                {
                    float[] xOffsets = new float[] { -380f, 0f, 380f };
                    for (int i = 0; i < 3; i++)
                    {
                        if (ui.levelUpButtons[i] != null)
                        {
                            Undo.RecordObject(ui.levelUpButtons[i].gameObject, $"Position Card {i + 1}");
                            RectTransform rt = ui.levelUpButtons[i].GetComponent<RectTransform>();
                            if (rt != null)
                            {
                                Undo.RecordObject(rt, $"Card Rect {i + 1}");
                                rt.sizeDelta = new Vector2(320f, 480f);
                                rt.anchoredPosition = new Vector2(xOffsets[i], -15f);
                            }

                            // Dark Navy button background
                            Image btnImg = ui.levelUpButtons[i].GetComponent<Image>();
                            if (btnImg != null)
                            {
                                Undo.RecordObject(btnImg, "Card Background");
                                btnImg.color = new Color(0.043f, 0.075f, 0.145f, 0.96f);
                            }

                            // Outline component
                            Outline outline = ui.levelUpButtons[i].GetComponent<Outline>();
                            if (outline == null)
                            {
                                outline = Undo.AddComponent<Outline>(ui.levelUpButtons[i].gameObject);
                            }
                            if (outline != null)
                            {
                                Undo.RecordObject(outline, "Card Outline");
                                outline.effectColor = new Color(0.12f, 0.22f, 0.35f, 0.8f);
                                outline.effectDistance = new Vector2(3f, -3f);
                            }
                        }
                    }
                }

                EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
                Debug.Log("[LevelUpUISetup] Sci-Fi Level Up UI successfully configured and scene marked dirty!");
                EditorUtility.DisplayDialog("Sci-Fi UI Configured", "Level Up UI has been updated to Modern Sci-Fi theme!\n\n1. Panel background set to #080D1A\n2. 4 legacy shop buttons hidden\n3. 3 Cards formatted to 320x480 with glow outlines\n4. Title styled with neon accents\n\nRemember to Save Scene (Ctrl + S)!", "Great!");
            }
        }
    }
}
#endif
