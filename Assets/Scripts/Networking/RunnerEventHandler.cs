using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking
{
    public class RunnerEventHandler : NetworkEvents
    {
        public static RunnerEventHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<RunnerEventHandler>();
                }

                return _instance;
            }
        }
        
        private static RunnerEventHandler _instance;
        
        [SerializeField] private TestGameManager gameManager;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            DontDestroyOnLoad(gameObject);
            
            OnSceneLoadDone.AddListener(OnSceneLoadDoneHandler);
        }
        
        private void OnSceneLoadDoneHandler(NetworkRunner runner)
        {
            if (SceneManager.GetActiveScene().buildIndex > 0)
            {
                if (runner.IsSceneAuthority)
                {
                    runner.Spawn(gameManager, null, null, runner.LocalPlayer);
                }
            }
            
        }
    }
}