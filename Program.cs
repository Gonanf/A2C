using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.CodeDom;

namespace Chaos
{
    class Arbys2Code
    {
        //El main sirve para obtener los argumentos luego del comando (ejemplo: Arbys2Code.exe -nashe)
        static void Main(string[] args)
        {
            string Vercion = "0.0.2";
            List<(string Valor, string Tipo)> Tokens = new List<(string Valor, string Tipo)>();
            var User = EnvironmentVariableTarget.User;
            var Old = Environment.GetEnvironmentVariable("PATH", User);
            var New = Old + Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (args.Length == 0)
            {
                Console.WriteLine("Instalar Arbys2Code en la variable de entorno PATH?");
                Console.WriteLine("S/N");
                switch (Console.ReadLine().ToLower())
                {
                    case "s":
                        if (Old.Contains(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)))
                        {
                            Console.WriteLine("Ya esta instalado en el sistema");
                            Console.WriteLine($"La ruta a Arbys2Code es:\n{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}");
                            Console.WriteLine("Arbys2Code Vercion " + Vercion);
                            Console.ReadLine();
                            Environment.Exit(0);
                            break;
                        }
                        Environment.SetEnvironmentVariable("Path", New, User);
                        Console.WriteLine($"La ruta a Arbys2Code es:\n{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}");
                        Console.WriteLine("Listo!");
                        Console.ReadLine();
                        Environment.Exit(0);
                        break;
                    case "n":
                        Console.WriteLine($"La ruta a Arbys2Code es:\n{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}");
                        Console.WriteLine("Arbys2Code Vercion " + Vercion);
                        Console.ReadLine();
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("No es un caracter valido");
                        Console.ReadLine();
                        Environment.Exit(0);
                        break;
                }


            }
            if (args.Length != 2)
            {
                Console.WriteLine("Arbys2Code necesita 2 argumentos, un metodo de ejecucion y un archivo .arb, coloque Arbys2Code Ayuda para mas informacion");
                Console.ReadLine();
                Environment.Exit(0);
            }
            switch (args[0])
            {
                case "JIT":
                    Console.WriteLine("Just In Time");
                    break;
                case "CMP":
                    Console.WriteLine("Compilado");
                    break;
                case "Ambos":
                    Console.WriteLine("JIT y CMP");
                    break;
                case "Depurar":
                    Console.WriteLine("Depurar");
                    break;
                case "Ayuda":
                    Console.WriteLine("Ayuda");
                    Console.ReadLine();
                    Environment.Exit(0);
                    break;
            }
            string Dir = Path.GetFullPath(args[1]);
            if (Dir != null)
            {
                if (System.IO.File.Exists(Dir + ".arb"))
                {
                    Dir += ".arb";
                }
                else
                {
                    Console.WriteLine("Archivo.arb no encontrado");
                }

            }
            else
            {
                Console.WriteLine("[ERROR] No existe el archivo especificado.");
                Console.ReadLine();
            }

            Tokenizador();
            Console.ForegroundColor = ConsoleColor.Green;
            Tokens.ForEach(tok =>
            {
                
                Console.WriteLine(tok.ToString());
            });
            Console.ForegroundColor = ConsoleColor.White;
            Perser(args[0]);
            void Tokenizador()
            {
                string Codigo = string.Join(" ", File.ReadAllLines(Dir));
                int Pos = 0;
                string Texto = "";
                string CharVal = "abcdefghijklmnñopqrstuvwxyzABCDEFGHIJKLMNÑOPQRSTUVWXYZ";
                string NumVal = "1234567890";
                string[] Funciones = {
      "Imprimir",
      "Leer",
      "Si",
      "Mientras",
      "Repetir",
      "Fin",
      "=",
      "+",
      "-",
      "*",
      "/"};
                while (Pos < Codigo.Length - 1)
                {
                    switch (Codigo[Pos])
                    {
                        case ' ':
                            if (Funciones.Contains(Texto))
                            {
                                Tokens.Add((Texto, "Funcion"));
                                Texto = "";

                                Avanzar();
                                break;
                            }
                            else if (Texto != " " && Texto != string.Empty)
                            {
                                if (EsNumerico())
                                {
                                    Tokens.Add((Texto, "Entero"));
                                    Texto = "";
                                    Avanzar();
                                    break;
                                }
                                else
                                {
                                    Tokens.Add((Texto, "Variable"));
                                    Texto = "";
                                    Avanzar();
                                    break;
                                }
                                
                            }
                            else
                            {
                                Avanzar();
                                Texto = "";
                            }
                            break;
                        case '<':
                            if (Texto != " " && Texto != string.Empty)
                            {
                                Tokens.Add((Texto, "Variable"));
                                Texto = "";
                            }
                            Avanzar();
                            while (Codigo[Pos] != '>')
                            {
                                Texto += Codigo[Pos];
                                Avanzar();
                            }
                            Tokens.Add((Texto, "String"));
                            Avanzar();
                            Texto = "";
                            break;
                        default:
                            Texto += Codigo[Pos];
                            Avanzar();
                            break;
                    }

                }
                if (Codigo[Pos] != ' ' && Codigo[Pos] != '<' && Codigo[Pos] != '>' && Texto != "\n" && Texto != "\r")
                {
                    Texto += Codigo[Pos];
                }
                if (Texto != " " && Texto != string.Empty)
                {
                    Tokens.Add((Texto, "Variable"));
                }
                bool EsNumerico()
                {
                    foreach(char c in Texto)
                    {
                        if (c < '0' || c > '9')
                        {
                            return false;
                        }
                    }
                    return true;
                }
                void Avanzar()
                {
                    if (Pos < Codigo.Length - 1)
                    {
                        Pos++;
                    }
                }
            }



            void Perser(string Valargs)
            {
                int Pos = 0;
                string Compilado = "using System;\n" +
                    "namespace Chaos {\n " +
                    "class A2C {\n" +
                    "static void Main(){\n";




                CSharpCodeProvider CodProv = new CSharpCodeProvider();
                CompilerParameters Param = new CompilerParameters();
                Param.GenerateExecutable = true;
                Param.OutputAssembly = "Main.exe";
                Param.GenerateInMemory = false;
                Param.ReferencedAssemblies.Add("system.dll");
                List<(string Nombre, string Tipo, string Valor)> Variables = new List<(string Nombre, string Tipo, string Valor)>();
                while (Pos < Tokens.Count())
                {
                    switch (Tokens[Pos].Tipo)
                    {
                        case "Funcion":
                            switch (Tokens[Pos].Valor)
                            {
                                case "Imprimir":
                                    if (SiExisteTokens(1))
                                    {
                                        Imprimir(Tokens[Pos + 1].Valor, Tokens[Pos + 1].Tipo);
                                        Avanzar(2);

                                    }
                                    continue;
                                case "Leer":
                                    switch (Valargs)
                                    {
                                        case "JIT":
                                            Console.ReadLine();
                                            break;
                                        case "CMP":
                                            Compilado += "System.Console.ReadLine();\n";
                                            break;
                                        case "Ambos":
                                            Console.ReadLine();
                                            Compilado += "System.Console.ReadLine();\n";
                                            break;
                                    }
                                    Avanzar(1);
                                    continue;
                                case "Si":

                                    continue;
                                case "Mientras":

                                    continue;
                                case "Repetir":

                                    continue;

                            }
                            break;
                        case "Variable":
                            if (SiExisteTokens(2))
                            {
                                if (Tokens[Pos + 1].Tipo == "Funcion")
                                {
                                    switch (Tokens[Pos + 1].Valor)
                                    {
                                        case "+":

                                            continue;
                                        case "-":

                                            continue;
                                        case "*":

                                            continue;
                                        case "/":

                                            continue;
                                        case "=":
                                            
                                            Asignar();
                                            Avanzar(3);
                                            continue;
                                        default:
                                            //Situacion de error
                                            Avanzar(1);
                                            break;
                                    }
                                }

                            }
                            //situacion de error
                            Avanzar(1);
                            continue;
                            default:
                            continue;
                    }
                    continue;
                }
                if (Valargs == "CMP" || Valargs == "Ambos")
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Compilado += "\nSystem.Console.ReadLine();}\n}\n}";
                    Console.WriteLine(Compilado);
                    CompilerResults Results = CodProv.CompileAssemblyFromSource(Param, Compilado);
                    if (Results.Errors.HasErrors)
                    {
                        foreach (CompilerError CompErr in Results.Errors)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(CompErr + " uwu");
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
                bool SiExisteTokens(int num)
                {
                    if (Pos + num < Tokens.Count())
                    {
                        return true;
                    }
                    else { return false; }
                }
                void Avanzar(int num)
                {
                    if (Pos + num < Tokens.Count())
                    {
                        Pos = Pos + num;
                    }
                    else
                    {
                        Pos = Tokens.Count();
                    }
                }
                int IndexVar(string Nombre)
                {
                    if (Variables.Count > 0)
                    {
                        for (int i = 0; i < Variables.Count; i++)
                        {
                            if(Variables[i].Nombre == Nombre)
                            {
                                return i;
                            }
                            
                        }
                        //Situacion de error
                        return -1;
                    }
                    else
                    {
                        //Situacion de error
                        return -1;
                    }
                }
                void Imprimir(string Valor, string Tipo)
                {
                    if (Tipo == "Variable")
                    {
                        int Actual = IndexVar(Valor);
                        if (Actual != -1)
                            {
                            switch (Valargs)
                                {
                                    case "JIT":
                                        Console.WriteLine(Variables[Actual].Valor);
                                        break;
                                    case "CMP":
                                        Compilado += " System.Console.WriteLine("+ Variables[Actual].Nombre + "); \n";
                                        break;
                                    case "Ambos":
                                        Console.WriteLine(Variables[Actual].Valor);
                                        Compilado += " System.Console.WriteLine(" + Variables[Actual].Nombre + ");\n";
                                        break;
                                }

                            }
                        else
                        {
                           
                        }
                        }
                    else
                    {
                        switch (Valargs)
                        {
                            case "JIT":
                                Console.WriteLine(Valor);
                                break;
                            case "CMP":
                                Compilado += " System.Console.WriteLine(\"" + Valor + "\");\n";
                                break;
                            case "Ambos":
                                Console.WriteLine(Valor);
                                Compilado += " System.Console.WriteLine(\"" + Valor + "\");\n";
                                break;
                        }
                    }
                }
                string Leer()
                { string Texto;
                    Texto = Console.ReadLine();
                    return Texto;
                }
                void Asignar()
                {
                    int Original = IndexVar(Tokens[Pos].Valor);
                    if (Tokens[Pos + 2].Tipo == "String" || Tokens[Pos + 2].Tipo == "Entero")
                    {
                        switch (Valargs)
                        {
                            case "JIT":
                                if (Original == -1)
                                {
                                    Variables.Add((Tokens[Pos].Valor, Tokens[Pos + 2].Tipo, Tokens[Pos + 2].Valor));
                                    Original = IndexVar(Tokens[Pos].Valor);
                                }
                                else
                                {
                                    Variables[Original] = ((Tokens[Pos].Valor, Tokens[Pos + 2].Tipo, Tokens[Pos + 2].Valor));
                                }
                             break;
                            case "CMP":
                                if (Original == -1)
                                {
                                    Compilado += "var " + Tokens[Pos].Valor + " = " + "\"" + Tokens[Pos + 2].Valor + "\"" + ";\n";
                                    Variables.Add((Tokens[Pos].Valor, Tokens[Pos + 2].Tipo, Tokens[Pos + 2].Valor));
                                    Original = IndexVar(Tokens[Pos].Valor);
                                }
                                else
                                {
                                    Compilado += Variables[Original].Nombre + " = " + "\"" + Tokens[Pos + 2].Valor + "\"" + ";\n";
                                    Variables[Original] = ((Tokens[Pos].Valor, Tokens[Pos + 2].Tipo, Tokens[Pos + 2].Valor));
                                }
                                break;
                            case "Ambos":
                                if (Original == -1)
                                {
                                    Variables.Add((Tokens[Pos].Valor, Tokens[Pos + 2].Tipo, Tokens[Pos + 2].Valor));
                                    Compilado += "var " + Tokens[Pos].Valor + " = " + "\"" + Tokens[Pos + 2].Valor + "\"" + ";\n";
                                    Original = IndexVar(Tokens[Pos].Valor);
                                }
                                else
                                {
                                    Variables[Original] = ((Tokens[Pos].Valor, Tokens[Pos + 2].Tipo, Tokens[Pos + 2].Valor));
                                    Compilado += Variables[Original].Nombre + " = " + "\"" + Tokens[Pos + 2].Valor + "\"" + ";\n";
                                }
                                break;
                        }
                        
                    }
                    else if (Tokens[Pos + 2].Tipo == "Variable")
                    {
                        switch (Valargs)
                        {
                            case "JIT":
                                if (Original == -1)
                                {
                                    int Nuevo = -1;
                                    if (SiExisteTokens(2))
                                    {
                                        Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    }
                                    else
                                    {
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    if (Nuevo != -1)
                                    {
                                        Variables.Add((Tokens[Pos].Valor, Variables[Nuevo].Tipo, Variables[Nuevo].Valor));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else
                                    {
                                        //Situacion de error
                                    }

                                }
                                else
                                {
                                    int Nuevo = -1;
                                    if (SiExisteTokens(2))
                                    {
                                        Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    }
                                    else
                                    {
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    if (Nuevo != -1)
                                    {
                                        Variables[Original] = ((Tokens[Pos].Valor, Variables[Nuevo].Tipo, Variables[Nuevo].Valor));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else
                                    {
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }

                                }
                                break;
                            case "CMP":
                                if (Original == -1)
                                {
                                    int Nuevo = -1;
                                    if (SiExisteTokens(2))
                                    {
                                        Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    }
                                    else
                                    {
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    if (Nuevo != -1)
                                    {
                                        Variables.Add((Tokens[Pos].Valor, Variables[Nuevo].Tipo, Variables[Nuevo].Valor));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                        Compilado += "var " + Variables[Original].Nombre + " = " + Variables[Nuevo] + ";\n";
                      
                                    }
                                    else
                                    {
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }

                                }
                                else
                                {
                                    int Nuevo = -1;
                                    if (SiExisteTokens(2))
                                    {
                                        Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    }
                                    else
                                    {
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    if (Nuevo != -1)
                                    {

                                        Variables[Original] = ((Variables[Original].Nombre, Variables[Nuevo].Tipo, Variables[Nuevo].Valor));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                        Compilado += Variables[Original].Nombre + " = " + Variables[Nuevo].Nombre + ";\n";

                                    }
                                    else
                                    {
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }

                                }
                                break;
                            case "Ambos":
                                if (Original == -1)
                                {
                                    int Nuevo = -1;
                                    if (SiExisteTokens(2))
                                    {
                                        Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    }
                                    else
                                    {
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    if (Nuevo != -1)
                                    {
                                        Variables.Add((Tokens[Pos].Valor,Variables[Nuevo].Tipo, Variables[Nuevo].Valor));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                        Compilado += "var " + Variables[Original].Nombre + " = " + Variables[Nuevo].Nombre +";\n";
                                    }
                                    else
                                    {
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }

                                }
                                else
                                {
                                    int Nuevo = -1;
                                    if (SiExisteTokens(2))
                                    {
                                        Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    }
                                    else
                                    {
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    if (Nuevo != -1)
                                    {
                                        Variables[Original] = ((Tokens[Pos].Valor, Variables[Nuevo].Tipo, Variables[Nuevo].Valor));
                                        Compilado += Variables[Original].Nombre + " = " + Variables[Nuevo].Nombre + ";\n";
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else
                                    {
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }

                                }
                                break;
                        }
                        
                    }
                    else if (Tokens[Pos + 2].Tipo == "Funcion")
                    {
                        if (Tokens[Pos + 2].Valor == "Leer")
                        {
                            switch (Valargs)
                            {
                                case "JIT":
                                    if (Original == -1)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        string tem = Console.ReadLine();
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Variables.Add((Tokens[Pos].Valor, "String", tem));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else {
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        string tem = Console.ReadLine();
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Variables[Original] = ((Tokens[Pos].Valor, "String", tem));
                                    }
                                    break;
                                case "CMP":
                                    if (Original == -1)
                                    {
                                        Compilado += "var " + Tokens[Pos].Valor + " = " + " System.Console.ReadLine();\n";
                                        Variables.Add((Tokens[Pos].Valor, "String", "{Vacio}"));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else
                                    {
                                        Compilado += Variables[Original].Nombre + " = " + " System.Console.ReadLine();\n";
                                        Variables[Original] = ((Tokens[Pos].Valor, "String", "{Vacio}"));
                                    }
                                    break;
                                    
                                case "Ambos":
                                    if (Original == -1)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        string tem = Console.ReadLine();
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Compilado += "var " + Tokens[Pos].Valor + " = " + " System.Console.ReadLine();\n";
                                        Variables.Add((Tokens[Pos].Valor, "String", tem));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        string tem = Console.ReadLine();
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Variables[Original] = ((Tokens[Pos].Valor, "String", tem));
                                        Compilado += Variables[Original].Nombre + " = " + " System.Console.ReadLine();\n";
                                    }
                                    
                                    break;
                            }
                        }
                        else
                        {
                            //Situacion de error
                            Environment.Exit(0);
                        }
                    }
                }
            }
        }
    }
}
