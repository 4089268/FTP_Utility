using System;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace FTP_Utility {
    class Program{


        static void Main(string[] args) {
            Console.WriteLine("FTP Utility");
            NetworkCredential credenciales = new NetworkCredential {
                UserName = "{Mi Usuario}",
                Password = "{Mi contraseña}"
            };
            //Ruta ftp: ftp://host.net:21

            if (args.Length < 1) {
                Console.WriteLine(" -md <Nombre Directorio> \t Crear un Nuevo directorio");
                Console.WriteLine(" -u <Archivo Origen> <Archivo Destino> \t Sube un archivo");
                Console.WriteLine(" -ls <Ruta destino> \t Obtiene el listado de los archivos en la ruta establecida");
                Console.WriteLine(" -uf <Ruta carpta local> <Ruta carpta destino> \t Sube los archivos de la carpeta local que no esten en la carpeta destino");
                Console.WriteLine(" -w <Ruta carpta local> <Ruta carpta destino> \t Detecta cambios y sube los archivos de la carpeta local que no esten en la carpeta destino");
                return;
            }
            string r1,r2,r3;
            
            switch (args[0].ToLower()){
                case "-md":
                    if(args.Length < 2) {
                        Console.WriteLine("Faltan argumentos");
                        return;
                    }
                    var resp1 =  FTP_Utility.CrearDirectorio(credenciales,args[1].ToString());
                    break;


                case "-u":
                    if (args.Length < 3) {
                        Console.WriteLine("Faltan parametros");
                        return;
                    }
                    r1 = args[1];
                    r2 = args[2];
                    var resp2 = FTP_Utility.SubirArchivo(credenciales, r1, r2, FTP_Utility.ModoPublicacion.SobreEscribir);
                    break;


                case "-ls":
                    if (args.Length < 2) {
                        Console.WriteLine("Faltan parametros");
                        return;
                    }
                    r1 = args[1];
                    var resp3 = FTP_Utility.ObtenerListadoArchivos(credenciales, r1);
                    foreach(var item in resp3.Data) {
                        Console.WriteLine($"\tArchivo: {item.Nombre}");
                    }
                    break;


                case "-uf":
                    if (args.Length < 3) {
                        Console.WriteLine("Faltan parametros");
                        return;
                    }
                    var r41 = args[1];
                    var r42 = args[2];
                    var resp4 = FTP_Utility.ActualizarCarpeta(credenciales, r41,r42);
                    Console.WriteLine($"\nFinalizado:\nOk:\t{resp4.Ok}\nResp:\t{resp4.Data}\nMsg:\t{resp4.Message}");
                    break;


                case "-w":
                    if (args.Length < 3) {
                        Console.WriteLine("Faltan parametros");
                        return;
                    }
                    var r51 = args[1];
                    var r52 = args[2];
                    var ftpU = new FTP_Utility(credenciales, r51, r52);
                    ftpU.IniciarWatcher();
                    Console.WriteLine($"\nFinalizado");
                    break;


                default:
                    Console.WriteLine(" -md <Nombre Directorio> \t Crear un Nuevo directorio");
                    Console.WriteLine(" -u <Archivo Origen> <Archivo Destino> \t Sube un archivo");
                    Console.WriteLine(" -ls <Ruta destino> \t Obtiene el listado de los archivos en la ruta establecida");
                    Console.WriteLine(" -uf <Ruta carpta local> <Ruta carpta destino> \t Sube los archivos de la carpeta local que no esten en la carpeta destino");
                    Console.WriteLine(" -w <Ruta carpta local> <Ruta carpta destino> \t Detecta cambios y sube los archivos de la carpeta local que no esten en la carpeta destino");
                    return;
            }

        }


    }
}
