using System;
using System.Collections.Generic;
using System.Text;

namespace FTP_Utility {
    public class ResponseFTP<T> {
        public int Ok { get; set; }
        public String Message{ get; set; }
        public T Data { get; set; }

        public ResponseFTP() {
            this.Ok = 0;
            this.Message = "";
        }

    }
}
