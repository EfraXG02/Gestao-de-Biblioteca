using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WIndows_Form
{
    // ============================================================================
    // 1. MODELOS DE DADOS (EQUIVALENTES ÀS STRUCTS EM C)
    // ============================================================================
    public class Livro
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Autor { get; set; } = "";
        public int AnoLancamento { get; set; }
        public bool Ativo { get; set; }
    }

    public class Aluno
    {
        public int IdEstudante { get; set; }
        public string NomeCompleto { get; set; } = "";
        public bool Ativo { get; set; }
    }

    public class Emprestimo
    {
        public int CodigoAluguel { get; set; }
        public int IdEstudante { get; set; }
        public int IdLivro { get; set; }
        public string Status { get; set; } = "PENDENTE"; // PENDENTE, ACEITE, REJEITADO, CANCELADO
    }

    public partial class Form1 : Form
    {
        // Banco de Dados em Memória (Listas Estáticas simulando os arquivos .dat)
        private static List<Livro> listaLivros = new List<Livro>();
        private static List<Aluno> listaAlunos = new List<Aluno>();
        private static List<Emprestimo> listaEmprestimos = new List<Emprestimo>();
        
        private static int proximoIdLivro = 1;
        private static int proximoCodigoAluguel = 1001;

        private Aluno? alunoLogado = null;

        // Componentes Visuais Principais
        private TabControl tabControlPrincipal;
        private TabPage pageLogin, pageADM, pageAluno;

        // Controles da Tela de Login
        private TextBox txtPinADM, txtIdAlunoLogin, txtIdAlunoCad, txtNomeAlunoCad;
        private Button btnLoginADM, btnLoginAluno, btnCadastrarAluno;

        // Controles do Painel ADM
        private TextBox txtLivroTitulo, txtLivroAutor, txtLivroAno, txtLivroIdAlterar;
        private DataGridView dgvLivrosADM, dgvAlunosADM, dgvPedidosPendentes;
        private Button btnCadastrarLivro, btnAlterarLivro, btnDeletarLivro, btnAceitarPedido, btnRejeitarPedido, btnLogoutADM;

        // Controles do Painel Aluno
        private TextBox txtBuscaTitulo;
        private DataGridView dgvLivrosAluno, dgvMeusEmprestimos;
        private Button btnSolicitarEmprestimo, btnCancelarEmprestimo, btnLogoutAluno;

        public Form1()
        {
            // Configurações Globais da Janela
            this.Text = "SISTEMA DE GESTÃO DE BIBLIOTECA CENTRAL";
            this.Size = new Size(950, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Criar o gerenciador de abas
            tabControlPrincipal = new TabControl() { Size = new Size(920, 640), Location = new Point(10, 10) };
            
            pageLogin = new TabPage("Portal de Acesso");
            pageADM = new TabPage("Painel Administrativo");
            pageAluno = new TabPage("Painel do Estudante");

            tabControlPrincipal.TabPages.Add(pageLogin);
            tabControlPrincipal.TabPages.Add(pageADM);
            tabControlPrincipal.TabPages.Add(pageAluno);
            this.Controls.Add(tabControlPrincipal);

            ConfigurarTelaLogin();
            ConfigurarTelaADM(); // <--- Vai chamar a versão responsiva com TableLayoutPanel
            ConfigurarTelaAluno();

            // Carga de dados iniciais para testes
            CargarDadosExemplo();
        }

        // ============================================================================
        // 2. CONFIGURAÇÃO DA INTERFACE GRÁFICA (LAYOUT COMBINADO MANUAL + RESPONSIVO)
        // ============================================================================
        
        private void ConfigurarTelaLogin()
        {
            // Grupo ADM (PIN 2026)
            GroupBox gbADM = new GroupBox() { Text = "Autenticação Administrativa", Location = new Point(30, 40), Size = new Size(380, 180) };
            gbADM.Controls.Add(new Label() { Text = "Introduza o PIN de Acesso ADM:", Location = new Point(20, 40), Size = new Size(200, 20) });
            txtPinADM = new TextBox() { Location = new Point(20, 70), Size = new Size(200, 20), PasswordChar = '*' };
            btnLoginADM = new Button() { Text = "Entrar como ADM", Location = new Point(20, 110), Size = new Size(150, 30) };
            btnLoginADM.Click += BtnLoginADM_Click;
            gbADM.Controls.Add(txtPinADM); gbADM.Controls.Add(btnLoginADM);

            // Grupo Aluno Login
            GroupBox gbAluno = new GroupBox() { Text = "Área do Estudante - Login", Location = new Point(450, 40), Size = new Size(380, 180) };
            gbAluno.Controls.Add(new Label() { Text = "Introduza o seu ID de Estudante:", Location = new Point(20, 40), Size = new Size(200, 20) });
            txtIdAlunoLogin = new TextBox() { Location = new Point(20, 70), Size = new Size(200, 20) };
            btnLoginAluno = new Button() { Text = "Fazer Login", Location = new Point(20, 110), Size = new Size(150, 30) };
            btnLoginAluno.Click += BtnLoginAluno_Click;
            gbAluno.Controls.Add(txtIdAlunoLogin); gbAluno.Controls.Add(btnLoginAluno);

            // Grupo Aluno Cadastro
            GroupBox gbAlunoCad = new GroupBox() { Text = "Primeiro Acesso / Criar Conta", Location = new Point(450, 250), Size = new Size(380, 230) };
            gbAlunoCad.Controls.Add(new Label() { Text = "Digite o ID desejado (Numérico):", Location = new Point(20, 35), Size = new Size(200, 20) });
            txtIdAlunoCad = new TextBox() { Location = new Point(20, 60), Size = new Size(320, 20) };
            gbAlunoCad.Controls.Add(new Label() { Text = "Digite o seu Nome Completo:", Location = new Point(20, 95), Size = new Size(200, 20) });
            txtNomeAlunoCad = new TextBox() { Location = new Point(20, 120), Size = new Size(320, 20) };
            btnCadastrarAluno = new Button() { Text = "Criar Conta", Location = new Point(20, 170), Size = new Size(150, 30) };
            btnCadastrarAluno.Click += BtnCadastrarAluno_Click;
            gbAlunoCad.Controls.Add(txtIdAlunoCad); gbAlunoCad.Controls.Add(txtNomeAlunoCad); gbAlunoCad.Controls.Add(btnCadastrarAluno);

            pageLogin.Controls.Add(gbADM);
            pageLogin.Controls.Add(gbAluno);
            pageLogin.Controls.Add(gbAlunoCad);
        }

        private void ConfigurarTelaADM()
        {
            // 1. Criar o Grid Principal da Tela ADM (Divide em duas colunas: Esquerda 45% e Direita 55%)
            TableLayoutPanel layoutADM = new TableLayoutPanel();
            layoutADM.Dock = DockStyle.Fill;
            layoutADM.ColumnCount = 2;
            layoutADM.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            layoutADM.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            layoutADM.RowCount = 1;
            layoutADM.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // 2. Painel da Esquerda (Formulário + Lista de Livros abaixo)
            FlowLayoutPanel painelEsquerdo = new FlowLayoutPanel() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true };

            // CRUD de Livros
            GroupBox gbCrud = new GroupBox() { Text = "Gerir Catálogo (Inserir / Alterar Livros)", Size = new Size(380, 250), Margin = new Padding(10) };
            gbCrud.Controls.Add(new Label() { Text = "ID do Livro (Para Alterar/Deletar):", Location = new Point(15, 25), Size = new Size(200, 15) });
            txtLivroIdAlterar = new TextBox() { Location = new Point(15, 45), Size = new Size(80, 20) };
            gbCrud.Controls.Add(new Label() { Text = "Título do Livro:", Location = new Point(15, 75), Size = new Size(100, 15) });
            txtLivroTitulo = new TextBox() { Location = new Point(15, 95), Size = new Size(340, 20) };
            gbCrud.Controls.Add(new Label() { Text = "Autor:", Location = new Point(15, 125), Size = new Size(100, 15) });
            txtLivroAutor = new TextBox() { Location = new Point(15, 145), Size = new Size(340, 20) };
            gbCrud.Controls.Add(new Label() { Text = "Ano de Lançamento:", Location = new Point(15, 175), Size = new Size(150, 15) });
            txtLivroAno = new TextBox() { Location = new Point(15, 195), Size = new Size(100, 20) };

            btnCadastrarLivro = new Button() { Text = "Cadastrar", Location = new Point(110, 215), Size = new Size(80, 25) };
            btnAlterarLivro = new Button() { Text = "Alterar", Location = new Point(195, 215), Size = new Size(80, 25) };
            btnDeletarLivro = new Button() { Text = "Deletar", Location = new Point(280, 215), Size = new Size(80, 25) };

            btnCadastrarLivro.Click += BtnCadastrarLivro_Click;
            btnAlterarLivro.Click += BtnAlterarLivro_Click;
            btnDeletarLivro.Click += BtnDeletarLivro_Click;

            gbCrud.Controls.Add(txtLivroIdAlterar); gbCrud.Controls.Add(txtLivroTitulo);
            gbCrud.Controls.Add(txtLivroAutor); gbCrud.Controls.Add(txtLivroAno);
            gbCrud.Controls.Add(btnCadastrarLivro); gbCrud.Controls.Add(btnAlterarLivro); gbCrud.Controls.Add(btnDeletarLivro);
            painelEsquerdo.Controls.Add(gbCrud);

            // Tabela de Livros Ativos
            GroupBox gbTabelaLivros = new GroupBox() { Text = "Todos os Livros Ativos no Catálogo", Size = new Size(380, 250), Margin = new Padding(10) };
            dgvLivrosADM = new DataGridView() { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            gbTabelaLivros.Controls.Add(dgvLivrosADM);
            painelEsquerdo.Controls.Add(gbTabelaLivros);

            btnLogoutADM = new Button() { Text = "Fazer Logout", Size = new Size(120, 30), Margin = new Padding(10), BackColor = Color.LightGray };
            btnLogoutADM.Click += (s, e) => { tabControlPrincipal.SelectedTab = pageLogin; };
            painelEsquerdo.Controls.Add(btnLogoutADM);

            // 3. Painel da Direita (Dividido verticalmente por igual: 50% Pedidos, 50% Alunos)
            TableLayoutPanel painelDireito = new TableLayoutPanel() { Dock = DockStyle.Fill };
            painelDireito.RowCount = 2;
            painelDireito.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            painelDireito.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            painelDireito.ColumnCount = 1;
            painelDireito.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Bloco Superior Direito: Pedidos
            GroupBox gbPedidos = new GroupBox() { Text = "Pedidos de Empréstimo Pendentes", Dock = DockStyle.Fill, Margin = new Padding(5) };
            TableLayoutPanel layoutBotoesPedidos = new TableLayoutPanel() { Dock = DockStyle.Bottom, Height = 40, ColumnCount = 2 };
            layoutBotoesPedidos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layoutBotoesPedidos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            
            btnAceitarPedido = new Button() { Text = "APROVAR PEDIDO", Dock = DockStyle.Fill, BackColor = Color.LightGreen };
            btnRejeitarPedido = new Button() { Text = "NEGAR PEDIDO", Dock = DockStyle.Fill, BackColor = Color.LightCoral };
            btnAceitarPedido.Click += BtnAceitarPedido_Click;
            btnRejeitarPedido.Click += BtnRejeitarPedido_Click;
            
            layoutBotoesPedidos.Controls.Add(btnAceitarPedido, 0, 0);
            layoutBotoesPedidos.Controls.Add(btnRejeitarPedido, 1, 0);

            dgvPedidosPendentes = new DataGridView() { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            gbPedidos.Controls.Add(dgvPedidosPendentes);
            gbPedidos.Controls.Add(layoutBotoesPedidos);
            painelDireito.Controls.Add(gbPedidos, 0, 0);

            // Bloco Inferior Direito: Alunos
            GroupBox gbAlunos = new GroupBox() { Text = "Lista de Alunos Cadastrados", Dock = DockStyle.Fill, Margin = new Padding(5) };
            dgvAlunosADM = new DataGridView() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            gbAlunos.Controls.Add(dgvAlunosADM);
            painelDireito.Controls.Add(gbAlunos, 0, 1);

            // 4. Integrar no Grid Principal
            layoutADM.Controls.Add(painelEsquerdo, 0, 0);
            layoutADM.Controls.Add(painelDireito, 1, 0);

            pageADM.Controls.Add(layoutADM);
        }

        private void ConfigurarTelaAluno()
        {
            // Catálogo e Busca
            GroupBox gbBusca = new GroupBox() { Text = "Catálogo de Livros Disponíveis", Location = new Point(20, 20), Size = new Size(410, 520) };
            gbBusca.Controls.Add(new Label() { Text = "Busca Filtrada por Título:", Location = new Point(15, 25), Size = new Size(200, 15) });
            txtBuscaTitulo = new TextBox() { Location = new Point(15, 45), Size = new Size(380, 20) };
            txtBuscaTitulo.TextChanged += TxtBuscaTitulo_TextChanged;

            dgvLivrosAluno = new DataGridView() { Location = new Point(15, 80), Size = new Size(380, 380), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            btnSolicitarEmprestimo = new Button() { Text = "SOLICITAR EMPRÉSTIMO DO LIVRO SELECIONADO", Location = new Point(15, 475), Size = new Size(380, 30), BackColor = Color.LightBlue };
            btnSolicitarEmprestimo.Click += BtnSolicitarEmprestimo_Click;
            gbBusca.Controls.Add(txtBuscaTitulo); gbBusca.Controls.Add(dgvLivrosAluno); gbBusca.Controls.Add(btnSolicitarEmprestimo);

            // Minhas Solicitações
            GroupBox gbSolicitacoes = new GroupBox() { Text = "Minhas Solicitações de Empréstimo", Location = new Point(450, 20), Size = new Size(400, 520) };
            dgvMeusEmprestimos = new DataGridView() { Location = new Point(15, 30), Size = new Size(370, 430), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            btnCancelarEmprestimo = new Button() { Text = "CANCELAR SOLICITAÇÃO PENDENTE", Location = new Point(15, 475), Size = new Size(370, 30) };
            btnCancelarEmprestimo.Click += BtnCancelarEmprestimo_Click;
            gbSolicitacoes.Controls.Add(dgvMeusEmprestimos); gbSolicitacoes.Controls.Add(btnCancelarEmprestimo);

            btnLogoutAluno = new Button() { Text = "Terminar Sessão", Location = new Point(20, 560), Size = new Size(120, 25) };
            btnLogoutAluno.Click += (s, e) => { alunoLogado = null; tabControlPrincipal.SelectedTab = pageLogin; };

            pageAluno.Controls.Add(gbBusca); pageAluno.Controls.Add(gbSolicitacoes); pageAluno.Controls.Add(btnLogoutAluno);
        }

        // ============================================================================
        // 3. REGRAS DE NEGÓCIO E LÓGICA DOS BOTÕES
        // ============================================================================

        private void BtnLoginADM_Click(object? sender, EventArgs e)
        {
            if (txtPinADM.Text == "2026")
            {
                txtPinADM.Clear();
                AtualizarTabelasADM();
                tabControlPrincipal.SelectedTab = pageADM;
                MessageBox.Show("Acesso administrativo concedido.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("PIN de acesso incorreto! Operação cancelada.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLoginAluno_Click(object? sender, EventArgs e)
        {
            if (int.TryParse(txtIdAlunoLogin.Text, out int idBusca))
            {
                var aluno = listaAlunos.FirstOrDefault(a => a.IdEstudante == idBusca && a.Ativo);
                if (aluno != null)
                {
                    alunoLogado = aluno;
                    txtIdAlunoLogin.Clear();
                    AtualizarTabelasAluno();
                    tabControlPrincipal.SelectedTab = pageAluno;
                    MessageBox.Show($"Login efetuado! Bem-vindo, {aluno.NomeCompleto}.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("ID de estudante não encontrado na base de dados!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("O ID deve ser numérico!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnCadastrarAluno_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtIdAlunoCad.Text, out int id) || string.IsNullOrWhiteSpace(txtNomeAlunoCad.Text))
            {
                MessageBox.Show("Dados inválidos! Preencha o ID numérico e o Nome.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (listaAlunos.Any(a => a.IdEstudante == id))
            {
                MessageBox.Show("Este ID de estudante já se encontra registado!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            listaAlunos.Add(new Aluno { IdEstudante = id, NomeCompleto = txtNomeAlunoCad.Text, Ativo = true });
            txtIdAlunoCad.Clear(); txtNomeAlunoCad.Clear();
            MessageBox.Show("Conta criada com sucesso! Podes logar com o teu ID.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCadastrarLivro_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLivroTitulo.Text) || string.IsNullOrWhiteSpace(txtLivroAutor.Text) || !int.TryParse(txtLivroAno.Text, out int ano))
            {
                MessageBox.Show("Por favor, preencha todos os campos corretamente!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ano < 100 || ano > 2026)
            {
                MessageBox.Show("Ano inválido (Permitido entre 100 e 2026)!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (listaLivros.Any(l => l.Ativo && l.Titulo.Equals(txtLivroTitulo.Text, StringComparison.OrdinalIgnoreCase) && l.Autor.Equals(txtLivroAutor.Text, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Livro duplicado no catálogo!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Livro novo = new Livro { Id = proximoIdLivro++, Titulo = txtLivroTitulo.Text, Autor = txtLivroAutor.Text, AnoLancamento = ano, Ativo = true };
            listaLivros.Add(novo);
            LimparCamposLivro();
            AtualizarTabelasADM();
            MessageBox.Show($"ID gerado: {novo.Id} | Livro guardado!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAlterarLivro_Click(object? sender, EventArgs e)
        {
            if (int.TryParse(txtLivroIdAlterar.Text, out int idBusca))
            {
                var livro = listaLivros.FirstOrDefault(l => l.Id == idBusca && l.Ativo);
                if (livro != null)
                {
                    livro.Titulo = txtLivroTitulo.Text;
                    livro.Autor = txtLivroAutor.Text;
                    livro.AnoLancamento = int.Parse(txtLivroAno.Text);
                    LimparCamposLivro();
                    AtualizarTabelasADM();
                    MessageBox.Show("Livro alterado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else { MessageBox.Show("Livro não encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void BtnDeletarLivro_Click(object? sender, EventArgs e)
        {
            if (int.TryParse(txtLivroIdAlterar.Text, out int idBusca))
            {
                var livro = listaLivros.FirstOrDefault(l => l.Id == idBusca && l.Ativo);
                if (livro != null)
                {
                    var conf = MessageBox.Show($"Confirmar remoção de {livro.Titulo}?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (conf == DialogResult.Yes)
                    {
                        livro.Ativo = false;
                        LimparCamposLivro();
                        AtualizarTabelasADM();
                        MessageBox.Show("Livro removido do catálogo.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void TxtBuscaTitulo_TextChanged(object? sender, EventArgs e)
        {
            AtualizarTabelasAluno();
        }

        private void BtnSolicitarEmprestimo_Click(object? sender, EventArgs e)
        {
            if (dgvLivrosAluno.CurrentRow != null && alunoLogado != null)
            {
                int idLivro = (int)dgvLivrosAluno.CurrentRow.Cells["Id"].Value;
                var conf = MessageBox.Show("Desejas solicitar o empréstimo deste livro?", "Solicitação", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (conf == DialogResult.Yes)
                {
                    Emprestimo emp = new Emprestimo { CodigoAluguel = proximoCodigoAluguel++, IdEstudante = alunoLogado.IdEstudante, IdLivro = idLivro, Status = "PENDENTE" };
                    listaEmprestimos.Add(emp);
                    AtualizarTabelasAluno();
                    MessageBox.Show($"Pedido enviado! Código gerado: #{emp.CodigoAluguel}", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnCancelarEmprestimo_Click(object? sender, EventArgs e)
        {
            if (dgvMeusEmprestimos.CurrentRow != null && alunoLogado != null)
            {
                int codAluguel = (int)dgvMeusEmprestimos.CurrentRow.Cells["CodigoAluguel"].Value;
                var emp = listaEmprestimos.FirstOrDefault(em => em.CodigoAluguel == codAluguel);
                
                if (emp != null)
                {
                    if (emp.Status != "PENDENTE")
                    {
                        MessageBox.Show("Não pode cancelar pedidos já processados pela ADM.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    emp.Status = "CANCELADO";
                    AtualizarTabelasAluno();
                    MessageBox.Show($"Pedido #{codAluguel} CANCELADO.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnAceitarPedido_Click(object? sender, EventArgs e)
        {
            ProcessarPedidoADM("ACEITE");
        }

        private void BtnRejeitarPedido_Click(object? sender, EventArgs e)
        {
            ProcessarPedidoADM("REJEITADO");
        }

        private void ProcessarPedidoADM(string novoStatus)
        {
            if (dgvPedidosPendentes.CurrentRow != null)
            {
                int codAluguel = (int)dgvPedidosPendentes.CurrentRow.Cells["CodigoAluguel"].Value;
                var emp = listaEmprestimos.FirstOrDefault(em => em.CodigoAluguel == codAluguel);
                if (emp != null)
                {
                    emp.Status = novoStatus;
                    AtualizarTabelasADM();
                    MessageBox.Show($"Pedido processado como: {novoStatus}!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // ============================================================================
        // 4. ATUALIZADORES DE INTERFACE E CARGA DE DADOS
        // ============================================================================

        private void AtualizarTabelasADM()
        {
            dgvLivrosADM.DataSource = null;
            dgvLivrosADM.DataSource = listaLivros.Where(l => l.Ativo).ToList();

            dgvAlunosADM.DataSource = null;
            dgvAlunosADM.DataSource = listaAlunos.Where(a => a.Ativo).ToList();

            dgvPedidosPendentes.DataSource = null;
            dgvPedidosPendentes.DataSource = listaEmprestimos.Where(em => em.Status == "PENDENTE").ToList();
        }

        private void AtualizarTabelasAluno()
        {
            string termo = txtBuscaTitulo.Text.ToLower();
            dgvLivrosAluno.DataSource = null;
            dgvLivrosAluno.DataSource = listaLivros.Where(l => l.Ativo && l.Titulo.ToLower().Contains(termo)).ToList();

            if (alunoLogado != null)
            {
                dgvMeusEmprestimos.DataSource = null;
                dgvMeusEmprestimos.DataSource = listaEmprestimos.Where(em => em.IdEstudante == alunoLogado.IdEstudante).ToList();
            }
        }

        private void LimparCamposLivro()
        {
            txtLivroIdAlterar.Clear(); txtLivroTitulo.Clear(); txtLivroAutor.Clear(); txtLivroAno.Clear();
        }

        private void CargarDadosExemplo()
        {
            listaAlunos.Add(new Aluno { IdEstudante = 123, NomeCompleto = "Efraim Kolemba", Ativo = true });
            listaLivros.Add(new Livro { Id = proximoIdLivro++, Titulo = "Calculo I", Autor = "James Stewart", AnoLancamento = 2015, Ativo = true });
            listaLivros.Add(new Livro { Id = proximoIdLivro++, Titulo = "Estruturas de Dados em C", Autor = "Tenenbaum", AnoLancamento = 2020, Ativo = true });
        }
    }
}