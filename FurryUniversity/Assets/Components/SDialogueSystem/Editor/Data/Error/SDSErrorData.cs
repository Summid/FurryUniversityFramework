using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDS.Data.Error
{
    public class SDSErrorData
    {
        public Color Color { get; set; }

        public SDSErrorData()
        {
            this.GenerateRandomColor();
        }

        private void GenerateRandomColor()
        {
            this.Color = new Color32(
                (byte)Random.Range(65, 256),
                (byte)Random.Range(50, 176),
                (byte)Random.Range(50, 176),
                255
                );
        }
    }
}