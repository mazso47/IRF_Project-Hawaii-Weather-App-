﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;

namespace HawaiiWeatherApp
{
    public partial class Form1 : Form
    {
        List<string> filenames = new List<string>();
        List<string> locations = new List<string>();
        List<weatherLinks> links = new List<weatherLinks>();

        Excel.Application xlApp; // A Microsoft Excel alkalmazás
        Excel.Workbook xlWB; // A létrehozott munkafüzet
        Excel.Worksheet xlSheet; // Munkalap a munkafüzeten belül

        public Form1()
        {
            InitializeComponent();
            fillLists();
            updateData();
        }

        private void fillLists()
        {
            filenames.AddRange
            (
                new List<string>
                {
                    "PHHI.xml", "PHSF.xml", "PHNL.xml", "PHTO.xml", "PHOG.xml", "PHKO.xml", "PHNG.xml", "PHBK.xml", "PHJH.xml", "PHNY.xml", "PHLI.xml", "PHJR.xml"
                }
            );

            locations.AddRange
            (
                new List<string>
                {
                    "Oahu", "Bradshaw Army Air Field", "Daniel K Inouye International Airport", "Hilo", "Kahului", "Kailua / Kona", "Kaneohe", "Kekaha", "Lahaina", "Lanai City", "Lihue", "Oahu"
                }
            );
        }

        private void getWeatherData()
        {

            //if (Directory.GetFileSystemEntries(Application.StartupPath.ToString() + "\\xmlFiles\\").Length == 0)
            //{
            //    updateData();
            //}

            XmlDocument xml = new XmlDocument();
            string selected = "";           
            for (int i = 0; i < links.Count; i++)
            {
                if (links[i].Location == textBox1.Text)
                {
                    selected = links[i].fileName;
                }
            }
            xml.Load("xmlFiles\\" + selected);
            label6.Text = xml.GetElementsByTagName("observation_time")[0].InnerText;
            label7.Text = xml.GetElementsByTagName("weather")[0].InnerText;
            label8.Text = xml.GetElementsByTagName("temp_c")[0].InnerText + "°C";
            label9.Text = xml.GetElementsByTagName("relative_humidity")[0].InnerText;
            label10.Text = xml.GetElementsByTagName("wind_mph")[0].InnerText;
            //https://stackoverflow.com/questions/897466/filter-list-object-without-using-foreach-loop-in-c2-0
        }

        
        private void updateData()
        {
            clearData();
            

            for (int i = 0; i < filenames.Count; i++)
            {
                weatherLinks l = new weatherLinks();
                l.Location = locations[i];
                l.fileName = filenames[i];
                links.Add(l);
            }
         
            WebClient webClient = new WebClient { UseDefaultCredentials = true };
            foreach (weatherLinks link in links)
            {
                webClient.Headers.Add("User-Agent: Other");
                string url = "https://w1.weather.gov/xml/current_obs/" + link.fileName;
                string localFilePath = Application.StartupPath.ToString() + "\\xmlFiles\\" + link.fileName;
                webClient.DownloadFile(url, localFilePath);
            }
        }

        private void clearData()
        {
            DirectoryInfo di = new DirectoryInfo(Application.StartupPath.ToString() + "\\xmlFiles");

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        private void CreateExcel()
        {
            try
            {
                // Excel elindítása és az applikáció objektum betöltése
                xlApp = new Excel.Application();

                // Új munkafüzet
                xlWB = xlApp.Workbooks.Add(Missing.Value);

                // Új munkalap
                xlSheet = xlWB.ActiveSheet;

                // Tábla létrehozása
                CreateTable(); // Ennek megírása a következő feladatrészben következik

                // Control átadása a felhasználónak
                xlApp.Visible = true;
                xlApp.UserControl = true;
            }
            catch (Exception ex) // Hibakezelés a beépített hibaüzenettel
            {
                string errMsg = string.Format("Error: {0}\nLine: {1}", ex.Message, ex.Source);
                MessageBox.Show(errMsg, "Error");

                // Hiba esetén az Excel applikáció bezárása automatikusan
                xlWB.Close(false, Type.Missing, Type.Missing);
                xlApp.Quit();
                xlWB = null;
                xlApp = null;
            }
        }

        private void CreateTable()
        {
            string[] headers = new string[]
            {
                "Location",
                "Observation Time",
                "Weather",
                "Temperature (°C)",
                "Humidity",
                "Wind (MpH)"
            };
            for (int i = 0; i < headers.Length; i++)
            {
                xlSheet.Cells[1, i+1] = headers[i];
            }


            XmlDocument xml = new XmlDocument();

            List<string> obs_time = new List<string>();
            List<string> weather = new List<string>();
            List<double> temp = new List<double>();
            List<int> humidity = new List<int>();
            List<double> wind = new List<double>();
            foreach (weatherLinks link in links)
            {
                xml.Load("xmlFiles\\" + link.fileName);
                string linkObsTime_tmp = xml.GetElementsByTagName("observation_time")[0].InnerText;
                string linkObsTime = linkObsTime_tmp.Substring(linkObsTime_tmp.Length - 25, 25);

                string linkWeather = xml.GetElementsByTagName("weather")[0].InnerText;
                double linkTemp = double.Parse(xml.GetElementsByTagName("temp_c")[0].InnerText + "°C");
                int linkHumidity = int.Parse(xml.GetElementsByTagName("relative_humidity")[0].InnerText);
                double linkWind = double.Parse(xml.GetElementsByTagName("wind_mph")[0].InnerText);

                obs_time.Add(linkObsTime);
                weather.Add(linkWeather);
                temp.Add(linkTemp);
                humidity.Add(linkHumidity);
                wind.Add(linkWind);
            }

            xlSheet.get_Range(
            GetCell(2, 1),
            GetCell(links.Count, 1)).Value2 = locations;

        }

        private string GetCell(int x, int y)
        {
            string ExcelCoordinate = "";
            int dividend = y;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                ExcelCoordinate = Convert.ToChar(65 + modulo).ToString() + ExcelCoordinate;
                dividend = (int)((dividend - modulo) / 26);
            }
            ExcelCoordinate += x.ToString();

            return ExcelCoordinate;
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            updateData();
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            getWeatherData();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CreateExcel();
        }
    }
}
