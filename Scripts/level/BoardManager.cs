using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardManager : Singleton<BoardManager> {

    [SerializeField]
    Board firstBoard = null;
    [SerializeField]
    Gate firstGate = null;

    Player player;
    Transform currentVirtualCamera;
    Transform virtualCamParent;
    Transform spawnParent;
    
    Board currentBoard;
    Gate fromGate;
    
    void Start() {
        virtualCamParent = GameObject.Find("VCams").transform;
        spawnParent = GameObject.Find("Spawns").transform;

        player = GameObject.Find("Player").GetComponent("Player") as Player;
        player.deathEvent += OnPlayerDeath;
        player.gateEvent += OnUpdateBoard;

        currentBoard = firstBoard;
        fromGate = firstGate;
        SwapVirtualCamera();
    }

    void ResetCurrentBoard() {
        string spawnName = currentBoard.Spawns.Where(x => x.TransitionId == fromGate.Id).Select(x => x.SpawnId).First();
        player.transform.position = spawnParent.Find("Spawn_" + spawnName).transform.position;
    }

    public void OnUpdateBoard(Gate fromGate) {
        this.fromGate = fromGate;
        currentBoard = currentBoard.Id == fromGate.BoardA.Id ? fromGate.BoardB : fromGate.BoardA;
        SwapVirtualCamera();
    }

    void SwapVirtualCamera() {
        if(currentVirtualCamera)
            currentVirtualCamera.gameObject.SetActive(false);
        
        currentVirtualCamera = virtualCamParent.Find("VCam_" + currentBoard.VirtualCameraId);
        currentVirtualCamera.gameObject.SetActive(true);
    }

    void OnPlayerDeath() {
        Invoke("ResetCurrentBoard", 1.0f);
    }
}
