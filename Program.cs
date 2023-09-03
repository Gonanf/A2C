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
            string Vercion = "0.0.6";    //
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
                            Console.WriteLine("Arbys2Code Vercion " + Vercion + " Edicion " + Edicion);
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
                        Console.WriteLine("Arbys2Code Vercion " + Vercion + " Edicion " + Edicion);
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
                    Console.ReadLine();
                    Environment.Exit(0);
                }

            }//Si no se encontro el directorio (Muy raro)
            else
            {
                Console.WriteLine("[ERROR] No existe el archivo especificado.");
                Console.ReadLine();
                Environment.Exit(0);
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
            Perser("Depurar", "");
            //Si no tiene errores continuar normalmente
            if (Errores.Count == 0)
            {
                Perser(args[0], Dir.Replace(".arb", ""));
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
                "/",
                "(",
                ")",
                "<",
                ">",
                "!"};

                //Mientras que la posicion del programa en los caracteres del codigo no supere la cantidad existente
                while (Pos < Codigo.Length)
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
                                else if (Texto == "Verdadero" || Texto == "Falso")
                                {
                                    //Si es un boleeano
                                    Tokens.Add((Texto, "Booleano"));
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
                        case '|':
                            //Si inicia un string, guardar todo token que aun no se guardo
                            if (Texto != " " && Texto != string.Empty)
                            {
                                Console.WriteLine("Los tokens no deben estar juntos");
                                Console.ReadLine();
                                Environment.Exit(0);
                            }
                            Avanzar();
                            //Mientras que el usuario no cerro el string
                            while (Codigo[Pos] != '|')
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
                if (Funciones.Contains(Texto))
                {
                    //Añadir con tipo funcion
                    Tokens.Add((Texto, "Funcion"));
                    Texto = "";
                    Avanzar();
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
                    }
                    else if (Texto == "Verdadero" || Texto == "Falso")
                    {
                        //Si es un boleeano
                        Tokens.Add((Texto, "Booleano"));
                        Texto = "";
                        Avanzar();
                    }
                    else
                    {
                        //Si no, es una variable
                        Tokens.Add((Texto, "Variable"));
                        Texto = "";
                        Avanzar();
                    }

                }
                else
                {
                    //Si el texto esta vacio, reiniciar e ignorar
                    Texto = "";
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
                    if (Pos < Codigo.Length)
                    {
                        Pos++;
                    }
                    else
                    {
                        Pos = Codigo.Length;
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
                string Compilado = "using System;\n"+
                    "namespace Chaos {\n" +
                    "class A2C {\n" +
                    "static void Main(){\n";
                int Contador = 0;
                bool Invalido = false;
                List<int> UltVar = new List<int>();
                List<bool> Validacion = new List<bool>();
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
                                        Avanzar(1);

                                        Compilado += "System.Console.WriteLine(";
                                        (string Valor, string Tipo) Resultado = OperacionM();
                                        //Imprimir el siguiente token (A mejorar con la suma)
                                        if (Valargs == "JIT" || Valargs == "Ambos")
                                        {
                                            if (Invalido == false) { Console.WriteLine(Resultado.Valor); }
                                            
                                        }
                                        Compilado += ");\n";
                                    }
                                    else
                                    {
                                        Avanzar(1);
                                        AñadirError("No hay suficientes tokens para imprimir");
                                        //situacion de error
                                    }
                                    continue;
                                //Si es un token Leer con tipo Funcion
                                case "Leer":
                                    ImprimirPorDebug("Letyed");
                                    if (Invalido == true)
                                    {
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
                                    }

                                    Avanzar(1);
                                    continue;
                                case "Si":
                                    if (SiExisteTokens(5))
                                    {
                                        Avanzar(1);
                                        Contador++;
                                        Compilado += "if (";
                                        if (FuncionSi())
                                        {
                                                Validacion.Add(false);
                                            Avanzar(1);
                                        }
                                        else
                                        {
                                                Validacion.Add(true);
                                            Invalido = true;
                                            Avanzar(1);
                                        }
                                            UltVar.Add(Variables.Count());
                                            Compilado += ")\n{\n";

                                    }
                                    else
                                    {
                                        Avanzar(1);
                                        AñadirError("No existe tokens suficientes para la funcion Si");
                                        continue;
                                    }
                                    continue;
                                case "Mientras":
                                    //Por hacer
                                    continue;
                                case "Repetir":
                                    //Por hacer
                                    continue;
                                case ")":
                                    Compilado += "\n}\n";
                                    if (Contador == 1)
                                    {
                                        
                                        Contador--;
                                        Invalido = false;
                                        for (int i = UltVar.Last(); i < Variables.Count(); i++)
                                        {
                                            Variables.RemoveAt(i);
                                        }
                                        UltVar.RemoveAt(UltVar.Count()-1);
                                        Validacion.Remove(Validacion.Last());
                                    }
                                    else
                                    {
                                        if (Invalido == true && Validacion[0] == false)
                                        {
                                            Invalido = false;
                                            Contador--;
                                            Validacion.Remove(Validacion.Last());
                                            for (int i = UltVar.Last(); i < Variables.Count(); i++)
                                            {
                                                Variables.RemoveAt(i);
                                            }
                                            UltVar.RemoveAt(UltVar.Count() - 1);
                                        }
                                        else
                                        {
                                            Contador--;
                                        }
                                    }
                                    
                                    Avanzar(1);
                                    break;
                                default:
                                    Avanzar(1);
                                    AñadirError("no es una funcion valida");
                                    break;
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
                                    int PosOriginal = Pos;
                                    int Originla = IndexVar(Tokens[Pos].Valor);
                                    Avanzar(2);
                                    if(Originla != -1)
                                    {
                                        Compilado += Tokens[PosOriginal].Valor + " =";
                                        (string Valor, string Tipo) Result = OperacionM();
                                        Variables[Originla] = (Variables[Originla].Nombre,Result.Tipo, Result.Valor);
                                        Compilado += ";\n";
                                    }
                                    else
                                    {
                                        Compilado += "var " + Tokens[PosOriginal].Valor + " =";
                                        (string Valor, string Tipo) Result = OperacionM();
                                        Variables.Add((Tokens[PosOriginal].Valor, Result.Tipo, Result.Valor));
                                        Compilado += ";\n";
                                    }

                                }
                                //Termina el if del valor de token + 1
                                else
                                {
                                    Avanzar(1);
                                    AñadirError("Esta bien declarado la varible");
                                }

                            }//Si no hay tokens suficientes para declarar una variable
                            else
                            {
                                ImprimirPorDebug("No existe tokens en tokens + 2");
                                if (Valargs == "Depurar")
                                {
                                    AñadirError("No hay suficientes tokens para declarar la variable ");
                                    //aqui añade a la lista de errores este error
                                }
                                //situacion de error
                                Avanzar(1);
                                continue;
                            }
                            continue;
                        default:
                            Avanzar(1);
                            AñadirError("El tipo del token no es valido");
                            break;

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

                //Termina imprimir
                //Añadir error: una manera facil de añadir un mensaje de error
                ////////////////////
                //  AÑADIR ERROR  //
                ////////////////////
                void AñadirError(string mensaje)
                {
                    Errores.Add((mensaje, Pos));
                    Console.WriteLine(mensaje);
                    Console.ReadLine();

                }

                ////////////////////
                //   OPERACION M  //
                ////////////////////
                (string Valor, string Tipo) OperacionM()
                {
                    (string Valor, string Tipo) Primero = Tokens[Pos];
                    if (SiExisteTokens(2))
                    {
                        (string Valor, string Tipo) Sumador = Primero;
                        if (Tokens[Pos + 1].Tipo == "Funcion" && Operaciones.Contains(Tokens[Pos + 1].Valor))
                        {
                            Sumador = PrimeroVal();
                        }
                        else
                        {
                            return PrimeroVal();
                        }

                        while (Tokens[Pos].Tipo == "Funcion" && Operaciones.Contains(Tokens[Pos].Valor) && Pos < Tokens.Count())
                        {

                            switch (Tokens[Pos + 1].Tipo)
                            {
                                case "String":
                                    switch (Tokens[Pos].Valor)
                                    {
                                        case "+":
                                            Sumador = (Sumador.Valor + Tokens[Pos + 1].Valor, "String");
                                            Compilado += " " + Tokens[Pos].Valor + " " + "\"" + Tokens[Pos + 1].Valor + "\"";
                                            break;
                                        default:
                                            Avanzar(1);
                                            AñadirError("Solo se puede sumar string");
                                            break;
                                    }
                                    break;
                                case "Entero":
                                    if (Sumador.Tipo == "Entero")
                                    {
                                        Compilado += " " + Tokens[Pos].Valor + " " + Tokens[Pos + 1].Valor;
                                        switch (Tokens[Pos].Valor)
                                        {
                                            case "+":
                                                Sumador = ((Int32.Parse(Sumador.Valor) + Int32.Parse(Tokens[Pos + 1].Valor)).ToString(), "Entero");
                                                break;
                                            case "-":
                                                Sumador = ((Int32.Parse(Sumador.Valor) - Int32.Parse(Tokens[Pos + 1].Valor)).ToString(), "Entero");
                                                break;
                                            case "*":
                                                Sumador = ((Int32.Parse(Sumador.Valor) * Int32.Parse(Tokens[Pos + 1].Valor)).ToString(), "Entero");
                                                break;
                                            case "/":
                                                Sumador = ((Int32.Parse(Sumador.Valor) / Int32.Parse(Tokens[Pos + 1].Valor)).ToString(), "Entero");
                                                break;
                                            default:
                                                Avanzar(1);
                                                AñadirError("Caracter operacion invalido");
                                                break;
                                        }
                                    }
                                    break;
                                case "Variable":
                                    int Nuevo = IndexVar(Tokens[Pos + 1].Valor);
                                    if (Nuevo != -1)
                                    {
                                        if (Variables[Nuevo].Tipo == "Entero" && Sumador.Tipo == "Entero")
                                        {
                                            Compilado += " " + Tokens[Pos].Valor + " " + Variables[Nuevo].Valor;
                                            switch (Tokens[Pos].Valor)
                                            {
                                                case "+":
                                                    Sumador = ((Int32.Parse(Sumador.Valor) + Int32.Parse(Variables[Nuevo].Valor)).ToString(), "Entero");
                                                    break;
                                                case "-":
                                                    Sumador = ((Int32.Parse(Sumador.Valor) - Int32.Parse(Variables[Nuevo].Valor)).ToString(), "Entero");
                                                    break;
                                                case "*":
                                                    Sumador = ((Int32.Parse(Sumador.Valor) * Int32.Parse(Variables[Nuevo].Valor)).ToString(), "Entero");
                                                    break;
                                                case "/":
                                                    Sumador = ((Int32.Parse(Sumador.Valor) / Int32.Parse(Variables[Nuevo].Valor)).ToString(), "Entero");
                                                    break;
                                                default:
                                                    Avanzar(1);
                                                    AñadirError("Caracter operacion invalido");
                                                    break;
                                            }

                                        }
                                        else
                                        {
                                            switch (Tokens[Pos].Valor)
                                            {
                                                case "+":
                                                    Sumador = (Sumador.Valor + Variables[Nuevo].Valor, "String");
                                                    Compilado += " " + Tokens[Pos].Valor + " " + Variables[Nuevo].Nombre;
                                                    break;
                                                default:
                                                    Avanzar(1);
                                                    AñadirError("Solo se puede sumar string");
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Avanzar(1);
                                        AñadirError("No puedes asignar una variable no declarada");
                                    }
                                    break;
                                case "Funcion":
                                    Avanzar(1);
                                    AñadirError("No se puede operar con una funcion");
                                    break;
                                default:
                                    Avanzar(1);
                                    AñadirError("No se puede operar con otro tipo ademas de string, entero y funcion");
                                    break;
                            }
                            
                            if (SiExisteTokens(2))
                            {
                                Avanzar(2);
                            }
                            else
                            {
                                Avanzar(2);
                                return Sumador;
                            }
                        }
                        return Sumador;
                    }
                    else
                    {
                        return PrimeroVal();
                    }
                    (string Valor, string Tipo) PrimeroVal()
                    {
                        if (Primero.Tipo == "Variable")
                        {
                            int Nuevo = IndexVar(Primero.Valor);
                            if (Nuevo != -1)
                            {
                                Avanzar(1);
                                Compilado += " " + Variables[Nuevo].Nombre;
                                return (Variables[Nuevo].Valor, Variables[Nuevo].Tipo);
                            }
                            else
                            {
                                Avanzar(1);
                                AñadirError("Esta asignando una variable no declarada");
                            }
                        }
                        else if (Primero.Tipo == "String")
                        {
                            Compilado += " " + "\"" + Primero.Valor + "\"";
                            Avanzar(1);
                            return (Primero.Valor, Primero.Tipo);
                        }
                        else if (Primero.Tipo == "Funcion")
                        {
                            if (Primero.Valor == "Leer")
                            {
                                string temp = "loremp";
                                Avanzar(1);
                                if (Valargs == "JIT" || Valargs == "Ambos" && Invalido == false)
                                {
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    temp = Console.ReadLine();
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                Compilado += " System.Console.ReadLine()";
                                return (temp, "String");
                                
                            }
                            else
                            {
                                Avanzar(1);
                                AñadirError("La unica funcion que se le puede asignar a una variable es la funcion Leer");
                                return ("", "");
                            }
                        }
                        else if (Primero.Tipo == "Entero")
                        {
                            Avanzar(1);
                            Compilado += " " + Primero.Valor;
                            return (Primero.Valor, Primero.Tipo);
                        }
                        else
                        {
                            Avanzar(1);
                            switch (Primero.Valor)
                            {
                                case "Verdadero":
                                    Compilado += " " + "true";
                                    return (Primero.Valor, Primero.Tipo);
                                case "Falso":
                                    Compilado += " " + "false";
                                    return (Primero.Valor, Primero.Tipo);
                            }
                            
                            
                        }
                        Avanzar(1);
                        Compilado += " " + Primero.Valor;
                        return (Primero.Valor, Primero.Tipo);

                    }

                }
                bool FuncionSi()
                {
                    (string Valor, string Tipo) Primero = OperacionM();
                    string Funcion = Tokens[Pos].Valor;
                    switch (Funcion)
                    {
                        case "<":
                            Compilado += " <";
                            break;
                        case ">":
                            Compilado += " >";
                            break;
                        case "=":
                            Compilado += " ==";
                            break;
                        case "!":
                            Compilado += " !=";
                            break;
                        default:
                            Avanzar(1);
                            AñadirError("Operacion logica no valida");
                            break;
                    }
                    Avanzar(1);
                    (string Valor, string Tipo) Segundo = OperacionM();
                    switch (Funcion)
                    {
                        case "<":
                           if(Primero.Tipo == "Entero" && Segundo.Tipo == "Entero")
                            {
                                if (Int32.Parse(Primero.Valor) < Int32.Parse(Segundo.Valor))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                Avanzar(1);
                                AñadirError("No se puede operar logicamente con otro tipo ademas de entero");
                                return false;
                            }
                            break;
                        case ">":
                            if (Primero.Tipo == "Entero" && Segundo.Tipo == "Entero")
                            {
                                if (Int32.Parse(Primero.Valor) > Int32.Parse(Segundo.Valor))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                Avanzar(1);
                                AñadirError("No se puede operar logicamente con otro tipo ademas de entero");
                                return false;
                            }
                            break;
                        case "=":
                            if (Primero.Tipo == "Entero" && Segundo.Tipo == "Entero")
                            {
                                if (Int32.Parse(Primero.Valor) == Int32.Parse(Segundo.Valor))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                Avanzar(1);
                                AñadirError("No se puede operar logicamente con otro tipo ademas de entero");
                                return false;
                            }
                            break;
                        case "!":
                            if (Primero.Tipo == "Entero" && Segundo.Tipo == "Entero")
                            {
                                if (Int32.Parse(Primero.Valor) != Int32.Parse(Segundo.Valor))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                Avanzar(1);
                                AñadirError("No se puede operar logicamente con otro tipo ademas de entero");
                                return false;
                            }
                            break;
                        default:
                            Avanzar(1);
                            AñadirError("No es una opcion valida " + Funcion);
                            return false;
                            break;
                    }
                }
            }
            //Fin void Perser
        }
        //Fin main
    }
    //Fin clase
}
//Fin namespace
