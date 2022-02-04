using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;


    public class SFTPconn
    {
       
       
        public void GetILexpfile()
        {
           
            DateTime today = DateTime.Today;
            string export_filename = "HSA_IL_" + today.ToString("yyyyMMdd") + ".exp";
         
            string export_textfile = "C:\\alegeus\\ilExportData.txt";
           
            Tamir.SharpSsh.Sftp ftp = new Tamir.SharpSsh.Sftp("ftp.wealthcareadmin.com", "benefledi", "VzVR4s4y");
            ftp.Connect();
            ftp.Get(export_filename, export_textfile);
            ftp.Close();


        }
    }

