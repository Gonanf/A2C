using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System;
using System.IO;
using System.Globalization;
using System.Text;
namespace Chaos
{
class Arbys2Code {


  //El main sirve para obtener los argumentos luego del comando (ejemplo: Arbys2Code.exe -nashe)
static void Main(string[] args){
  string Vercion = "0.0.1";
  List<(string Valor, string Tipo)> Tokens = new();
  var User = EnvironmentVariableTarget.User;
  var Old = Environment.GetEnvironmentVariable("PATH",User);
  var New = Old + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
  if (args.Length == 0){
    Console.WriteLine("Instalar Arbys2Code en la variable de entorno PATH?");
    Console.WriteLine("S/N");
    switch (Console.ReadLine().ToLower()){
    case "s":
      if (Old.Contains(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))){
        Console.WriteLine("Ya esta instalado en el sistema");
        Console.WriteLine($"La ruta a Arbys2Code es:\n{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");
        Console.WriteLine("Arbys2Code Vercion " + Vercion);
        Console.ReadLine();
        Environment.Exit(0);
        break;
      }
        Environment.SetEnvironmentVariable("Path", New, User);
        Console.WriteLine($"La ruta a Arbys2Code es:\n{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");
        Console.WriteLine("Listo!");
        Console.ReadLine();
        Environment.Exit(0);
        break;
    case "n":
      Console.WriteLine($"La ruta a Arbys2Code es:\n{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");
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
  if (args.Length != 2){
    Console.WriteLine("Arbys2Code necesita 2 argumentos, un metodo de ejecucion y un archivo .arb, coloque Arbys2Code Ayuda para mas informacion");
    Console.ReadLine();
    Environment.Exit(0);
  }
  switch(args[0]){
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
  if (Dir != null){
      if (File.Exists(Dir + ".arb")){
        Dir += ".arb";
      }
      else{
        Console.WriteLine("Archivo.arb no encontrado");
      }
      
  }
  else{
        Console.WriteLine("[ERROR] No existe el archivo especificado.");
        Console.ReadLine();
  }
 
 Tokenizador();
 Tokens.ForEach(delegate((string Valor, string Tipo) token){
  Console.WriteLine(token.Tipo + " " + token.Valor);
});
Perser(args[0]);
void Tokenizador(){
     string Codigo = File.ReadAllText(Dir);
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
      "=",
      "+",
      "-",
      "*",
      "/"};
     while(Pos < Codigo.Length - 1){
        switch(Codigo[Pos]){
          case ' ':
            if (Funciones.Contains(Texto)){
              Tokens.Add((Texto,"Funcion"));
              Texto = "";
              
              Avanzar();
              break;
              }
              else if (Texto != " " && Texto != string.Empty){
                Tokens.Add((Texto,"Variable"));
                Texto = "";
                Avanzar();
                break;
              }else{
                Avanzar();
                Texto = "";
              }
              break;
          case '<':
              if (Texto != " " && Texto != string.Empty){
                Tokens.Add((Texto,"Variable"));
                Texto = "";
              }
              Avanzar();
              while (Codigo[Pos] != '>'){
                Texto += Codigo[Pos];
                Avanzar();
              }
              Tokens.Add((Texto,"String"));
              Avanzar();
              Texto = "";
              break;
          default:
            Texto += Codigo[Pos];
            Avanzar();
            break;
        }
        
    }
    if (Codigo[Pos] != ' ' && Codigo[Pos] != '<' && Codigo[Pos] != '>'){
      Texto += Codigo[Pos];
    }
    if (Texto != " " && Texto != string.Empty){
          Tokens.Add((Texto,"Variable"));
        }
    void Avanzar(){
  if (Pos < Codigo.Length - 1){
    Pos++;
  }
}
  }



void Perser(string args){
  int Pos = 0;
  string Compilado = "using System; \n";
  List<(string Nombre,string Tipo, string Valor)> Variables = new();
  while(Pos<Tokens.Count() - 1){
    switch(Tokens[Pos].Tipo){
      case "Funcion":
        switch(Tokens[Pos].Valor){
          case "Imprimir":
            if (SiExisteTokens(1)){
              
              Imprimir(Tokens[Pos + 1].Valor,Tokens[Pos + 1].Tipo);
              Avanzar(1);
              
            }
          break;
          case "Leer":

          break;
          case "Si":

          break;
          case "Mientras":

          break;
          case "Repetir":

          break;

        }
      break;
      case "Variable":
        if(SiExisteTokens(1))
        {
          if (Tokens[Pos + 1].Tipo == "Funciones"){
              switch(Tokens[Pos + 1].Valor){
                case "+":

                break;
                case "-":

                break;
                case "*":

                break;
                case "/":

                break;
                case "=":
                
                break;
              }
          }
          
        }
        break;
      default:
          break;
        }
      break;
    }
    bool SiExisteTokens(int num){
      if(Pos + num < Tokens.Count()){
        return true;}else{return false;}
      }
    int IndexVar(string Nombre){

        return 1;
      }
    void Avanzar(int num){
      if(Pos + num < Tokens.Count() - 1){
        Pos = Pos+num;
      }else{
        Pos = Tokens.Count() - 1;
      }
    }
   void Imprimir(string Valor, string Tipo){
      if (Variables.Count() > 0 && Tipo == "Variable"){
        for (int i = 0; i<Variables.Count(); i++){
          if(Variables[i].Nombre == Valor){
            switch (args){
              case "JIT":
                Console.WriteLine(Variables[i].Valor);
                break; 
              case "CMP":
                Compilado += " Console.WriteLine(" + Variables[i].Valor + "); \n";
                break;
              case "Ambos":
                Console.WriteLine(Variables[i].Valor);
                Compilado += " Console.WriteLine(" + Variables[i].Valor + ");";
                break;
            }
            
          }
        }
      }
      else{
      switch (args){
              case "JIT":
                Console.WriteLine(Valor);
                break; 
              case "CMP":
                Compilado += " Console.WriteLine(" + Valor + ");";
                break;
              case "Ambos":
                Console.WriteLine(Valor);
                Compilado += " Console.WriteLine(" + Valor + ");";
                break;
        }
            }
          }
        }
      }
    }
  }
