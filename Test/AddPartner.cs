using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Test.Properties;

namespace Test
{
    public partial class AddPartner : Form
    {
        public PanelPartner panelPartner;
        public Boolean isEditMode = false;
        // Имя партнера для редактирования
        public string NamePartnerToEdit { get; set; }
        public FlowLayoutPanel flowLayoutPanel { get; set; }

        // Временное имя выбранного изображения (только имя файла, без пути)
        private string selectedImageFileName = null;

        //  Конструктор формы
        public AddPartner(FlowLayoutPanel flowLayoutPanel, Boolean isEditMode)      
        {
            InitializeComponent();
            // Инициализация полей
            this.flowLayoutPanel = flowLayoutPanel;
            // Установка режима формы
            this.isEditMode = isEditMode;
            // Подключаем обработчики событий
            this.Load += AddPartner_Load;
            this.buttonAddPartner.Click += (s, e) => { if (this.isEditMode) UpdatePartner(); else AddNewPartner(); };
            this.buttonObzor.Click += buttonObzor_Click;
            // Настройка заголовка и текста кнопки в зависимости от режима
            if (isEditMode)
            {
                this.Text = "Редактирование партнера";
                buttonAddPartner.Text = "Сохранить изменения";
            }
            else
            {
                this.Text = "Добавление партнера";
                buttonAddPartner.Text = "Добавить партнера";
            }
        }

        // Копируем выбранный файл в папку Images приложения и возвращаем имя файла
        private string CopySelectedImageToImagesFolder()
        {
            if (string.IsNullOrEmpty(openFileDialog1.FileName) || !File.Exists(openFileDialog1.FileName))
                return null;

            try
            {
                string imagesDir = Path.Combine(Environment.CurrentDirectory, "Images");
                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                string sourcePath = openFileDialog1.FileName;
                string originalName = Path.GetFileName(sourcePath);
                string destPath = Path.Combine(imagesDir, originalName);

                // Если источник уже находится в папке Images — ничего не копируем
                if (Path.GetFullPath(sourcePath).Equals(Path.GetFullPath(destPath), StringComparison.OrdinalIgnoreCase))
                {
                    return originalName;
                }
                // Создаём уникальное имя файла, если в папке уже есть файл с таким именем
                string destFileName = originalName;
                string destFullPath = Path.Combine(imagesDir, destFileName);
                while (File.Exists(destFullPath))
                {
                    destFileName = Path.GetFileNameWithoutExtension(originalName) + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + Path.GetExtension(originalName);
                    destFullPath = Path.Combine(imagesDir, destFileName);
                }
                // Копируем потоками, разрешая чтение исходного файла даже если другой процесс держит его открытым для чтения
                using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var destStream = new FileStream(destFullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    sourceStream.CopyTo(destStream);
                }
                return destFileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось скопировать изображение: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        // Метод для добавления нового партнера в базу данных
        private void AddNewPartner()
        {
            // Если пользователь выбрал изображение — копируем и сохраняем имя файла
            selectedImageFileName = CopySelectedImageToImagesFolder();

            // Подключение к базе данных
            using (NpgsqlConnection connection = new NpgsqlConnection(Resources.connectBD))
            {
                // Открытие соединения
                connection.Open();
                // SQL-запрос для вставки нового партнера (включаем url_image)
                string query = "INSERT INTO public.partner_test(type_partner_id, name_partner_pk, director, mail, phone_number, adress, inn, rating, url_image)" +
                    "VALUES(@type_partner_id, @name_partner_pk, @director, @mail, @phone_number, @adress, @inn, @rating, @url_image)";
                // Создание команды с параметрами
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    //  Добавление параметров к команде
                    command.Parameters.AddWithValue("type_partner_id", comboType.Text);
                    command.Parameters.AddWithValue("name_partner_pk", textNamePartner.Text);
                    command.Parameters.AddWithValue("director", textDirector.Text);
                    command.Parameters.AddWithValue("mail", textMail.Text);
                    command.Parameters.AddWithValue("phone_number", maskedPhone.Text.Remove(0, 3));
                    command.Parameters.AddWithValue("adress", textAdress.Text);
                    command.Parameters.AddWithValue("inn", long.Parse(maskedInn.Text));
                    command.Parameters.AddWithValue("rating", int.Parse(numericRating.Text));
                    command.Parameters.AddWithValue("url_image", selectedImageFileName != null ? (object)selectedImageFileName : (object)DBNull.Value);
                    //  Выполнение команды
                    command.ExecuteNonQuery();
                    {
                        // Закрытие соединения
                        connection.Close();
                    }
                    // Создание и настройка панели партнера для отображения в интерфейсе
                    PanelPartner panelPartner = new PanelPartner();
                    panelPartner.Namepartner = textNamePartner.Text;
                    panelPartner.TypePartner = comboType.Text;
                    panelPartner.Director = "Директор: " + textDirector.Text;
                    panelPartner.Phone = "Телефон: " + maskedPhone.Text;
                    panelPartner.Rating = "Рейтинг: " + numericRating.Text;
                    panelPartner.Discount = "Скидка: 0%";
                    panelPartner.UrlImage = selectedImageFileName;
                    //Для событий объявил Инн, чтобы редачить проверку
                    panelPartner.Inn = maskedInn.Text;
                    // Установка меток на панели
                    panelPartner.setLabels();
                    // Добавление панели партнера в FlowLayoutPanel
                    flowLayoutPanel.Controls.Add(panelPartner);
                    MessageBox.Show("Партнер успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Закрытие формы
                    this.Close();
                }
            }
        }
        
        // Метод для обновления информации о существующем партнере в базе данных
        private void UpdatePartner()
        {
            // Если пользователь выбрал новое изображение — скопируем и обновим selectedImageFileName
            if (!string.IsNullOrEmpty(openFileDialog1.FileName) && File.Exists(openFileDialog1.FileName))
            {
                selectedImageFileName = CopySelectedImageToImagesFolder();
            }

            using (NpgsqlConnection connection = new NpgsqlConnection(Resources.connectBD))
            {
                connection.Open();
                string query = @"UPDATE public.partner_test
	                            SET type_partner_id=@type_partner_id, name_partner_pk=@name_partner_pk, director=@director, mail=@mail, phone_number=@phone_number, adress=@adress, inn=@inn, rating=@rating, url_image=@url_image
	                            WHERE name_partner_pk=@name_partner;";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("type_partner_id", comboType.Text);
                    command.Parameters.AddWithValue("name_partner_pk", textNamePartner.Text);
                    command.Parameters.AddWithValue("director", textDirector.Text);
                    command.Parameters.AddWithValue("mail", textMail.Text);
                    command.Parameters.AddWithValue("phone_number", maskedPhone.Text.Remove(0, 3));
                    command.Parameters.AddWithValue("adress", textAdress.Text);
                    command.Parameters.AddWithValue("inn", long.Parse(maskedInn.Text));
                    command.Parameters.AddWithValue("rating", int.Parse(numericRating.Text));
                    command.Parameters.AddWithValue("name_partner", NamePartnerToEdit);
                    command.Parameters.AddWithValue("url_image", selectedImageFileName != null ? (object)selectedImageFileName : (object)DBNull.Value);
                    command.ExecuteNonQuery();
                    {
                            connection.Close();
                    }
                    MessageBox.Show("Партнер успешно обновлён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    panelPartner.Namepartner = textNamePartner.Text;
                    panelPartner.TypePartner = comboType.Text;
                    panelPartner.Director = "Директор: " + textDirector.Text;
                    panelPartner.Phone = "Телефон: " + maskedPhone.Text;
                    panelPartner.Rating = "Рейтинг: " + numericRating.Text;
                    panelPartner.Discount = panelPartner.Discount;
                    panelPartner.UrlImage = selectedImageFileName;
                    panelPartner.Inn = maskedInn.Text;
                    panelPartner.setLabels();
                    this.Close();
                }
            }
        }
        private void AddPartner_Load(object sender, EventArgs e)
        {
            if (!isEditMode)
                return;

            if (string.IsNullOrEmpty(NamePartnerToEdit))
                return;

            LoadPartnerData();
        }

        private void buttonObzor_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Копируем выбранное изображение сразу в папку Images и используем копию для превью
                string copiedName = CopySelectedImageToImagesFolder();
                if (!string.IsNullOrEmpty(copiedName))
                {
                    selectedImageFileName = copiedName;
                    string path = Path.Combine(Environment.CurrentDirectory, "Images", copiedName);
                    if (File.Exists(path))
                        pictureBox1.ImageLocation = path;
                    else
                        pictureBox1.ImageLocation = openFileDialog1.FileName; // fallback
                }
                else
                {
                    // если копирование не удалось — показываем оригинал
                    pictureBox1.ImageLocation = openFileDialog1.FileName;
                }
            }
        }
        // Загрузка данных партнера для редактирования
        private void LoadPartnerData()
        {
            try
            {
                using (var connection = new NpgsqlConnection(Resources.connectBD))
                {
                    connection.Open();
                    string query = "SELECT type_partner_id, name_partner_pk, director, mail, phone_number, adress, inn, rating, url_image FROM public.partner_test WHERE name_partner_pk = @name";
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("name", NamePartnerToEdit);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                PanelPartner panelPartner = new PanelPartner();
                                comboType.Text = reader.GetString(0);
                                textNamePartner.Text = reader.GetString(1);
                                textDirector.Text = reader.GetString(2);
                                textMail.Text = reader.GetString(3);
                                maskedPhone.Text = reader.GetString(4);
                                textAdress.Text = reader.GetString(5);
                                maskedInn.Text = reader.GetInt64(6).ToString();
                                numericRating.Text = reader.GetInt32(7).ToString();
                                selectedImageFileName = reader["url_image"] != DBNull.Value ? reader["url_image"].ToString() : null;
                                if (!string.IsNullOrEmpty(selectedImageFileName))
                                {
                                    var path = Path.Combine(Environment.CurrentDirectory, "Images", selectedImageFileName);
                                    if (File.Exists(path))
                                        pictureBox1.ImageLocation = path;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных партнера: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void buttonShowHistory_Click(object sender, EventArgs e)
        {
            HistoryForm historyForm = new HistoryForm(NamePartnerToEdit);
            historyForm.ShowDialog();
        }

        private void buttonObzor_Click_1(object sender, EventArgs e)
        {

        }

        private void maskedPhone_Enter(object sender, EventArgs e)
        {
            maskedPhone.SelectionStart = 3;
        }

        private void maskedPhone_Click(object sender, EventArgs e)
        {
            if (maskedPhone.Text != "+7            ")
            {
                maskedPhone.Text = "+7            ";
            }
            maskedPhone.SelectionStart = 3;
        }
        private void maskedPhone_Leave(object sender, EventArgs e)
        {
            if (maskedPhone.Text == "+7            ")
            {
                maskedPhone.Text = panelPartner.Phone.Replace(" +7 ", "");
            }
        }

        private void maskedInn_Click(object sender, EventArgs e)
        {
           
            if (panelPartner == null)
            {
                panelPartner = new PanelPartner();
            }
            panelPartner.Inn = "";
            if (maskedInn.Text != "          ")
            {
                panelPartner.Inn = maskedInn.Text;
                maskedInn.Text = "          ";
            }
            maskedInn.SelectionStart = 0;
        }
        private void maskedInn_Leave(object sender, EventArgs e)
        {
            if (maskedInn.Text == "")
            {
                maskedInn.Text = panelPartner.Inn;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }    
    }
}
