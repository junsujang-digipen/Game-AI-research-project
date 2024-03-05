using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WFC{

    public class CellCandidate{
        public int cellType;
        public int Offset;
        public CellCandidate(int type, int offset = 0){
            this.cellType = type;
            this.Offset = offset;
        }
    }
    public class CellInformation
    {
        public List<CellCandidate> candidates  = new List<CellCandidate>();
        public bool isChoosen = false;

    }

}