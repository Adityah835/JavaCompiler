/********************************************************************
*** NAME : ADITYA HARSHVARDHAN                                    ***
*** CLASS : CSC 446                                               ***
*** ASSIGNMENT : Recursive Descent Parser Actions                 ***
*** DUE DATE : 3/30/2020                                          ***
*** INSTRUCTOR : Dr. Hamer                                        ***
*********************************************************************
*** DESCRIPTION : This program reads from a java source code file ***
*** determines the appropriate token types, stores the values in  ***
*** appropriate global variable and parses the source files based ***
*** on defined grammar rules and stores the lexemes in a symbol   ***
*** table.                                                        ***
********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;

namespace JavaCompiler
{

    class Program
    {
        /********************************************************************
        *** FUNCTION Main                                                 ***
        *********************************************************************
        *** DESCRIPTION : This is the main function that starts by calling***
        *** the BeginProgramParser                                        ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : string[]                                        ***
        *** RETURN : int                                                  ***
        ********************************************************************/
        static int Main(string[] args)
        {

            Console.WriteLine("Assignment 5 - Java Recursive Descent Parser Actions");
            Console.WriteLine("Name - Aditya Harshvardhan");
            Console.WriteLine("Due date - 3/30/2020");
            Console.WriteLine("");
            Console.Write("Press any key to begin... ");
            Console.ReadKey();
            Console.Clear();

            if (args.Length != 1)
            {
                Console.Write("Usage : ", args[0]);
                Console.WriteLine("filename");
                return 1;
            }
            else
            {
                LexAnalyzer.stream = new StreamReader(args[0]);
                LexAnalyzer.Token = LexAnalyzer.Symbol.unknownt;
                LexAnalyzer.LineNo = 1;

                RecursiveDescentParser.BeginProgramParser();

            }
            Console.WriteLine();
            Console.Write("Parsing complete. Press any key to exit... ");
            Console.ReadKey();
            return 0;

        }
    }
}