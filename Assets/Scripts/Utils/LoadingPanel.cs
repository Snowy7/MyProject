using TMPro;
using UnityEngine;

namespace Utils
{
    public class LoadingPanel : MonoBehaviour
    {
        public static LoadingPanel Instance { get; private set; }
        
        [Header("Loading Panel")]
        [SerializeField] GameObject loadingPanel;
        [SerializeField] TMP_Text loadingText;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            DontDestroyOnLoad(gameObject);
            
            Hide();
        }
        
        public void Show(string message = "Loading...")
        {
            if (loadingText) loadingText.text = message;
            loadingPanel.SetActive(true);
        }
        
        public void Hide()
        {
            loadingPanel.SetActive(false);
        }
    }
}