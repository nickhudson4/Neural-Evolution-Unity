using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    NetworkManager networkManager;
    int index;
    bool isInit = false;

    public void init(int index, NetworkManager networkManager){
        this.networkManager = networkManager;
        this.index = index;

        isInit = true;
    }

    void Update(){

    }

    void OnTriggerEnter(Collider other){
        GameObject otherGO = other.gameObject;
        if (otherGO.tag != "Player" || !isInit){ return; }
        networkManager.pressurePlateOnTrigger(index, otherGO);

    }
}
