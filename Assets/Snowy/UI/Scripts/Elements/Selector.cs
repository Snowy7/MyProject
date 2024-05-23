using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Snowy.UI
{
    public class Selector : MonoBehaviour
    {
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TMP_Text text;
        public UnityEvent onValueChanged;
        
        private int m_index;
        private string[] m_options = {"Option 1", "Option 2", "Option 3"};
        
        private void Start()
        {
            leftButton.onClick.AddListener(OnLeftButtonClicked);
            rightButton.onClick.AddListener(OnRightButtonClicked);
            UpdateText();
        }
        
        private void OnLeftButtonClicked()
        {
            m_index--;
            if (m_index < 0)
            {
                m_index = m_options.Length - 1;
            }
            UpdateText();
        }
        
        private void OnRightButtonClicked()
        {
            m_index++;
            if (m_index >= m_options.Length)
            {
                m_index = 0;
            }
            UpdateText();
        }
        
        private void UpdateText()
        {
            text.text = m_options[m_index];
            onValueChanged.Invoke();
        }
        
        public void SetOptions(string[] options)
        {
            m_options = options;
            m_index = 0;
            UpdateText();
        }
        
        public void SetIndex(int index)
        {
            m_index = index;
            UpdateText();
        }
        
        public string GetSelectedOption()
        {
            return m_options[m_index];
        }
        
        public int GetSelectedIndex()
        {
            return m_index;
        }
    }
}