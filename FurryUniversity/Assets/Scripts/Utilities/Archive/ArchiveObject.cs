using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SFramework.Utilities.Archive
{
    [Serializable]
    public class Archive
    {
        public List<ArchiveObject> ArchiveObjects;

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (ArchiveObject archiveObject in this.ArchiveObjects)
            {
                stringBuilder.Append(archiveObject.ToString() + "\n");
            }

            return stringBuilder.ToString();
        }
    }
    
    [Serializable]
    public class ArchiveObject
    {
        public int ArchiveIndex;
        
        public string ChapterName; // file name

        public string DialogueName;

        public int ContentIndex; // >= content count if at choice

        public ArchiveObject(int archiveIndex)
        {
            this.ArchiveIndex = archiveIndex;
            this.ChapterName = string.Empty;
            this.DialogueName = string.Empty;
            this.ContentIndex = -1;
        }
        
        public ArchiveObject Merge(ArchiveObject archiveObject)
        {
            if (archiveObject == null)
                return this;
            
            if (!string.IsNullOrEmpty(archiveObject.ChapterName))
                this.ChapterName = archiveObject.ChapterName;

            if (!string.IsNullOrEmpty(archiveObject.DialogueName))
                this.DialogueName = archiveObject.DialogueName;

            if (archiveObject.ContentIndex > 0)
                this.ContentIndex = archiveObject.ContentIndex;
            
            return this;
        }

        public override string ToString()
        {
            return $"ChapterName:{this.ChapterName}; DialogueName:{this.DialogueName}; ContentIndex:{this.ContentIndex}";
        }
    }
}