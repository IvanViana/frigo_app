using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using FirebirdSql.Data.FirebirdClient;
using System.IO;
using System.Xml.Linq;

namespace frigobom_c
{
    public partial class Form1 : Form
    {
        private string arq_migracao;

        private string strConn ;
        FbConnection conn;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //timer1.Enabled = true;

           }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // exibir_azure();
            timer1.Enabled = false;
            button2.Text = "sem timer";
            MessageBox.Show("Conectado", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void bt_azure_Click(object sender, EventArgs e)
        {

            


            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                /*  builder.DataSource = "tadeu.database.windows.net";
                  builder.UserID = "tadeu";
                  builder.Password = "Frigobom123!";
                  builder.InitialCatalog = "frigobom";*/

                builder.DataSource = txt_azure_ds.Text.ToString();
                builder.UserID = txt_azure_user.Text.ToString();
                builder.Password = txt_azure_pass.Text.ToString();
                builder.InitialCatalog = txt_azure_db.Text.ToString();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    Console.WriteLine("\nQuery data example:");
                    Console.WriteLine("=========================================\n");

                    connection.Open();
                    StringBuilder sb = new StringBuilder();



                    sb.Append("SELECT TOP(1000) nome, tipo_pessoa, id_produto, quantidade, precounitario, id , id_seq");
                    sb.Append(" FROM dbo.vendas_import;");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                
                                dataGridView1.Rows.Add(reader.GetString(0),reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6));

                                 //  Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (SqlException eg)
            {
                // Console.WriteLine(eg.ToString());
            }
            // Console.ReadLine();
            
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //Logica para tarefa em background ser feita
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Reporta o progresso do BackgroundWorker
        }
        public static string CaminhoDadosXML(string caminho)
        {

            if (caminho.IndexOf("\\bin\\Debug") != 0)
            {
                caminho = caminho.Replace("\\bin\\Debug", "");
            }
            return caminho;
        }


        private void bt_firebird_Click(object sender, EventArgs e)
        {
            string caminho;
            string firebird_ds="";
            string firebird_db="";
            string firebird_user="";
            string firebird_pass="";

            caminho = AppDomain.CurrentDomain.BaseDirectory;

            // adicionar um try caso o arquivo não exista
            var prods = from p in XElement.Load((CaminhoDadosXML(caminho) + @"Dados\Conexoes.xml")).Elements("Conexao")
                        where p.Element("tipo").Value == "fb"
                        select new
                        {
                            servidor = p.Element("servidor").Value,
                            usuario = p.Element("usuario").Value,
                            senha = p.Element("senha").Value,
                            banco = p.Element("banco").Value,
                        };

            // Executa a consulta
            foreach (var produto in prods)
            {
                firebird_ds = produto.servidor;
                firebird_db = produto.banco;
                firebird_user = produto.usuario;
                firebird_pass = produto.senha;
            }

            if (testar_conexao(firebird_ds, firebird_db, firebird_user, firebird_pass) == false) {
                return;

            }


            strConn = "@DataSource=" + firebird_ds + "; Database="+ firebird_db + "; username= "+ firebird_user + "; password = "+ firebird_pass + "";
            conn = new FbConnection(strConn);
         


            String fb_mysql;
            fb_mysql = @"select first 1000
                        tb_pessoas.nome,
                        tb_pessoas.tipo_pessoa,
                        mov_nf_itens.id_produto,
                        mov_nf_itens.quantidade,
                        mov_nf_itens.precounitario,
                        mov_nf_itens.id,
                        mov_nf_itens.id_seq
                    from mov_nf_itens
                       inner join mov_nf on (mov_nf_itens.id = mov_nf.id)
                       left outer join tb_pessoas on (mov_nf.id_pessoa = tb_pessoas.id)
                       left outer join tb_produtos on (mov_nf_itens.id_produto = tb_produtos.id)
                    where 
                       (
                          (tb_pessoas.nome is not null )
                       )";

            FbCommand cmd = new FbCommand(fb_mysql, conn);
           // FbDataAdapter DA = new FbDataAdapter(cmd);
            FbDataAdapter DA2 = new FbDataAdapter(cmd);
            DataSet DS = new DataSet();
            DataTable DT = new DataTable();
            conn.Open();
            
          //  DA.Fill(DS, "teste");
            DA2.Fill(DT);

           // dataGridView2.DataSource = DS;
           // dataGridView2.DataMember = "teste";

            //textBox2.Text += "fim conexao";
          //  textBox2.Text += "GErar SQL";

            //comecar o loop
            string str = string.Empty;
            string str2 = string.Empty;

            int i = 0;
            foreach (DataRow dRow in DT.Rows)
            {
                i++;
                if (i == 1)
                {
                    str += "('" + dRow["nome"].ToString() + "','" + dRow["tipo_pessoa"].ToString() + "','" + dRow["id_produto"].ToString() + "','" + dRow["quantidade"].ToString() + "','" + dRow["precounitario"].ToString() + "','" + dRow["id"].ToString() + "','" + dRow["id_seq"].ToString() + "')\r\n";
                }
                else {
                    str += ",('" + dRow["nome"].ToString() + "','" + dRow["tipo_pessoa"].ToString() + "','" + dRow["id_produto"].ToString() + "','" + dRow["quantidade"].ToString() + "','" + dRow["precounitario"].ToString() + "','" + dRow["id"].ToString() + "','" + dRow["id_seq"].ToString() + "')\r\n";
                }
                
            }
            str2 = @"INSERT INTO [dbo].[vendas_import]
           ([nome]
           ,[tipo_pessoa]
           ,[id_produto]
           ,[quantidade]
           ,[precounitario]
           ,[id]
           ,[id_seq])
           VALUES  " + str+" ";


           // textBox1.Text = str2;
           // textBox2.Text += "gerado SQL";


            string nomeArquivo = @"C:\Users\Ivan\Documents\FRIGOBOM DADOS.FDB\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".sql";

            // Cria um novo arquivo e devolve um StreamWriter para ele

            StreamWriter writer = new StreamWriter(nomeArquivo);

            // Agora é só sair escrevendo

            writer.WriteLine(str2);

            // Não esqueça de fechar o arquivo ao terminar

            writer.Close();

            conn.Close();


            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        

            builder.DataSource = txt_azure_ds.Text.ToString();
            builder.UserID = txt_azure_user.Text.ToString();
            builder.Password = txt_azure_pass.Text.ToString();
            builder.InitialCatalog = txt_azure_db.Text.ToString();

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();


                StreamReader texto = new StreamReader(nomeArquivo);
                // textBox3.Text = texto.ReadToEnd();
                String sql = texto.ReadToEnd();

                ///fecho e deleto o arquivo.
                texto.Close();
                if (File.Exists(nomeArquivo))
                {
                    File.Delete(nomeArquivo);
                }

                //exporto dados.
                SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
            }

            MessageBox.Show("Exportação Finalizada", "Acabou!! ;-)");

        }

        private void  exibir_azure()  {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                /*  builder.DataSource = "tadeu.database.windows.net";
                  builder.UserID = "tadeu";
                  builder.Password = "Frigobom123!";
                  builder.InitialCatalog = "frigobom";*/

                builder.DataSource = txt_azure_ds.Text.ToString();
                builder.UserID = txt_azure_user.Text.ToString();
                builder.Password = txt_azure_pass.Text.ToString();
                builder.InitialCatalog = txt_azure_db.Text.ToString();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    
                    
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT top(20) nome, tipo_pessoa, id_produto, quantidade, precounitario, id , id_seq");
                    sb.Append(" FROM dbo.vendas_import;");
                    String sql = sb.ToString();


                    SqlDataAdapter da = new SqlDataAdapter();
                    SqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText = sql;
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();

                    connection.Open();

                    da.Fill(ds, "teste");
   


                    bindingSource1.DataSource = ds.Tables[0].DefaultView;
                   


                    bindingNavigator1.BindingSource = bindingSource1;

                    dataGridView1.DataSource = bindingSource1;

                    //using (SqlCommand command = new SqlCommand(sql, connection))
                    //{
                    //    using (SqlDataReader reader = command.ExecuteReader())
                    //    {
                    //        while (reader.Read())
                    //        {

                    //            dataGridView1.Rows.Add(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6));

                    //            //  Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                    //        }
                    //    }
                    //}
                    connection.Close();
                }
            }
            catch (SqlException eg)
            {
                 Console.WriteLine(eg.ToString());
            }
            // Console.ReadLine();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            exibir_azure();
        }

        private void toAzureToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {
           

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void azureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frm_con_azure frm = new frm_con_azure();
            frm.Show();
        }

        private void firebirdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frm_con_fb frm1 = new frm_con_fb();
            frm1.Show();
        }

        public static Boolean testar_conexao(string ds, string db, string user, string pass)
        {
            string firebird_ds = ds;
            string firebird_db = db;
            string firebird_user = user;
            string firebird_pass = pass;
            string strConn;
            FbConnection conn;

            strConn = "@DataSource=" + firebird_ds + "; Database=" + firebird_db + "; username= " + firebird_user + "; password = " + firebird_pass + "";
            conn = new FbConnection(strConn);
            try
            {
                conn.Open();
                if (conn.State == ConnectionState.Open)
                {
                  // MessageBox.Show("Conectado", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    conn.Close();
                    return true;
                }
            }
            catch (FbException Erro)
            {

                MessageBox.Show(Erro.Message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return false;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            

            if (timer1.Enabled == true){
                timer1.Enabled = false;
                button2.Text = "sem timer";
            }
            else {
                timer1.Enabled = true;
                button2.Text = "com timer";
            }
         
        }
    }
}
