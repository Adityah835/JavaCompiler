using System;
using System.IO;
using Symbol = JavaCompiler.LexAnalyzer.Symbol;
using myLex = JavaCompiler.LexAnalyzer;
using VarType = JavaCompiler.VariableType;
using System.Collections.Generic;

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
        *** FUNCTION SignOp                                               ***
        *********************************************************************
        *** DESCRIPTION : This function is grammar rule for a sign of a   ***
        *** factor of a term in the program                               ***
        ***                                                               ***
        ***               SignOp -> -                                     ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void SignOp()
        {
            if (myLex.Lexeme == "-")
            {
                match(Symbol.addop);
            }
            else
            {
                Console.WriteLine("Parse Error at Line " + myLex.LineNo + "Expected: \'-\', Found: "+ myLex.Lexeme);
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        /********************************************************************
        *** FUNCTION Mulop                                                ***
        *********************************************************************
        *** DESCRIPTION : This function is grammar rule for multiplication***
        *** operator present in a term in the program                     ***
        ***                                                               ***
        ***               Mulop -> * | / | &&                             ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static string Mulop()
        {
            var mulop = myLex.Lexeme;
            match(Symbol.mulop);
            return mulop;
        }

        /********************************************************************
        *** FUNCTION Addop                                                ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for addition  ***
        *** operators present in a term in the program                    ***
        ***                                                               ***
        ***          Addop -> + | - | ||                                  ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static string Addop()
        {
            var addop = myLex.Lexeme;
            match(Symbol.addop);
            return addop;
        }

        /********************************************************************
        *** FUNCTION Factor                                               ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for one factor***
        *** of a term in the program                                      ***
        ***                                                               ***
        ***          Factor -> idt | numt | ( Expr ) | ! Factor |         ***
        ***                    SignOp Factor | true | false               ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        //private static void Factor()
        private static TableEntry Factor(TableEntry Factor_Ptr)
        {
            TableEntry TempEntr;

            if(myLex.Token == Symbol.idt)
            {
                CheckIfDeclaredVariable(myLex.Lexeme);
                //CheckIfDeclaredVariable(Factor_Ptr.Lexeme);
                Factor_Ptr = SymTable.Lookup(myLex.Lexeme);
                match(Symbol.idt);
            }
            else if (myLex.Token == Symbol.numt)
            {
                Factor_Ptr = NewTempTableEntry();
                EmitCode(FormatThreeAddrCode(ConvertEntryToBasePointer(Factor_Ptr), "=", myLex.Lexeme));
                match(Symbol.numt);
            }
            else if (myLex.Token == Symbol.lparent)
            {
                match(Symbol.lparent);
                Factor_Ptr = Expr();
                match(Symbol.rparent);
            }
            else if (myLex.Token == Symbol.notop)
            {
                TempEntr = NewTempTableEntry();
                match(Symbol.notop);
                //Factor();
                Factor_Ptr = Factor(Factor_Ptr);
                EmitCode(FormatThreeAddrCode(ConvertEntryToBasePointer(TempEntr), "=", "!" + ConvertEntryToBasePointer(Factor_Ptr)));
            }
            else if (myLex.Token == Symbol.addop)
            {
                TempEntr = NewTempTableEntry();
                SignOp();
                //Factor();
                Factor_Ptr = Factor(Factor_Ptr);
                EmitCode(FormatThreeAddrCode(ConvertEntryToBasePointer(TempEntr), "=", "-1", "*",  ConvertEntryToBasePointer(Factor_Ptr)));
            }
            else if (myLex.Token == Symbol.truet)
            {
                match(Symbol.truet);
            }
            else if (myLex.Token == Symbol.falset)
            {
                match(Symbol.falset);
            }
            else
            {
                Console.WriteLine("Parse Error at Line " + myLex.LineNo + " : Invalid expression");
                Console.WriteLine("");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(-1);
            }
            return Factor_Ptr;

        }

        /********************************************************************
        *** FUNCTION MoreFactor                                           ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for a more    ***
        *** factors of a term in the program                              ***
        ***                                                               ***
        ***          MoreFactor -> Mulop Factor MoreFactor | ε            ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static TableEntry MoreFactor(TableEntry MoreFactor_Ptr)
        {
            if (myLex.Token == Symbol.mulop)
            {
                var tempEntr = NewTempTableEntry();
                TableEntry Factor_Ptr = null;

                var Op = Mulop();

                Factor_Ptr = Factor(Factor_Ptr);
                MoreFactor(MoreFactor_Ptr);
                EmitCode(FormatThreeAddrCode(ConvertEntryToBasePointer(tempEntr), "=", ConvertEntryToBasePointer(MoreFactor_Ptr), Op, ConvertEntryToBasePointer(Factor_Ptr)));

                MoreFactor_Ptr = tempEntr;
            }
            return MoreFactor_Ptr;
        }

        /********************************************************************
        *** FUNCTION Term                                                 ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for a single  ***
        *** term in the program                                           ***
        ***                                                               ***
        ***          Term -> Factor MoreFactor                            ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static TableEntry Term()
        {
            TableEntry Factor_Ptr = null;


            Factor_Ptr = Factor(Factor_Ptr);
            Factor_Ptr = MoreFactor(Factor_Ptr);

            return Factor_Ptr;
            
        }

        /********************************************************************
        *** FUNCTION MoreTerm                                             ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for a more    ***
        *** terms in the program                                          ***
        ***                                                               ***
        ***         MoreTerm -> Addop Term MoreTerm |  ε                  ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static TableEntry MoreTerm(TableEntry MoreTerm_Ptr)
        {
            if (myLex.Token == Symbol.addop)
            {
                var temp_ptr = NewTempTableEntry();
                TableEntry Term_Ptr;

                var Op = Addop();
                Term_Ptr = Term();

                EmitCode(FormatThreeAddrCode(ConvertEntryToBasePointer(temp_ptr), "=",ConvertEntryToBasePointer(MoreTerm_Ptr), Op, ConvertEntryToBasePointer(Term_Ptr)));

                MoreTerm_Ptr = temp_ptr;

                return MoreTerm(MoreTerm_Ptr);
            }
            return MoreTerm_Ptr;
        }

        /********************************************************************
        *** FUNCTION SimpleExpr                                           ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for simple    ***
        *** expression in the program                                     ***
        ***                                                               ***
        ***          SimpleExpr -> Term MoreTerm                          ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static TableEntry SimpleExpr()
        {
            TableEntry Term_Ptr = null;

            Term_Ptr = Term();
            Term_Ptr = MoreTerm(Term_Ptr);

            return Term_Ptr;
        }

        /********************************************************************
        *** FUNCTION Relation                                             ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for relation  ***
        *** that expends upon an expression in the program                ***
        ***                                                               ***
        ***           Relation -> SimpleExpr                              ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static TableEntry Relation()
        {
            TableEntry SimpleExpr_Ptr = null;
            SimpleExpr_Ptr = SimpleExpr();
            return SimpleExpr_Ptr;
        }
        

        /********************************************************************
        *** FUNCTION Expr                                                 ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for expressions**
        *** in the program                                                ***
        ***                                                               ***
        ***           Expr -> Relation | ε                                ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static TableEntry Expr()
        {
            TableEntry Relation_Ptr = null;
            switch (myLex.Token)
            {
                case Symbol.idt:
                case Symbol.numt:
                case Symbol.lparent:
                case Symbol.notop:
                case Symbol.addop:
                case Symbol.truet:
                case Symbol.falset: Relation_Ptr = Relation(); break;
                default: break;
            }
            return Relation_Ptr;
            
        }
    
        /********************************************************************
        *** FUNCTION IOStat                                               ***
        *********************************************************************
        *** DESCRIPTION : This function is grammar rule for i/o statement ***
        *** in the program                                                ***
        ***                                                               ***
        ***           IOStat -> ε                                         ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void IOStat()
        {

        }

        /********************************************************************
        *** FUNCTION ParamsTails                                          ***
        *********************************************************************
        *** DESCRIPTION : This function is grammar rule for method call   ***
        *** parameter tail in the program                                 ***
        ***                                                               ***
        ***     ParamsTail -> , idt ParamsTail | , num ParamsTail| ε      ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void ParamsTail()
        {
            if (myLex.Token == Symbol.commat)
            {
                match(Symbol.commat);

                TableEntry entry = SymTable.Lookup(myLex.Lexeme);

                if (myLex.Token == Symbol.idt)
                {
                    match(Symbol.idt);
                    ParamsTail();

                    EmitCode(FormatThreeAddrCode("push", ConvertEntryToBasePointer(entry)));
                }
                else if (myLex.Token == Symbol.numt)
                {
                    var numEntr = myLex.Lexeme;
                    match(Symbol.numt);
                    ParamsTail();

                    EmitCode(FormatThreeAddrCode("push", numEntr));
                }

                
            }

        }

        /********************************************************************
        *** FUNCTION Params                                               ***
        *********************************************************************
        *** DESCRIPTION : This function is grammar rule for method call   ***
        *** parameters in the program                                     ***
        ***                                                               ***
        ***        MethodCall -> idt ParamsTail| num ParamsTail| ε        ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void Params()
        {
            if(myLex.Token == Symbol.idt)
            {
                TableEntry entry = SymTable.Lookup(myLex.Lexeme);
                
                match(Symbol.idt);
                ParamsTail();

                EmitCode(FormatThreeAddrCode("push", ConvertEntryToBasePointer(entry)));
            }
            else if(myLex.Token == Symbol.numt)
            {
                var numEntr = myLex.Lexeme;

                match(Symbol.numt);
                ParamsTail();

                EmitCode(FormatThreeAddrCode("push", numEntr));
            }


        }

        /********************************************************************
        *** FUNCTION ClassName                                            ***
        *********************************************************************
        *** DESCRIPTION : This function is grammar rule for class name of ***
        *** a methodcall in the program                                   ***
        ***                                                               ***
        ***                      ClassName -> idt                         ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void ClassName()
        {
            match(Symbol.idt);
        }


        /********************************************************************
        *** FUNCTION MethodCall                                           ***
        *********************************************************************
        *** DESCRIPTION : This function is grammar rule for method call in***
        *** the program                                                   ***
        ***                                                               ***
        ***            MethodCall -> ClassName.idt ( Params )             ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void MethodCall()
        {
            ClassName();
            match(Symbol.periodt);

            currentCalledMethodName = myLex.Lexeme;

            match(Symbol.idt);
            match(Symbol.lparent);
            Params();
            match(Symbol.rparent);

            EmitCode(FormatThreeAddrCode("call", currentCalledMethodName));
            
        }


        /********************************************************************
        *** FUNCTION AssignStat                                           ***
        *********************************************************************
        *** DESCRIPTION : This function is grammar rule for assignment    ***
        *** statement in the program                                      ***
        ***                                                               ***
        ***    AssignStat -> idt = Expr | idt = MethodCall | MethodCall   ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void AssignStat()
        {
            var tabEntry = SymTable.Lookup(myLex.Lexeme);

            if (tabEntry != null)
            {
                if (tabEntry.TypeOfEntry == EntryType.classEntry)
                {
                    MethodCall();
                }
                else
                {
                    TableEntry Expr_Ptr = null;
                    CheckIfDeclaredVariable(myLex.Lexeme);
                    currentExprVar = myLex.Lexeme;                  
                    match(Symbol.idt);
                    match(Symbol.assignop);

                    if (myLex.Token == Symbol.idt)
                    {
                        var idEntry = SymTable.Lookup(myLex.Lexeme);
                        if (idEntry.TypeOfEntry == EntryType.classEntry)
                        {
                            MethodCall();
                            var entry = SymTable.Lookup(currentExprVar);
                            EmitCode(FormatThreeAddrCode(ConvertEntryToBasePointer(entry), "=" , "_ax"));
                        }
                        else
                        {
                            //New Stuff
                            Expr_Ptr = Expr();
                            EmitCode(FormatThreeAddrCode(ConvertEntryToBasePointer(tabEntry), "=", ConvertEntryToBasePointer(Expr_Ptr)));
                        
                            
                        }
                    }
                    else
                    {
                        //New Stuff
                        Expr_Ptr = Expr();
                        EmitCode(FormatThreeAddrCode(ConvertEntryToBasePointer(tabEntry), "=", ConvertEntryToBasePointer(Expr_Ptr)));

                    }

                }
            }
            else
            {
                Console.WriteLine("Error at Line " + myLex.LineNo + " : \"" + myLex.Lexeme + "\" is not defined ");
                Console.WriteLine("");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(-1);
            }

        }

        /********************************************************************
        *** FUNCTION Statement                                            ***
        *********************************************************************
        *** DESCRIPTION : This function is grammar rule for a statement   ***
        *** in the program                                                ***
        ***                                                               ***
        ***           Statement -> AssignStat | IOStat                    ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/

        private static void Statement()
        {
            if (myLex.Token == Symbol.idt)
            {
                AssignStat();
            }
            else
            {
                IOStat();
            }
        }

        /********************************************************************
        *** FUNCTION StatTail                                             ***
        *********************************************************************
        *** DESCRIPTION : This function is grammar rule for a statement   ***
        *** that trails the first statement in the program                ***
        ***                                                               ***
        ***           StatTail -> Statement ; StatTail | ε                ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void StatTail()
        {
            if (myLex.Token == Symbol.idt)
            {
                Statement();
                match(Symbol.semicolont);
                StatTail();
            }
        }

        /********************************************************************
        *** FUNCTION SeqOfStatements                                      ***
        *********************************************************************
        *** DESCRIPTION : This function is the grammar rule for sequence  ***
        *** of statements in the program                                  ***
        ***                                                               ***
        ***         SeqOfStatements -> Statement ; StatTail | ε           ***
        ***                                                               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void SeqOfStatements()
        {
            if (myLex.Token == Symbol.idt)
            {
                Statement();
                match(Symbol.semicolont);
                StatTail();
            }   
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
                SymTable.Lookup<VarEntry>(myLex.Lexeme).IsFuncParam = true;
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
                SymTable.Lookup<VarEntry>(myLex.Lexeme).IsFuncParam = true;
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

                // Proc <currentMethodName>
                EmitCode(FormatThreeAddrCode("proc", myLex.Lexeme));
                //

                currentSizeOfLocalVars = 0;

                match(Symbol.idt);
                match(Symbol.lparent);

                currentDepth += 1;
                currentOffset = 0;
                
                FormalList();
                match(Symbol.rparent);
                match(Symbol.lcurlyt);
                VarDecl();


                SymTable.Lookup<FuncEntry>(currentMethodName).SizeOfLocalVar = currentSizeOfLocalVars;

                SeqOfStatements();
                match(Symbol.returnt);
                Expr();
                match(Symbol.semicolont);
                match(Symbol.rcurlyt);

                EmitCode(FormatThreeAddrCode("endp", currentMethodName));
                EmitCode(FormatThreeAddrCode());

                SymTable.WriteTable(currentDepth);
                SymTable.DeleteDepth(currentDepth);
                currentDepth -= 1;

                //SymTable.Lookup<FuncEntry>(currentMethodName).SizeOfLocalVar = currentSizeOfLocalVars;

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
                currentSizeOfLocalVars += SymTable.Lookup<VarEntry>(myLex.Lexeme).Size;
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
                    currentSizeOfLocalVars += SymTable.Lookup<ConstEntry>(myLex.Lexeme).Size;
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

            EmitCode(FormatThreeAddrCode("proc", currentMethodName));

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

            EmitCode(FormatThreeAddrCode("endp", currentMethodName));

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

        private static void CheckIfDeclaredVariable(string Lexeme)
        {
            try
            {
                var VarEntr = SymTable.Lookup<VarEntry>(Lexeme);
              
            }
            catch (Exception)
            {
                try
                {
                    var ConstEntr = SymTable.Lookup<ConstEntry>(Lexeme);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error at Line " + myLex.LineNo + " : \"" + Lexeme + "\" is not defined ");
                    Console.WriteLine("");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(-1);
                }
            }
        }

        private static VarEntry NewTempTableEntry()
        {
            string temp = "_t" + newTempCount.ToString();
            SymTable.Insert(temp, Symbol.idt, currentDepth, EntryType.variableEntry);

            VarEntry varEntry = SymTable.Lookup<VarEntry>(temp);
            varEntry.Offset = currentOffset;
            varEntry.Size += 2;

            currentOffset += varEntry.Size;

            newTempCount++;
            return varEntry;
        }

        private static string ConvertEntryToBasePointer(TableEntry entry)
        {
            if(entry.Depth > 1)
            {
                if(entry.TypeOfEntry == EntryType.variableEntry)
                {
                    var VarEntr = SymTable.Lookup<VarEntry>(entry.Lexeme);
                    var Method = SymTable.Lookup<FuncEntry>(currentMethodName);

                    if (VarEntr.IsFuncParam)
                    {
                        return "_bp+" + (VarEntr.Offset + 4).ToString();
                    }
                    else
                    {
                        return "_bp-" + (Math.Abs(Method.SizeOfParameters - VarEntr.Offset) + 2).ToString();
                    }
                }
                else if(entry.TypeOfEntry == EntryType.constEntry)
                {
                    var ConstEntr = SymTable.Lookup<ConstEntry>(entry.Lexeme);
                    var Method = SymTable.Lookup<FuncEntry>(currentMethodName);

                    return "_bp-" + (Math.Abs(Method.SizeOfParameters - ConstEntr.Offset) + 2).ToString();
                }
                
                return string.Empty;
            }
            else
            {
                return entry.Lexeme;
            }
        }

        private static string FormatThreeAddrCode(string idt1 = " ", string equals = " ", string idt2 = " ", string op = " ", string idt3 = " ")
        {
            return String.Format("{0, -6} {1, -4} {2, -6} {3, -4} {4, -6}", idt1, equals, idt2, op, idt3);
        }

        private static void EmitCode(string code)
        {
            threeAddressCode.Add(code);
        }

        private static void GenerateThreeAddressCodeFile()
        {
            var threeAddrCodeFileName = Path.GetFileNameWithoutExtension(sourceFileName) + ".tac";
            File.WriteAllLines(threeAddrCodeFileName, threeAddressCode.ToArray());
        }

        private static SymbolTable SymTable;
        private static int currentDepth;
        private static int currentOffset;
        private static int currentSizeOfLocalVars;
        private static string currentMethodName;
        private static string currentClassName;
        private static VarType VarType;
        private static string currentCalledMethodName;
        private static string currentExprVar;
        private static List<string> threeAddressCode;
        private static int newTempCount = 1;
        private static string sourceFileName; 
       
               
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
        public static void BeginProgramParser(string fileName)
        {
            SymTable = new SymbolTable();
            currentDepth = 0;
            currentOffset = 0;
            threeAddressCode = new List<string>();
            sourceFileName = fileName;

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

            GenerateThreeAddressCodeFile();
        }


    }
}