using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AIDebug {
    
    AIMaster self;
    TextMesh AIStateDebug;
    // List<TextMesh> textMeshes = new();

    public AIDebug(AIMaster self){
        this.self = self;
    }

    public void GenerateTextGameObject(string text){
        GameObject textGameObject = new();
        textGameObject.transform.parent = self.transform;
        textGameObject.name = $"{text}_text";
        TextMesh textComponent = textGameObject.AddComponent<TextMesh>();
        // Set up initial text properties
        textComponent.text = text; // Initial text
        textComponent.characterSize = 0.1f; // Text size
        textComponent.fontSize = 24; // Font size
        textComponent.anchor = TextAnchor.MiddleCenter; // Text anchor (centered)
        textComponent.alignment = TextAlignment.Center; // Text alignment
        textComponent.color = Color.white; // Text color
        textComponent.transform.position = self.transform.position; // Position above the GameObject
        // textMeshes.Add(textComponent);
    }

    public void InitGenerateStateDebug(){
        GameObject textGameObject = new();
        textGameObject.transform.parent = self.transform;
        textGameObject.name = "DebugTitle_text";
        TextMesh textComponent = textGameObject.AddComponent<TextMesh>();
        // Set up initial text properties
        textComponent.text = "AI State: Initializing"; // Initial text
        textComponent.characterSize = 0.1f; // Text size
        textComponent.fontSize = 50; // Font size
        textComponent.anchor = TextAnchor.MiddleCenter; // Text anchor (centered)
        textComponent.alignment = TextAlignment.Center; // Text alignment
        textComponent.color = Color.white; // Text color
        textComponent.transform.position = self.transform.position;
        AIStateDebug = textComponent;
    }

    public void UpdateAIStateText(AIState newState) {
        if (AIStateDebug != null) {
            AIStateDebug.text = $"AI State: {newState}" ;
        }
    }
    
}
