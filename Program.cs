using System.Diagnostics;

class Arbys2Code {
  
  //El main sirve para obtener los argumentos luego del comando (ejemplo: Arbys2Code.exe -nashe)
static void Main(string[] args){
  List<(string Nombre, string Tipo,string Valor)> Tokens = new();
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
Tokens.ForEach(delegate((string Nombre, string Tipo, string Valor) token){
  Console.WriteLine(token.Nombre + " " + token.Tipo + " " + token.Valor);
});
    Arbys2Code Clase = new Arbys2Code();
void Tokenizador(){
     string Codigo = File.ReadAllText(Dir);
     int Pos = 0;
     string Texto = "";
     string CharVal = "abcdefghijklmnñopqrstuvwxyzABCDEFGHIJKLMNÑOPQRSTUVWXYZ";
     string[] Funciones = {
      "Imprimir",
      "Leer",
      "Si",
      "Mientras",
      "Repetir",
      "="};
     while(Pos < Codigo.Length - 1){
        if (CharVal.Contains(Codigo[Pos])){
          Texto += Codigo[Pos];
          Avanzar();
        }
        switch(Codigo[Pos]){
          case ' ':
            if (Funciones.Contains(Texto)){
              Tokens.Add((Texto,"Funcion","{Vacio}"));
              Texto = "";
              
              Avanzar();
              break;
              }
              else if (Texto != " " && Texto != string.Empty){
                Tokens.Add((Texto,"Variable","{Vacio}"));
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
                Tokens.Add((Texto,"Variable","{Vacio}"));
                Texto = "";
              }
              Avanzar();
              while (Codigo[Pos] != '>'){
                Texto += Codigo[Pos];
                Avanzar();
              }
              Tokens.Add(("{Vacio}","String",Texto));
              Avanzar();
              Texto = "";
              break;
        }
        
    }
    if (Codigo[Pos] != ' '){
      Texto += Codigo[Pos];
    }
    if (Texto != " " && Texto != string.Empty){
          Tokens.Add((Texto,"Variable","{Vacio}"));
        }
    void Avanzar(){
  if (Pos < Codigo.Length - 1){
    Pos++;
  }
}
  }
}
}
