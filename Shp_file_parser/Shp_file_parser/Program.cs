using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.Odbc;
using System.Data;

namespace Shp_file_parser
{
    struct Box
    {
        public double Xmin, Xmax;
        public double Ymin, Ymax;
    }
    struct Point
    {
        public double X, Y;
    }

    class Program
    {
        struct BoxFull
        {
            public double Xmin, Xmax;
            public double Ymin, Ymax;
            public double Zmin, Zmax;
            public double Mmin, Mmax;
        }


        static void Main(string[] args)
        {
            //Encoding utf8 = Encoding.GetEncoding("UTF-8");
            //Encoding win1251 = Encoding.GetEncoding("Windows-1251");

            //string FilePath = "C:\\Users\\Administrator\\Desktop\\";
            //string DBF_FileName = "gis.dbf";
            //OdbcConnection obdcconn = new System.Data.Odbc.OdbcConnection();
            //obdcconn.ConnectionString = "Driver={Microsoft dBase Driver (*.dbf)};SourceType=DBF;SourceDB=" + FilePath + ";Exclusive=No; NULL=NO;DELETED=NO;BACKGROUNDFETCH=NO;";
            //obdcconn.Open();
            //OdbcCommand oCmd = obdcconn.CreateCommand();
            //oCmd.CommandText = "SELECT * FROM " + DBF_FileName;

            ///*Load data to table*/

            //DataTable dt1 = new DataTable();
            //dt1.Load(oCmd.ExecuteReader());
            //obdcconn.Close();

            ///*Bind data to grid*/
            //var text = dt1.Rows[1][2].ToString();
            //byte[] win1251Bytes = win1251.GetBytes(text);
            //byte[] utf8Bytes = Encoding.Convert(utf8, win1251, win1251Bytes);
            //var _text = win1251.GetString(utf8Bytes);

            BoxFull boxFull;
            byte[] ArrayMainFile = new byte[1];
            byte[] ArrayIndexFile = new byte[1];

            string currDir = Environment.CurrentDirectory;
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //MainFile
            using (FileStream fstream = File.OpenRead(@"gis_osm_places_a_free_1_sib.shp"))
            {
                byte[] array = new byte[fstream.Length]; // Массив куда считываем весь файл в байтовом формате
                fstream.Read(array, 0, array.Length);
                Array.Resize(ref ArrayMainFile, array.Length);
                array.CopyTo(ArrayMainFile, 0);
            }
            // ----------------

            //IndexFile
            //using (FileStream fstream = File.OpenRead(@"C:\Users\vpm26\Desktop\UCLab\South Federal District_shp\gis_osm_places_a_free_1.shx"))
            //{
            //	byte[] array = new byte[fstream.Length];
            //	fstream.Read(array, 0, array.Length);
            //	Array.Resize(ref ArrayIndexFile, array.Length);
            //	array.CopyTo(ArrayIndexFile, 0);
            //}
            // ----------------


            using (StreamWriter sw = new StreamWriter(@"results.txt", false))
            {
                // -MainFile
                // --File Header

                const int SizeofInt = sizeof(int);
                const int SizeofDouble = sizeof(double);

                byte[] array_temp_int = new byte[4];
                byte[] array_temp_double = new byte[SizeofDouble];
                Array.ConstrainedCopy(ArrayMainFile, 0, array_temp_int, 0, SizeofInt);
                Array.Reverse(array_temp_int);
                int file_code = BitConverter.ToInt32(array_temp_int, 0);

                Array.ConstrainedCopy(ArrayMainFile, 24, array_temp_int, 0, SizeofInt);
                Array.Reverse(array_temp_int);
                int file_length = BitConverter.ToInt32(array_temp_int, 0) * 2;

                Array.ConstrainedCopy(ArrayMainFile, 28, array_temp_int, 0, SizeofInt);
                int version = BitConverter.ToInt32(array_temp_int, 0);

                Array.ConstrainedCopy(ArrayMainFile, 32, array_temp_int, 0, SizeofInt);
                int shape_type = BitConverter.ToInt32(array_temp_int, 0);

                byte[] array_temp_8Double = new byte[SizeofDouble * 8];
                Array.ConstrainedCopy(ArrayMainFile, 36, array_temp_8Double, 0, SizeofDouble * 8);
                boxFull.Xmin = BitConverter.ToDouble(array_temp_8Double, SizeofDouble * 0);
                boxFull.Ymin = BitConverter.ToDouble(array_temp_8Double, SizeofDouble * 1);
                boxFull.Xmax = BitConverter.ToDouble(array_temp_8Double, SizeofDouble * 2);
                boxFull.Ymax = BitConverter.ToDouble(array_temp_8Double, SizeofDouble * 3);
                boxFull.Zmin = BitConverter.ToDouble(array_temp_8Double, SizeofDouble * 4);
                boxFull.Zmax = BitConverter.ToDouble(array_temp_8Double, SizeofDouble * 5);
                boxFull.Mmin = BitConverter.ToDouble(array_temp_8Double, SizeofDouble * 6);
                boxFull.Mmax = BitConverter.ToDouble(array_temp_8Double, SizeofDouble * 7);

                const string Format = "file_code= {0}\nfile_length= {1}\nversion= {2}\nshape_type= {3}\n";
                const string Format1 = "box.Xmin= {0}\nbox.Ymin= {1}\nbox.Xmax= {2}\nbox.Ymax= {3}\nbox.Zmin= {4}\n" +
                    "box.Zmax= {5}\nbox.Mmin= {6}\nbox.Mmax= {7}";
                Console.WriteLine(Format, file_code, file_length, version, shape_type);
                Console.WriteLine(Format1, boxFull.Xmin, boxFull.Ymin, boxFull.Xmax, boxFull.Ymax, boxFull.Zmin,
                    boxFull.Zmax, boxFull.Mmin, boxFull.Mmax);
                sw.WriteLine("file_code= " + file_code + "\r\nfile_length = " + file_length + "\r\nversion= " + version +
                    "\r\nshape_type= " + shape_type + "\r\n");
                sw.WriteLine("box.Xmin= " + boxFull.Xmin + "\r\nbox.Ymin= " + boxFull.Ymin + "\r\nbox.Xmax= " + boxFull.Xmax +
                    "\r\nbox.Ymax= " + boxFull.Ymax + "\r\nbox.Zmin= " + boxFull.Zmin + "\r\nbox.Zmax= " + boxFull.Zmax +
                    "\r\nbox.Mmin= " + boxFull.Mmin + "\r\nbox.Mmax= " + boxFull.Mmax);

                // ---Record Header
                int offset = 100;
                int[] RecordNumber = new int[1] { 1 };
                int[] ContentLength = new int[1];

                Polygon[] polygon = new Polygon[1];

                for (int i = 0; offset < ArrayMainFile.Length; i++)
                {
                    Box box;
                    Array.Resize(ref RecordNumber, i + 1);
                    Array.Resize(ref ContentLength, i + 1);
                    Array.Resize(ref polygon, i + 1);
                    polygon[i] = new Polygon();

                    Array.ConstrainedCopy(ArrayMainFile, offset, array_temp_int, 0, SizeofInt);
                    offset += SizeofInt;
                    Array.Reverse(array_temp_int);
                    RecordNumber[i] = BitConverter.ToInt32(array_temp_int, 0);

                    Array.ConstrainedCopy(ArrayMainFile, offset, array_temp_int, 0, SizeofInt);
                    offset += SizeofInt;
                    Array.Reverse(array_temp_int);
                    ContentLength[i] = BitConverter.ToInt32(array_temp_int, 0);

                    Console.WriteLine("\n  RecordNumber[{0}]: {1}", i, RecordNumber[i]);
                    Console.WriteLine("  ContentLength[{0}]: {1}", i, ContentLength[i]);
                    sw.WriteLine("\r\nRecordNumber[" + i + "]: " + RecordNumber[i]);
                    sw.WriteLine("ContentLength[" + i + "]: " + ContentLength[i]);

                    //FOR Polygon ONLY!!!!!
                    int ShapeType = BitConverter.ToInt32(ArrayMainFile, offset);
                    Console.WriteLine("  ShapeType: {0}", ShapeType);
                    offset += SizeofInt;

                    //               if (1 == ShapeType) ///temp
                    //{
                    //                   double X = BitConverter.ToDouble(array_temp_8Double, SizeofDouble * 0);
                    //                   double Y = BitConverter.ToDouble(array_temp_8Double, SizeofDouble * 1);
                    //                   sw.WriteLine("XY:\t" + X + "\t" + Y);

                    //               }
                    //               continue;
                    if (25 == ShapeType)
                    {
                        var a = 0;
                    }
                        if (5 != ShapeType) ///temp
					{
                        Console.WriteLine("\n>\tError! ShapeType is NOT 5.");
                        sw.WriteLine("\r\n>\tError! ShapeType is NOT 5.");
                        continue;
                        //System.Environment.Exit(1);
                    }

                    Array.ConstrainedCopy(ArrayMainFile, offset, array_temp_8Double, 0, 4 * SizeofDouble);
                    offset += SizeofDouble * 4;
                    box.Xmin = BitConverter.ToDouble(array_temp_8Double, SizeofDouble * 0);
                    box.Ymin = BitConverter.ToDouble(array_temp_8Double, SizeofDouble * 1);
                    box.Xmax = BitConverter.ToDouble(array_temp_8Double, SizeofDouble * 2);
                    box.Ymax = BitConverter.ToDouble(array_temp_8Double, SizeofDouble * 3);

                    Console.WriteLine("  box.Xmin= {0}\n  box.Ymin= {1}\n  box.Xmax= {2}\n  box.Ymax= {3}",
                        box.Xmin, box.Ymin, box.Xmax, box.Ymax);
                    sw.WriteLine("box\t[Xmin\\Ymin\\Xmax\\Ymax]:\t" + box.Xmin + "\t" + box.Ymin + "\t" + box.Xmax +
                        "\t" + box.Ymax);

                    polygon[i].Box = box;
                    polygon[i].NumParts = BitConverter.ToInt32(ArrayMainFile, offset);
                    offset += SizeofInt;
                    polygon[i].NumPoints = BitConverter.ToInt32(ArrayMainFile, offset);
                    offset += SizeofInt;
                    polygon[i].ResizeParts(polygon[i].NumParts);
                    polygon[i].ResizePoints(polygon[i].NumPoints);
                    Console.WriteLine("  polygon[{0}].NumParts= {1}", i, polygon[i].NumParts);
                    Console.WriteLine("  polygon[{0}].NumPoints= {1}", i, polygon[i].NumPoints);
                    sw.WriteLine("polygon[" + i + "].NumParts= " + polygon[i].NumParts);
                    sw.WriteLine("polygon[" + i + "].NumPoints= " + polygon[i].NumPoints);

                    for (int j = 0; j < polygon[i].NumParts; j++)
                    {
                        polygon[i].Parts[j] = BitConverter.ToInt32(ArrayMainFile, offset);
                        Console.WriteLine("  polygon[{0}].Parts[{1}] = {2}", i, j, polygon[i].Parts[j]);
                        sw.WriteLine("polygon[" + i + "].Parts[" + j + "]= " + polygon[i].Parts[j]);
                        offset += SizeofInt;
                    }
                    for (int j = 0; j < polygon[i].NumPoints; j++)
                    {
                        polygon[i].Points[j].X = BitConverter.ToDouble(ArrayMainFile, offset);
                        polygon[i].Points[j].Y = BitConverter.ToDouble(ArrayMainFile, offset + SizeofDouble);

                        //----------
                        int ii1 = BitConverter.ToInt32(ArrayMainFile, offset);
                        int ii2 = BitConverter.ToInt32(ArrayMainFile, offset + 4);
                        if (ii1 == 5 || ii2 == 5)
                        {
                            Console.WriteLine("\n---Что-то тут равно 5ти int... Пересмотри логику");
                            sw.WriteLine("\n---Что-то тут равно 5ти int... Пересмотри логику");
                            Console.ReadKey();
                        }
                        //--------------
                        offset += SizeofDouble * 2;
                        Console.WriteLine(">>  polygon[{0}].Points[{1}] : X= {2}\tY= {3}",
                            i, j, polygon[i].Points[j].X, polygon[i].Points[j].Y);
                        sw.WriteLine(">>\tpolygon[" + i + "].Points[" + j + "]:\t" + polygon[i].Points[j].X +
                            "\t" + polygon[i].Points[j].Y);

                    }

                }
                // ---
                // --
                // -
            }



            // -IndexFile
            // --
        }
    }

    class Polygon
    {
        public Box Box;
        public int NumParts;
        public int NumPoints;
        public int[] Parts = new int[1];
        public Point[] Points = new Point[1];

        public Polygon() { }
        public Polygon(Box box, int numParts, int numPoints)
        {
            Box = box;
            NumParts = numParts;
            NumPoints = numPoints;
            Array.Resize(ref Parts, NumParts);
            Array.Resize(ref Points, NumPoints);
        }

        public void ResizeParts(int newSize)
        {
            Array.Resize(ref Parts, newSize);
        }
        public void ResizePoints(int newSize)
        {
            Array.Resize(ref Points, newSize);
        }
    }
}
