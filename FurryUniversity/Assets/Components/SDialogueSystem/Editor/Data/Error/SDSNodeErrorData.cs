using SDS.Elements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDS.Data.Error
{
    public class SDSNodeErrorData
    {
        public SDSErrorData ErrorData { get; set; }
        public List<SDSNode> Nodes { get; set; }

        public SDSNodeErrorData()
        {
            this.ErrorData = new SDSErrorData();
            this.Nodes = new List<SDSNode>();
        }
    }
}