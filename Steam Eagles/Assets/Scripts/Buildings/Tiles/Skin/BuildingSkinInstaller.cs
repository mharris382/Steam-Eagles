using Buildings.Tiles.Skin;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector;
#endif
[CreateAssetMenu(fileName = "BuildingSkinInstaller", menuName = "Steam Eagles/Installers/BuildingSkinInstaller")]
public class BuildingSkinInstaller : ScriptableObjectInstaller<BuildingSkinInstaller>
{
    [Required, InlineProperty()]
    public TileSkin tileSkin;
    public override void InstallBindings()
    {
        Container.Bind<TileSkin>().FromInstance(tileSkin).AsSingle().NonLazy().IfNotBound();
    }

    [Button(), ShowIf("@this.tileSkin == null")]
    void CreateAndSaveNewTileSkin()
    {
#if UNITY_EDITOR
        if (tileSkin != null) return;
        var assetPath = AssetDatabase.GetAssetPath(this);
        var newSkin = ScriptableObject.CreateInstance<TileSkin>();
        Undo.RecordObject(this, "Create New Tile Skin");
        AssetDatabase.AddObjectToAsset(newSkin, assetPath);
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(newSkin);
        AssetDatabase.SaveAssets();
#endif
    }
}