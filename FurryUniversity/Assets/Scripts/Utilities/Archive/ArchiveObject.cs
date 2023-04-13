using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Utilities.Archive
{
    public class ArchiveObject
    {
        public string ChapterName; // file name

        public string DialogueName;

        public int ContentIndex;

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