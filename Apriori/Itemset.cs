
namespace Apriori
{
    class Itemset
    {
        public byte[] items;
        public int support = 0;

        public Itemset(byte item)
        {
            items = new byte[] { item };
            support = 1;
        }

        public Itemset(byte[] itemset1, byte[] itemset2, int length) 
        {
            items = new byte[length+1];
            for (int i = 0; i < length-1; i++)
            {
                items[i] = itemset1[i];
            }

            if (itemset1[length-1]<itemset2[length-1])
            {
                items[length - 1] = itemset1[length - 1];
                items[length] = itemset2[length - 1];
            }
            else
            {
                items[length - 1] = itemset2[length - 1];
                items[length] = itemset1[length - 1];
            }
        }
    }
}
