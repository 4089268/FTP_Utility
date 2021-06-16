using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace FTP_Utility {
    public class FTP_Utility {
        public enum ModoPublicacion {
            SobreEscribir = 1,
            No_SobreEscribir = 2,
            Mas_Nuevo = 3
        }

        private string rutaLocal = "";
        private string rutaftp = "";
        private NetworkCredential credenciales;

        public FTP_Utility(NetworkCredential Credenciales, string RutaLocal, string RutaFTP) {
            this.credenciales = Credenciales;
            this.rutaLocal = RutaLocal;
            this.rutaftp = RutaFTP;
        }


        //****** Funciones ******
        public void IniciarWatcher() {
            using (FileSystemWatcher watcher = new FileSystemWatcher()) {
                watcher.Path = this.rutaLocal;

                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName;

                // Only watch text files.
                watcher.Filter = "*.*";

                // Add event handlers.
                watcher.Changed += OnChanged;
                watcher.Created += OnChanged;
                //watcher.Deleted += OnChanged;
                watcher.Renamed += OnRenamed;

                // Begin watching.
                watcher.EnableRaisingEvents = true;

                // Wait for the user to quit the program.
                Console.WriteLine("Presione 'q' para salir.");
                while (Console.Read() != 'q') ;
            }
        }
        private void OnChanged(object source, FileSystemEventArgs e) {
            ActualizarCarpeta(this.credenciales, this.rutaLocal,this.rutaftp);
            Console.WriteLine("\n");
        }
        private void OnRenamed(object source, RenamedEventArgs e) {
            ActualizarCarpeta(this.credenciales, this.rutaLocal, this.rutaftp);
            Console.WriteLine("\n");
        }



        //****** Funciones Estaticas ******
        public static ResponseFTP<string> CrearDirectorio(NetworkCredential credenciales, string path) {
            var respuestaFTP = new ResponseFTP<string>();
            try {
                WebRequest wRequest = WebRequest.Create(path);
                wRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                wRequest.Credentials = credenciales;

                using (var resp = (FtpWebResponse)wRequest.GetResponse()) {
                    if (resp.StatusCode == FtpStatusCode.CommandOK) {
                        Console.WriteLine(resp.StatusDescription + "\n" + resp.StatusCode);
                        respuestaFTP.Ok = 1;
                        respuestaFTP.Data = "Archivo Creado";
                    }
                    else {
                        Console.WriteLine(resp.StatusDescription);
                        respuestaFTP.Data = $"Respuesta no esperada: {resp.StatusDescription}";
                    }
                }
            }
            catch (Exception er) {
                Console.WriteLine("Error al crear el directorio...\n" + er.Message + er.StackTrace);
                respuestaFTP.Message = er.Message + "\n" + er.StackTrace;
                respuestaFTP.Data = $"Error: {er.Message}";
            }

            return respuestaFTP;
        }

        public static ResponseFTP<string> SubirArchivo(NetworkCredential credenciales, string file_source, string file_dest, ModoPublicacion modo) {
            var respuestaFTP = new ResponseFTP<string>();

            Console.WriteLine("\tSubiendo archivo");
            try {
                using (WebClient client = new WebClient()) {
                    client.Credentials = credenciales;
                    client.UploadFile(file_dest, WebRequestMethods.Ftp.UploadFile, file_source);
                }
                respuestaFTP.Ok = 1;
                respuestaFTP.Data = "\tArchivo subido";
            }
            catch (Exception er) {
                Console.WriteLine("Error al subir el archivo..\n" + er.Message + "\n" + er.StackTrace);
                respuestaFTP.Message = er.Message;
                respuestaFTP.Message = "No se pudo subir el archivo";
            }

            return respuestaFTP;
        }

        public static ResponseFTP<List<FTP_File>> ObtenerListadoArchivos(NetworkCredential credenciales, string ftpPath) {
            Console.WriteLine("Obteniendo listado archivos");
            var respuestaFTP = new ResponseFTP<List<FTP_File>>();

            try {

                FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(ftpPath);
                ftpWebRequest.Credentials = credenciales;
                ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                FtpWebResponse response = (FtpWebResponse)ftpWebRequest.GetResponse();

                //****** Formatear respuesta par aobtener listado de Archivos
                List<FTP_File> listadoArchivos = new List<FTP_File>();
                using (StreamReader streamR = new StreamReader(response.GetResponseStream())) {
                    string line = streamR.ReadLine();
                    while (!string.IsNullOrEmpty(line)) {
                        if (!line.Contains("<DIR>")) {
                            var tmpItemFTP = new FTP_File();

                            //*** Obtener la fecha
                            var tmpDate = new DateTime(2000 + int.Parse(line.Substring(6, 2)), int.Parse(line.Substring(0, 2)), int.Parse(line.Substring(3, 2)));
                            //*** Obtener la hora
                            var tmpHour = DateTime.Parse(line.Substring(10, 7));

                            tmpItemFTP.UltimaModif = DateTime.Parse($"{tmpDate.ToString("dd/MM/yyyy")} { tmpHour.ToString("HH:mm:ss")}");

                            var tmpS = line.Substring(17, (line.Length - 17));
                            var sIndex = 0;
                            for (int i = 0; i < tmpS.Length; i++) {
                                if (tmpS.ToCharArray()[i] != ' ') {
                                    sIndex = i;
                                    break;
                                }
                            }

                            var tmpArrs = tmpS.Substring(sIndex, (tmpS.Length - (sIndex))).Split(" ");

                            //*** Obtener el tamaño
                            var tamaño = tmpArrs[0];
                            tmpItemFTP.Tamaño = long.Parse(tamaño);

                            var nombre = "";
                            for (int j = 1; j < tmpArrs.Length; j++) {
                                nombre += tmpArrs[j];
                                nombre += " ";
                            }
                            //*** Quitar el ultimo espacio del nombre
                            if (nombre.ToCharArray()[nombre.Length - 1] == ' ') {
                                nombre = nombre.Substring(0, nombre.Length - 1);
                            }
                            tmpItemFTP.Nombre = nombre;
                            tmpItemFTP.Extencion = nombre.Split(".")[1];

                            listadoArchivos.Add(tmpItemFTP);
                        }
                        line = streamR.ReadLine();
                    }
                }

                respuestaFTP.Ok = 1;
                respuestaFTP.Data = listadoArchivos;

            }
            catch (Exception err) {
                respuestaFTP.Message = err.Message;
                respuestaFTP.Data = null;
            }

            return respuestaFTP;
        }

        public static ResponseFTP<List<FTP_File>> ObtenerArchivosPendientes(NetworkCredential credenciales, string rutaCarpetaLocal, string rutaCarpetaFtp) {
            Console.WriteLine("Obteniendo listado de archivos pendientes");
            var respuestaFTP = new ResponseFTP<List<FTP_File>>();

            //*** Obtner Listado de archivos en el FTP
            var tmpResp = ObtenerListadoArchivos(credenciales, rutaCarpetaFtp);
            if (tmpResp.Ok == 0) {
                respuestaFTP.Message = "Error al obtener los archivos de la carpeta local: \n" + tmpResp.Message;
                return respuestaFTP;
            }
            List<FTP_File> listaArchivosFTP = tmpResp.Data;


            //*** Obteniendo archivos en la carpeta Local
            FileInfo[] archivosLocales = null;
            try {
                var xD = new DirectoryInfo(rutaCarpetaLocal);
                archivosLocales = xD.GetFiles();
            }
            catch (Exception err) {
                respuestaFTP.Message = "Error al obtener archivos locales: " + err.Message + " \n" + err.StackTrace;
                return respuestaFTP;
            }


            //*** Compara Archivos y generar Lista de Archivos a subir
            List<FTP_File> archivosPorSubir = new List<FTP_File>();
            foreach (var itemLocal in archivosLocales) {
                bool omitir = false;
                foreach (var itemFtp in listaArchivosFTP) {
                    if (itemFtp.Nombre.ToLower() == itemLocal.Name.ToLower()) {
                        if (itemFtp.Tamaño >= itemLocal.Length) {
                            omitir = true;
                        }
                    }
                }
                if (!omitir) {
                    archivosPorSubir.Add( new FTP_File { 
                        Nombre = itemLocal.Name ,
                        Tamaño = itemLocal.Length,
                        UltimaModif = itemLocal.LastWriteTime
                    });
                }
            }
            respuestaFTP.Ok = 1;
            respuestaFTP.Data = archivosPorSubir;

            return respuestaFTP;
        }

        public static ResponseFTP<string> ActualizarCarpeta(NetworkCredential credenciales, string rutaCarpetaLocal, string rutaCarpetaFtp) {
            var respuestaFTP = new ResponseFTP<string>();
            Console.WriteLine("Actualizando Carpeta");


            //*** Obteniendo archivos en la carpeta FTP
            var tmpResp = ObtenerArchivosPendientes(credenciales, rutaCarpetaLocal, rutaCarpetaFtp);
            if (tmpResp.Ok == 0) {
                respuestaFTP.Message = tmpResp.Message;
                return respuestaFTP;
            }
            List<FTP_File> archivosPendientes= tmpResp.Data;

            //*** Subir Archivos
            foreach (var itemPorSubir in archivosPendientes) {

                //*** Si el ultimo caracter de la rutaCarpetaLocal es un '/' or un '\', combinar la rutaCarpetaLocal y el nombre del archivo sin agregar el '/'
                var tmpURl = (rutaCarpetaLocal.ToCharArray()[rutaCarpetaLocal.Length - 1] == '/' || rutaCarpetaLocal.ToCharArray()[rutaCarpetaLocal.Length - 1] == '\\') ? $"{rutaCarpetaLocal}{itemPorSubir.Nombre}" : $"{rutaCarpetaLocal}/{itemPorSubir.Nombre}";
                var tmpURlFTp = (rutaCarpetaFtp.ToCharArray()[rutaCarpetaFtp.Length - 1] == '/' || rutaCarpetaFtp.ToCharArray()[rutaCarpetaFtp.Length - 1] == '\\') ? $"{rutaCarpetaFtp}{itemPorSubir.Nombre}" : $"{rutaCarpetaFtp}/{itemPorSubir.Nombre}";
                Console.WriteLine($"\tSubiendo Archivo: {tmpURl} a {tmpURlFTp} ");

                var tmpRes = FTP_Utility.SubirArchivo(credenciales, tmpURl, tmpURlFTp, ModoPublicacion.SobreEscribir);
                if (tmpRes.Ok == 1) {
                    Console.WriteLine($"\tArchivo subido");
                }
                else {
                    Console.WriteLine($"\tNo se pudo subir el archivo: {tmpRes.Message}");
                }

                respuestaFTP.Data = "Comletado todos los archivos pendientes por subir";
            }
            respuestaFTP.Ok = 1;
            return respuestaFTP;
        }
        
    }
}
