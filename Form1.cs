using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QaGeneral_Apk_Kurma
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }



        int EklenenDosyaId = 0;
        ArrayList BagliSeriNumaralar = new ArrayList();
        ArrayList cmdIdList = new ArrayList();
        String KullAdi = SystemInformation.UserName;
        Process cmd = new Process();
        Boolean ProgramKapat = false;



        void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.Copy;
            }
          
        }

        void listView1_DragDrop(object sender, DragEventArgs e)
        {
            Boolean ListedeVar = false;
            try
            {string[] DosyaAdiveYolu = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string Dosya in DosyaAdiveYolu){

                    // Aydın Rega
                    foreach (ListViewItem item in listView1.Items)
                    {
                        if (item.SubItems[0].Text == Path.GetFileNameWithoutExtension(Dosya))
                        {
                            MessageBox.Show("Aynı Öğeyi 2 Defa Atamazsınız. " + Path.GetFileNameWithoutExtension(Dosya) + " Zaten Listede Var");
                            ListedeVar = true;
                            continue;
                        }
                       
                    }

                    if (ListedeVar)
                    {
                        ListedeVar = false;
                        continue;
                    }

                    FileAttributes attr = File.GetAttributes(Dosya);                
                    FileInfo DosyaBilgisi = new FileInfo(Dosya);

                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                 
                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = Path.GetFileNameWithoutExtension(Dosya);

                        lvi.SubItems.Add(BoyutHesapla(KlasorBoyut(new DirectoryInfo(Dosya))).ToString());

                        lvi.SubItems.Add("Eklendi");

                        lvi.SubItems.Add("Klasör");

                        lvi.SubItems.Add(Dosya);
                        listView1.Items.Add(lvi);

                        label2.Text = "Yapılan İşlem : " + Path.GetFileNameWithoutExtension(Dosya) + " İsimli Klasör Listeye Eklendi Eklendi";

                    }
                    else
                    {
                     
                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = Path.GetFileNameWithoutExtension(Dosya);

                        lvi.SubItems.Add((BoyutHesapla(DosyaBilgisi.Length)));

                        lvi.SubItems.Add("Eklendi");

                        lvi.SubItems.Add(DosyaBilgisi.Extension);

                        lvi.SubItems.Add(Dosya);
                        listView1.Items.Add(lvi);

                       
                        label2.Text = "Yapılan İşlem : " + Path.GetFileNameWithoutExtension(Dosya) + " isimli Dosya Listeye Eklendi";

                       
                    }

                    foreach (ColumnHeader column in listView1.Columns)
                    {
                        column.Width = -3;
                    }

                    EklenenDosyaId++;

                }
            }
            catch
            {
                MessageBox.Show("Hata !");
            }
                     
        }

        public static long KlasorBoyut(DirectoryInfo yol)
        {
            return yol.GetFiles().Sum(fi => fi.Length) + yol.GetDirectories().Sum(di => KlasorBoyut(di));
        }

        public static string BoyutHesapla(long gelenboyut)
        {
            string sonucboyut = "";

            if (gelenboyut >= 1073741824)
            {
                sonucboyut += (gelenboyut / 1073741824) + " GB ";
                gelenboyut = gelenboyut % 1073741824;
            }

            if (gelenboyut >= 1048576)
            {
                sonucboyut += (gelenboyut / 1048576) + ",";
                gelenboyut = gelenboyut % 1048576;
            }

            if (gelenboyut >= 1024)
            {
                sonucboyut += (gelenboyut / 1024) + " KB ";
                gelenboyut = gelenboyut % 1024;
            }
            //sonucboyut += gelenboyut + "";

            return sonucboyut;
        }

        private void CihazTespitEt()
        {

            string[] SeriNumaraList = AdbKomut("adb devices").Split('\n');
            string Seri = "";
            ArrayList SilinecekSeriNum = new ArrayList();
            ArrayList GuncelEklenenSeriler = new ArrayList();


            for (int i = 1; i < SeriNumaraList.Count() - 2; i++)
            {

                Seri = SeriNumaraList[i].Replace("device", "");

                if (Seri.Contains("daemon") == true)
                {
                    CihazTespitEt();
                    return;
                }
                else if (Seri != "" && BagliSeriNumaralar.Contains(Seri) == false)
                {

                    //listView1.Items.Add(Seri);
                    BagliSeriNumaralar.Add(Seri);

                }

                GuncelEklenenSeriler.Add(Seri);
            }
            for (int z = 0; z < BagliSeriNumaralar.Count; z++)
            {

                if (GuncelEklenenSeriler.Contains(BagliSeriNumaralar[z].ToString()) == false)
                {

                    SilinecekSeriNum.Add(BagliSeriNumaralar[z]);

                }


                for (int g = 0; g < SilinecekSeriNum.Count; g++)
                {

                    try
                    {


                        BagliSeriNumaralar.Remove(SilinecekSeriNum[g].ToString());

                        int ListCount = listView1.Items.Count;

                        for (int y = 0; y < ListCount; y++)
                        {

                            if (listView1.Items[y].Text == SilinecekSeriNum[g].ToString())
                            {
                                listView1.Items[y].Remove();
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        return;
                    }

                }

            }

            label5.Text = "Bağlı Cihaz Sayısı : " + (BagliSeriNumaralar.Count).ToString();
        }

        public string AdbKomut(string Komut)
        {
            try
            {
                Process My_Process = new Process();
                ProcessStartInfo My_Process_Info = new ProcessStartInfo();
                My_Process_Info.FileName = "cmd.exe";
                My_Process_Info.Arguments = "/c " + Komut;
                My_Process_Info.WorkingDirectory = "C:\\Users\\" + KullAdi + "\\Documents\\QaGeneral\\xtool_scripts";
                My_Process_Info.CreateNoWindow = true;
                My_Process_Info.UseShellExecute = false;
                My_Process_Info.RedirectStandardOutput = true;
                My_Process_Info.RedirectStandardError = true;
                My_Process.EnableRaisingEvents = true;
                My_Process.StartInfo = My_Process_Info;
                My_Process.Start();
                string Process_StandardOutput = My_Process.StandardOutput.ReadToEnd();
                if (Process_StandardOutput != null)
                {
                    My_Process.Dispose();
                    return Process_StandardOutput;
                }
            }
            catch (Exception ex)
            {
                return "HATA : " + ex.Message;
            }

            return "OK";
        }

        void CmdCommandExit(string cmdkomut)
        {
            cmdIdList.Clear();
            string YeniKomutt;
            ArrayList CalisanCmdList = new ArrayList();
            int KapananCmd = 0;
            ArrayList KapananCmdList = new ArrayList();
            KapananCmdList.Clear();

            cmd.StartInfo.FileName = "cmd.exe";

            for (int i = 0; (i
                        <= (BagliSeriNumaralar.Count - 1)); i++)
            {
                YeniKomutt = cmdkomut.Replace("adb shell", ("adb -s "
                                + (BagliSeriNumaralar[i].ToString() + " shell"))).Replace("adb", ("adb -s " + BagliSeriNumaralar[i].ToString()));
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                cmd.StartInfo.Arguments = ("/k cd C:\\Users\\"
                            + (KullAdi + ("\\Documents\\QaGeneral\\xtool_scripts & "
                            + (YeniKomutt + " & exit"))));
                cmd.Start();
                cmdIdList.Add(cmd.Id.ToString());
            }

            while (true)
            {
                CalisanCmdList.Clear();
                foreach (Process islem in Process.GetProcessesByName("cmd"))
                {
                    CalisanCmdList.Add(islem.Id.ToString());
                }

                for (int s = 0; (s
                            <= (cmdIdList.Count - 1)); s++)
                {
                    if ((CalisanCmdList.Contains(cmdIdList[s].ToString()) || KapananCmdList.Contains(cmdIdList[s].ToString())))
                    {

                    }
                    else
                    {
                        KapananCmdList.Add(cmdIdList[s].ToString());
                        KapananCmd++;
                    }

                }

                if ((cmdIdList.Count == KapananCmd))
                {
                    break;
                }

                Bekle(0.1);
            }

        }

        void Bekle(double beklenensaniye)
        {
            for (double i = 0; (i
                        <= (beklenensaniye * 100)); i++)
            {
                System.Threading.Thread.Sleep(10);
                Application.DoEvents();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {


            label2.Text = "Yapılan işlem : Bağlı Cihazlar Kontrol Ediliyor";
            CihazTespitEt();

            if (listView1.Items.Count == 0)
            {
                MessageBox.Show("Hiçbir Öğe Eklemediniz.");
                label2.Text = "Yapılan işlem : Hiçbir Apk Dosyası Eklemediniz.";
                return;
            }

            if (BagliSeriNumaralar.Count == 0)
            {
                MessageBox.Show("Hiçbir Cihaz Bağlı Değil Yada Bulunamadı");
                label2.Text = "Hiçbir Cihaz Bağlı Değil Yada Bulunamadı";
                return;
            }

            if (listView1.CheckedItems.Count == 0)
            {
                MessageBox.Show("Hiçbir Öğeyi Seçmediniz");
                label2.Text = "Hiçbir Apk Seçmediniz";
                return;
            }


            foreach (ListViewItem item in listView1.CheckedItems)
            {

                if (ProgramKapat)
                {
                    return;
                }

                if (item.SubItems[3].Text== ".apk")
                {

                  

                    if (checkBox1.Checked)
                    {
                        label2.Text = "Yapılan İşlem : " + item.SubItems[0].Text + " isimli Apk Kuruluyor.";
                        item.SubItems[2].Text = "Yükleniyor";
                        foreach (ColumnHeader column in listView1.Columns)
                        {
                            column.Width = -3;
                        }
                        CmdCommandExit("adb install -r " + "\"" + item.SubItems[4].Text + "\"");
                        item.SubItems[2].Text = "Yüklendi";
                    }
                    else {
                        label2.Text = "Yapılan İşlem : " + item.SubItems[0].Text + " isimli Apk Cihaza Gönderiliyor.";
                        item.SubItems[2].Text = "Gönderiliyor";
                        foreach (ColumnHeader column in listView1.Columns)
                        {
                            column.Width = -3;
                        }
                        CmdCommandExit("adb push " + "\"" + item.SubItems[4].Text + "\"" + " /sdcard/" + "\"" + item.SubItems[0].Text + "\"" + item.SubItems[3].Text);
                        item.SubItems[2].Text = "Gönderildi";
                    }

                   
                }else if(item.SubItems[3].Text == "Klasör")
                {

                    label2.Text = "Yapılan İşlem : " + item.SubItems[0].Text + " isimli Klasör Cihaza Gönderiliyor.";
                    item.SubItems[2].Text = "Gönderiliyor";
                    foreach (ColumnHeader column in listView1.Columns)
                    {
                        column.Width = -3;
                    }
                    CmdCommandExit("adb push " + "\"" + item.SubItems[4].Text + "\"" + " /sdcard/");
                    item.SubItems[2].Text = "Gönderildi";

                }
                else
                {
                    label2.Text = "Yapılan İşlem : " + item.SubItems[0].Text + " isimli Dosya Cihaza Gönderiliyor.";
                    item.SubItems[2].Text = "Gönderiliyor";
                    foreach (ColumnHeader column in listView1.Columns)
                    {
                        column.Width = -3;
                    }
                    CmdCommandExit("adb push " + "\"" + item.SubItems[4].Text + "\"" + " /sdcard/" + "\"" + item.SubItems[0].Text + "\"" + item.SubItems[3].Text);
                    item.SubItems[2].Text = "Gönderildi";
                }
                           
            }

            label2.Text = "Yapılan İşlem : Tüm Liste Yüklendi";
            foreach (ListViewItem item in listView1.CheckedItems)
            {
                item.SubItems[2].Text = "İşlem Bekliyor";
            }
            foreach (ColumnHeader column in listView1.Columns)
            {
                column.Width = -3;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(listView1.Items.Count > 0)
            {
                listView1.Items.Clear();
                label2.Text = "Yapılan İşlem : Tüm Liste Kaldırıldı";

            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.CheckedItems)
            {
                listView1.Items.RemoveAt(item.Index);
                label2.Text = "Yapılan İşlem : Seçili Öğeler Listeden Kaldırıldı";
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                listView1.Items[item.Index].Checked = true;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            ProgramKapat = true;
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            KaanOrtak.Global.Arayuz_Tasarim(this);
        }
    }
}
