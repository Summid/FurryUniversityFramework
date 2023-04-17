using System;
using System.Collections.Generic;

namespace SFramework.Utilities.Archive
{
    [Serializable]
    public class ArchiveList
    {
        public List<ArchiveObject> ArchiveObjects;
    }
    
    [Serializable]
    public class ArchiveObject
    {
        public int ArchiveIndex;
        
        public string ChapterName; // file name

        public string DialogueName;

        public int ContentIndex; // >= content count if at choice

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