using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GCharacter))]
public class GCharacterEditor : Editor {

  private bool showInfo = true;

  public override void OnInspectorGUI() {
    DrawDefaultInspector();
    GCharacter character = (GCharacter) target;
    GUILayout.Space(EditorGUIUtility.singleLineHeight);
    showInfo = EditorGUILayout.Foldout(showInfo, "Info");
    if (showInfo) {
      EditorGUI.indentLevel ++;
      EditorGUILayout.ObjectField("Seat", character.seat, typeof(Seat), true);
      EditorGUILayout.ObjectField("Vehicle", character.vehicle, typeof(Vehicle), true);
      EditorGUILayout.ObjectField("Carried", character.carried, typeof(Carryable), true);
      EditorGUI.indentLevel --;
    }
  }

}
