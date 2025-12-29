    using DocumentFormat.OpenXml.Spreadsheet;
    using GenstarXKulayInventorySystem.Shared.DTOS;
    using GenstarXKulayInventorySystem.Shared.Helpers;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
    using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

    namespace GenstarXKulayInventorySystem.Client.Pages.Sales.DailySales;

    public partial class GetAllDailySales
    {
        [Parameter, SupplyParameterFromQuery(Name = "pageskip")]
        public int PageSkip { get; set; } = 0;
        [Parameter, SupplyParameterFromQuery(Name ="pagetake")]
        public int PageTake { get; set; } = 10;
        [Parameter, SupplyParameterFromQuery(Name = "date")]
        public string? DateString { get; set; } 
        [Inject] protected HttpClient HttpClient { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected ILogger<GetAllDailySales> Logger { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected UserState UserState { get; set; } = default!;
        private MudTable<DailySaleDto>? dailySaleTable;
        protected List<DailySaleDto> Sales { get; set; } = new List<DailySaleDto>();
        protected IEnumerable<DailySaleDto> FilteredSales { get; set; } = new List<DailySaleDto>();
        protected BranchOption Branch { get; set; } 
        protected bool IsLoading { get; set; } = false;
        protected DateTime Today { get; set; } = DateTime.Now;
        protected DateTime SelectedDate { get; set; } = DateTime.Now;
        private int CurrentPageIndex { get; set; }
        protected string SearchText { get; set; } = string.Empty;
        protected SaleSearchCategory SelectedSearchCategory { get; set; } = SaleSearchCategory.ReceiptNumber;
        protected override void OnInitialized ()
        {
            Branch = UserState.Branch.GetValueOrDefault();
            //await LoadData();
        }

        
   
        protected override async Task OnParametersSetAsync()
        {
            if (PageTake <= 0)
                PageTake = 10;

            if (PageSkip < 0)
                PageSkip = 0;

            if (!string.IsNullOrEmpty(DateString) && DateTime.TryParse(DateString, out var parsedDate))
            {
                SelectedDate = parsedDate;
            
            }
        

            CurrentPageIndex = PageTake > 0 ? PageSkip / PageTake : 0;

            // Load sales whenever query parameters change
            await LoadData();
        }



        protected async Task LoadData()
        {
            IsLoading = true;
            try
            {
                var response = await HttpClient.GetAsync($"api/sales/all/{Branch}/{SelectedDate:yyyy-MM-dd}");
                if (response.IsSuccessStatusCode)
                {
                    var sales = await response.Content.ReadFromJsonAsync<List<DailySaleDto>>();
                    if (sales != null)
                    {
                        Sales = sales;
                        FilteredSales = Sales;
                        if (Sales.Count == 0)
                        {
                            Snackbar.Add("No Sales Found", Severity.Warning);
                        }
                    }
                }
                else
                {
                    Snackbar.Add("Failed to load sales", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading sales data: {ex.Message}");
                Snackbar.Add("An error occurred while loading data", Severity.Error);
            }
            finally
            {
                await Task.Delay(1000);
                IsLoading = false;
            }
        }
    
        protected async Task OnDateChanged(DateTime? newDate)
        {
            if (newDate == null)
                return;

            SelectedDate = newDate.Value;
            PageSkip = 0;
            UpdateQuery();
            await LoadData();
        }

        private void OnPageChanged(int pageIndex)
        {
            CurrentPageIndex = pageIndex;
            PageSkip = pageIndex * PageTake;
            UpdateQuery();
        }

        private void OnRowsPerPageChanged(int size)
        {
            PageTake = size;
            PageSkip = 0;
            UpdateQuery();
        }

        private void UpdateQuery()
        {
            NavigationManager.NavigateTo(
                $"/sales?pageskip={PageSkip}&pagetake={PageTake}&date={SelectedDate:yyyy-MM-dd}",
                replace: true
            );
        }
        protected void CreateSaleAsync()
        {
            NavigationManager.NavigateTo($"/sales/create?pageskip={PageSkip}&pagetake={PageTake}&date={SelectedDate:yyyy-MM-dd}");
        }

        protected void ViewSaleAsync(int Id)
        {
            NavigationManager.NavigateTo($"sales/view/{Id}?pageskip={PageSkip}&pagetake={PageTake}&date={SelectedDate:yyyy-MM-dd}");
        }


    private void ApplySearch()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredSales = Sales;
            PageSkip = 0;
            return;
        }

        FilteredSales = SelectedSearchCategory switch
        {
            SaleSearchCategory.ReceiptNumber =>
                Sales.Where(s =>
                    !string.IsNullOrEmpty(s.RecieptReference) &&
                    s.RecieptReference.Contains(SearchText, StringComparison.OrdinalIgnoreCase)),

            SaleSearchCategory.ClientName =>
                Sales.Where(s =>
                    !string.IsNullOrEmpty(s.Client?.ClientName) &&
                    s.Client.ClientName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)),

            _ => Sales
        };

        PageSkip = 0;
    }

    private void OnSearchChanged(string value)
    {
        SearchText = value;
        ApplySearch();
    }

    private void OnSearchCategoryChanged(SaleSearchCategory category)
    {
        SelectedSearchCategory = category;
        ApplySearch();
    }
}
