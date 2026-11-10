using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace KPSS
{
    public partial class Form1 : Form
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private int saniye = 0;
        private bool calisiyorMu = false;
        private Button aktifButon;

        public Form1()
        {
            InitializeComponent();

            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KPSS_Takip_Paneli"
            );

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _dbPath = Path.Combine(dir, "KPSSDB.db");

            string defaultDb = Path.Combine(Application.StartupPath, "KPSSDB.db");

            // Sadece ilk kurulurken kopyala
            if (!File.Exists(_dbPath) && File.Exists(defaultDb))
                File.Copy(defaultDb, _dbPath, overwrite: false);

            _connectionString = $"Data Source={_dbPath};Version=3;";
        }

        #region Veritabanı ve Kurulum

        void VeritabaniKur()
        {
            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                con.Open();

                string sql = @"
                CREATE TABLE IF NOT EXISTS CalismaKayitlari (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Tarih TEXT,
                    Ders TEXT,
                    SureDakika INTEGER,
                    Soru INTEGER,
                    Notlar TEXT
                );

                CREATE TABLE IF NOT EXISTS DenemeKayitlari (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Tarih TEXT,
                    Turkce_Dogru INTEGER,
                    Turkce_Yanlis INTEGER,
                    Turkce_Bos INTEGER,
                    Matematik_Dogru INTEGER,
                    Matematik_Yanlis INTEGER,
                    Matematik_Bos INTEGER,
                    Tarih_Dogru INTEGER,
                    Tarih_Yanlis INTEGER,
                    Tarih_Bos INTEGER,
                    Cografya_Dogru INTEGER,
                    Cografya_Yanlis INTEGER,
                    Cografya_Bos INTEGER,
                    Vatandaslik_Dogru INTEGER,
                    Vatandaslik_Yanlis INTEGER,
                    Vatandaslik_Bos INTEGER,
                    Guncel_Dogru INTEGER,
                    Guncel_Yanlis INTEGER,
                    Guncel_Bos INTEGER
                );

                CREATE TABLE IF NOT EXISTS BransDenemeleri (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Tarih TEXT,
                    Ders TEXT,
                    Dogru INTEGER,
                    Yanlis INTEGER,
                    Bos INTEGER
                );";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Sınav Türü ve Hedef Puan Yönetimi

        DateTime SinavTarihiGetir(string sinavTuru)
        {
            return sinavTuru switch
            {
                "Lisans" => new DateTime(2027, 9, 12),
                "Önlisans" => new DateTime(2026, 9, 20),
                "Ortaöğretim" => new DateTime(2026, 10, 25),
                "AGS (Akademi Giriş Sınavı)" => new DateTime(2027, 5, 8),
                _ => new DateTime(2026, 10, 25)
            };
        }

        void SinavTuruSorVeKaydet()
        {
            string secim = Interaction.InputBox(
                "Hangi sınava hazırlanıyorsun?\n\n" +
                "1 - KPSS Lisans\n" +
                "2 - KPSS Önlisans\n" +
                "3 - KPSS Ortaöğretim\n" +
                "4 - AGS (Akademi Giriş Sınavı)\n\n" +
                "Lütfen seçiminizi girin (1-4):",
                "Sınav Türü Belirleme",
                "3"
            );

            string sinav = secim switch
            {
                "1" => "Lisans",
                "2" => "Önlisans",
                "3" => "Ortaöğretim",
                "4" => "AGS (Akademi Giriş Sınavı)",
                _ => "Ortaöğretim"
            };

            Properties.Settings.Default.SecilenSinavTuru = sinav;
            Properties.Settings.Default.Save();
        }

        void HedefPuanSorVeKaydet()
        {
            string giris = Interaction.InputBox(
                "Hedeflediğiniz Puanı Giriniz :)",
                "Hedef Puan Belirleme",
                "85"
            );

            if (int.TryParse(giris, out int puan))
            {
                Properties.Settings.Default.HedefPuan = puan;
                Properties.Settings.Default.Save();
                lblHedefPuan.Text = puan.ToString();
                MessageBox.Show("Hedef puanın kaydedildi 💪", "Bilgi");
            }
            else
            {
                MessageBox.Show("Geçerli bir sayı girmelisin.", "Uyarı");
            }
        }

        void KalanGunuGuncelle()
        {
            string secilenSinav = Properties.Settings.Default.SecilenSinavTuru;
            if (string.IsNullOrEmpty(secilenSinav))
            {
                secilenSinav = "Ortaöğretim";
            }

            DateTime sinavTarihi = SinavTarihiGetir(secilenSinav);
            TimeSpan kalanSure = sinavTarihi.Date - DateTime.Now.Date;

            if (kalanSure.Days >= 0)
            {
                lblKalanGun.Text = kalanSure.Days.ToString();
            }
            else
            {
                lblKalanGun.Text = "Tamamlandı";
            }
        }

        #endregion

        #region Form Olayları ve Navigasyon

        private void Form1_Load(object sender, EventArgs e)
        {
            VeritabaniKur();

            // Form kenarlarını yuvarlama
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, 40, 40, 180, 90);
            path.AddArc(this.Width - 40, 0, 40, 40, 270, 90);
            path.AddArc(this.Width - 40, this.Height - 40, 40, 40, 0, 90);
            path.AddArc(0, this.Height - 40, 40, 40, 90, 90);
            path.CloseAllFigures();
            this.Region = new Region(path);

            this.BackColor = ColorTranslator.FromHtml("#A24671");

            // Sınav Türü Kontrolü
            if (string.IsNullOrEmpty(Properties.Settings.Default.SecilenSinavTuru))
            {
                SinavTuruSorVeKaydet();
            }

            // Hedef Puan Kontrolü
            if (Properties.Settings.Default.HedefPuan == 0)
            {
                HedefPuanSorVeKaydet();
            }
            else
            {
                lblHedefPuan.Text = Properties.Settings.Default.HedefPuan.ToString();
            }

            // Başlangıç ekranını Dashboard yap
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(pnlDashboard);
            pnlDashboard.Dock = DockStyle.Fill;
            pnlDashboard.Visible = true;

            pnlKayitEkle.Visible = false;
            pnlKronometre.Visible = false;
            pnlDenemeGir.Visible = false;
            pnlDenemeList.Visible = false;
            pnlSoruAnaliz.Visible = false;
            pnlBransDeneme.Visible = false;

            DashboardVerileriYukle();
            DersDagilimiYukle();
        }

        void MenuButonAktifYap(Button btn)
        {
            if (aktifButon != null)
            {
                aktifButon.BackColor = Color.FromArgb(238, 139, 136);
                aktifButon.ForeColor = Color.FromArgb(253, 238, 226);
            }

            btn.BackColor = Color.FromArgb(255, 170, 170);
            btn.ForeColor = Color.White;
            aktifButon = btn;
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            MenuButonAktifYap(btnDashboard);
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(pnlDashboard);
            pnlDashboard.Dock = DockStyle.Fill;
            pnlDashboard.Visible = true;

            DashboardVerileriYukle();
            DersDagilimiYukle();
        }

        private void btnCalismaKaydi_Click(object sender, EventArgs e)
        {
            MenuButonAktifYap(btnCalismaKaydi);
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(pnlKayitEkle);
            pnlKayitEkle.Dock = DockStyle.Fill;
            pnlKayitEkle.Visible = true;
            DashboardVerileriYukle();
        }

        private void btnCalismaSureleri_Click(object sender, EventArgs e)
        {
            MenuButonAktifYap(btnCalismaSureleri);
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(pnlKronometre);
            pnlKronometre.Dock = DockStyle.Fill;
            pnlKronometre.Visible = true;
        }

        private void btnDenemeGiris_Click(object sender, EventArgs e)
        {
            MenuButonAktifYap(btnDenemeGiris);
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(pnlDenemeGir);
            pnlDenemeGir.Dock = DockStyle.Fill;
            pnlDenemeGir.Visible = true;
            DashboardVerileriYukle();
        }

        private void btnDenemeListesi_Click(object sender, EventArgs e)
        {
            MenuButonAktifYap(btnDenemeListesi);
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(pnlDenemeList);
            pnlDenemeList.Dock = DockStyle.Fill;
            pnlDenemeList.Visible = true;

            DenemeListesiYukle();
            SonDenemeOzetYaz();
        }

        private void btnCozulenSorular_Click(object sender, EventArgs e)
        {
            MenuButonAktifYap(btnCozulenSorular);
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(pnlSoruAnaliz);
            pnlSoruAnaliz.Dock = DockStyle.Fill;
            pnlSoruAnaliz.Visible = true;

            SoruToplamlariYukle();
        }

        private void btnBransDeneme_Click(object sender, EventArgs e)
        {
            MenuButonAktifYap(btnBransDeneme);
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(pnlBransDeneme);
            pnlBransDeneme.Dock = DockStyle.Fill;
            pnlBransDeneme.Visible = true;

            BransListeYukle();
        }

        private void pnlCardGun_Paint(object sender, PaintEventArgs e)
        {
            KalanGunuGuncelle();
        }

        #endregion

        #region Dashboard ve İstatistikler

        private void DashboardVerileriYukle()
        {
            KalanGunuGuncelle();

            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                con.Open();

                // Toplam Çalışma Süresi
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT SUM(SureDakika) FROM CalismaKayitlari", con))
                {
                    object sonuc = cmd.ExecuteScalar();
                    int dakika = (sonuc != DBNull.Value && sonuc != null) ? Convert.ToInt32(sonuc) : 0;
                    int saat = dakika / 60;
                    int dk = dakika % 60;
                    lblToplamSure.Text = $"{saat}s {dk}dk";
                }

                // Toplam Çözülen Soru
                using (SQLiteCommand cmd2 = new SQLiteCommand("SELECT SUM(Soru) FROM CalismaKayitlari", con))
                {
                    object soruSonuc = cmd2.ExecuteScalar();
                    lblToplamSoru.Text = (soruSonuc != DBNull.Value && soruSonuc != null) ? soruSonuc.ToString() : "0";
                }
            }

            string[] sozler =
            {
                "Bu masa, geleceğini yazdığın yer.",
                "Bugün çalışıyorsun, yarın rahat ediyorsun.",
                "Şu an zor ama vazgeçersen daha zor.",
                "Ben sana inanıyorum, gerisi zaten geliyor.",
                "Yorulduğunda durma, sadece bana bak.",
                "Bu yolun sonunda ikimiz de kazanıyoruz.",
                "Sen çalışırken ben seninle gurur duyuyorum.",
                "Bir gün bu günleri ‘iyi ki’ diye hatırlayacağız.",
                "Bugün yorgunsan, yarın ben taşırım seni.",
                "Bu masada yalnızca ders yok, umut var.",
                "Seni sadece sonuçta değil, bu süreçte de seviyorum.",
                "Yapabilirsin!",
                "Gözlerin dolsa bile, geleceğin çok aydınlık.",
                "Birlikte hayal kurduk, şimdi sıra gerçekleştirmekte.",
                "Hayalin senden vazgeçmez.",
                "Vazgeçmek yok!",
                "Sen kendine inanmasan bile, ben sana inanıyorum.",
                "Her sayfa seni hayaline biraz daha yaklaştırıyor.",
                "Yorulduğunu biliyorum ama yalnız değilsin.",
                "Hedefine kilitlen.",
                "Senden daha iyisi yok!",
                "Kendine güven.",
                "Bugün emek, yarın sonuç."
            };

            Random r = new Random();
            lblGunlukSoz.Text = sozler[r.Next(sozler.Length)];
        }

        void DersDagilimiYukle()
        {
            Dictionary<string, int> dersler = new()
            {
                { "Türkçe", 0 },
                { "Matematik", 0 },
                { "Tarih", 0 },
                { "Coğrafya", 0 },
                { "Vatandaşlık", 0 }
            };

            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT Ders, SUM(SureDakika) FROM CalismaKayitlari GROUP BY Ders", con))
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string ders = dr[0].ToString();
                        int dakika = Convert.ToInt32(dr[1]);
                        if (dersler.ContainsKey(ders))
                        {
                            dersler[ders] = dakika;
                        }
                    }
                }
            }

            BarAyarla(barTR, dersler["Türkçe"], lblTR_Sure);
            BarAyarla(barMAT, dersler["Matematik"], lblMAT_Sure);
            BarAyarla(barTAR, dersler["Tarih"], lblTAR_Sure);
            BarAyarla(barCOG, dersler["Coğrafya"], lblCOG_Sure);
            BarAyarla(barVAT, dersler["Vatandaşlık"], lblVAT_Sure);
        }

        void BarAyarla(Panel bar, int dakika, Label lbl)
        {
            int maxWidth = 200;
            int width = Math.Clamp(dakika, 1, 4000) * maxWidth / 4000;
            bar.Width = width;

            int saat = dakika / 60;
            int dk = dakika % 60;
            lbl.Text = $"{saat}s {dk}dk";
        }

        void SoruToplamlariYukle()
        {
            lblTR_Soru.Text = "0";
            lblMAT_Soru.Text = "0";
            lblTAR_Soru.Text = "0";
            lblCOG_Soru.Text = "0";
            lblVAT_Soru.Text = "0";
            lblGNC_Soru.Text = "0";

            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT Ders, SUM(Soru) FROM CalismaKayitlari GROUP BY Ders", con))
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string ders = dr[0].ToString();
                        int toplam = Convert.ToInt32(dr[1]);

                        switch (ders)
                        {
                            case "Türkçe": lblTR_Soru.Text = toplam.ToString(); break;
                            case "Matematik": lblMAT_Soru.Text = toplam.ToString(); break;
                            case "Tarih": lblTAR_Soru.Text = toplam.ToString(); break;
                            case "Coğrafya": lblCOG_Soru.Text = toplam.ToString(); break;
                            case "Vatandaşlık": lblVAT_Soru.Text = toplam.ToString(); break;
                            case "Güncel": lblGNC_Soru.Text = toplam.ToString(); break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Kronometre

        private void btnBaslat_Click(object sender, EventArgs e)
        {
            calisiyorMu = true;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            saniye++;
            TimeSpan t = TimeSpan.FromSeconds(saniye);
            lblSure.Text = t.ToString(@"hh\:mm\:ss");
        }

        private void btnDur_Click(object sender, EventArgs e)
        {
            calisiyorMu = false;
            timer1.Stop();
        }

        private void btnSifirla_Click(object sender, EventArgs e)
        {
            saniye = 0;
            lblSure.Text = "00:00:00";
        }

        private void btnZamanKaydet_Click(object sender, EventArgs e)
        {
            if (cbZamanDers.SelectedIndex == -1)
            {
                MessageBox.Show("Ders seçmelisin!");
                return;
            }

            int dakika = saniye / 60;

            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(
                    "INSERT INTO CalismaKayitlari (Tarih, Ders, SureDakika, Soru, Notlar) VALUES (@tarih,@ders,@sure,0,'Kronometre')", con))
                {
                    cmd.Parameters.AddWithValue("@tarih", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ders", cbZamanDers.Text);
                    cmd.Parameters.AddWithValue("@sure", dakika);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("⏳ Süre başarıyla kaydedildi!");
            saniye = 0;
            lblSure.Text = "00:00:00";
            DashboardVerileriYukle();
            DersDagilimiYukle();
        }

        #endregion

        #region Kayıt ve Deneme İşlemleri

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            int toplamDakika = (int)numSaat.Value * 60 + (int)numDakika.Value;

            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(
                    "INSERT INTO CalismaKayitlari (Tarih, Ders, SureDakika, Soru, Notlar) VALUES (@tarih,@ders,@sure,@soru,@not)", con))
                {
                    cmd.Parameters.AddWithValue("@tarih", dtTarih.Value);
                    cmd.Parameters.AddWithValue("@ders", cbDers.Text);
                    cmd.Parameters.AddWithValue("@sure", toplamDakika);
                    cmd.Parameters.AddWithValue("@soru", numSoru.Value);
                    cmd.Parameters.AddWithValue("@not", txtNot.Text);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("✔ Çalışma Kaydı Eklendi!", "Başarılı");

            numSaat.Value = 0;
            numDakika.Value = 0;
            numSoru.Value = 0;
            cbDers.SelectedIndex = -1;
            txtNot.Clear();

            DashboardVerileriYukle();
        }

        private void btnDenemeKaydet_Click(object sender, EventArgs e)
        {
            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(@"
                INSERT INTO DenemeKayitlari 
                (Tarih,
                Turkce_Dogru, Turkce_Yanlis, Turkce_Bos,
                Matematik_Dogru, Matematik_Yanlis, Matematik_Bos,
                Tarih_Dogru, Tarih_Yanlis, Tarih_Bos,
                Cografya_Dogru, Cografya_Yanlis, Cografya_Bos,
                Vatandaslik_Dogru, Vatandaslik_Yanlis, Vatandaslik_Bos,
                Guncel_Dogru, Guncel_Yanlis, Guncel_Bos)
                VALUES
                (@tarih,
                @td,@ty,@tb,
                @md,@my,@mb,
                @trd,@try,@trb,
                @cgd,@cgy,@cgb,
                @vd,@vy,@vb,
                @gd,@gy,@gb)", con))
                {
                    cmd.Parameters.AddWithValue("@tarih", DateTime.Now);
                    cmd.Parameters.AddWithValue("@td", numTD.Value);
                    cmd.Parameters.AddWithValue("@ty", numTY.Value);
                    cmd.Parameters.AddWithValue("@tb", numTB.Value);

                    cmd.Parameters.AddWithValue("@md", numMD.Value);
                    cmd.Parameters.AddWithValue("@my", numMY.Value);
                    cmd.Parameters.AddWithValue("@mb", numMB.Value);

                    cmd.Parameters.AddWithValue("@trd", numTRD.Value);
                    cmd.Parameters.AddWithValue("@try", numTRY.Value);
                    cmd.Parameters.AddWithValue("@trb", numTRB.Value);

                    cmd.Parameters.AddWithValue("@cgd", numCGD.Value);
                    cmd.Parameters.AddWithValue("@cgy", numCGY.Value);
                    cmd.Parameters.AddWithValue("@cgb", numCGB.Value);

                    cmd.Parameters.AddWithValue("@vd", numVTD.Value);
                    cmd.Parameters.AddWithValue("@vy", numVTY.Value);
                    cmd.Parameters.AddWithValue("@vb", numVTB.Value);

                    cmd.Parameters.AddWithValue("@gd", numGD.Value);
                    cmd.Parameters.AddWithValue("@gy", numGY.Value);
                    cmd.Parameters.AddWithValue("@gb", numGB.Value);

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("📌 Deneme Kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        double Net(DataGridViewRow r, string d, string y)
        {
            return Convert.ToDouble(r.Cells[d].Value) - Convert.ToDouble(r.Cells[y].Value) * 0.25;
        }

        void DenemeListesiYukle()
        {
            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                con.Open();
                using (SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT * FROM DenemeKayitlari", con))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;

                    if (!dataGridView1.Columns.Contains("ToplamNet"))
                        dataGridView1.Columns.Add("ToplamNet", "Toplam Net");

                    if (!dataGridView1.Columns.Contains("Puan"))
                        dataGridView1.Columns.Add("Puan", "Puan");

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.IsNewRow) continue;

                        double tr = Net(row, "Turkce_Dogru", "Turkce_Yanlis");
                        double mat = Net(row, "Matematik_Dogru", "Matematik_Yanlis");
                        double tar = Net(row, "Tarih_Dogru", "Tarih_Yanlis");
                        double cog = Net(row, "Cografya_Dogru", "Cografya_Yanlis");
                        double vat = Net(row, "Vatandaslik_Dogru", "Vatandaslik_Yanlis");
                        double gnc = Net(row, "Guncel_Dogru", "Guncel_Yanlis");

                        double toplam = tr + mat + tar + cog + vat + gnc;
                        double puan = (toplam * 0.45) + 50;

                        row.Cells["ToplamNet"].Value = toplam.ToString("0.00");
                        row.Cells["Puan"].Value = puan.ToString("0.00");
                    }
                }
            }
        }

        void SonDenemeOzetYaz()
        {
            if (dataGridView1.Rows.Count <= 1) return;

            int lastIndex = dataGridView1.AllowUserToAddRows
                            ? dataGridView1.Rows.Count - 2
                            : dataGridView1.Rows.Count - 1;

            if (lastIndex < 0) return;

            var row = dataGridView1.Rows[lastIndex];

            double tr = Net(row, "Turkce_Dogru", "Turkce_Yanlis");
            double mat = Net(row, "Matematik_Dogru", "Matematik_Yanlis");
            double tar = Net(row, "Tarih_Dogru", "Tarih_Yanlis");
            double cog = Net(row, "Cografya_Dogru", "Cografya_Yanlis");
            double vat = Net(row, "Vatandaslik_Dogru", "Vatandaslik_Yanlis");
            double gnc = Net(row, "Guncel_Dogru", "Guncel_Yanlis");

            lblTrNet.Text = tr.ToString("0.00");
            lblMatNet.Text = mat.ToString("0.00");
            lblTarNet.Text = tar.ToString("0.00");
            lblCogNet.Text = cog.ToString("0.00");
            lblVatNet.Text = vat.ToString("0.00");
            lblGncNet.Text = gnc.ToString("0.00");

            double toplam = tr + mat + tar + cog + vat + gnc;
            lblToplamNet.Text = toplam.ToString("0.00");

            double puan = (toplam * 0.45) + 50;
            lblPuan.Text = puan.ToString("0.00");
        }

        void BransListeYukle()
        {
            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                con.Open();
                using (SQLiteDataAdapter da = new SQLiteDataAdapter(
                    "SELECT Id, Tarih, Ders, Dogru, Yanlis, Bos, " +
                    "(Dogru - Yanlis * 0.25) AS Net " +
                    "FROM BransDenemeleri ORDER BY Ders, Id DESC", con))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgBransListe.DataSource = dt;
                }
            }
        }

        private void btnBransKaydet_Click_1(object sender, EventArgs e)
        {
            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(
                    "INSERT INTO BransDenemeleri (Tarih, Ders, Dogru, Yanlis, Bos) VALUES (@tarih,@ders,@d,@y,@b)", con))
                {
                    cmd.Parameters.AddWithValue("@tarih", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ders", cbBransDers.Text);
                    cmd.Parameters.AddWithValue("@d", numBransDogru.Value);
                    cmd.Parameters.AddWithValue("@y", numBransYanlis.Value);
                    cmd.Parameters.AddWithValue("@b", numBransBos.Value);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("✔ Branş denemesi kaydedildi!", "Başarılı");
            BransListeYukle();
        }

        #endregion

        #region Sistem ve Pencere Kontrolleri

        private void btnFabrikaAyari_Click(object sender, EventArgs e)
        {
            DialogResult onay = MessageBox.Show(
                "Tüm veriler silinecek ve uygulama başa dönecek!\nDevam etmek istiyor musun?",
                "SIFIRLAMA ONAYI",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (onay == DialogResult.No) return;

            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                con.Open();
                new SQLiteCommand("DELETE FROM CalismaKayitlari", con).ExecuteNonQuery();
                new SQLiteCommand("DELETE FROM DenemeKayitlari", con).ExecuteNonQuery();
                new SQLiteCommand("DELETE FROM BransDenemeleri", con).ExecuteNonQuery();
            }

            Properties.Settings.Default.HedefPuan = 0;
            Properties.Settings.Default.SecilenSinavTuru = "";
            Properties.Settings.Default.Save();

            MessageBox.Show("Tüm veriler sıfırlandı. Uygulama yeniden başlatılacak.", "Temizlendi");
            Application.Restart();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        #endregion
    }
}