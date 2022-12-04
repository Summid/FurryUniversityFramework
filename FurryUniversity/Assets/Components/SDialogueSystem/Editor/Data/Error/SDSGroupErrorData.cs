using SDS.Elements;
using System.Collections.Generic;

namespace SDS.Data.Error
{
    public class SDSGroupErrorData
    {
        public SDSErrorData ErrorData { get; set; }
        public List<SDSGroup> Groups { get; set; }
        
        public SDSGroupErrorData()
        {
            this.ErrorData = new SDSErrorData();
            this.Groups = new List<SDSGroup>();
        }
    }
}