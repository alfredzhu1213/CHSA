using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;


public class Class1
{
	

        public void TestMethod1()
        {

            DateTime today = DateTime.Today;
            string export_filename = "HSA_IL_" + today.ToString("yyyyMMdd") + ".exp";
            Debug.WriteLine("Does this work??????????????????????????");
            Tamir.SharpSsh.Sftp ftp = new Tamir.SharpSsh.Sftp("sftp://ftp.wealthcareadmin.com/", "benefledi", "VzVR4s4y");
            ftp.Connect();
            ftp.Get(export_filename, "C:/alegeus/il2.txt");
        }
    }
