using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NotesStatics
{
    public static Dictionary<string, List<bool>> notesPre = new Dictionary<string, List<bool>>(){
        {"001", new List<bool>() {true, false, true, }},
        {"002", new List<bool>() { } }
    };
}
