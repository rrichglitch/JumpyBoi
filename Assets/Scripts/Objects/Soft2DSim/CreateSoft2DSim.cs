using UnityEngine;
using UnityEditor;

public class CreateSoft2DSim
{
    [MenuItem("GameObject/2D Object/Soft2DSim")]
    public static void create(MenuCommand menuCommand){
        GameObject go = new GameObject("Soft 2D Obj", typeof(Soft2DSim));
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
}
