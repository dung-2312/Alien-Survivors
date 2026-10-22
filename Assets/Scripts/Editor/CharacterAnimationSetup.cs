#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AlienSurvivors.EditorTools
{
    [InitializeOnLoad]
    public static class CharacterAnimationSetup
    {
        static CharacterAnimationSetup()
        {
            EditorApplication.delayCall += AutoSetup;
        }

        [MenuItem("Tools/Survivors/Setup Character Walk Animations")]
        public static void SetupAllCharacterAnimations()
        {
            AutoSetup();
            Debug.Log("[CharacterAnimationSetup] Finished setting up walk and idle animations for all characters.");
        }

        private static void AutoSetup()
        {
            SetupHeroKnight();
            SetupLightBandit();
            SetupEvilWizard();
            SetupFemaleAdventurer();
            SetupForestRed();
        }

        private static void SetupHeroKnight()
        {
            CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/DataCharacters/Char_HeroKnight.asset");
            if (data == null) return;

            string texturePath = "Assets/Hero Knight - Pixel Art/Sprites/HeroKnight.png";
            Sprite[] allSprites = LoadSprites(texturePath);
            if (allSprites == null || allSprites.Length == 0) return;

            List<Sprite> runList = new List<Sprite>();
            List<Sprite> idleList = new List<Sprite>();

            // HeroKnight_8 to 17 are run frames
            for (int i = 8; i <= 17; i++)
            {
                Sprite s = FindSprite(allSprites, "HeroKnight_" + i);
                if (s != null) runList.Add(s);
            }

            // HeroKnight_0 to 7 are idle frames
            for (int i = 0; i <= 7; i++)
            {
                Sprite s = FindSprite(allSprites, "HeroKnight_" + i);
                if (s != null) idleList.Add(s);
            }

            if (runList.Count > 0) data.walkFrames = runList.ToArray();
            if (idleList.Count > 0) data.idleFrames = idleList.ToArray();
            data.walkFrameRate = 12f;
            data.idleFrameRate = 7f;

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        private static void SetupLightBandit()
        {
            CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/DataCharacters/Char_LightBandit.asset");
            if (data == null) return;

            string texturePath = "Assets/Bandits - Pixel Art/Sprites/LightBandit.png";
            Sprite[] allSprites = LoadSprites(texturePath);
            if (allSprites == null || allSprites.Length == 0) return;

            List<Sprite> runList = new List<Sprite>();
            List<Sprite> idleList = new List<Sprite>();

            for (int i = 8; i <= 15; i++)
            {
                Sprite s = FindSprite(allSprites, "LightBandit_" + i);
                if (s != null) runList.Add(s);
            }

            for (int i = 0; i <= 3; i++)
            {
                Sprite s = FindSprite(allSprites, "LightBandit_" + i);
                if (s != null) idleList.Add(s);
            }

            if (runList.Count > 0) data.walkFrames = runList.ToArray();
            if (idleList.Count > 0) data.idleFrames = idleList.ToArray();
            data.walkFrameRate = 12f;
            data.idleFrameRate = 6f;

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        private static void SetupEvilWizard()
        {
            CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/DataCharacters/Char_EvilWizard.asset");
            if (data == null) return;

            Sprite[] walkSprites = LoadSprites("Assets/Evil Wizard 3/Sprites/Walk.png");
            Sprite[] idleSprites = LoadSprites("Assets/Evil Wizard 3/Sprites/Idle.png");

            List<Sprite> walkList = new List<Sprite>();
            if (walkSprites != null)
            {
                for (int i = 0; i <= 7; i++)
                {
                    Sprite s = FindSprite(walkSprites, "Walk_" + i);
                    if (s != null) walkList.Add(s);
                }
            }

            List<Sprite> idleList = new List<Sprite>();
            if (idleSprites != null)
            {
                for (int i = 0; i <= 7; i++)
                {
                    Sprite s = FindSprite(idleSprites, "Idle_" + i);
                    if (s != null) idleList.Add(s);
                }
            }

            if (walkList.Count > 0) data.walkFrames = walkList.ToArray();
            if (idleList.Count > 0) data.idleFrames = idleList.ToArray();
            data.walkFrameRate = 10f;
            data.idleFrameRate = 6f;

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        private static void SetupFemaleAdventurer()
        {
            CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/DataCharacters/Char_FemaleAdventurer.asset");
            if (data == null) return;

            Sprite[] walkSprites = LoadSprites("Assets/DataCharacters/The Female Adventurer - Free/The Female Adventurer - Free/Walk/walk_Down.png");
            Sprite[] idleSprites = LoadSprites("Assets/DataCharacters/The Female Adventurer - Free/The Female Adventurer - Free/Idle/Idle_Down.png");

            List<Sprite> walkList = new List<Sprite>();
            if (walkSprites != null)
            {
                for (int i = 0; i <= 7; i++)
                {
                    Sprite s = FindSprite(walkSprites, "walk_Down_" + i);
                    if (s != null) walkList.Add(s);
                }
            }

            List<Sprite> idleList = new List<Sprite>();
            if (idleSprites != null)
            {
                for (int i = 0; i <= 7; i++)
                {
                    Sprite s = FindSprite(idleSprites, "Idle_Down_" + i);
                    if (s != null) idleList.Add(s);
                }
            }

            if (walkList.Count > 0) data.walkFrames = walkList.ToArray();
            if (idleList.Count > 0) data.idleFrames = idleList.ToArray();
            data.walkFrameRate = 10f;
            data.idleFrameRate = 6f;

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        private static void SetupForestRed()
        {
            CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/DataCharacters/Char_ForestRed.asset");
            if (data == null) return;

            Sprite[] animSprites = LoadSprites("Assets/DataCharacters/ForestNpcs/ForestNpcs_Animations.png");
            if (animSprites == null || animSprites.Length == 0) return;

            List<Sprite> walkList = new List<Sprite>();
            for (int i = 0; i <= 3; i++)
            {
                Sprite s = FindSprite(animSprites, "ForestNpcs_Animations_" + i);
                if (s != null) walkList.Add(s);
            }

            if (walkList.Count > 0) data.walkFrames = walkList.ToArray();
            data.walkFrameRate = 8f;

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        private static Sprite[] LoadSprites(string assetPath)
        {
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            List<Sprite> sprites = new List<Sprite>();
            foreach (var obj in objects)
            {
                if (obj is Sprite sp)
                {
                    sprites.Add(sp);
                }
            }
            return sprites.ToArray();
        }

        private static Sprite FindSprite(Sprite[] sprites, string name)
        {
            if (sprites == null) return null;
            foreach (var s in sprites)
            {
                if (s != null && s.name == name) return s;
            }
            return null;
        }
    }
}
#endif
