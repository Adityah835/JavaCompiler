using System;
using System.Collections.Generic;
using Symbol = JavaCompiler.LexAnalyzer.Symbol;

namespace JavaCompiler
{

    public enum VariableType { floatType, intType, booleanType, voidType }

    public enum EntryType { constEntry, variableEntry, functionEntry, classEntry }

    //VARIABLE
    public class VarEntry : TableEntry
    {
        public VariableType Type;
        public int Offset;
        public int Size;
    }

    //CONSTANT
    public class ConstEntry : TableEntry
    {
        public VariableType TypeOfConstant;
        public int Size;
        public int Offset;
        public int Value;
        public float ValueR;
    }

    //METHOD          
    public class FuncEntry : TableEntry
    {
        public int SizeOfLocalVar;
        public int SizeOfParameters;
        public int NumberOfParameters;
        public VariableType ReturnType;
    }

    //CLASS
    public class ClassEntry : TableEntry
    {
        public int SizeOfLocalVar;
        public LinkedList<string> FunctionNames;
        public LinkedList<string> VariablesNames;
    }

    //TableEntry
    public class TableEntry
    {
        public Symbol Token;
        public string Lexeme;
        public int Depth;
        public EntryType TypeOfEntry;

    }

    //Symbol Table for the lexemes
    public class SymbolTable
    {
        const int TableSize = 211;
        
        public LinkedList<TableEntry>[] tableEntries;
        
        public SymbolTable()
        {
            tableEntries = null;
            tableEntries = new LinkedList<TableEntry>[TableSize];
        }

        public void Insert(string Lexeme, Symbol Token, int Depth, EntryType TypeOfEntry)
        {
            switch (TypeOfEntry)
            {
                case EntryType.classEntry: InsertClassEntry(Lexeme, Token, Depth); break;
                case EntryType.constEntry: InsertConstEntry(Lexeme, Token, Depth); break;
                case EntryType.functionEntry: InsertFuncEntry(Lexeme, Token, Depth); break;
                case EntryType.variableEntry: InsertVarEntry(Lexeme, Token, Depth);  break;
                default: Console.WriteLine("Error! Cannot insert into the symbol table"); break;
            }
        }

        //Lookup
        public T Lookup<T>(string Lexeme)
        {
            var position = Hash(Lexeme);
            var tableEntry = tableEntries[position]?.First;

            while (tableEntry != null)
            {
                if (tableEntry.Value.Lexeme == Lexeme)
                {
                    return (T)Convert.ChangeType(tableEntry.Value, typeof(T));
                }

                tableEntry = tableEntry.Next;
            }

            return (T)Convert.ChangeType(tableEntry.Value, typeof(TableEntry));
        }

        public void DeleteDepth(int Depth)
        {
            for (int i = 0; i < TableSize; i++)
            {
                var tableEntry = tableEntries[i]?.First;
                while (tableEntry != null)
                {
                    if (tableEntry.Value.Depth.Equals(Depth))
                    {
                        tableEntries[i].Remove(tableEntry);
                    }
                    tableEntry = tableEntry.Next;
                }
            }
        }

        public void WriteTable(int Depth)
        {
            for (int i = 0; i < TableSize; i++)
            {
                var tableEntry = tableEntries[i]?.First;
                while (tableEntry != null)
                {
                    if (tableEntry.Value.Depth.Equals(Depth))
                    {
                        Console.WriteLine("Lexeme \'" + tableEntry.Value.Lexeme + "\' at Depth " + tableEntry.Value.Depth + " is of type \'" + tableEntry.Value.TypeOfEntry +"\'");
                        Console.WriteLine("");
                    }
                    tableEntry = tableEntry.Next;
                }
            }
        }

        private int Hash(string Lexeme)
        {
            //From the provided notes
            uint h = 0, g;
            for (int i = 0; i < Lexeme.Length; i++)
            {
                char c = Lexeme[i];
                h = (h << 24) + c;
                g = h & 0xF0000000;
                if (g != 0)
                {
                    h = h ^ (g >> 24);
                    h = h ^ g;
                }
            }
            return (int)h % TableSize;

        }
        private void InsertClassEntry(string Lexeme, Symbol Token, int Depth)
        {
            var classEntry = new ClassEntry()
            {
                Lexeme = Lexeme,
                Token = Token,
                Depth = Depth,
                TypeOfEntry = EntryType.classEntry,
                SizeOfLocalVar = 0,
                FunctionNames = new LinkedList<string>(),
                VariablesNames = new LinkedList<string>()
            };

            var position = Hash(Lexeme);

            if (tableEntries[position] == null)
            {
                tableEntries[position] = new LinkedList<TableEntry>();

                tableEntries[position].AddFirst(classEntry);
                
            }
            else if (tableEntries[position].First.Value.Lexeme == Lexeme && tableEntries[position].First.Value.Depth == Depth)
            {

                Console.Write("Error at Line " + LexAnalyzer.LineNo + " : \"" + Lexeme + "\" is already defined. Press any key to exit....");
                Console.ReadKey();
                Environment.Exit(-1);
            }
            else
            {
                tableEntries[position].AddFirst(classEntry);
            }

        }

        private void InsertConstEntry(string Lexeme, Symbol Token, int Depth)
        {
    
            var constEntry = new ConstEntry()
            {
                Lexeme = Lexeme,
                Token = Token,
                Depth = Depth,
                TypeOfEntry = EntryType.constEntry,
                 
            };

            var position = Hash(Lexeme);

            if (tableEntries[position] == null)
            {
                tableEntries[position] = new LinkedList<TableEntry>();

                tableEntries[position].AddFirst(constEntry);

            }
            else if (tableEntries[position].First.Value.Lexeme == Lexeme && tableEntries[position].First.Value.Depth == Depth)
            {

                Console.Write("Error at Line " + LexAnalyzer.LineNo + " : \"" + Lexeme + "\" is already defined. Press any key to exit....");
                Console.ReadKey();
                Environment.Exit(-1);
            }
            else
            {
                tableEntries[position].AddFirst(constEntry);
            }

        }

        private void InsertFuncEntry(string Lexeme, Symbol Token, int Depth)
        {
            var funcEntry = new FuncEntry()
            {
                Lexeme = Lexeme,
                Token = Token,
                Depth = Depth,
                TypeOfEntry = EntryType.functionEntry,

            };

            var position = Hash(Lexeme);

            if (tableEntries[position] == null)
            {
                tableEntries[position] = new LinkedList<TableEntry>();

                tableEntries[position].AddFirst(funcEntry);

            }
            else if (tableEntries[position].First.Value.Lexeme == Lexeme && tableEntries[position].First.Value.Depth == Depth)
            {

                Console.Write("Error at Line " + LexAnalyzer.LineNo + " : \"" + Lexeme + "\" is already defined. Press any key to exit....");
                Console.ReadKey();
                Environment.Exit(-1);
            }
            else
            {
                tableEntries[position].AddFirst(funcEntry);
            }

        }

        private void InsertVarEntry(string Lexeme, Symbol Token, int Depth)
        {
            var varEntry = new VarEntry()
            {
                Lexeme = Lexeme,
                Token = Token,
                Depth = Depth,
                TypeOfEntry = EntryType.variableEntry,

            };

            var position = Hash(Lexeme);

            if (tableEntries[position] == null)
            {
                tableEntries[position] = new LinkedList<TableEntry>();

                tableEntries[position].AddFirst(varEntry);

            }
            else if (tableEntries[position].First.Value.Lexeme == Lexeme && tableEntries[position].First.Value.Depth == Depth)
            {

                Console.Write("Error at Line " + LexAnalyzer.LineNo + " : \"" + Lexeme + "\" is already defined. Press any key to exit....");
                Console.ReadKey();
                Environment.Exit(-1);
            }
            else
            {
                tableEntries[position].AddFirst(varEntry);
            }

        }

    }
}
