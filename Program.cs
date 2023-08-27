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
using System.Runtime.Remoting.Contexts;

namespace Chaos
{
    class Arbys2Code
    {
        //El main sirve para obtener los argumentos luego del comando (ejemplo: Arbys2Code.exe -nashe)
        static void Main(string[] args)
        {
            string Vercion = "0.0.3";
            string Edicion = "Estandar";
            List<(string Valor, string Tipo)> Tokens = new List<(string Valor, string Tipo)>();
            List<(string Nombre, int Token)> Errores = new List<(string Nombre, int Token)>();
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
                if (Edicion == "Debug")
                {
                    Console.WriteLine(tok.ToString());
                }
            });
            Console.ForegroundColor = ConsoleColor.White;
            Perser("Depurar");
            if (Errores.Count == 0)
            {
                Perser(args[0]);
            }
            else
            {
                for (int i = 0; i < Errores.Count; i++)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(Errores[i].Nombre + " Con token " + Tokens[Errores[i].Token]);
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey();
                Environment.Exit(0);
            }
            void Tokenizador()
            {
                string Codigo = string.Join(" ", File.ReadAllLines(Dir));
                int Pos = 0;
                string Texto = "";
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
                string Operaciones = "+-*/";
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
                            ImprimirPorDebug("Es funcion");
                            switch (Tokens[Pos].Valor)
                            {
                                case "Imprimir":
                                    ImprimirPorDebug("Es imprimir");
                                    if (SiExisteTokens(1))
                                    {
                                        ImprimirPorDebug("Imprimiendo");
                                        Imprimir(Tokens[Pos + 1].Valor, Tokens[Pos + 1].Tipo);
                                        Avanzar(2);
                                        ImprimirPorDebug("Avanzado 2");
                                    }
                                    else
                                    {
                                        //situacion de error
                                        Environment.Exit(0);
                                    }
                                    continue;
                                case "Leer":
                                    switch (Valargs)
                                    {
                                        case "JIT":
                                            ImprimirPorDebug("JIT Leyendo");
                                            Console.ReadLine();
                                            break;
                                        case "CMP":
                                            ImprimirPorDebug("CMP compilando");
                                            Compilado += "System.Console.ReadLine();\n";
                                            break;
                                        case "Ambos":
                                            ImprimirPorDebug("Ambos Leyendo y Compilando");
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
                            ImprimirPorDebug("Es variable");
                            if (SiExisteTokens(2))
                            {
                                if (Tokens[Pos + 1].Tipo == "Funcion")
                                {
                                    int Original = IndexVar(Tokens[Pos].Valor);
                                    int Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    switch (Tokens[Pos + 1].Valor)
                                    {
                                        
                                        
                                        case "=":
                                                ImprimirPorDebug("Asignando");
                                                (string Tipo, string Valor) Anterior = (String.Empty, String.Empty);
                                                if (Original != -1)
                                                {
                                                   Anterior = (Variables[Original].Tipo, Variables[Original].Valor);
                                                }
                                                Asignar();
                                                Original = IndexVar(Tokens[Pos].Valor);
                                                ImprimirPorDebug("Asignado");
                                                Avanzar(3);
                                                ImprimirPorDebug("Avanzando 3");
                                            if (Tokens[Pos].Tipo == "Funcion" && Operaciones.Contains(Tokens[Pos].Valor)) {
                                                (string Tipo, string Valor) Resultado;
                                                ImprimirPorDebug("Empezando a sumar");
                                                if (Anterior.Tipo != String.Empty && Tokens[Pos + 1].Tipo == "Variable" && Tokens[Pos + 1].Valor == Variables[Original].Nombre)
                                                {
                                                    Resultado = OperacionVV((Variables[Original].Tipo, Variables[Original].Valor), (Anterior.Tipo, Anterior.Valor), Tokens[Pos].Valor);
                                                    Variables[Original] = (Variables[Original].Nombre, Resultado.Tipo, Resultado.Valor);
                                                }
                                                else
                                                {
                                                    Resultado = OperacionVV((Variables[Original].Tipo, Variables[Original].Valor), (Tokens[Pos + 1].Tipo, Tokens[Pos + 1].Valor), Tokens[Pos].Valor);
                                                    Variables[Original] = (Variables[Original].Nombre, Resultado.Tipo, Resultado.Valor);
                                                }
                                                Avanzar(2);
                                                ImprimirPorDebug("Avanzo 2, termino de sumar el primer valor");
                                                while (Operaciones.Contains(Tokens[Pos].Valor))
                                                {
                                                    ImprimirPorDebug("Sumando");

                                                    if (Anterior.Tipo != String.Empty && Tokens[Pos + 1].Tipo == "Variable" && Tokens[Pos + 1].Valor == Variables[Original].Nombre)
                                                    {
                                                        Resultado = OperacionVV((Variables[Original].Tipo, Variables[Original].Valor), (Anterior.Tipo, Anterior.Valor), Tokens[Pos].Valor);
                                                        Variables[Original] = (Variables[Original].Nombre, Resultado.Tipo, Resultado.Valor);
                                                    }
                                                    else
                                                    {
                                                        Resultado = OperacionVV((Variables[Original].Tipo, Variables[Original].Valor), (Tokens[Pos + 1].Tipo, Tokens[Pos + 1].Valor), Tokens[Pos].Valor);
                                                        Variables[Original] = (Variables[Original].Nombre, Resultado.Tipo, Resultado.Valor);
                                                    }
                                                    Avanzar(2);
                                                }
                                            }
                                           
                                            
                                            continue;
                                        default:
                                            ImprimirPorDebug("Luego de la variable no hay operacion");
                                            if (Valargs == "Depurar")
                                            {
                                                //Situacion de advertencia
                                            }
                                            //Situacion de error
                                            Avanzar(1);
                                            break;
                                    }
                                }

                            }
                            else
                            {
                                ImprimirPorDebug("No existe tokens en tokens + 2");
                                if (Valargs == "Depurar")
                                {
                                    //aqui añade a la lista de errores este error
                                }
                                //situacion de error
                                Avanzar(1);
                                continue;
                            }
                            continue;
                            
                    }
                    continue;
                }
                if (Valargs == "CMP" || Valargs == "Ambos")
                {
                    ImprimirPorDebug("Empezando a compilar");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Compilado += "\nSystem.Console.ReadLine();}\n}\n}";
                    Console.WriteLine(Compilado);
                    CompilerResults Results = CodProv.CompileAssemblyFromSource(Param, Compilado);
                    ImprimirPorDebug("Compilado");
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
                void ImprimirPorDebug(string texto)
                {
                    if (Edicion == "Debug")
                    {
                        if (SiExisteTokens(0))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(texto + " " + Tokens[Pos] + " " + Pos);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            Avanzar(1);
                        }
                       
                    }
                }
                bool SiExisteTokens(int num)
                {
                    if (Pos + num < Tokens.Count())
                    {
                        return true;
                    }
                    else {
                        return false; }
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
                        ImprimirPorDebug("Hay variables");
                        for (int i = 0; i < Variables.Count; i++)
                        {
                            ImprimirPorDebug("Buscando variable: " + Nombre + " Actual: " + Variables[i]);
                            if(Variables[i].Nombre == Nombre)
                            {
                                ImprimirPorDebug("Encontrado " + Variables[i] + " index " + i);
                                return i;
                            }
                            
                        }
                        ImprimirPorDebug("No se encontro la variable (No se declaro)");
                        //Situacion de error
                        return -1;
                    }
                    else
                    {
                        ImprimirPorDebug("Intentando llamar a una variable en una lista vacia");
                        //Situacion de error
                        return -1;
                    }
                }
                void Imprimir(string Valor, string Tipo)
                {
                    if (Tipo == "Variable")
                    {
                        ImprimirPorDebug("Es variable");
                        int Actual = IndexVar(Valor);
                        if (Actual != -1)
                            {
                            ImprimirPorDebug(Variables[Actual] + " Existe");
                            switch (Valargs)
                                {
                                    case "JIT":
                                        ImprimirPorDebug("JIT imprimiendo variable");
                                        Console.WriteLine(Variables[Actual].Valor);
                                        break;
                                    case "CMP":
                                    ImprimirPorDebug("CMP compilando imprimiendo variable");
                                        Compilado += " System.Console.WriteLine("+ Variables[Actual].Nombre + "); \n";
                                        break;
                                    case "Ambos":
                                    ImprimirPorDebug("Ambos compilando e imprimiendo variable");
                                        Console.WriteLine(Variables[Actual].Valor);
                                        Compilado += " System.Console.WriteLine(" + Variables[Actual].Nombre + ");\n";
                                        break;
                                }

                            }
                        else
                        {
                            ImprimirPorDebug("No existe la variable a imprimir");
                           //Situacion de error
                           Environment.Exit(0);
                        }
                    }
                    else
                    {
                        switch (Valargs)
                        {
                            case "JIT":
                                ImprimirPorDebug("JIT imprimir valor");
                                Console.WriteLine(Valor);
                                break;
                            case "CMP":
                                ImprimirPorDebug("CMP compilar imprimir valor");
                                Compilado += " System.Console.WriteLine(\"" + Valor + "\");\n";
                                break;
                            case "Ambos":
                                ImprimirPorDebug("Ambos compilar e imprimir valor");
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
                void AñadirError(string mensaje)
                {
                    Errores.Add((mensaje, Pos));
                }
                (string Tipo, string Valor) OperacionVV((string Tipo, string Valor) Var1, (string Tipo, string Valor) Var2,string Operacion)
                {
                    if (Var2.Tipo == "Variable")
                    {
                        int Temporal = IndexVar(Var2.Valor);
                        if (Temporal != -1)
                        {
                            Var2 = (Variables[Temporal].Tipo, Variables[Temporal].Valor);
                        }
                        else
                        {
                            //Situacion de error
                        }
                    }

                    if (Var1.Tipo == "Entero" && Var2.Tipo == "Entero")
                    {
                        switch (Operacion)
                        {
                            case "+":
                                return ("Entero", (Int32.Parse(Var1.Valor) + Int32.Parse(Var2.Valor)).ToString());
                            case "-":
                                return ("Entero", (Int32.Parse(Var1.Valor) - Int32.Parse(Var2.Valor)).ToString());
                            case "*":
                                return ("Entero", (Int32.Parse(Var1.Valor) * Int32.Parse(Var2.Valor)).ToString());
                            case "/":
                                return ("Entero", (Int32.Parse(Var1.Valor) / Int32.Parse(Var2.Valor)).ToString());
                        }
                    }
                    else if(Var1.Tipo == "String")
                    {
                        switch (Operacion)
                        {
                            case "+":
                                return ("String", Var1.Valor + Var2.Valor);
                            case "-":
                                //Situacion de error
                                return ("", "");
                            case "*":
                                //Situacion de error
                                return ("", "");
                            case "/":
                                //Situacion de error
                                return ("", "");
                        }
                    }

                    //situacion de error
                    return ("", "");
                }
             

                void Asignar()
                {
                    int Original = IndexVar(Tokens[Pos].Valor);
                    if (Tokens[Pos + 2].Tipo == "String" || Tokens[Pos + 2].Tipo == "Entero")
                    {
                        ImprimirPorDebug("Asignar string o entero");
                        switch (Valargs)
                        {
                            case "JIT":
                                if (Original == -1)
                                {
                                    ImprimirPorDebug("JIT si no existe variable, añadirlo");
                                    Variables.Add((Tokens[Pos].Valor, Tokens[Pos + 2].Tipo, Tokens[Pos + 2].Valor));
                                    Original = IndexVar(Tokens[Pos].Valor);
                                }
                                else
                                {
                                    ImprimirPorDebug("JIT si existe la variable, cambiar su valor y tipos");
                                    Variables[Original] = ((Tokens[Pos].Valor, Tokens[Pos + 2].Tipo, Tokens[Pos + 2].Valor));
                                }
                             break;
                            case "CMP":
                                if (Original == -1)
                                {
                                    ImprimirPorDebug("CMP si no existe variable, añadirlo y compilarlo");
                                    Compilado += "var " + Tokens[Pos].Valor + " = " + "\"" + Tokens[Pos + 2].Valor + "\"" + ";\n";
                                    Variables.Add((Tokens[Pos].Valor, Tokens[Pos + 2].Tipo, Tokens[Pos + 2].Valor));
                                    Original = IndexVar(Tokens[Pos].Valor);
                                }
                                else
                                {
                                    ImprimirPorDebug("CMP si existe la variable, cambiarlo y compilarlo");
                                    Compilado += Variables[Original].Nombre + " = " + "\"" + Tokens[Pos + 2].Valor + "\"" + ";\n";
                                    Variables[Original] = ((Tokens[Pos].Valor, Tokens[Pos + 2].Tipo, Tokens[Pos + 2].Valor));
                                }
                                break;
                            case "Ambos":
                                if (Original == -1)
                                {
                                    ImprimirPorDebug("Ambos si no existe variable, añadirlo y compilarlo");
                                    Variables.Add((Tokens[Pos].Valor, Tokens[Pos + 2].Tipo, Tokens[Pos + 2].Valor));
                                    Compilado += "var " + Tokens[Pos].Valor + " = " + "\"" + Tokens[Pos + 2].Valor + "\"" + ";\n";
                                    Original = IndexVar(Tokens[Pos].Valor);
                                }
                                else
                                {
                                    ImprimirPorDebug("CMP si existe variable, cambiarlo y compilarlo");
                                    Variables[Original] = ((Tokens[Pos].Valor, Tokens[Pos + 2].Tipo, Tokens[Pos + 2].Valor));
                                    Compilado += Variables[Original].Nombre + " = " + "\"" + Tokens[Pos + 2].Valor + "\"" + ";\n";
                                }
                                break;
                            case "Depurar":
                                if (Original == -1)
                                {
                                    ImprimirPorDebug("Depurar si no existe variable, añadirlo");
                                    Variables.Add((Tokens[Pos].Valor, Tokens[Pos + 2].Tipo, Tokens[Pos + 2].Valor));
                                    Original = IndexVar(Tokens[Pos].Valor);
                                }
                                else
                                {
                                    ImprimirPorDebug("Depurar si existe la variable, cambiar su valor y tipos");
                                    Variables[Original] = ((Tokens[Pos].Valor, Tokens[Pos + 2].Tipo, Tokens[Pos + 2].Valor));
                                }
                                break;
                        }
                        
                    }
                    else if (Tokens[Pos + 2].Tipo == "Variable")
                    {
                        ImprimirPorDebug("Si a asignar es una variable");
                        switch (Valargs)
                        {
                            case "JIT":
                                if (Original == -1)
                                {
                                    ImprimirPorDebug("JIT si no existe la variable original");
                                    int Nuevo = -1;
                                    if (SiExisteTokens(2))
                                    {
                                        Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    }
                                    else
                                    {
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    if (Nuevo != -1)
                                    {
                                        ImprimirPorDebug("La variable nueva existe, añadir su tipo y valor");
                                        Variables.Add((Tokens[Pos].Valor, Variables[Nuevo].Tipo, Variables[Nuevo].Valor));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else
                                    {
                                        ImprimirPorDebug("La variable nueva no existe");
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }

                                }
                                else
                                {
                                    ImprimirPorDebug("JIT si existe la variable original");
                                    int Nuevo = -1;
                                    if (SiExisteTokens(2))
                                    {
                                        Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    }
                                    else
                                    {
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    if (Nuevo != -1)
                                    {
                                        ImprimirPorDebug("Si la variable nueva existe");
                                        Variables[Original] = ((Tokens[Pos].Valor, Variables[Nuevo].Tipo, Variables[Nuevo].Valor));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else
                                    {
                                        ImprimirPorDebug("La variable nueva no existe");
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }

                                }
                                break;
                            case "CMP":
                                if (Original == -1)
                                {
                                    ImprimirPorDebug("CMP la variable original no existe");
                                    int Nuevo = -1;
                                    if (SiExisteTokens(2))
                                    {
                                        Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    }
                                    else
                                    {
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    if (Nuevo != -1)
                                    {
                                        ImprimirPorDebug("Si la nueva variable existe");
                                        Variables.Add((Tokens[Pos].Valor, Variables[Nuevo].Tipo, Variables[Nuevo].Valor));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                        Compilado += "var " + Variables[Original].Nombre + " = " + Variables[Nuevo] + ";\n";
                      
                                    }
                                    else
                                    {
                                        ImprimirPorDebug("La nueva variable no existe");
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }

                                }
                                else
                                {
                                    ImprimirPorDebug("CMP La variable original existe");
                                    int Nuevo = -1;
                                    if (SiExisteTokens(2))
                                    {
                                        Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    }
                                    else
                                    {
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    if (Nuevo != -1)
                                    {
                                        ImprimirPorDebug("La variable nueva existe");
                                        Variables[Original] = ((Variables[Original].Nombre, Variables[Nuevo].Tipo, Variables[Nuevo].Valor));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                        Compilado += Variables[Original].Nombre + " = " + Variables[Nuevo].Nombre + ";\n";

                                    }
                                    else
                                    {
                                        ImprimirPorDebug("La nueva variable no existe");
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }

                                }
                                break;
                            case "Ambos":
                                if (Original == -1)
                                {
                                    ImprimirPorDebug("Ambos la variable original no existe");
                                    int Nuevo = -1;
                                    if (SiExisteTokens(2))
                                    {
                                        Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    }
                                    else
                                    {
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    if (Nuevo != -1)
                                    {
                                        ImprimirPorDebug("La variable nueva existe");
                                        Variables.Add((Tokens[Pos].Valor,Variables[Nuevo].Tipo, Variables[Nuevo].Valor));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                        Compilado += "var " + Variables[Original].Nombre + " = " + Variables[Nuevo].Nombre +";\n";
                                    }
                                    else
                                    {
                                        ImprimirPorDebug("La nueva variable no existe");
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }

                                }
                                else
                                {
                                    ImprimirPorDebug("Ambos la variable original existe");
                                    int Nuevo = -1;
                                    if (SiExisteTokens(2))
                                    {
                                        Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    }
                                    else
                                    {
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    if (Nuevo != -1)
                                    {
                                        ImprimirPorDebug("La variable nueva existe");
                                        Variables[Original] = ((Tokens[Pos].Valor, Variables[Nuevo].Tipo, Variables[Nuevo].Valor));
                                        Compilado += Variables[Original].Nombre + " = " + Variables[Nuevo].Nombre + ";\n";
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else
                                    {
                                        ImprimirPorDebug("La variable nueva no existe");
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }

                                }
                                break;
                            case "Depurar":
                                if (Original == -1)
                                {
                                    ImprimirPorDebug("Depurar si no existe la variable original");
                                    int Nuevo = -1;
                                    if (SiExisteTokens(2))
                                    {
                                        Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    }
                                    else
                                    {
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    if (Nuevo != -1)
                                    {
                                        ImprimirPorDebug("La variable nueva existe, añadir su tipo y valor");
                                        Variables.Add((Tokens[Pos].Valor, Variables[Nuevo].Tipo, Variables[Nuevo].Valor));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else
                                    {
                                        ImprimirPorDebug("La variable nueva no existe");
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }

                                }
                                else
                                {
                                    ImprimirPorDebug("Depurar si existe la variable original");
                                    int Nuevo = -1;
                                    if (SiExisteTokens(2))
                                    {
                                        Nuevo = IndexVar(Tokens[Pos + 2].Valor);
                                    }
                                    else
                                    {
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    if (Nuevo != -1)
                                    {
                                        ImprimirPorDebug("Si la variable nueva existe");
                                        Variables[Original] = ((Tokens[Pos].Valor, Variables[Nuevo].Tipo, Variables[Nuevo].Valor));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else
                                    {
                                        ImprimirPorDebug("La variable nueva no existe");
                                        if (Valargs == "Depurar")
                                        {
                                            //Aqui añade a la lista de errores el error
                                        }
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }

                                }
                                break;
                        }
                        
                    }
                    else if (Tokens[Pos + 2].Tipo == "Funcion")
                    {
                        ImprimirPorDebug("Asignado es una funcion");
                        if (Tokens[Pos + 2].Valor == "Leer")
                        {
                            ImprimirPorDebug("Asignado es Leer");
                            switch (Valargs)
                            {
                                case "JIT":
                                    if (Original == -1)
                                    {
                                        ImprimirPorDebug("JIT la variable original no existe");
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        string tem = Console.ReadLine();
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Variables.Add((Tokens[Pos].Valor, "String", tem));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else {
                                        ImprimirPorDebug("JIT la variable original existe");
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        string tem = Console.ReadLine();
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Variables[Original] = ((Tokens[Pos].Valor, "String", tem));
                                    }
                                    break;
                                case "CMP":
                                    if (Original == -1)
                                    {
                                        ImprimirPorDebug("CMP la variable original no existe");
                                        Compilado += "var " + Tokens[Pos].Valor + " = " + " System.Console.ReadLine();\n";
                                        Variables.Add((Tokens[Pos].Valor, "String", "{Vacio}"));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else
                                    {
                                        ImprimirPorDebug("CMP la variable original existe");
                                        Compilado += Variables[Original].Nombre + " = " + " System.Console.ReadLine();\n";
                                        Variables[Original] = ((Tokens[Pos].Valor, "String", "{Vacio}"));
                                    }
                                    break;
                                    
                                case "Ambos":
                                    if (Original == -1)
                                    {
                                        ImprimirPorDebug("Ambos la variable original no existe");
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        string tem = Console.ReadLine();
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Compilado += "var " + Tokens[Pos].Valor + " = " + " System.Console.ReadLine();\n";
                                        Variables.Add((Tokens[Pos].Valor, "String", tem));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else
                                    {
                                        ImprimirPorDebug("Ambos la variable original existe");
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        string tem = Console.ReadLine();
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Variables[Original] = ((Tokens[Pos].Valor, "String", tem));
                                        Compilado += Variables[Original].Nombre + " = " + " System.Console.ReadLine();\n";
                                    }
                                  
                                    break;
                                case "Depurar":
                                    if (Original == -1)
                                    {
                                        ImprimirPorDebug("Depurar la variable original no existe");
                                        string tem = "Lorem psium";
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Variables.Add((Tokens[Pos].Valor, "String", tem));
                                        Original = IndexVar(Tokens[Pos].Valor);
                                    }
                                    else
                                    {
                                        ImprimirPorDebug("Depurar la variable original existe");
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        string tem = "Lorem psium";
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Variables[Original] = ((Tokens[Pos].Valor, "String", tem));
                                    }
                                break;
                            }
                        }
                        else
                        {
                            ImprimirPorDebug("Es alguna otra funcion ademas de Leer");
                            if (Valargs == "Depurar")
                            {
                                //Aqui añade a la lista de errores el error
                            }
                            //Situacion de error
                            Environment.Exit(0);
                        }
                    }
                }
            }
        }
    }
}
