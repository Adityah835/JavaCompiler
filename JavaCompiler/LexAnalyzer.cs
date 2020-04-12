using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace JavaCompiler
{
    public static class LexAnalyzer
    {
        public static Symbol Token;
        public static string Lexeme { get; set; }
        public static char ch;
        public static int LineNo;
        public static int Value;
        public static double ValueR;
        public static string Literal;
        public static bool IsComment() { return Comment; }
        public static StreamReader stream { get; set; }

        public enum Symbol
        {
            classt, publict, statict, voidt, maint, Stringt, extendst,
            returnt, intt, booleant, ift, elset, whilet, printlnt, lengtht,
            truet, falset, thist, newt, finalt, floatt,

            relop, addop, mulop, assignop, notop,

            lparent, rparent, lcurlyt, rcurlyt, lsqrbract, rsqrbract, commat,
            semicolont, periodt, qoutet,

            numt, idt, eoft, unknownt

        };

        public static Dictionary<Symbol, string> reserveWords = new Dictionary<Symbol, string>
        {
            {Symbol.classt, "class" },
            {Symbol.publict, "public" },
            {Symbol.statict, "static" } ,
            {Symbol.voidt, "void" },
            {Symbol.maint, "main" },
            {Symbol.Stringt, "String" },
            {Symbol.extendst, "extends" },
            {Symbol.returnt, "return" },
            {Symbol.intt, "int" },
            {Symbol.booleant, "boolean" },
            {Symbol.ift, "if" },
            {Symbol.elset, "else" },
            {Symbol.whilet, "while" },
            {Symbol.printlnt, "System.out.println" },
            {Symbol.lengtht, "length" },
            {Symbol.truet, "true" },
            {Symbol.falset, "false" },
            {Symbol.thist, "this" },
            {Symbol.newt, "new" },
            {Symbol.finalt, "final"},
            {Symbol.floatt, "float" }
        };


        /********************************************************************
        *** FUNCTION GetNextToken                                         ***
        *********************************************************************
        *** DESCRIPTION : This function is reponsible to get a token to be***
        *** processed by calling GetNextCh function                       ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        public static void GetNextToken()
        {
            Value = 0;
            ValueR = 0.0;
            Literal = "";
            Lexeme = "";
            Token = Symbol.unknownt;
            Comment = false;

            if (!EOF)
            {
                while (ch <= ' ' && !stream.EndOfStream)
                {

                    GetNextCh();
                }

                Lexeme = string.Concat(Lexeme, ch);

                ProcessToken(); 
            }
            else Token = Symbol.eoft;
            
        }

        /********************************************************************
        *** FUNCTION DisplayToken                                         ***
        *********************************************************************
        *** DESCRIPTION : This function is reponsible to display the token***
        *** that has been processed along with the token type             ***
        *** processed by calling GetNextCh function                       ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        public static void DisplayToken()
        {

            if (displayCounter < 20)
            {
                if (Literal != string.Empty)
                    Console.WriteLine("Literal".PadRight(10, ' ') + Literal);
                else
                    Console.WriteLine(Token.ToString().PadRight(10, ' ') + Lexeme);
                displayCounter++;
            }
            else
            {
                displayCounter = 0;
                Console.Write("Press any key to continue.... ");
                Console.ReadKey();
                if (Literal != string.Empty)
                    Console.WriteLine("\r" + "Literal".PadRight(10, ' ') + Literal + "             ");
                else
                    Console.WriteLine("\r" + Token.ToString().PadRight(10, ' ') + Lexeme + "             ");
            }
        }

        /********************************************************************
        *** FUNCTION GetNextCh                                            ***
        *********************************************************************
        *** DESCRIPTION : This function is reponsible to get next character**
        *** in the source code                                            ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        public static void GetNextCh()
        {
            if (!stream.EndOfStream)
            {
                ch = (char)stream.Read();

                if (ch == '\n')
                    LineNo++;
            }
            else
            {
                ch = '\0';
                Token = Symbol.eoft;
            }

        }


        private static Regex AlphaNumSymbols = new Regex("^[a-zA-Z0-9_]");
        private static Regex ResWordSymbols = new Regex("^[a-zA-z.]");
        private static Regex NumSymbols = new Regex("^[0-9]");
        private static int displayCounter = 0;
        private static bool EOF = false;
        private static bool Comment = false;

        /********************************************************************
        *** FUNCTION ProcessToken                                         ***
        *********************************************************************
        *** DESCRIPTION : This function is reponsible process the read token*
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void ProcessToken()
        {
                     
            if (Lexeme[0] >= 'A' && Lexeme[0] <= 'Z')
            {
                ProcessWordToken();
            }

            else if (Lexeme[0] >= 'a' && Lexeme[0] <= 'z')
            {
                ProcessWordToken();
            }

            else if (Lexeme[0] >= '0' && Lexeme[0] <= '9')
            {
                ProcessNumToken();
            }


            else if (Lexeme[0] == '<' || Lexeme[0] == '>' || Lexeme[0] == '=' || Lexeme[0] == '!')
            {
                GetNextCh();
                if (ch == '=')
                    ProcessDoubleToken();
                else
                {
                    ProcessSingleToken();
                }
            }

            else if (Lexeme[0] == '&')
            {
                GetNextCh();
                if (ch == '&')
                    ProcessDoubleToken();
                else
                {
                    ProcessSingleToken();
                }
            }

            else if (Lexeme[0] == '|')
            {
                GetNextCh();
                if (ch == '|')
                    ProcessDoubleToken();
                else
                {
                    ProcessSingleToken();
                }
            }

            else if (Lexeme[0] == '\"')
            {
                ProcessLiteral();

                if (Lexeme.Length == 1)
                    ProcessSingleToken();
            }

            else if (Lexeme[0] == '/')
            {
                GetNextCh();
                if (ch == '/')
                {
                    Comment = true;
                    ProcessSingleLineComment();
                }
                else if (ch == '*')
                {
                    Comment = true;
                    GetNextCh();
                    Lexeme = "";
                    ProcessMultiLineComment();
                }
                else
                {
                    ProcessSingleToken();
                }

            }

            else
            {
                ProcessSingleToken();
                GetNextCh();
            }

            if (stream.EndOfStream)
                EOF = true;
        }

        /********************************************************************
        *** FUNCTION ProcessWordToken                                     ***
        *********************************************************************
        *** DESCRIPTION : This function is reponsible process the read token*
        *** that has alphabets, numbers or both                           ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void ProcessWordToken()
        {
            GetNextCh();

            while (AlphaNumSymbols.IsMatch(ch.ToString()))
            {
                Lexeme = string.Concat(Lexeme, ch);
                GetNextCh();
            }
            
            if (Lexeme.Length > 1)
                ProcessReserveWord();
            else
                Token = Symbol.idt;

        }

        /********************************************************************
        *** FUNCTION ProcessReserveWord                                   ***
        *********************************************************************
        *** DESCRIPTION : This function is reponsible process the lexeme  ***
        *** by checking if it is a reserved word or an identity token     ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void ProcessReserveWord()
        {
            foreach (var myDict in reserveWords)
            {
                if (myDict.Value.StartsWith(Lexeme) && !myDict.Value.Equals(Lexeme))
                {
                    ProcessLongReserveWord();
                    break;
                }
                else if (myDict.Value.Equals(Lexeme))
                {
                    Token = myDict.Key;
                    break;
                }
            }
            if (Token == Symbol.unknownt && Lexeme.Length <= 31)
                Token = Symbol.idt;
        }

        /********************************************************************
        *** FUNCTION ProcessLongReserveWord                               ***
        *********************************************************************
        *** DESCRIPTION : This function is reponsible process the lexeme  ***
        *** by checking if it is a long reserved word token               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void ProcessLongReserveWord()
        {
            while (ResWordSymbols.IsMatch(ch.ToString()))
            {
                Lexeme = string.Concat(Lexeme, ch);
                GetNextCh();
            }

            foreach (var myDict in reserveWords)
            {
                if (myDict.Value.Equals(Lexeme))
                {
                    Token = myDict.Key;
                    return;
                }
            }
            
        }

        /********************************************************************
        *** FUNCTION ProcessNumToken                                      ***
        *********************************************************************
        *** DESCRIPTION : This function is reponsible process the lexeme  ***
        *** by checking if it is a number token                           ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void ProcessNumToken()
        {

            GetNextCh();

            while (ch >= '0' && ch <= '9')
            {
                Lexeme = string.Concat(Lexeme, ch);
                GetNextCh();
            }

            if (ch == '.')
            {
                Lexeme = string.Concat(Lexeme, ch);
                GetNextCh();

                if (ch < '0' && ch > '9') Token = Symbol.unknownt;

                while (ch >= '0' && ch <= '9')
                {
                    Lexeme = string.Concat(Lexeme, ch);
                    GetNextCh();
                    Token = Symbol.numt;
                    ValueR = double.Parse(Lexeme);

                }

            }
            else
            {
                Token = Symbol.numt;
                Value = int.Parse(Lexeme);
            }

        }

        /********************************************************************
        *** FUNCTION ProcessSingleLineComment                             ***
        *********************************************************************
        *** DESCRIPTION : This function is reponsible to ignore all the   ***
        *** contents enclosed withing the a single-line comment           ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void ProcessSingleLineComment()
        {
            Lexeme = "";
            while (ch != '\n')
            {
                GetNextCh();
            }

        }

        /********************************************************************
        *** FUNCTION ProcessMultiLineComment                              ***
        *********************************************************************
        *** DESCRIPTION : This function is reponsible to ignore all the   ***
        *** contents enclosed withing the a multi-line comment            ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void ProcessMultiLineComment()
        {
            char tempChar = ch;
            GetNextCh();
            
            while(!stream.EndOfStream)
            {
                if (tempChar == '*' && ch == '/')
                {
                    GetNextCh();
                    return;
                }   

                else
                {
                    tempChar = ch;
                    GetNextCh();
                }
            }

        }


        /********************************************************************
        *** FUNCTION ProcessLiteral                                       ***
        *********************************************************************
        *** DESCRIPTION : This function is reponsible to process the content*
        *** enclosed within the double quotes ""                          ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void ProcessLiteral()
        {
            GetNextCh();
            while (ch != '\n') 
            {
                if(ch == '\"')
                {
                    Lexeme = string.Concat(Lexeme, ch);
                    Literal = Lexeme;
                    GetNextCh();
                    return;
                }

                else if(ch != '\r' && ch !='\n')
                {
                    Lexeme = string.Concat(Lexeme, ch);
                    GetNextCh();
                }
                else
                {
                    return;
                }
                
            }
            if (ch == '\n')
                LineNo++;
        }

        /********************************************************************
        *** FUNCTION ProcessSingleToken                                   ***
        *********************************************************************
        *** DESCRIPTION : This function is reponsible to categorize all the**
        *** single operators and symbols in the source code               ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void ProcessSingleToken()
        {
            if (Lexeme[0] == '<' || Lexeme[0] == '>')
            {
                Token = Symbol.relop;
                
            }
            else if(Lexeme[0] == '+' || Lexeme[0] == '-')
            {
                Token = Symbol.addop;
                
            }
            else if(Lexeme[0] == '*' || Lexeme[0] == '/')
            {
                Token = Symbol.mulop;
                
            }
            else if(Lexeme[0] == '=')
            {
                Token = Symbol.assignop;
                
            }
            else if(Lexeme[0] == '.')
            {
                Token = Symbol.periodt;
            }
            else if(Lexeme[0] == '(')
            {
                Token = Symbol.lparent;
            }
            else if(Lexeme[0] == ')')
            {
                Token = Symbol.rparent;
            }
            else if(Lexeme[0] == '{')
            {
                Token = Symbol.lcurlyt;
            }
            else if(Lexeme[0] == '}')
            {
                Token = Symbol.rcurlyt;
            }
            else if(Lexeme[0] == '[')
            {
                Token = Symbol.lsqrbract;
            }
            else if(Lexeme[0] == ']')
            {
                Token = Symbol.rsqrbract;
            }
            else if(Lexeme[0] == ';')
            {
                Token = Symbol.semicolont;
            }
            else if(Lexeme[0] == ',')
            {
                Token = Symbol.commat;
            }
            else if(Lexeme[0] == '\"')
            {
                Token = Symbol.qoutet;
            }
            else if(Lexeme[0] == '!')
            {
                Token = Symbol.notop;
            }
            
        }

        /********************************************************************
        *** FUNCTION ProcessDoubleToken                                   ***
        *********************************************************************
        *** DESCRIPTION : This function is reponsible to categorize all the**
        *** double operators like relational and arithmetic operators     ***
        *** INPUT ARGS : -                                                ***
        *** OUTPUT ARGS : -                                               ***
        *** IN/OUT ARGS : -                                               ***
        *** RETURN : void                                                 ***
        ********************************************************************/
        private static void ProcessDoubleToken()
        {
            Lexeme = string.Concat(Lexeme, ch);

            if (Lexeme.Equals("==") || Lexeme.Equals("!=") || Lexeme.Equals("<=") || Lexeme.Equals(">="))
                Token = Symbol.relop;

            if (Lexeme.Equals("||"))
                Token = Symbol.addop;

            if (Lexeme.Equals("&&"))
                Token = Symbol.mulop;

            GetNextCh();
        }

    }   
}
