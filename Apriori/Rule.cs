using System.Collections.Generic;

namespace Apriori
{
    class Rule
    {
        public List<byte> leftSide;
        public List<byte> rightSide;
        public double support;
        public double confidence;

        public Rule(List<byte> leftSide, List<byte> rightSide, double support, double confidence) 
        {
            this.leftSide = leftSide;
            this.rightSide = rightSide;
            this.support = support;
            this.confidence = confidence;
        }
    }
}
