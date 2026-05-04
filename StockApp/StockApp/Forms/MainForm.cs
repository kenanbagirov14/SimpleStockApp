using StockApp.Models;
using StockApp.Services;
using ClosedXML.Excel;

namespace StockApp.Forms;

public class MainForm : Form
{
    private readonly ProductService _productService = new();
    private readonly StockService _stockService = new();

    private TextBox txtBarcode;
    private TextBox txtProductName;
    private NumericUpDown numQuantity;
    private Label lblSelectedProduct;
    private DataGridView gridProducts;

    private Button btnAddProduct;
    private Button btnUpdateProduct;
    private Button btnDeleteProduct;
    private Button btnStockIn;
    private Button btnStockOut;
    private Button btnExportExcel;
    private Button btnBackupDb;
    private Button btnRestoreDb;
    private Button btnClear;

    private Product selectedProduct;
    private bool isChangingBarcodeProgrammatically;

    public MainForm()
    {
        InitializeComponent();
        LoadProducts();
        SelectFirstRowIfExists();
        SetButtonsState();
    }

    private void InitializeComponent()
    {
        Text = "Stok Nəzarət";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        this.Padding = new Padding(20);


        var lblBarcode = new Label
        {
            Text = "Barkod:",
            Left = 20,
            Top = 20,
            Width = 100
        };

        txtBarcode = new TextBox
        {
            Left = 130,
            Top = 18,
            Width = 250
        };

        txtBarcode.KeyDown += TxtBarcode_KeyDown;
        txtBarcode.KeyPress += TxtBarcode_KeyPress;
        txtBarcode.TextChanged += TxtBarcode_TextChanged;

        var lblProductName = new Label
        {
            Text = "Məhsul adı:",
            Left = 20,
            Top = 60,
            Width = 100
        };

        txtProductName = new TextBox
        {
            Left = 130,
            Top = 58,
            Width = 250
        };

        btnAddProduct = new Button
        {
            Text = "Əlavə et",
            Left = 400,
            Top = 56,
            Width = 100,
            Height = 35
            
        };

        btnAddProduct.Click += BtnAddProduct_Click;

        btnUpdateProduct = new Button
        {
            Text = "Dəyiş",
            Left = 510,
            Top = 56,
            Width = 100,
            Height = 35,
            Enabled = false
        };

        btnUpdateProduct.Click += BtnUpdateProduct_Click;

        btnDeleteProduct = new Button
        {
            Text = "Sil",
            Left = 620,
            Top = 56,
            Width = 100,
            Height = 35,
            Enabled = false
        };

        btnDeleteProduct.Click += BtnDeleteProduct_Click;

        lblSelectedProduct = new Label
        {
            Text = "Seçilmiş məhsul: yoxdur",
            Left = 20,
            Top = 105,
            Width = 850
        };

        var lblQuantity = new Label
        {
            Text = "Miqdar:",
            Left = 20,
            Top = 145,
            Width = 100
        };

        numQuantity = new NumericUpDown
        {
            Left = 130,
            Top = 143,
            Width = 120,
            DecimalPlaces = 0,
            Minimum = 1,
            Maximum = 1000000,
            Value = 1
        };

        numQuantity.ValueChanged += (s, e) => SetButtonsState();

        btnStockIn = new Button
        {
            Text = "Stoka giriş",
            Left = 270,
            Top = 140,
            Width = 120,
            Height = 35,
            Enabled = false
        };

        btnStockIn.Click += BtnStockIn_Click;

        btnStockOut = new Button
        {
            Text = "Stokdan çıxış",
            Left = 400,
            Top = 140,
            Width = 120,
            Height = 35,
            Enabled = false
        };

        btnStockOut.Click += BtnStockOut_Click;


        btnExportExcel = new Button
        {
            Text = "Excel export",
            Left = 540,
            Top = 140,
            Height = 35,
            Width = 120
        };

        btnExportExcel.Click += BtnExportExcel_Click;

        btnBackupDb = new Button
        {
            Text = "DB backup",
            Left = 670,
            Top = 140,
            Height = 35,
            Width = 120
        };

        btnBackupDb.Click += BtnBackupDb_Click;


        btnRestoreDb = new Button
        {
            Text = "DB Restore",
            Left = 800,
            Top = 140,
            Height = 35,
            Width = 120
        };

        btnRestoreDb.Click += BtnRestoreDb_Click;

        btnClear = new Button
        {
            Text = "Təmizlə",
            Left = 730,
            Top = 56,
            Height = 35,
            Width = 100
        };

        btnClear.Click += BtnClear_Click;

        gridProducts = new DataGridView
        {
            Left = 20,
            Top = 200,
            Height = 400,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };

        gridProducts.SelectionChanged += GridProducts_SelectionChanged;

        Controls.Add(lblBarcode);
        Controls.Add(txtBarcode);
        Controls.Add(lblProductName);
        Controls.Add(txtProductName);
        Controls.Add(btnAddProduct);
        Controls.Add(btnUpdateProduct);
        Controls.Add(btnDeleteProduct);
        Controls.Add(lblSelectedProduct);
        Controls.Add(lblQuantity);
        Controls.Add(numQuantity);
        Controls.Add(btnStockIn);
        Controls.Add(btnStockOut);
        Controls.Add(gridProducts);
        Controls.Add(btnExportExcel);
        Controls.Add(btnBackupDb);
        Controls.Add(btnRestoreDb);
        Controls.Add(btnClear);


        this.Resize += (s, e) =>
        {
            gridProducts.Left = 20; // hər halda sabitlə
            gridProducts.Width = this.ClientSize.Width - 40; // sol+sağ margin

            gridProducts.Height = this.ClientSize.Height - gridProducts.Top - 20; // aşağı margin
        };
    }

    private void TxtBarcode_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (char.IsControl(e.KeyChar))
            return;

        if (!char.IsDigit(e.KeyChar))
            e.Handled = true;
    }

    private void TxtBarcode_TextChanged(object sender, EventArgs e)
    {
        if (isChangingBarcodeProgrammatically)
            return;

        selectedProduct = null;
        lblSelectedProduct.Text = "Seçilmiş məhsul: yoxdur";

        gridProducts.ClearSelection();
        SetButtonsState();
    }

    private void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        e.SuppressKeyPress = true;

        var barcode = txtBarcode.Text.Trim();

        if (!ValidateBarcode(barcode))
            return;

        selectedProduct = _productService.GetByBarcode(barcode);

        if (selectedProduct == null)
        {
            lblSelectedProduct.Text = "Məhsul tapılmadı. Adını yazıb əlavə edə bilərsən.";
            SetButtonsState();
            txtProductName.Focus();
            return;
        }

        FillProductInputs(selectedProduct);
        SelectRowByProductId(selectedProduct.Id);
        ShowSelectedProduct();
        SetButtonsState();

        numQuantity.Focus();
    }

    private void GridProducts_SelectionChanged(object sender, EventArgs e)
    {
        if (gridProducts.CurrentRow == null)
            return;

        if (gridProducts.CurrentRow.DataBoundItem is not Product product)
            return;

        selectedProduct = product;

        FillProductInputs(product);
        ShowSelectedProduct();
        SetButtonsState();
    }

    private void BtnAddProduct_Click(object sender, EventArgs e)
    {
        var barcode = txtBarcode.Text.Trim();
        var name = txtProductName.Text.Trim();
        var quantity = numQuantity.Text.Trim();

        if (!ValidateBarcode(barcode))
            return;

        if (!ValidateProductName(name))
            return;

        try
        {
            var existingProduct = _productService.GetByBarcode(barcode);

            if (existingProduct != null)
            {
                MessageBox.Show("Bu barkodla məhsul artıq mövcuddur.");
                return;
            }

            _productService.AddProduct(barcode, name, quantity);

            MessageBox.Show("Məhsul əlavə edildi.");

            LoadProducts();

            selectedProduct = _productService.GetByBarcode(barcode);
            SelectRowByProductId(selectedProduct.Id);
            FillProductInputs(selectedProduct);
            ShowSelectedProduct();
            SetButtonsState();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Xəta: " + ex.Message);
        }
    }

    private void BtnUpdateProduct_Click(object sender, EventArgs e)
    {
        if (selectedProduct == null)
        {
            MessageBox.Show("Dəyişmək üçün məhsul seç.");
            return;
        }

        var barcode = txtBarcode.Text.Trim();
        var name = txtProductName.Text.Trim();

        if (!ValidateBarcode(barcode))
            return;

        if (!ValidateProductName(name))
            return;

        try
        {
            var existingProduct = _productService.GetByBarcode(barcode);

            if (existingProduct != null && existingProduct.Id != selectedProduct.Id)
            {
                MessageBox.Show("Bu barkod başqa məhsula aiddir.");
                return;
            }

            _productService.UpdateProduct(selectedProduct.Id, barcode, name);

            MessageBox.Show("Məhsul dəyişdirildi.");

            LoadProducts();

            selectedProduct = _productService.GetByBarcode(barcode);
            SelectRowByProductId(selectedProduct.Id);
            FillProductInputs(selectedProduct);
            ShowSelectedProduct();
            SetButtonsState();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Xəta: " + ex.Message);
        }
    }

    private void BtnDeleteProduct_Click(object sender, EventArgs e)
    {
        if (selectedProduct == null)
        {
            MessageBox.Show("Silmək üçün məhsul seç.");
            return;
        }

        var confirm = MessageBox.Show(
            $"'{selectedProduct.Name}' məhsulu silinsin?",
            "Təsdiq",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
            return;

        try
        {
            _productService.DeleteProduct(selectedProduct.Id);

            MessageBox.Show("Məhsul silindi.");

            selectedProduct = null;
            ClearInputs();

            LoadProducts();
            SelectFirstRowIfExists();
            SetButtonsState();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Xəta: " + ex.Message);
        }
    }

    private void BtnStockIn_Click(object sender, EventArgs e)
    {
        if (!ValidateStockOperation())
            return;

        try
        {
            _stockService.StockIn(selectedProduct.Id, numQuantity.Value);

            MessageBox.Show("Stoka giriş edildi.");

            var productId = selectedProduct.Id;

            LoadProducts();
            SelectRowByProductId(productId);
            RefreshSelectedProduct();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Xəta: " + ex.Message);
        }
    }

    private void BtnStockOut_Click(object sender, EventArgs e)
    {
        if (!ValidateStockOperation())
            return;

        if (numQuantity.Value > selectedProduct.Quantity)
        {
            MessageBox.Show("Stokda kifayət qədər məhsul yoxdur.");
            return;
        }

        try
        {
            _stockService.StockOut(selectedProduct.Id, numQuantity.Value);

            MessageBox.Show("Stokdan çıxış edildi.");

            var productId = selectedProduct.Id;

            LoadProducts();
            SelectRowByProductId(productId);
            RefreshSelectedProduct();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Xəta: " + ex.Message);
        }
    }

    private bool ValidateBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            MessageBox.Show("Barkod boş ola bilməz.");
            return false;
        }

        if (!barcode.All(char.IsDigit))
        {
            MessageBox.Show("Barkod yalnız rəqəmlərdən ibarət olmalıdır.");
            return false;
        }

        if (barcode.Length < 4)
        {
            MessageBox.Show("Barkod ən az 4 rəqəm olmalıdır.");
            return false;
        }

        return true;
    }

    private bool ValidateProductName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Məhsul adı boş ola bilməz.");
            return false;
        }

        if (name.Length < 2)
        {
            MessageBox.Show("Məhsul adı ən az 2 simvol olmalıdır.");
            return false;
        }

        return true;
    }

    private bool ValidateStockOperation()
    {
        if (selectedProduct == null)
        {
            MessageBox.Show("Əvvəl məhsul seç.");
            return false;
        }

        if (numQuantity.Value <= 0)
        {
            MessageBox.Show("Miqdar 0-dan böyük olmalıdır.");
            return false;
        }

        if (numQuantity.Value != Math.Truncate(numQuantity.Value))
        {
            MessageBox.Show("Miqdar yalnız tam ədəd ola bilər.");
            return false;
        }

        return true;
    }

    private void SetButtonsState()
    {
        var hasProduct = selectedProduct != null;
        var hasValidQuantity = numQuantity != null && numQuantity.Value > 0;

        if (btnStockIn != null)
            btnStockIn.Enabled = hasProduct && hasValidQuantity;

        if (btnStockOut != null)
            btnStockOut.Enabled = hasProduct && hasValidQuantity;

        if (btnUpdateProduct != null)
            btnUpdateProduct.Enabled = hasProduct;

        if (btnDeleteProduct != null)
            btnDeleteProduct.Enabled = hasProduct;
    }

    private void FillProductInputs(Product product)
    {
        isChangingBarcodeProgrammatically = true;

        txtBarcode.Text = product.Barcode;
        txtProductName.Text = product.Name;

        isChangingBarcodeProgrammatically = false;
    }

    private void ClearInputs()
    {
        isChangingBarcodeProgrammatically = true;

        txtBarcode.Clear();
        txtProductName.Clear();
        numQuantity.Value = 1;

        isChangingBarcodeProgrammatically = false;

        lblSelectedProduct.Text = "Seçilmiş məhsul: yoxdur";
    }

    private void ShowSelectedProduct()
    {
        if (selectedProduct == null)
        {
            lblSelectedProduct.Text = "Seçilmiş məhsul: yoxdur";
            return;
        }

        lblSelectedProduct.Text =
            $"Seçilmiş məhsul: {selectedProduct.Name} | Barkod: {selectedProduct.Barcode} | Qalıq: {selectedProduct.Quantity}";
    }

    private void RefreshSelectedProduct()
    {
        if (selectedProduct == null)
            return;

        selectedProduct = _productService.GetByBarcode(selectedProduct.Barcode);

        FillProductInputs(selectedProduct);
        ShowSelectedProduct();
        SetButtonsState();
    }

    private void LoadProducts()
    {
        gridProducts.DataSource = null;
        gridProducts.DataSource = _productService.GetAll();

        if (gridProducts.Columns["Id"] != null)
            gridProducts.Columns["Id"].HeaderText = "ID";

        if (gridProducts.Columns["Barcode"] != null)
            gridProducts.Columns["Barcode"].HeaderText = "Barkod";

        if (gridProducts.Columns["Name"] != null)
            gridProducts.Columns["Name"].HeaderText = "Məhsul adı";

        if (gridProducts.Columns["Quantity"] != null)
            gridProducts.Columns["Quantity"].HeaderText = "Qalıq";

        if (gridProducts.Columns["CreatedDate"] != null)
            gridProducts.Columns["CreatedDate"].HeaderText = "Yaranma tarixi";
    }

    private void SelectFirstRowIfExists()
    {
        if (gridProducts.Rows.Count == 0)
        {
            selectedProduct = null;
            ClearInputs();
            return;
        }

        gridProducts.Rows[0].Selected = true;
        gridProducts.CurrentCell = gridProducts.Rows[0].Cells[0];

        if (gridProducts.Rows[0].DataBoundItem is Product product)
        {
            selectedProduct = product;
            FillProductInputs(product);
            ShowSelectedProduct();
        }
    }

    private void SelectRowByProductId(int productId)
    {
        foreach (DataGridViewRow row in gridProducts.Rows)
        {
            if (row.DataBoundItem is Product product && product.Id == productId)
            {
                row.Selected = true;
                gridProducts.CurrentCell = row.Cells[0];

                selectedProduct = product;
                FillProductInputs(product);
                ShowSelectedProduct();
                SetButtonsState();

                return;
            }
        }
    }

    private void BtnExportExcel_Click(object sender, EventArgs e)
    {
        var products = _productService.GetAll();

        if (products == null || products.Count == 0)
        {
            MessageBox.Show("Export etmək üçün məlumat yoxdur.");
            return;
        }

        using var saveFileDialog = new SaveFileDialog
        {
            Filter = "Excel faylı (*.xlsx)|*.xlsx",
            FileName = $"Stok_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };

        if (saveFileDialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Stok");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Barkod";
            worksheet.Cell(1, 3).Value = "Məhsul adı";
            worksheet.Cell(1, 4).Value = "Qalıq";
            worksheet.Cell(1, 5).Value = "Yaranma tarixi";

            var row = 2;

            foreach (var product in products)
            {
                worksheet.Cell(row, 1).Value = product.Id;
                worksheet.Cell(row, 2).Value = product.Barcode;
                worksheet.Cell(row, 3).Value = product.Name;
                worksheet.Cell(row, 4).Value = product.Quantity;
                worksheet.Cell(row, 5).Value = product.CreatedDate.ToString("dd.MM.yyyy HH:mm:ss");

                row++;
            }

            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(saveFileDialog.FileName);

            MessageBox.Show("Excel export edildi.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Excel export zamanı xəta: " + ex.Message);
        }
    }

    private void BtnBackupDb_Click(object sender, EventArgs e)
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stock.db");

        if (!File.Exists(dbPath))
        {
            MessageBox.Show("Database faylı tapılmadı.");
            return;
        }

        using var saveFileDialog = new SaveFileDialog
        {
            Filter = "SQLite database (*.db)|*.db",
            FileName = $"stock_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
        };

        if (saveFileDialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            File.Copy(dbPath, saveFileDialog.FileName, overwrite: true);

            MessageBox.Show("Database backup alındı.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Backup zamanı xəta: " + ex.Message);
        }
    }

    private void BtnRestoreDb_Click(object sender, EventArgs e)
    {
        var confirm = MessageBox.Show(
            "Restore etdikdə hazırkı bütün data silinəcək. Davam edək?",
            "Diqqət",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
            return;

        using var openFileDialog = new OpenFileDialog
        {
            Filter = "SQLite database (*.db)|*.db"
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stock.db");

            // Əgər file istifadə olunursa problem çıxmasın deyə əvvəl app restart planlayırıq
            File.Copy(openFileDialog.FileName, dbPath, true);

            MessageBox.Show("Database bərpa olundu. App yenidən açılacaq.");

            Application.Restart();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Restore zamanı xəta: " + ex.Message);
        }
    }

    private void BtnClear_Click(object sender, EventArgs e)
    {
        selectedProduct = null;

        ClearInputs();

        gridProducts.ClearSelection();

        SetButtonsState();
    }
}