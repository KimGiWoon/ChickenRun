using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class BounceTile : MonoBehaviour
{
    [SerializeField] private bool _isTest = true;
    [SerializeField] private float _power;
    private void Start()
    {
        if (_power != 0) return;
        switch (tag)
        {
            case "BounceTile1" :
                _power = 10f;
                break;
            case "BounceTile2" :
                _power = 7f;
                break;
            case "BounceTile3" :
                _power = 9f;
                break;
            case "BounceTile4" :
                _power = 13f;
                break;
            case "BounceTile5" :
                _power = 21f;
                break;
        }
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_isTest)
        {
            TestPlayerController player = other.gameObject.GetComponent<TestPlayerController>();
            if (player != null)
            {
                player.Bounce(_power);
            }
        }
        else
        {
            PlayerController_Map2 player = other.gameObject.GetComponent<PlayerController_Map2>();
            if (player != null)
            {
                player.Bounce(_power);
            }
        }
    }
}
