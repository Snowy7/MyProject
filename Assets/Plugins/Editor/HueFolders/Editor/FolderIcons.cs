using System;
using UnityEngine;

namespace HueFolders
{
    [Serializable] public class FolderIcon
    {
        public Texture2D icon;
        public Color color;
        public string[] folderNames;
    }
    
    [CreateAssetMenu(menuName = "Plugins/Editor/Icons/FolderIcons", fileName = "FolderIcons")]
    public class FolderIcons : ScriptableObject
    {
        public FolderIcon[] folderIcons;
        
        public void OnValidate()
        {
            
        }
        
        public Texture2D GetIcon(string folderName)
        {
            foreach (var folderIcon in folderIcons)
            {
                foreach (var name in folderIcon.folderNames)
                {
                    if (folderName == name)
                    {
                        return folderIcon.icon;
                    }
                }
            }

            return null;
        }
        
        public Color? GetColor(string folderName)
        {
            foreach (var folderIcon in folderIcons)
            {
                foreach (var name in folderIcon.folderNames)
                {
                    if (folderName == name)
                    {
                        return folderIcon.color;
                    }
                }
            }

            return null;
        }
    }
}