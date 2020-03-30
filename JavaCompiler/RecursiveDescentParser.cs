using System;
using Symbol = JavaCompiler.LexAnalyzer.Symbol;
using myLex = JavaCompiler.LexAnalyzer;
using VarType = JavaCompiler.VariableType;

namespace JavaCompiler
{
    public static class RecursiveDescentParser
    {
        /********************************************************************
        *** FUNCTION match                                                ***
        *********************************************************************
        *** DESCRIPTION : This function is reponsible to check if the     ***
        *** current token matches the desired token                       ***
        *** INPUT ARGS : Symbol (enum)                                    ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void match(Symbol desired)
        {
            if (myLex.Token == desired)
            {
                myLex.GetNextToken();

                while (myLex.IsComment() == true && myLex.Token != Symbol.eoft)
                {
                    myLex.GetNextToken();
                }
            }
            else
            {
                Console.Write("Parse Error at Line " + myLex.LineNo + ". Expected: " + desired.ToString() + ", Found: "); myLex.DisplayToken();
                Console.WriteLine("");
                Console.Write("Press any key to exit... ");
                Console.ReadKey();
                Environment.Exit(-1);
            }
        }

        /********************************************************************
        *** FUNCTION Expr                                                 ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for expressions**
        *** in the program                                                ***
        ***                                                               ***
        ***           Expr -> ε                                           ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void Expr()
        {

        }

        /********************************************************************
        *** FUNCTION SeqOfStatements                                      ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for statements***
        *** in the program                                                ***
        ***                                                               ***
        ***           SeqOfStatements -> ε                                ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void SeqOfStatements()
        {

        }

        /********************************************************************
        *** FUNCTION FormalRest                                           ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for remaining ***
        *** formal parameters in the program                              ***
        ***                                                               ***
        ***              FormalRest -> , Type idt FormalRest | ε          ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void FormalRest()
        {
            if (myLex.Token == Symbol.commat)
            {
                match(Symbol.commat);
                Type();

                SymTable.Insert(myLex.Lexeme, Symbol.idt, currentDepth, EntryType.variableEntry);
                SymTable.Lookup<VarEntry>(myLex.Lexeme).Offset = currentOffset;
                SymTable.Lookup<VarEntry>(myLex.Lexeme).Type = VarType;
                UpdateCurrentOffset();
                UpdateVarSize();

                SymTable.Lookup<FuncEntry>(currentMethodName).NumberOfParameters += 1;
                SymTable.Lookup<FuncEntry>(currentMethodName).SizeOfParameters += SymTable.Lookup<VarEntry>(myLex.Lexeme).Size;

                match(Symbol.idt);
                FormalRest();
            }

        }

        /********************************************************************
        *** FUNCTION FormalList                                           ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for all the   ***
        *** formal parameters of a function in the program                ***
        ***                                                               ***
        ***            FormalList -> Type idt FormalRest | ε              ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void FormalList()
        {
            if(myLex.Token != Symbol.rparent)
            {
                Type();
                
                SymTable.Insert(myLex.Lexeme, Symbol.idt, currentDepth, EntryType.variableEntry);
                SymTable.Lookup<VarEntry>(myLex.Lexeme).Offset = currentOffset;
                SymTable.Lookup<VarEntry>(myLex.Lexeme).Type = VarType;
                UpdateCurrentOffset();
                UpdateVarSize();

                SymTable.Lookup<FuncEntry>(currentMethodName).NumberOfParameters += 1;
                SymTable.Lookup<FuncEntry>(currentMethodName).SizeOfParameters += SymTable.Lookup<VarEntry>(myLex.Lexeme).Size;

                match(Symbol.idt);
                FormalRest();
            }
            
            
        }

        /********************************************************************
        *** FUNCTION MethodDecl                                           ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for a method  ***
        *** declaration in the program                                    ***
        ***                                                               ***
        ***  MethodDecl -> public Type idt (FormalList)                   ***
        ***            { VarDecl SeqOfStats returnt Expr; } MethodDecl| ε ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void MethodDecl()
        {
            if (myLex.Token == Symbol.publict)
            {
                match(Symbol.publict);
                Type();

                SymTable.Insert(myLex.Lexeme, Symbol.idt, currentDepth, EntryType.functionEntry);
                SymTable.Lookup<FuncEntry>(myLex.Lexeme).ReturnType = VarType;

                SymTable.Lookup<ClassEntry>(currentClassName).FunctionNames.AddFirst(myLex.Lexeme);

                currentMethodName = myLex.Lexeme;

                match(Symbol.idt);
                match(Symbol.lparent);

                currentDepth += 1;
                currentOffset = 0;
                
                FormalList();
                match(Symbol.rparent);
                match(Symbol.lcurlyt);
                VarDecl();
                SeqOfStatements();
                match(Symbol.returnt);
                Expr();
                match(Symbol.semicolont);
                match(Symbol.rcurlyt);

                SymTable.WriteTable(currentDepth);
                SymTable.DeleteDepth(currentDepth);
                currentDepth -= 1;

                MethodDecl();
            }

        }

        /********************************************************************
        *** FUNCTION Type                                                 ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for a data types*
        *** allowed in the program                                        ***
        ***                                                               ***
        ***               Type -> intt | booleant | voidt                 ***  
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void Type()
        {
            switch (myLex.Token)
            {
                case Symbol.intt: match(Symbol.intt); VarType = VarType.intType; break;
                case Symbol.booleant: match(Symbol.booleant); VarType = VarType.booleanType; break;
                case Symbol.voidt: match(Symbol.voidt); VarType = VarType.voidType;  break;
                case Symbol.floatt: match(Symbol.floatt); VarType = VarType.floatType; break;
                
                default: Console.Write("Parse Error at Line No " + myLex.LineNo + ". Expected intt, booleant or voidt, Found: "); myLex.DisplayToken();
                         Console.WriteLine("");
                         Console.Write("Press any key to exit... ");
                         Console.ReadKey();
                         Environment.Exit(-1);
                         break;

            }
        }

        /********************************************************************
        *** FUNCTION IdentifierList                                       ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for the       ***
        *** identifier list of a data type declared in a class in the     ***
        *** program                                                       ***
        ***                                                               ***
        ***     IdentifierList -> idt| IdentifierList, idt                ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void IdentifierList()
        {
            SymTable.Insert(myLex.Lexeme, Symbol.idt, currentDepth, EntryType.variableEntry);
            SymTable.Lookup<VarEntry>(myLex.Lexeme).Offset = currentOffset;
            SymTable.Lookup<VarEntry>(myLex.Lexeme).Type = VarType;
            UpdateCurrentOffset();
            UpdateVarSize();

            if (currentMethodName == string.Empty)
            {
                SymTable.Lookup<ClassEntry>(currentClassName).VariablesNames.AddFirst(myLex.Lexeme);
                SymTable.Lookup<ClassEntry>(currentClassName).SizeOfLocalVar += SymTable.Lookup<VarEntry>(myLex.Lexeme).Size;
            }
            else
            {
                SymTable.Lookup<FuncEntry>(currentMethodName).SizeOfLocalVar += SymTable.Lookup<VarEntry>(myLex.Lexeme).Size;
            }

            match(Symbol.idt);

            if(myLex.Token == Symbol.commat)
            {
                match(Symbol.commat);
                IdentifierList();
            }
        }

        /********************************************************************
        *** FUNCTION VarDecl                                              ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for a variable***
        *** declaration in the program                                    ***
        ***                                                               ***
        ***        VarDecl -> Type IdentifierList; VarDecl|               ***
        ***                   final Type idt = numt; VarDecl| ε           ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void VarDecl()
        {
            if (myLex.Token == Symbol.finalt)
            {
                match(Symbol.finalt);
                Type();

                var varName = myLex.Lexeme;
                SymTable.Insert(myLex.Lexeme, Symbol.idt, currentDepth, EntryType.constEntry);
                SymTable.Lookup<ConstEntry>(myLex.Lexeme).TypeOfConstant = VarType;
                UpdateCurrentOffset();
                UpdateConstSize();


                if(currentMethodName == string.Empty)
                {
                    SymTable.Lookup<ClassEntry>(currentClassName).VariablesNames.AddFirst(myLex.Lexeme);
                    SymTable.Lookup<ClassEntry>(currentClassName).SizeOfLocalVar += SymTable.Lookup<ConstEntry>(myLex.Lexeme).Size;
                    
                }
                else
                {
                    SymTable.Lookup<FuncEntry>(currentMethodName).SizeOfLocalVar += SymTable.Lookup<ConstEntry>(myLex.Lexeme).Size;
                }

                match(Symbol.idt);
                match(Symbol.assignop);

                try
                {
                    if (SymTable.Lookup<ConstEntry>(varName).TypeOfConstant == VarType.intType)
                    {
                        SymTable.Lookup<ConstEntry>(varName).Value = int.Parse(myLex.Lexeme);
                    }

                    else if (SymTable.Lookup<ConstEntry>(varName).TypeOfConstant == VarType.floatType)
                    {
                        SymTable.Lookup<ConstEntry>(varName).ValueR = float.Parse(myLex.Lexeme);
                    }

                }
                catch(Exception)
                {
                    Console.WriteLine("Parse Error! Illegal data being assigned to the variable " + varName + "at Line "+ myLex.LineNo);
                    Console.ReadKey();
                    Environment.Exit(-1);
                }

                match(Symbol.numt);
                match(Symbol.semicolont);
                VarDecl();
            }
            else if(myLex.Token == Symbol.intt || myLex.Token == Symbol.booleant || myLex.Token == Symbol.floatt || myLex.Token == Symbol.voidt)
            {
                Type();
                if(myLex.Token != Symbol.idt)
                {
                    Console.Write("Parse Error at Line No "+ myLex.LineNo +": Expecting: idt, Found: ");
                    myLex.DisplayToken();
                    Console.Write("Press any key to exit... ");
                    Console.ReadKey();
                    Environment.Exit(-1);
                }
                IdentifierList();
                match(Symbol.semicolont);
                VarDecl();
            }
            else if (myLex.Token == Symbol.idt)
            {
                Console.Write("Parse Error at Line No " + myLex.LineNo + " Expecting: intt, booleant or voidt, Found:");
                myLex.DisplayToken();
                Console.ReadKey();
                Console.Write("Press any key to exit... ");
                Environment.Exit(-1);
            }

        }

        /********************************************************************
        *** FUNCTION ClassDecl                                            ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for a class   ***
        *** declaration in the program                                    ***
        ***                                                               ***
        ***      ClassDecl -> class idt { VarDecl MethodDecl } |          ***
        ***                   class idt extends idt {VarDecl MethodDecl}  ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void ClassDecl()
        {
            match(Symbol.classt);

            SymTable.Insert(myLex.Lexeme, Symbol.idt, currentDepth, EntryType.classEntry);
            currentClassName = myLex.Lexeme;
            currentMethodName = string.Empty;

            match(Symbol.idt);

            if (myLex.Token == Symbol.extendst)
            {
                match(Symbol.extendst);

                SymTable.Insert(myLex.Lexeme, Symbol.idt, currentDepth, EntryType.classEntry);

                match(Symbol.idt);
            }

            match(Symbol.lcurlyt);

            currentDepth += 1;

            currentOffset = 0;

            VarDecl();
            MethodDecl();
            match(Symbol.rcurlyt);

            SymTable.WriteTable(currentDepth);
            SymTable.DeleteDepth(currentDepth);

            currentDepth -= 1;

        }

        /********************************************************************
        *** FUNCTION MoreClasses                                          ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for all class ***
        *** declarations except the main class in the program             ***
        ***                                                               ***
        ***        MoreClasses -> ClassDecl MoreClasses | ε               ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void MoreClasses()
        {
            if (myLex.Token == Symbol.classt)
            {
                ClassDecl();
                currentClassName = "";
                currentMethodName = "";

                MoreClasses();
            }
            else if (myLex.Token == Symbol.finalt)
            {
                return;
            }
            else 
            { 
                Console.Write("Parse Error at Line No " + myLex.LineNo + ". Expected: classt, Found: "); myLex.DisplayToken(); ;
                Console.WriteLine("");
                Console.Write("Press any key to exit... ");
                Console.ReadKey();
                Environment.Exit(-1);
            }

            
        }

        /********************************************************************
        *** FUNCTION MainClass                                            ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for main class***
        *** declarations in the program                                   ***
        ***                                                               ***
        ***   MainClass -> finalt classt idt { publict statict voidt      ***
        ***                main ( String [] idt) { SeqOfStatements} }     ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void MainClass()
        {
            match(Symbol.finalt);
            match(Symbol.classt);

            SymTable.Insert(myLex.Lexeme, Symbol.idt, currentDepth, EntryType.classEntry);
            currentClassName = myLex.Lexeme;
            currentMethodName = string.Empty;

            match(Symbol.idt);
            match(Symbol.lcurlyt);

            currentDepth += 1;
            currentOffset = 0;

            match(Symbol.publict);
            match(Symbol.statict);
            match(Symbol.voidt);

            SymTable.Insert(myLex.Lexeme, Symbol.maint, currentDepth, EntryType.functionEntry);
            SymTable.Lookup<FuncEntry>(myLex.Lexeme).ReturnType = VarType;
            currentMethodName = myLex.Lexeme;

            match(Symbol.maint);
            match(Symbol.lparent);
            match(Symbol.Stringt);
            match(Symbol.lsqrbract);
            match(Symbol.rsqrbract);
            match(Symbol.idt);
            match(Symbol.rparent);
            match(Symbol.lcurlyt);

            currentDepth += 1;
            currentOffset = 0;

            SeqOfStatements();
            match(Symbol.rcurlyt);

            SymTable.WriteTable(currentDepth);
            SymTable.DeleteDepth(currentDepth);

            currentDepth -= 1;

            match(Symbol.rcurlyt);

            SymTable.WriteTable(currentDepth);
            SymTable.DeleteDepth(currentDepth);
            
            currentDepth -= 1;

        }
        private static void UpdateCurrentOffset()
        {
            switch (VarType)
            {
                case VarType.booleanType: currentOffset += 1; break;
                case VarType.floatType: currentOffset += 4; break;
                case VarType.intType: currentOffset += 2; break;
                case VarType.voidType: currentOffset += 0; break;
            }
        }
        private static void UpdateVarSize()
        {
            switch (VarType)
            {
                case VarType.booleanType: SymTable.Lookup<VarEntry>(myLex.Lexeme).Size = 1; break;
                case VarType.floatType: SymTable.Lookup<VarEntry>(myLex.Lexeme).Size = 4; break;
                case VarType.intType: SymTable.Lookup<VarEntry>(myLex.Lexeme).Size = 2; break;
                case VarType.voidType: SymTable.Lookup<VarEntry>(myLex.Lexeme).Size = 0; break;
            }
        }

        private static void UpdateConstSize()
        {
            switch (VarType)
            {
                case VarType.booleanType: SymTable.Lookup<ConstEntry>(myLex.Lexeme).Size = 1; break;
                case VarType.floatType: SymTable.Lookup<ConstEntry>(myLex.Lexeme).Size = 4; break;
                case VarType.intType: SymTable.Lookup<ConstEntry>(myLex.Lexeme).Size = 2; break;
                case VarType.voidType: SymTable.Lookup<ConstEntry>(myLex.Lexeme).Size = 0; break;
            }
        }

        private static SymbolTable SymTable;
        private static int currentDepth;
        private static int currentOffset;
        private static string currentMethodName;
        private static string currentClassName;
        private static VarType VarType;

        /********************************************************************
        *** FUNCTION BeginProgramParser                                   ***
        *********************************************************************
        *** DESCRIPTION : This function starts the parsing process by     ***
        *** calling MoreClasses and MainClass parts of the program        ***
        ***                                                               ***
        ***               Prog -> MoreClasses MainClass                   ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        public static void BeginProgramParser()
        {
            SymTable = new SymbolTable();
            currentDepth = 0;
            currentOffset = 0;

            myLex.GetNextToken();

            while (myLex.IsComment() == true && myLex.Token != Symbol.eoft)
            {
                myLex.GetNextToken(); 
            }
            
            MoreClasses();
            MainClass();

            SymTable.WriteTable(currentDepth);
            SymTable.DeleteDepth(currentDepth);
                        
            if(myLex.Token != Symbol.eoft)
            {
                Console.WriteLine("Error - Unused tokens");
                Console.WriteLine("");
                Console.Write("Press any key to exit... ");
                Console.ReadKey();
                Environment.Exit(-1);
            }
        }


    }
}