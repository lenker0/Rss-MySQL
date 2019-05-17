using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using MySql.Data.MySqlClient;

namespace WindowsFormsApplication2
{
    struct RSS //структура для листа 
    {
        public string title, description, link, pubdate;
        public RSS(string t, string d, string p, string l) //конструктор
        {
            title = t; description = d; link = l; pubdate = p;
        }
    };
    public partial class Form1 : Form
    {
        List<RSS> Items = new List<RSS>(); 
        string[] razd = new string[] { "<title>", "</title>", "<description>", "</description>", "<link>", "</link>", "<pubDate>", "</pubDate>", "\n"}; //сепараторы для сплита
        public Form1()
        {
            InitializeComponent();
        }
        private void addtext(string s, string v) //добавляем элемент из rss ленты 
        {
            switch (v) //в зависимости от v (варианта) опр цвет
            {
                case "title":
                    richTextBox1.SelectionColor = Color.Red;
                    break;
                case "description":
                    richTextBox1.SelectionColor = Color.Black;
                    break;
                case "link":
                    richTextBox1.SelectionColor = Color.Blue;
                    break;
                case "pubDate":
                    richTextBox1.SelectionColor = Color.Gray;
                    break;
            }
            richTextBox1.AppendText(s + "\n");
        }
        private void showRSS()  //метод для обработки запросов и отображения в richTextBox1
        {
            string uri = textBox1.Text; //ссылка
            string text;
            try //для обработки неправильных ссылок и прочих ошибок
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri); //создание запроса по ссылке
                HttpWebResponse response = (HttpWebResponse)request.GetResponse(); //получение ответа
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8)) //поток для считывания текста
                {
                    text = sr.ReadToEnd();
                }
                string[] separ = { "<item>" };
                string[] item = text.Split(separ, StringSplitOptions.RemoveEmptyEntries); //разделение по item (блокам инофрмации)
                for (int i = 1; i < item.Length; i++) //с 1 потому что, 0 - шапка rss ленты
                {
                    string[] elem = item[i].Split(razd, StringSplitOptions.RemoveEmptyEntries); //разделение блока на части
                    RSS block = new RSS(elem[0], elem[3], elem[4].Remove(elem[4].Length - 6, 6), elem[1]); //создание нормального блока
                    addtext(block.title, "title");
                    addtext(block.link, "link");
                    addtext(block.description, "description");
                    addtext(block.pubdate, "pubDate");
                    richTextBox1.AppendText("\n");
                    Items.Add(block); //добавление в лист целого блока
                }
            }
            catch (Exception)
            {
                richTextBox1.Text = "Error";
            }
        }
        private void saveBase()
        {
            string connStr = "server=localhost;user=root;database=base;CharSet=utf8;"; //параметры соединения
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            MySqlCommand command = new MySqlCommand("TRUNCATE rss", conn); //очистка БД для перезаписи
            command.ExecuteNonQuery(); //выполнение комманд
            for (int i = 0; i < Items.Count; i++)
            {
                string query = "INSERT INTO rss(Title,Description,Date,Link) VALUES ('" + Items[i].title + "','" + Items[i].description + "','" + Items[i].pubdate + "','" + Items[i].link + "')"; //составление запросов для вставки
                command = new MySqlCommand(query, conn);
                command.ExecuteNonQuery();
            }
            conn.Close();
        }
        private void readBase()
        {
            string connStr = "server=localhost;user=root;database=base;CharSet=utf8;";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            string query = "SELECT Title,Description,Date,Link FROM rss"; //запрос для выделения
            MySqlCommand command = new MySqlCommand(query, conn);
            MySqlDataReader sr = command.ExecuteReader(); //открытие копии
            while (sr.Read())
            {
                richTextBox2.SelectionColor = Color.Red;
                richTextBox2.AppendText(sr[0].ToString() + "\n");
                richTextBox2.SelectionColor = Color.Blue;
                richTextBox2.AppendText(sr[3].ToString() + "\n");
                richTextBox2.SelectionColor = Color.Black;
                richTextBox2.AppendText(sr[1].ToString() + "\n");
                richTextBox2.SelectionColor = Color.Gray;
                richTextBox2.AppendText(sr[2].ToString() + "\n\n");
            }
            sr.Close();
            conn.Close();
        }
        private void Clear() //очистка всех текстов и листа
        {
            Items.Clear();
            richTextBox1.Text = "";
            richTextBox2.Text = "";
            textBox1.Text = "";
        }
        private void ClearData() //очистка БД
        {
            string connStr = "server=localhost;user=root;database=base;CharSet=utf8;";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            MySqlCommand command = new MySqlCommand("TRUNCATE rss", conn);
            command.ExecuteNonQuery();
            conn.Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Items.Clear();
            richTextBox1.Text = "";
            showRSS();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            saveBase();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "";
            readBase();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            Clear();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            ClearData();
        }
    }
}
