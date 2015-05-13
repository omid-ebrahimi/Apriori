using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Apriori
{
    class Program
    {
        #region Define Variables
        const string Database = "Database.txt";
        const string SortedDatabase = "SortedDatabase.txt";
        static TextReader dbReader;
        static TextWriter dbWriter;
        static double minSupport = 0;
        static double minConfidence = 0;
        static string transaction;
        static int numberOfTransactions = 0;
        static Itemset candidate;
        static Hashtable oneItemsets = new Hashtable();
        static List<Itemset> candidates;
        static List<Itemset> frequentItemsets;
        static List<Itemset> allFrequentItemsets = new List<Itemset>();
        static List<Rule> rules = new List<Rule>();
        static string[] stringItemset;
        static List<byte> byteTransaction = new List<byte>();
        static byte byteItem;
        static byte counter;
        #endregion

        static void Main(string[] args)
        {
            #region Initialization
            Console.WriteLine("Please enter relative SUPPORT in percent (ex: 33 => 33%):");
            minSupport = double.Parse(Console.ReadLine()) / 100;
            Console.WriteLine("Please enter CONFIDENCE:");
            minConfidence = double.Parse(Console.ReadLine()) / 100;
            Console.WriteLine("Apriori algorithm started...");
            Console.WriteLine("Scan DB...");
            Console.WriteLine("Make C1...");
            dbReader = new StreamReader(Database);
            dbWriter = new StreamWriter(SortedDatabase);

            transaction = dbReader.ReadLine();
            while (transaction != null)
            {
                stringItemset = transaction.Split();
                extractOneItemsetsAndGetByteTransaction(stringItemset);
                byteTransaction.Sort();
                transaction = "";
                foreach (byte byteItem in byteTransaction)
                    transaction += byteItem + " ";
                transaction = transaction.TrimEnd();
                dbWriter.WriteLine(transaction);
                numberOfTransactions++;
                transaction = dbReader.ReadLine();
            }
            dbReader.Close();
            dbWriter.Close();

            candidates = new List<Itemset>();
            foreach (DictionaryEntry entry in oneItemsets)
            {
                candidates.Add((Itemset)entry.Value);
            }
            oneItemsets.Clear();

            minSupport = Math.Ceiling(minSupport * numberOfTransactions);
            #endregion

            Apriori();

            #region Build Association Rules
            Console.WriteLine("Build Association Rules...\n");
            foreach (Itemset itemset in allFrequentItemsets)
            {
                if (itemset.items.Length > 1)
                {
                    findAssociationRules(itemset.items, itemset.support);
                }
            } 
            #endregion

            rules.Reverse();

            #region Sort Rules by Confidence (Insertion Sort)
            Rule x;
            int j;
            for (int i = 1; i < rules.Count; i++)
            {
                x = rules[i];
                j = i - 1;
                while (j >= 0 && rules[j].confidence < x.confidence && !((rules[j].leftSide.Count + rules[j].rightSide.Count) > (x.leftSide.Count + x.rightSide.Count)))
                {
                    rules[j + 1] = rules[j];
                    j = j - 1;
                }
                rules[j + 1] = x;
            }
            #endregion

            #region Sort Rules by Support (Insertion Sort)
            for (int i = 1; i < rules.Count; i++)
            {
                x = rules[i];
                j = i - 1;
                while (j >= 0 && rules[j].support < x.support && !(rules[j].confidence > x.confidence) && !((rules[j].leftSide.Count + rules[j].rightSide.Count) > (x.leftSide.Count + x.rightSide.Count)))
                {
                    rules[j + 1] = rules[j];
                    j = j - 1;
                }
                rules[j + 1] = x;
            } 
            #endregion

            #region Show Results
            string result;
            foreach (Rule rule in rules)
            {
                result = "";
                foreach (byte item in rule.leftSide)
                {
                    result += item + " ";
                }
                result += "=> ";
                foreach (byte item in rule.rightSide)
                {
                    result += item + " ";
                }
                result += "  confidence: " + rule.confidence + "  support: " + rule.support;
                Console.WriteLine(result);
            }
            Console.WriteLine("\nThe End"); 
            #endregion

            Console.ReadLine();
        }

        private static void extractOneItemsetsAndGetByteTransaction(string[] transaction)
        {
            Itemset oneItemset;
            byteTransaction.Clear();
            foreach (string item in transaction)
            {
                byteItem = byte.Parse(item);
                byteTransaction.Add(byteItem);
                if (oneItemsets[item] != null)
                {
                    oneItemset = (Itemset)oneItemsets[item];
                    oneItemset.support++;
                }
                else
                {
                    oneItemsets[item] = new Itemset(byteItem);
                }
            }
        }

        private static void Apriori()
        {
            while (candidates.Count != 0)
            {
                #region Make L
                Console.WriteLine("Make L" + candidates[0].items.Length + "...");
                frequentItemsets = new List<Itemset>();
                foreach (Itemset candidate in candidates)
                {
                    if (candidate.support >= minSupport)
                    {
                        frequentItemsets.Add(candidate);
                    }
                }
                allFrequentItemsets.AddRange(frequentItemsets);
                #endregion

                #region Generate new candidates
                Console.WriteLine("Make C" + (candidates[0].items.Length + 1) + "...");
                bool joinable;
                candidates = new List<Itemset>();
                for (int i = 0; i < frequentItemsets.Count; i++)
                {
                    for (int j = i + 1; j < frequentItemsets.Count; j++)
                    {
                        joinable = true;
                        for (int k = 0; k < frequentItemsets[0].items.Length - 1; k++)
                        {
                            if (frequentItemsets[i].items[k] != frequentItemsets[j].items[k])
                            {
                                joinable = false;
                                break;
                            }
                        }
                        if (joinable)
                        {
                            candidate = new Itemset(frequentItemsets[i].items, frequentItemsets[j].items, frequentItemsets[0].items.Length);
                            candidates.Add(candidate);
                        }
                    }
                }
                #endregion

                #region Scan DB for obtain support of each candidate
                Console.WriteLine("Scan SortedDB...");
                dbReader = new StreamReader(SortedDatabase);
                transaction = dbReader.ReadLine();
                while (transaction != null)
                {
                    stringItemset = transaction.Split();
                    byteTransaction.Clear();
                    foreach (string stringItem in stringItemset)
                        byteTransaction.Add(byte.Parse(stringItem));
                    foreach (Itemset candidate in candidates)
                    {
                        counter = 0;
                        for (int j = 0; j < byteTransaction.Count; j++)
                        {
                            if (candidate.items[counter] == byteTransaction[j])
                            {
                                counter++;
                                if (counter == candidate.items.Length)
                                {
                                    candidate.support += 1;
                                    break;
                                }
                            }
                        }
                    }
                    transaction = dbReader.ReadLine();
                }
                dbReader.Close();
                #endregion
            }
            Console.WriteLine("End of Apriori Algorithm\n");
        }

        private static void findAssociationRules(byte[] set, int support)
        {
            #region Definitions
            char[] binary;
            double confidence;
            List<byte> subset;
            List<byte> complementSubset;
            #endregion

            for (int i = 1; i < Math.Pow(2, set.Length) - 1; i++)
            {
                confidence = 0;
                subset = new List<byte>();
                complementSubset = new List<byte>();

                binary = Convert.ToString(i, 2).ToCharArray();

                for (int j = 0; j < set.Length - binary.Length; j++)
                {
                    complementSubset.Add(set[j]);
                }

                for (int j = 0; j < binary.Length; j++)
                {
                    if (binary[j] == '1')
                    {
                        subset.Add(set[set.Length - binary.Length + j]);
                    }
                    else
                    {
                        complementSubset.Add(set[set.Length - binary.Length + j]);
                    }
                }

                #region Compute Confidence
                bool equal = true;
                for (int j = 0; j < allFrequentItemsets.Count; j++)
                {
                    if (allFrequentItemsets[j].items.Length == subset.Count)
                    {
                        for (int k = 0; k < allFrequentItemsets[j].items.Length; k++)
                        {
                            if (allFrequentItemsets[j].items[k] != subset[k])
                            {
                                equal = false;
                                break;
                            }
                        }
                        if (equal)
                        {
                            confidence = support / (double)allFrequentItemsets[j].support;
                        }
                        else
                        {
                            equal = true;
                        }
                    }
                } 
                #endregion
                
                if (confidence >= minConfidence)
                {
                    rules.Add(new Rule(subset, complementSubset, support / (double)numberOfTransactions, confidence));
                }
            }
        }
    }
}
