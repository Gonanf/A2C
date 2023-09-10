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
            string Vercion = "0.0.10";    //
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
                Console.WriteLine("Instalar A2C en la variable de entorno PATH?");
                Console.WriteLine("S/N");
                switch (Console.ReadLine().ToLower())
                {
                    case "s":
                        //Si dijo que si, verificamos si ya esta en las variables de entorno
                        if (Old.Contains(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)))
                        {
                            Console.WriteLine("Ya esta instalado en el sistema");
                            Console.WriteLine($"La ruta a A2C es:\n{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}");
                            Console.WriteLine("A2C Vercion " + Vercion + " Edicion " + Edicion);
                            Console.ReadLine();
                            Environment.Exit(0);
                            break;
                        }
                        //Si no esta, lo añadiremos
                        Environment.SetEnvironmentVariable("Path", New, User);
                        Console.WriteLine($"La ruta a A2C es:\n{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}");
                        Console.WriteLine("Listo!");
                        Console.ReadLine();
                        Environment.Exit(0);
                        break;
                    case "n":
                        //Si dijo que no, pues eso
                        Console.WriteLine($"La ruta a A2C es:\n{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}");
                        Console.WriteLine("A2C Vercion " + Vercion + " Edicion " + Edicion);
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
                Console.WriteLine("A2C necesita 2 argumentos, un metodo de ejecucion y un archivo .arb, coloque Arbys2Code Ayuda para mas informacion");
                Console.WriteLine("A2C " + String.Join(" ", args));
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
                    Console.WriteLine("A2C es un lenguaje de programacion hecho por estudiantes de programacion de una tecnica\n\nSINTAXIS: Es literal, tiene que estar perfectamente escrito y tener espacios por cada accion o habra errores\n\nFunciones:\nImprimir -> Imprime en pantalla\nLeer -> Lee lo que el usuario le coloque en la consola\n{Leer} -> Toma los numeros que coloque el usuario en la consola\nSi -> Es un condicional, verifica si es verdadero\n\nTipos de datos:\n|Texto| -> String, tipo de valor que almacena caracteres\n3123 -> Para los numeros no se necesitan ningun identificador\nVariable -> Las variables no se necesitan declarar con un tipo, no deben ser ninguna funcion\nVerdadero -> Booleano verdadero\nFalso -> Booleano falso\n\nOPERADORES:\n\nOperadores matematicos:\n+ -> Suma, lo soportan los enteros y strings\n- -> Resta, lo soportan los enteros\n* -> Multiplicacion, lo soportan los enteros\n/ -> Divicion, lo soportan los enteros\n\nOperadores logicos:\n< -> Menor a, lo soportan los enteros\n> -> Mayor a, lo soportan los enteros\n= -> Igual a, los soportan todos los tipos de datos\n! -> Distinto a, lo soportan todos los tipos de datos\n\nESTRUCTURA:\nImprimir |Texto| + Variable -> Este es un ejemplo de imprimir\nVariable = Leer -> Almacena en Variable lo que el usuario coloque en la consola\nVariable = {Leer} -> Almacena en Variable los numeros que el usuario coloque en la consola\nSi 1 = 2 ( Imprimir |Esto no puede pasar!| ) -> Este es un ejemplo, verifica si 1 es igual a 2, como es falso no ejecutara lo que este dentro de las parentesis\n\nEn caso de encontrar errores reportelo aqui: https://github.com/Gonanf/A2C");
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

                if (args[0] == "Depurar")
                {
                    Console.WriteLine("\nSin errores detectados\n");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                Perser(args[0], Dir.Replace(".arb", ""));
            }
            else //Si tiene errores mostrarlos
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(Errores[0].Nombre + " Con token " + Tokens[Errores[0].Token]);
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
                "!",
                "Y",
                "O",
                "{Leer}"};

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
                string Compilado = "using System;\n" +
                    "namespace Chaos {\n" +
                    "class A2C {\n" +
                    "static void Main(){\n";
                List<int> Posicion = new List<int>();
                List<int> LocVars = new List<int>();
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
                                        ImprimirPorDebug("Empezando a operar matematicamente");
                                        (string Valor, string Tipo) Resultado = OperacionM();
                                        ImprimirPorDebug(Resultado.Valor + "/" + Resultado.Tipo);
                                        //Imprimir el siguiente token (A mejorar con la suma)
                                        if (Valargs != "CMP" && Valargs != "Depurar")
                                        {
                                            if (!Validacion.Contains(false)) { Console.WriteLine(Resultado.Valor); }

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
                                    if (!Validacion.Contains(false))
                                    {
                                        if (Valargs != "Depurar" && Valargs != "CMP")
                                        {
                                            ImprimirPorDebug("Ambos Leyendo y Compilando");
                                            Console.ReadLine();

                                        }
                                        Compilado += "System.Console.ReadLine();\n";

                                    }
                                    ImprimirPorDebug("Leyendo");
                                    Avanzar(1);
                                    continue;
                                case "{Leer}":
                                    ImprimirPorDebug("Letyed");
                                    if (!Validacion.Contains(false))
                                    {
                                        if (Valargs != "Depurar" && Valargs != "CMP")
                                        {
                                            ImprimirPorDebug("Ambos Leyendo y Compilando");
                                            Console.ReadLine();

                                        }
                                        Compilado += "System.Console.ReadLine();\n";

                                    }
                                    ImprimirPorDebug("Leyendo");
                                    Avanzar(1);
                                    continue;
                                case "Si":
                                    if (SiExisteTokens(5))
                                    {
                                        Avanzar(1);
                                        Compilado += "if (";
                                        if (OperacionL())
                                        {
                                            ImprimirPorDebug("VERDEROOOOOOOOOOOOOOO");
                                            Validacion.Add(true);
                                        }
                                        else
                                        {
                                            Validacion.Add(false);
                                        }
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
                                    if (Validacion.Count > 0)
                                    {
                                        Validacion.RemoveAt(Validacion.Count() - 1);
                                    }
                                    Avanzar(1);
                                    break;
                                default:
                                    AñadirError("no es una funcion valida");
                                    Avanzar(1);
                                    break;
                                case "(":
                                    Avanzar(1);
                                    break;
                            }
                            ImprimirPorDebug("Wahtafas");
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
                                    if (Originla != -1)
                                    {
                                        Compilado += Tokens[PosOriginal].Valor + " =";
                                        (string Valor, string Tipo) Result = OperacionM();
                                        if (!Validacion.Contains(false))
                                        {
                                            ImprimirPorDebug("Asignando VARIAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                                            Variables[Originla] = (Variables[Originla].Nombre, Result.Tipo, Result.Valor);
                                        }
                                        Compilado += ";\n";
                                    }
                                    else
                                    {
                                        Compilado += "var " + Tokens[PosOriginal].Valor + " =";
                                        (string Valor, string Tipo) Result = OperacionM();
                                        if (!Validacion.Contains(false))
                                        {
                                            ImprimirPorDebug("Asignando VARIAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                                            Variables.Add((Tokens[PosOriginal].Valor, Result.Tipo, Result.Valor));
                                        }
                                        Compilado += ";\n";
                                    }

                                }
                                //Termina el if del valor de token + 1
                                else
                                {
                                    Avanzar(1);
                                    AñadirError("No esta bien declarado la varible " + Tokens[Pos]);
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
                            ImprimirPorDebug(Tokens[Pos + 1].ToString() + "UWU");
                            AñadirError("El tipo del token no es valido");
                            Avanzar(1);
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
                    Compilado += "\nSystem.Console.ReadLine();\n}\n}\n}";
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
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(texto + " " + Tokens[Pos] + " " + Pos);
                            Console.WriteLine("No hay espacio!!!!!!!!!!");
                            Console.ForegroundColor = ConsoleColor.White;
                            Pos = Tokens.Count();
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
                    ImprimirPorDebug("A avanzar");
                    if (Pos + num < Tokens.Count())
                    {
                        Pos = Pos + num;
                        ImprimirPorDebug("Avanzando ");
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
                    Console.WriteLine(mensaje + " " + Tokens[Pos] + " " + Pos);
                    Console.ReadLine();

                }

                ////////////////////
                //   OPERACION M  //
                ////////////////////
                //Realiza operaciones matematicas infinitas (+, -, *, /)
                (string Valor, string Tipo) OperacionM()
                {
                    ImprimirPorDebug("Operando");
                    //Guarda el primer valor a operar, el que no se le añade antes un operador (Es la exepcion)
                    (string Valor, string Tipo) Primero = Tokens[Pos];
                    //Si hay tokens suficientes
                    if (SiExisteTokens(2))
                    {
                        ImprimirPorDebug("Existen tokens para operar");
                        //Esto esta demas, pero que importa
                        (string Valor, string Tipo) Sumador = Primero;
                        //Si el siguiente token es un operador matematico
                        if (Tokens[Pos + 1].Tipo == "Funcion" && Operaciones.Contains(Tokens[Pos + 1].Valor))
                        {
                            ImprimirPorDebug("Sumando mas");
                            //Operar y guardar los primeros valores
                            Sumador = PrimeroVal();
                        }
                        else //Si no es un operador matematico
                        {
                            ImprimirPorDebug("No hay tokens de operacion");
                            //Devolver la operacion de los tokens
                            return PrimeroVal();
                        }
                        //Si sigue habiendo operadores, empezar la operacion infinita
                        while (Tokens[Pos].Tipo == "Funcion" && Operaciones.Contains(Tokens[Pos].Valor) && Pos < Tokens.Count())
                        {
                            switch (Tokens[Pos + 1].Tipo)
                            {
                                //Si es un string el token a operar
                                case "String":
                                    switch (Tokens[Pos].Valor)
                                    {
                                        //Solo se pueden sumar strings
                                        case "+":
                                            ImprimirPorDebug("Esta sumando string " + Sumador);
                                            Sumador = (Sumador.Valor + Tokens[Pos + 1].Valor, "String");
                                            Compilado += " " + Tokens[Pos].Valor + " " + "\"" + Tokens[Pos + 1].Valor + "\"";
                                            break;
                                        default:
                                            ImprimirPorDebug("Invalida operacion string " + Sumador);
                                            AñadirError("Solo se puede sumar string");
                                            Avanzar(1);
                                            break;
                                    }
                                    break;
                                    //Si es un entero el token a operar
                                case "Entero":
                                    //Si la suma hasta ahora esta siendo entero
                                    if (Sumador.Tipo == "Entero")
                                    {
                                        Compilado += " " + Tokens[Pos].Valor + " " + Tokens[Pos + 1].Valor;
                                        switch (Tokens[Pos].Valor) //Los enteros pueden hacer todas las operaciones
                                        {
                                            //Convertir de string a entero, operar y luego convertirlo a string de nuevo y guardar
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

                                                AñadirError("Caracter operacion invalido");
                                                Avanzar(1);
                                                break;
                                        }
                                    }
                                    break;
                                //Si el siguiente token a operar es una variable
                                case "Variable":
                                    //Buscar la variable en la lista de variables
                                    int Nuevo = IndexVar(Tokens[Pos + 1].Valor);
                                    ImprimirPorDebug("Sumando var");
                                    if (Nuevo != -1)
                                    {
                                        //Si la variable existe
                                        if (Variables[Nuevo].Tipo == "Entero" && Sumador.Tipo == "Entero")
                                        {
                                            //Si ambos, la operacion hasta ahora y la nueva variable son enteros
                                            Compilado += " " + Tokens[Pos].Valor + " " + Variables[Nuevo].Nombre;
                                            switch (Tokens[Pos].Valor)
                                            {
                                                //Convertir de string a entero, operar y luego convertirlo a string de nuevo y guardar
                                                case "+":
                                                    Sumador = ((Int32.Parse(Variables[Nuevo].Valor) + Int32.Parse(Sumador.Valor)).ToString(), "Entero");
                                                    break;
                                                case "-":
                                                    Sumador = ((Int32.Parse(Variables[Nuevo].Valor) - Int32.Parse(Sumador.Valor)).ToString(), "Entero");
                                                    break;
                                                case "*":
                                                    Sumador = ((Int32.Parse(Variables[Nuevo].Valor) * Int32.Parse(Sumador.Valor)).ToString(), "Entero");
                                                    break;
                                                case "/":
                                                    Sumador = ((Int32.Parse(Variables[Nuevo].Valor) / Int32.Parse(Sumador.Valor)).ToString(), "Entero");
                                                    break;
                                                default:
                                                    AñadirError("Caracter operacion invalido");
                                                    Avanzar(1);
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            //Si alguno no es entero, tratarlos como string
                                            switch (Tokens[Pos].Valor)
                                            {
                                                case "+":
                                                    Sumador = (Sumador.Valor + Variables[Nuevo].Valor, "String");
                                                    Compilado += " " + Tokens[Pos].Valor + " " + Variables[Nuevo].Nombre;
                                                    break;
                                                default:
                                                    AñadirError("Solo se puede sumar string");
                                                    Avanzar(1);
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Si no existe la variable a operar
                                        AñadirError("No puedes asignar una variable no declarada");
                                        Avanzar(1);
                                    }
                                    break;
                                case "Funcion":
                                    //No se puede sumar con Leer o {Leer} por que aun no se declaro su contenido
                                    AñadirError("No se puede operar con una funcion");
                                    Avanzar(1);
                                    break;
                                default:
                                    //Si trata de operar con Booleanos
                                    AñadirError("No se puede operar con otro tipo ademas de string, entero y funcion");
                                    Avanzar(1);
                                    break;
                            }
                            //Termina el switch del siguiente token
                            if (SiExisteTokens(2))
                            {
                                //Si se puede avanzar, avanzar
                                Avanzar(2);
                            }
                            else
                            {
                                //Si no se puede avanzar, terminar
                                Avanzar(2);
                                return Sumador;
                            }
                        }
                        //Terminar de todas formas
                        return Sumador;
                    }
                    //No hay tokens suficientes para la operacion infinita
                    else
                    {
                        //Devolver el primer valor convertido
                        return PrimeroVal();
                    }
                    //Situacion de exepcion del primer valor
                    (string Valor, string Tipo) PrimeroVal()
                    {
                        //Si el primero es una variable
                        if (Primero.Tipo == "Variable")
                        {
                            //Buscar la variable
                            int Nuevo = IndexVar(Primero.Valor);
                            if (Nuevo != -1)
                            {
                                //Si la variable existe
                                Avanzar(1);
                                //Compilar y devolver
                                Compilado += " " + Variables[Nuevo].Nombre;
                                return (Variables[Nuevo].Valor, Variables[Nuevo].Tipo);
                            }
                            else
                            {
                                //Si no existe la variable a asignar
                                AñadirError("Esta asignando una variable no declarada");
                                Avanzar(1);
                            }
                        }
                        //Si el primero es un string
                        else if (Primero.Tipo == "String")
                        {
                            //Compilar y devolver
                            Compilado += " " + "\"" + Primero.Valor + "\"";
                            Avanzar(1);
                            return (Primero.Valor, Primero.Tipo);
                        }
                        //Si el primero es una funcion
                        else if (Primero.Tipo == "Funcion")
                        {
                            //Si el primero es Leer
                            if (Primero.Valor == "Leer")
                            {
                                //Colocar texto de debug
                                string temp = "Loremp";
                                Avanzar(1);
                                //Si tiene que ejecutar en la consola y no esta invalidado por sus padres (no es un referencia)
                                if (Valargs != "CMP" && Valargs != "Depurar" && !Validacion.Contains(false))
                                {
                                    //Pedir texto y guardar temporalmente
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    temp = Console.ReadLine();
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                //Compilar y devolver
                                Compilado += " System.Console.ReadLine()";
                                return (temp, "String");

                            }
                            //Esto es lo mismo que Leer, pero para enteros
                            else if (Primero.Valor == "{Leer}")
                            {
                                //Colocar texto de debug
                                string temp = "10";
                                Avanzar(1);
                                //Si tiene que ejecutar en la consola y no esta invalidado por sus padres (no es un referencia)
                                if (Valargs != "CMP" && Valargs != "Depurar" && !Validacion.Contains(false))
                                {
                                    //Pedir texto y guardar temporalmente
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    temp = Console.ReadLine();
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                //Compilar y devolver
                                Compilado += " Int32.Parse(System.Console.ReadLine())";
                                return (temp, "Entero");
                            }
                            //Si es alguna otra funcion
                            else
                            {
                                AñadirError("La unica funcion que se le puede asignar a una variable es la funcion Leer");
                                Avanzar(1);
                                return ("", "");
                            }
                        }
                        //Si no es funcion, pero es un entero
                        else if (Primero.Tipo == "Entero")
                        {
                            //Compilar y devolver
                            Avanzar(1);
                            Compilado += " " + Primero.Valor;
                            return (Primero.Valor, Primero.Tipo);
                        }
                        //Si no es ninguno, debe ser un Booleano
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
                        //Si no es ninguno de los casos anteriores, simplemente devolver y rezar a lo mejor
                        Avanzar(1);
                        Compilado += " " + Primero.Valor;
                        return (Primero.Valor, Primero.Tipo);

                    }
                    //Fin primero val
                }
                ////////////////////
                //   OPERACION L  //
                ////////////////////
                bool OperacionL()
                {
                    //Si hay tokens suficientes (Condicion Operador Condicion)
                    if (SiExisteTokens(3))
                    {
                        ImprimirPorDebug("Hay toknes para los ¿e sii");
                        //Operar los primeros valores
                        bool Sumador = FuncionSi();
                        ImprimirPorDebug("Se hizo la primera operacion logica");
                        //Si le siguie un concatenador
                        if (Tokens[Pos].Valor == "Y" || Tokens[Pos].Valor == "O" && Tokens[Pos].Tipo == "Funcion")
                        {
                            switch (Tokens[Pos].Valor)
                            {
                                case "Y":
                                    //Traducir Y al lenguaje C# y compilar
                                    Compilado += " && ";
                                    break;
                                case "O":
                                    //Traducir O al lenguaje C# y compilar
                                    Compilado += " || ";
                                    break;
                            }
                            //Operador es el concatenador actual
                            string Operador = Tokens[Pos].Valor;
                            Avanzar(1);
                            //Si hay un concatenador, empezar la operacion infinita
                            while (Operador == "Y" || Operador == "O" && Tokens[Pos].Tipo == "Funcion")
                            {
                                //Operar el siguiente valor
                                bool Resultado = FuncionSi();
                                //Añadir a la compilacion
                                switch (Tokens[Pos].Valor)
                                {
                                    case "Y":
                                        Compilado += " && ";
                                        break;
                                    case "O":
                                        Compilado += " || ";
                                        break;
                                }
                                switch (Tokens[Pos].Valor)
                                {
                                    case "Y":
                                        //Si ambos son iguales, devolver verdadero, sino, falso
                                        ImprimirPorDebug("Es Y");
                                        if (Sumador == Resultado)
                                        {
                                            ImprimirPorDebug("Sumador es igual a nuevo ");
                                            Sumador = true;
                                        }
                                        else
                                        {
                                            ImprimirPorDebug("Sumador no es igual a nuevo ");
                                            Sumador = false;
                                        }
                                        break;
                                    case "O":
                                        //Si uno de las condicionales es verdadero, devolver verdadero, sino, falso
                                        if (Sumador || Resultado)
                                        {
                                            Sumador = true;
                                        }
                                        else
                                        {
                                            Sumador = false;
                                        }
                                        break;
                                }
                                //Actualizar operador y avanzar
                                Operador = Tokens[Pos].Valor;
                                Avanzar(1);
                            }
                            //Termina operacion infinita
                        }
                        //Termina primer IF, entregar el resultado
                        ImprimirPorDebug("Entregando " + Sumador);
                        return Sumador;
                    }
                    else
                    {
                        //Si no hay tokens para operar infinitamente, devolver la primera operacion
                        ImprimirPorDebug("No hay tokens suficientes, operar logicamente 1 vez");
                        return FuncionSi();
                    }
                }
                //Fin operacionL
                ////////////////////
                //   Funcion SI   //
                ////////////////////
                bool FuncionSi()
                {
                    //NOTA: Esto pudo estar integrado a operacion L, pero no me dio ganaaaaaaaaaaaas
                    //Realizar la primera operacion matematica
                    (string Valor, string Tipo) Primero = OperacionM();
                    ImprimirPorDebug("Primer valor a operar " + Primero);
                    //Guardar el operador logico
                    string Funcion = Tokens[Pos].Valor;
                    //Compilar el operador logico
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
                            AñadirError("Operacion logica no valida");
                            Avanzar(1);
                            break;
                    }
                    Avanzar(1);
                    //Realizar la segunda operacion matematica
                    (string Valor, string Tipo) Segundo = OperacionM();
                    ImprimirPorDebug("Segundo valor a operar " + Segundo);
                    switch (Funcion)
                    {
                        //Si es menor a: Solo lo pueden hacer los enteros
                        case "<":
                            //Si ambos son enteros
                            if (Primero.Tipo == "Entero" && Segundo.Tipo == "Entero")
                            {
                                //Realizar la condicion
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
                                //Sino dar error
                                AñadirError("No se puede operar logicamente con otro tipo ademas de entero");
                                Avanzar(1);
                                return false;
                            }
                        //Si es mayor a: Solo lo pueden hacer los enteros
                        case ">":
                            //Si ambos son enteros
                            if (Primero.Tipo == "Entero" && Segundo.Tipo == "Entero")
                            {
                                //Realizar la condicion
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
                                //Sino dar error
                                AñadirError("No se puede operar logicamente con otro tipo ademas de entero");
                                Avanzar(1);
                                return false;
                            }
                        //Si es igual a: Lo pueden hacer todos los tipos de datos
                        case "=":
                            //Si ambos son enteros, verificar por numero
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
                            //Si ambos son booleanos, verificar por string
                            else if (Primero.Tipo == "Booleano" && Segundo.Tipo == "Booleano")
                            {
                                if (Primero.Valor == Segundo.Valor)
                                {
                                    return true;
                                }
                                else { return false; }
                            }
                            //Si ambos son string, verificar por texto
                            else if (Primero.Tipo == "String" && Segundo.Tipo == "String")
                            {
                                ImprimirPorDebug("Ambos son strings");
                                if (Primero.Valor == Segundo.Valor)
                                {
                                    ImprimirPorDebug("Ambos strings son igualkes " + Primero + "/" + Segundo);
                                    return true;
                                }
                                else { ImprimirPorDebug("Ambos strings son diferentes " + Primero + "/" + Segundo); return false; }
                            }
                            //NOTA: Seccion a repensar
                            else
                            {
                                AñadirError("No se puede operar logicamente");
                                Avanzar(1);
                                return false;
                            }
                        //Si es diferente a: Lo pueden hacer todos los tipos de datos
                        case "!":
                            //Si ambos son enteros, verificar por numero
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
                            //Si ambos son booleanos, verificar por string
                            else if (Primero.Tipo == "Booleano" && Segundo.Tipo == "Booleano")
                            {
                                if (Primero.Valor != Segundo.Valor)
                                {
                                    return true;
                                }
                                else { return false; }
                            }
                            //Si ambos son string, verificar por texto
                            else if (Primero.Tipo == "String" && Segundo.Tipo == "String")
                            {
                                ImprimirPorDebug("Ambos son strings");
                                if (Primero.Valor != Segundo.Valor)
                                {
                                    ImprimirPorDebug("Los strings son distintos " + Primero + "/" + Segundo);
                                    return true;
                                }
                                else { ImprimirPorDebug("Los strings son iguales " + Primero + "/" + Segundo); return false; }
                            }
                            //NOTA: Seccion a repensar
                            else
                            {
                                AñadirError("No se puede operar logicamente");
                                Avanzar(1);
                                return false;
                            }
                        //Si es algun operador logico que no existe en el lenguaje
                        default:
                            AñadirError("No es una opcion valida " + Funcion);
                            Avanzar(1);
                            return false;
                    }
                    //Fin switch Operador
                }
                //Fin Funcion Si
            }
            //Fin void Perser
        }
        //Fin main
    }
    //Fin clase
}
//Fin namespace
