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
        //El main sirve para obtener los argumentos luego del comando (ejemplo: Arbys2Code JIT Archivo)
        static void Main(string[] args)
        {
            //DATOS DE SOFTWARE////////////
            string Vercion = "0.0.4";    //
            string Edicion = "Estandar";   //
            //////////////////////////////
            //Lista de tokens, almacenaran las funciones y variable
            List<(string Valor, string Tipo)> Tokens = new List<(string Valor, string Tipo)>();
            //Lista de errores, almacenaran los errores para luego mostrarlos en una lista
            List<(string Nombre, int Token)> Errores = new List<(string Nombre, int Token)>();
            //Obtener el usuario
            var User = EnvironmentVariableTarget.User;
            //Obtener el PATH de las variables de entorno del usuario
            var Old = Environment.GetEnvironmentVariable("PATH", User);
            //Almacenar el PATH junto a nuestro directorio para posterior ingreso
            var New = Old + Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //Si no coloco ningun argumento (Para los que solo hicieron doble click en el .exe)
            if (args.Length == 0)
            {
                Console.WriteLine("Instalar Arbys2Code en la variable de entorno PATH?");
                Console.WriteLine("S/N");
                switch (Console.ReadLine().ToLower())
                {
                    case "s":
                        //Si dijo que si, verificamos si ya esta en las variables de entorno
                        if (Old.Contains(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)))
                        {
                            Console.WriteLine("Ya esta instalado en el sistema");
                            Console.WriteLine($"La ruta a Arbys2Code es:\n{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}");
                            Console.WriteLine("Arbys2Code Vercion " + Vercion);
                            Console.ReadLine();
                            Environment.Exit(0);
                            break;
                        }
                        //Si no esta, lo añadiremos
                        Environment.SetEnvironmentVariable("Path", New, User);
                        Console.WriteLine($"La ruta a Arbys2Code es:\n{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}");
                        Console.WriteLine("Listo!");
                        Console.ReadLine();
                        Environment.Exit(0);
                        break;
                    case "n":
                        //Si dijo que no, pues eso
                        Console.WriteLine($"La ruta a Arbys2Code es:\n{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}");
                        Console.WriteLine("Arbys2Code Vercion " + Vercion);
                        Console.ReadLine();
                        Environment.Exit(0);
                        break;
                    default:
                        //Si puso cualquier cosa
                        Console.WriteLine("No es un caracter valido");
                        Console.ReadLine();
                        Environment.Exit(0);
                        break;
                }

            //Si no hay argumentos suficientes
            }
            if (args.Length != 2)
            {
                Console.WriteLine("Arbys2Code necesita 2 argumentos, un metodo de ejecucion y un archivo .arb, coloque Arbys2Code Ayuda para mas informacion");
                Console.ReadLine();
                Environment.Exit(0);
            }
            //Si hay argumentos suficientes, iterar por el primeroa
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
            //Obtener el directorio del archivo (A2C tiene que estar ejecutandose sobre el en la terminal)
            string Dir = Path.GetFullPath(args[1]);
            //Si la direccion existe
            if (Dir != null)
            {
                //Verificar que existe un archivo con .arb
                if (System.IO.File.Exists(Dir + ".arb"))
                {
                    //Entonces añadirle
                    Dir += ".arb";
                }
                else //Si no lo encuentra
                {
                    Console.WriteLine("Archivo.arb no encontrado");
                }

            }//Si no se encontro el directorio (Muy raro)
            else
            {
                Console.WriteLine("[ERROR] No existe el archivo especificado.");
                Console.ReadLine();
            }
            //Empezar el tokenizador, devolvera tokens
            Tokenizador();
            //DEBUG: ver los tokens que procesa el tokenizador
            Console.ForegroundColor = ConsoleColor.Green;
            Tokens.ForEach(tok =>
            {
                if (Edicion == "Debug")
                {
                    Console.WriteLine(tok.ToString());
                }
            });
            Console.ForegroundColor = ConsoleColor.White;
            //Verificar que el codigo no tenga errores
            Perser("Depurar","");
            //Si no tiene errores continuar normalmente
            if (Errores.Count == 0)
            {
                Perser(args[0],Dir.Replace(".arb",""));
            }
            else //Si tiene errores mostrarlos
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
            //Empezando tokenizador
            void Tokenizador()
            {
                //Obtener el texto del archivo especificado
                string Codigo = string.Join(" ", File.ReadAllLines(Dir));
                int Pos = 0;
                string Texto = "";
                string NumVal = "1234567890";
                //Funciones validas
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
                //Mientras que la posicion del programa en los caracteres del codigo no supere la cantidad existente
                while (Pos < Codigo.Length - 1)
                {
                    switch (Codigo[Pos])
                    {
                        //Si es un espacio el caracter
                        case ' ':
                            //Si es una funcion
                            if (Funciones.Contains(Texto))
                            {
                                //Añadir con tipo funcion
                                Tokens.Add((Texto, "Funcion"));
                                Texto = "";

                                Avanzar();
                                break;
                            }
                            //Si no es una funcion y no esta vacio
                            else if (Texto != " " && Texto != string.Empty)
                            {
                                //Si el texto es numerico
                                if (EsNumerico())
                                {
                                    //Añadirlo como entero
                                    Tokens.Add((Texto, "Entero"));
                                    Texto = "";
                                    Avanzar();
                                    break;
                                }
                                else
                                {
                                    //Si no, es una variable
                                    Tokens.Add((Texto, "Variable"));
                                    Texto = "";
                                    Avanzar();
                                    break;
                                }

                            }
                            else
                            {
                                //Si el texto esta vacio, reiniciar e ignorar
                                Avanzar();
                                Texto = "";
                            }
                            break;
                        case '<':
                            //Si inicia un string, guardar todo token que aun no se guardo
                            if (Texto != " " && Texto != string.Empty)
                            {
                                Tokens.Add((Texto, "Variable"));
                                Texto = "";
                               
                            }
                            Avanzar();
                            //Mientras que el usuario no cerro el string
                            while (Codigo[Pos] != '>')
                            {
                                //Obtener y guardar sus caracteres
                                Texto += Codigo[Pos];
                                Avanzar();
                            }
                            //Cuando termina, lo guardamos como string
                            Tokens.Add((Texto, "String"));
                            Avanzar();
                            Texto = "";
                            break;
                        default:
                            //Si es un caracter normal, guardarlo hasta que se presente una oportunidad de guardado
                            Texto += Codigo[Pos];
                            Avanzar();
                            break;
                    }
                    //Termina switch

                }
                //Termina el while, añadir el ultimo caracter o guardar la ultima variable
                if (Codigo[Pos] != ' ' && Codigo[Pos] != '<' && Codigo[Pos] != '>' && Texto != "\n" && Texto != "\r")
                {
                    Texto += Codigo[Pos];
                }
                //Si es una funcion
                if (Funciones.Contains(Texto))
                {
                    //Añadir con tipo funcion
                    Tokens.Add((Texto, "Funcion"));
                    Texto = "";

                    Avanzar();
                }
                //Si es numerico
                if (EsNumerico())
                {
                    //Añadirlo como entero
                    Tokens.Add((Texto, "Entero"));
                    Texto = "";
                    Avanzar();
                }
                //Si no concuerda, guardar como variable del usuario
                if (Texto != " " && Texto != string.Empty)
                {
                    Tokens.Add((Texto, "Variable"));
                }
                //FUNCION: detecta si un string solamente contiene numeros
                bool EsNumerico()
                {
                    foreach (char c in Texto)
                    {
                        if (c < '0' || c > '9')
                        {
                            return false;
                        }
                    }
                    return true;
                }
                //FUNCION: verifica que la posicion no se pase de la cantidad de caracteres en el codigo
                void Avanzar()
                {
                    if (Pos < Codigo.Length - 1)
                    {
                        Pos++;
                    }
                }
                //Termina funcion
            }
            //Termina tokenizador

            //Empieza Perser, este obtendra los tokens y los analizara para encontrarle sentido
            void Perser(string Valargs, string Valnom)
            {
                //Operaciones validas
                string Operaciones = "+-*/";
                int Pos = 0;
                //Las primeras lineas en la traduccion de A2C a C#
                string Compilado = "using System;\n" +
                    "namespace Chaos {\n" +
                    "class A2C {\n" +
                    "static void Main(){\n";
                //CodeDOM
                CSharpCodeProvider CodProv = new CSharpCodeProvider();
                CompilerParameters Param = new CompilerParameters();
                //Generar .exe
                Param.GenerateExecutable = true;
                //Generar exe con el nombre del archivo
                Param.OutputAssembly = Valnom + ".exe";
                //Generarse en un archivo en el almacenamiento
                Param.GenerateInMemory = false;
                //Añadir la libreria del sistema (Para console.writeLine y demas)
                Param.ReferencedAssemblies.Add("system.dll");
                //Lista de variables del usuario
                List<(string Nombre, string Tipo, string Valor)> Variables = new List<(string Nombre, string Tipo, string Valor)>();
                //Mientras que la posicion no se pase de la cantidad de tokens
                while (Pos < Tokens.Count())
                {
                    //Fijar instrucciones en casos de la posicion actual del token
                    switch (Tokens[Pos].Tipo)
                    {
                        //Si es una funcion
                        case "Funcion":
                            ImprimirPorDebug("Es funcion");
                            switch (Tokens[Pos].Valor)
                            {
                                //Si es un token Imprimir con tipo Funcion
                                case "Imprimir":
                                    ImprimirPorDebug("Es imprimir");
                                    //Si hay tokens suficientes
                                    if (SiExisteTokens(1))
                                    {
                                        ImprimirPorDebug("Imprimiendo");
                                        //Imprimir el siguiente token (A mejorar con la suma)
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
                                //Si es un token Leer con tipo Funcion
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
                                    //Por hacer
                                    continue;
                                case "Mientras":
                                    //Por hacer
                                    continue;
                                case "Repetir":
                                    //Por hacer
                                    continue;

                            }
                            break;
                        //Si el token es de tipo variable
                             ////////////////////
                            //ASIGNAR VARIABLE//
                           ////////////////////
                        case "Variable":
                            ImprimirPorDebug("Es variable");
                            //Si tiene tokens suficientes (Para el = y un asignador)
                            if (SiExisteTokens(2))
                            {
                                //Si el siguiente token es una funcion
                                if (Tokens[Pos + 1].Tipo == "Funcion" && Tokens[Pos + 1].Valor == "=")
                                {
                                    (string Valor, string Tipo) Resultado;
                                    int PosNombre = Pos;
                                    int Original = IndexVar(Tokens[Pos].Valor);
                                    if (Original == -1)
                                    {
                                        Compilado += "var " + Tokens[Pos].Valor + " = ";
                                        Resultado = OperarVVM(Tokens[Pos + 2], Pos + 2);
                                        Variables.Add((Tokens[PosNombre].Valor,Resultado.Tipo,Resultado.Valor));
                                    }
                                    else
                                    {
                                        Compilado += Tokens[Pos].Valor + " = ";
                                        Resultado = OperarVVM(Tokens[Pos + 2], Pos + 2);
                                        Variables[Original] = ((Variables[Original].Nombre, Resultado.Tipo, Resultado.Valor));
                                    }
                                    Compilado += ";\n";
                                     


                                }
                                //Termina el if del valor de token + 1

                            }//Si no hay tokens suficientes para declarar una variable
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
                    //Termina switch principal
                    continue;
                }
                //Termina while principal
                ////////////////////
                //    COMPILAR    //
                ////////////////////
                if (Valargs == "CMP" || Valargs == "Ambos")
                {
                    //Empezar a compilar
                    ImprimirPorDebug("Empezando a compilar");
                    //Mostrar el codigo traducido
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Compilado += "\nSystem.Console.ReadLine();}\n}\n}";
                    Console.WriteLine(Compilado);
                    //Compilar
                    CompilerResults Results = CodProv.CompileAssemblyFromSource(Param, Compilado);
                    ImprimirPorDebug("Compilado");
                    //Mostrar errores
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
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////
                //     DEBUG      //
                ////////////////////
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
                ////////////////////
                //SI EXISTE TOKENS//
                ////////////////////
                //Si existe los tokens despues de la suma
                bool SiExisteTokens(int num)
                {
                    if (Pos + num < Tokens.Count())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                //Aumentar en index la posicion
                ////////////////////
                //    AVANZAR     //
                ////////////////////
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
                //Obtener el index de la variable a buscar
                ////////////////////
                //   INDEX VAR    //
                ////////////////////
                int IndexVar(string Nombre)
                {
                    //Si hay variables
                    if (Variables.Count > 0)
                    {
                        ImprimirPorDebug("Hay variables");
                        //iterar por todas las variables
                        for (int i = 0; i < Variables.Count; i++)
                        {
                            ImprimirPorDebug("Buscando variable: " + Nombre + " Actual: " + Variables[i]);
                            //Comparar con el buscado
                            if (Variables[i].Nombre == Nombre)
                            {
                                //Si es el buscado, entregar el index
                                ImprimirPorDebug("Encontrado " + Variables[i] + " index " + i);
                                return i;
                            }
                            //sino seguir buscando

                        }
                        //Si no se encuentra la variable a buscar
                        ImprimirPorDebug("No se encontro la variable (No se declaro)");
                        //Situacion de error
                        return -1;
                    }
                    else
                    {
                        //Si no hay ningun valor en la lista de variables
                        ImprimirPorDebug("Intentando llamar a una variable en una lista vacia");
                        //Situacion de error
                        return -1;
                    }
                }
                //Fin IndexVar
                //Imprime valores
                ////////////////////
                //    IMPRIMIR    //
                ////////////////////
                void Imprimir(string Valor, string Tipo)
                {
                    //Si el valor a imprimir es una variable
                    if (Tipo == "Variable")
                    {
                        ImprimirPorDebug("Es variable");
                        //Verificar que exista
                        int Actual = IndexVar(Valor);
                        if (Actual != -1)
                        {
                            //Si existe
                            ImprimirPorDebug(Variables[Actual] + " Existe");
                            //Imprimir su valor
                            switch (Valargs)
                            {
                                case "JIT":
                                    ImprimirPorDebug("JIT imprimiendo variable");
                                    Console.WriteLine(Variables[Actual].Valor);
                                    break;
                                case "CMP":
                                    ImprimirPorDebug("CMP compilando imprimiendo variable");
                                    Compilado += " System.Console.WriteLine(" + Variables[Actual].Nombre + "); \n";
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
                            //Si no existe dar error
                            ImprimirPorDebug("No existe la variable a imprimir");
                            //Situacion de error
                            Environment.Exit(0);
                        }
                    //Si el token a imprimir no es variable
                    }
                    else
                    {
                        //Debe ser o Entero o String
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
                    //Situacion de error
                }
                //Termina imprimir
                //Añadir error: una manera facil de añadir un mensaje de error
                ////////////////////
                //  AÑADIR ERROR  //
                ////////////////////
                void AñadirError(string mensaje)
                {
                    Errores.Add((mensaje, Pos));
                }
                //OPERACION VVM
                (string Valor, string Tipo) OperarVVM((string Valor, string Tipo) V1, int Token)
                {
                    if (!Operaciones.Contains(Tokens[Token + 1].Valor))
                    {
                        switch (V1.Tipo)
                        {
                            case "Funcion":
                                if (V1.Valor == "Leer")
                                {
                                    Compilado += " System.Console.ReadLine()";
                                    if (Valargs == "JIT" || Valargs == "Ambos")
                                    {
                                        Avanzar(Token + 1 - Pos);
                                        return (Console.ReadLine(), "String");
                                    }
                                    else
                                    {
                                        Avanzar(Token + 1 - Pos);
                                        return ("Loremp", "String");
                                        
                                    }
                                }
                                break;
                            case "Variable":
                                int Original = IndexVar(V1.Valor);
                                if (Original == -1)
                                {
                                    //Situacion de error
                                    Environment.Exit(0);
                                }
                                else
                                {
                                    Compilado += " " + Variables[Original].Nombre;
                                    Avanzar(Token + 1 - Pos);
                                    return (Variables[Original].Valor, Variables[Original].Tipo);
                                }
                                break;
                        }
                        return V1;
                    }
                    else
                    {
                        Token++;
                        (string Valor, string Tipo) Sumados = V1;
                        switch (V1.Tipo)
                        {
                            case "String":
                                Compilado += " " + "\"" + V1.Valor + "\"";
                                break;
                            case "Funcion":
                                if (V1.Valor == "Leer")
                                {
                                    Compilado += " System.Console.ReadLine()";
                                    if (Valargs == "JIT" || Valargs == "Ambos")
                                    {
                                        string temp = Console.ReadLine();
                                        Sumados = (temp, "String");
                                    }
                                }
                                else
                                {
                                    //Situacion de error
                                    Environment.Exit(0);
                                }
                                break;
                            case "Variable":
                                int Original = IndexVar(V1.Valor);
                                if (Original != -1)
                                {
                                    Compilado += " " + V1.Valor;
                                    Sumados = (Variables[Original].Valor, Variables[Original].Tipo);
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("No existe var " + V1);
                                    //Caso de error
                                    Environment.Exit(0);
                                }
                                break;
                            default:
                                Compilado += " " + V1.Valor;
                                break;
                        }
                        while (Operaciones.Contains(Tokens[Token].Valor))
                        {
                            Compilado += " " + Tokens[Token].Valor + " ";
                            switch (Tokens[Token + 1].Tipo)
                            {
                                case "Funcion":
                                    if (Tokens[Token + 1].Valor == "Leer")
                                    {
                                        //Situacion de error
                                        Environment.Exit(0);
                                    }
                                    break;
                                case "Variable":
                                    int Nuevo = IndexVar(Tokens[Token + 1].Valor);
                                    if (Nuevo == -1)
                                    {
                                        //Caso de error
                                        Environment.Exit(0);
                                    }
                                    else if (Sumados.Tipo == "Entero" && Variables[Nuevo].Tipo == "Entero")
                                    {
                                        Compilado += " " + Variables[Nuevo].Nombre;
                                        switch (Tokens[Token].Valor)
                                        {
                                            case "+":
                                                Sumados = ((Int32.Parse(Sumados.Valor) + Int32.Parse(Variables[Nuevo].Valor)).ToString(), "Entero");
                                                break;
                                            case "-":
                                                Sumados = ((Int32.Parse(Sumados.Valor) - Int32.Parse(Variables[Nuevo].Valor)).ToString(), "Entero");
                                                break;
                                            case "*":
                                                Sumados = ((Int32.Parse(Sumados.Valor) * Int32.Parse(Variables[Nuevo].Valor)).ToString(), "Entero");
                                                break;
                                            case "/":
                                                Sumados = ((Int32.Parse(Sumados.Valor) / Int32.Parse(Variables[Nuevo].Valor)).ToString(), "Entero");
                                                break;
                                            default:
                                                //Caso de error
                                                Environment.Exit(0);
                                                break;
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        Compilado += " " + Variables[Nuevo].Nombre;
                                        switch (Tokens[Token].Valor)
                                        {
                                            case "+":
                                                
                                                Sumados = (Sumados.Valor + Variables[Nuevo].Valor, "String");
                                                break;
                                            default:
                                                //Caso de error
                                                Environment.Exit(0);
                                                break;
                                        }

                                    }
                                    break;
                                case "Entero":
                                    if (Sumados.Tipo == "Entero")
                                    {
                                        Compilado += " " + Tokens[Token + 1].Valor;
                                        switch (Tokens[Token].Valor)
                                        {
                                            case "+":
                                                Sumados = ((Int32.Parse(Sumados.Valor) + Int32.Parse(Tokens[Token + 1].Valor)).ToString(), "Entero");
                                                break;
                                            case "-":
                                                Sumados = ((Int32.Parse(Sumados.Valor) - Int32.Parse(Tokens[Token + 1].Valor)).ToString(), "Entero");
                                                break;
                                            case "*":
                                                Sumados = ((Int32.Parse(Sumados.Valor) * Int32.Parse(Tokens[Token + 1].Valor)).ToString(), "Entero");
                                                break;
                                            case "/":
                                                Sumados = ((Int32.Parse(Sumados.Valor) / Int32.Parse(Tokens[Token + 1].Valor)).ToString(), "Entero");
                                                break;
                                            default:
                                                //Caso de error
                                                Environment.Exit(0);
                                                break;
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        Compilado += " " + "\"" + Tokens[Token + 1].Valor + "\"";
                                        switch (Tokens[Token].Valor)
                                        {
                                            case "+":
                                                Sumados = (Sumados.Valor + Tokens[Token + 1].Valor, "String");
                                                break;
                                            default:
                                                //Caso de error
                                                Environment.Exit(0);
                                                break;
                                        }
                                    }
                                    break;
                                default:
                                    Compilado += " " + "\"" + Tokens[Token + 1].Valor + "\"";
                                    switch (Tokens[Token].Valor)
                                    {
                                        case "+":
                                            Sumados = (Sumados.Valor + Tokens[Token + 1].Valor, "String");
                                            break;
                                        default:
                                            //Caso de error
                                            Environment.Exit(0);
                                            break;
                                    }
                                    break;
                            }
                            Token += 2;

                        }
                        Avanzar(Token - Pos);
                        return Sumados;
                    }
                }
                //Termina OperarVVM
                }
            //Fin void Perser
            }
        //Fin main
        }
    //Fin clase
    }
//Fin namespace
