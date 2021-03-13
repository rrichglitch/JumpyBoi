using UnityEngine;
using UnityEditor;

public class CreateSoft2DSim
{
    [MenuItem("GameObject/2D Object/SquishySprite")]
    public static void create(MenuCommand menuCommand){
        GameObject go = new GameObject("Squishy Sprite", typeof(SpriteSquishifier));
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
}
