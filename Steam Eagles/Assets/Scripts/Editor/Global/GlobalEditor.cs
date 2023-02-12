using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using PhysicsFun.DynamicBlocks;
using Players;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Spaces;
using UnityEditor;
using Utilities;

namespace MyEditor.Global
{
    public class GlobalEditor : OdinMenuEditorWindow
    {

        [MenuItem("Tools/Global Editor Menu")]
        private static void Open()
        {
            var window = GetWindow<GlobalEditor>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true);
            var characterConfigs = GetCharacterConfigs().ToArray();
            
            if (characterConfigs.Length <= 1)
            {
                if(characterConfigs.Length == 1)
                    tree.Add("Character Config", characterConfigs[0]);
            }
            else
            {
                foreach (var characterConfig in characterConfigs)
                {
                    tree.Add($"Character Config/{characterConfig.name}", characterConfig);
                }
            }

            var players = FindAllPlayers();
            foreach (var player in players)
            {
                tree.Add($"Players/{player.name}", player);
            }

            var saveSlots = FindAllSaveSlots();
            foreach (var saveSlot in saveSlots)
            {
                tree.Add($"Save Slots/{saveSlot.name}", saveSlot);
            }
            
            var gameFX = FindAllGameFX();
            foreach (var gameFxe in gameFX)
            {
                tree.Add($"Game FX/{gameFxe.name}", gameFxe);
            }

            var pipeTiles = FindAllPipeTiles();
            var solidTiles = FindAllSolidTiles();
            var dynamicBlocks = FindAllDynamicBlocks();
            foreach (var pipeTile in pipeTiles)
            {
                tree.Add($"Tiles/Pipe Tiles/{pipeTile.name}", pipeTile);
            }

            foreach (var solidTile in solidTiles)
            {
                tree.Add($"Tiles/Solid Tiles/{solidTile.name}", solidTile);
            }

            foreach (var dynamicBlock in dynamicBlocks)
            {
                tree.Add($"Tiles/Dynamic Blocks/{dynamicBlock.name}", dynamicBlock);
            }

            return tree;
        }

        IEnumerable<CharacterConfig> GetCharacterConfigs()
        {
            return AssetDatabase.FindAssets("t:CharacterConfig")
                .Select(t => AssetDatabase.LoadAssetAtPath<CharacterConfig>(AssetDatabase.GUIDToAssetPath(t)));
        }
        IEnumerable<GameFX> FindAllGameFX()
        {
            return AssetDatabase.FindAssets("t:GameFX")
                .Select(t => AssetDatabase.LoadAssetAtPath<GameFX>(AssetDatabase.GUIDToAssetPath(t)));
        }
        IEnumerable<Player> FindAllPlayers()
        {
            return AssetDatabase.FindAssets("t:Player")
                .Select(t => AssetDatabase.LoadAssetAtPath<Player>(AssetDatabase.GUIDToAssetPath(t)));
        }
        IEnumerable<DynamicBlock> FindAllDynamicBlocks()
        {
            return AssetDatabase.FindAssets("t:DynamicBlock")
                .Select(t => AssetDatabase.LoadAssetAtPath<DynamicBlock>(AssetDatabase.GUIDToAssetPath(t)));
        }
        IEnumerable<PuzzleTile> FindAllPuzzleTiles()
        {
            return AssetDatabase.FindAssets("t:PuzzleTile")
                .Select(t => AssetDatabase.LoadAssetAtPath<PuzzleTile>(AssetDatabase.GUIDToAssetPath(t)));
        }
        
        IEnumerable<SolidTile> FindAllSolidTiles()
        {
            return AssetDatabase.FindAssets("t:SolidTile")
                .Select(t => AssetDatabase.LoadAssetAtPath<SolidTile>(AssetDatabase.GUIDToAssetPath(t)));
        }
        IEnumerable<PipeTile> FindAllPipeTiles()
        {
            return AssetDatabase.FindAssets("t:PipeTile")
                .Select(t => AssetDatabase.LoadAssetAtPath<PipeTile>(AssetDatabase.GUIDToAssetPath(t)));
        }

        IEnumerable<SaveSlot> FindAllSaveSlots()
        {
            return AssetDatabase.FindAssets("t:SaveSlot")
                .Select(t => AssetDatabase.LoadAssetAtPath<SaveSlot>(AssetDatabase.GUIDToAssetPath(t)));
        }
    }
}
