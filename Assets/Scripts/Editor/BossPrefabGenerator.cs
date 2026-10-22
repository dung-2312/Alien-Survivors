#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AlienSurvivors.EditorTools
{
    // Builds "Boss - Brood Queen.prefab" from the Green Bee (a giant hive queen that summons bees)
    // and assigns it to the EnemySpawner in the open scene.
    public static class BossPrefabGenerator
    {
        const string BodyPath = "Assets/Prefabs/Enemies/Enemy 1 - Green Bee.prefab";
        const string MinionPath = "Assets/Prefabs/Enemies/Enemy 1 - Green Bee.prefab";
        const string BossPath = "Assets/Prefabs/Enemies/Boss - Brood Queen.prefab";

        [MenuItem("Tools/Survivors/Create Brood Queen Boss")]
        public static void CreateBroodQueen()
        {
            GameObject bodySource = AssetDatabase.LoadAssetAtPath<GameObject>(BodyPath);
            GameObject minion = AssetDatabase.LoadAssetAtPath<GameObject>(MinionPath);
            if (bodySource == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not find " + BodyPath, "OK");
                return;
            }

            GameObject go = Object.Instantiate(bodySource);
            go.transform.position = Vector3.zero;
            BossController.ConfigureAsBroodQueen(go, minion);

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, BossPath);
            Object.DestroyImmediate(go);

            EnemySpawner spawner = Object.FindAnyObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                Undo.RecordObject(spawner, "Assign Boss");
                spawner.bossPrefab = saved;
                EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene);
            }

            Selection.activeObject = saved;
            EditorGUIUtility.PingObject(saved);
            EditorUtility.DisplayDialog("Brood Queen created",
                BossPath + (spawner != null
                    ? "\n\nAssigned to the EnemySpawner (arrives at " + spawner.bossSpawnTime + "s). Save the scene to keep it."
                    : "\n\nNo EnemySpawner in the open scene - open Main and assign it to EnemySpawner > Boss Prefab."),
                "OK");
        }
    }
}
#endif
