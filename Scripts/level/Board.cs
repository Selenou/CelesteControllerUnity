using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Board", menuName = "2DLab/Board")]
public class Board : ScriptableObject {
    public int Id;
    public List<Spawn> Spawns;
    public string VirtualCameraId;

    [Serializable]
    public class Spawn { 
        public int TransitionId;
        public string SpawnId;
    }
} 