using System;
using JSONInterpreter;
using StaticTemplates.MusicGame;
using UnityEngine;

namespace Test
{
    public class JsonTest:MonoBehaviour
    {
        private void Start()
        {
            string json=Resources.Load("Test/TestNote").ToString();
            JsonInterpreter<MealNotes> interpreter = new JsonInterpreter<MealNotes>();
            MealNotes notes = interpreter.Interpret(json);
            Debug.Log(notes.spritePath1);
            Debug.Log(notes.spritePath2);
            Debug.Log(notes.spritePath3);
            Debug.Log(notes.spritePath4);
            Debug.Log(notes.track1);
            Debug.Log(notes.track2);
            Debug.Log(notes.track3);
            Debug.Log(notes.track4);
        }
    }
}